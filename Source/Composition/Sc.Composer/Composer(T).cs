using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sc.Abstractions.Events;
using Sc.Abstractions.Lifecycle;
using Sc.Diagnostics;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Composer
{
	/// <summary>
	/// A complete <see cref="IComposer{TTarget}"/> implementation.
	/// This class fully implements the interface, but you must implement
	/// <see cref="GetTarget"/>, which is invoked on each composition,
	/// and must construct or return the target for each composition.
	/// You may provide a delegate to the constructor to implement that method.
	/// Please notice: this implementation is thread-safe for the complete
	/// implementation of the <see cref="IComposer{TTarget}"/> interface:
	/// the lists of participants here are protected by a lock,
	/// as is the full composition sequence; but, the lock WILL be released
	/// before the <see cref="Composed"/> event is raised --- and importantly,
	/// the composed <typeparamref name="TTarget"/> on each composition event
	/// will then be raised outside the lock. If you potentially attach
	/// handlers to this event re-composition is implemented,
	/// your implementation must either construct a new target on each event,
	/// or otherwise a singleton returned target object must be intrinsically
	/// thread-safe and could be mutated by more than one possible composition
	/// event sequence in any order --- BUT, only from handlers on THIS
	/// <see cref="Composed"/> event. You may otherwise simply restrict
	/// re-composition. Note also that NO <typeparamref name="TTarget"/>
	/// is disposed by this implementation.
	/// </summary>
	public class Composer<TTarget>
			: IComposer<TTarget>,
					IDispose
	{
		/// <summary>
		/// This object must be locked for all access to the collections.
		/// </summary>
		protected readonly object SyncLock = new object();

		/// <summary>
		/// The actual list of all participants added in <see cref="Participants"/>.
		/// </summary>
		protected readonly List<(IComposerParticipant<TTarget> participant, bool oneTimeOnly)> Participants
				= new List<(IComposerParticipant<TTarget> participant, bool oneTimeOnly)>(1);

		private readonly Func<TTarget> getTarget;
		private bool disposeProvidersWithThis;

		private ComposerExceptionPolicy composeExceptionPolicy
				= ComposerExceptionPolicy.ThrowUnhandledComposerException;

		private bool failOnFirstParticipantException;
		private AggregateComposerException lastComposeError;
		private bool isDisposed;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTarget">Required: this will be invoked in
		/// <see cref="GetTarget"/>,
		/// and can provide a complete implementation.
		/// You MUST otherwise use the protected constructor and
		/// override <see cref="GetTarget"/>.</param>
		/// <param name="disposeProvidersWithThis">Specifies how added providers are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public Composer(
				Func<TTarget> getTarget,
				bool disposeProvidersWithThis = true)
				: this(disposeProvidersWithThis)
			=> this.getTarget = getTarget ?? throw new ArgumentNullException(nameof(getTarget));

		/// <summary>
		/// Protected constructor for subclasses. NOTICE that you MUST
		/// override <see cref="GetTarget"/>.
		/// </summary>
		/// <param name="disposeProvidersWithThis">Specifies how added providers are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		protected Composer(bool disposeProvidersWithThis = true)
			=> this.disposeProvidersWithThis = disposeProvidersWithThis;


		/// <summary>
		/// This protected virtual event handler is added to all
		/// <see cref="IRequestComposition{TTarget}"/> participants, and handles their
		/// <see cref="IRequestComposition{TTarget}.CompositionRequested"/> event.
		/// This handles the given event parameters, and then
		/// invokes the <see cref="HandleCompose"/> method.
		/// </summary> 
		protected virtual void HandleCompositionRequested(object sender, RequestCompositionEventArgs<TTarget> eventArgs)
		{
			TraceSources.For(GetType())
					.Verbose("Handling {0} for participant: {1}", nameof(IRequestComposition<TTarget>), sender);
			eventArgs.Handle(this);
			HandleCompose();
		}


		/// <summary>
		/// This protected virtual method implements all compositions.
		/// The composition proceeds this way:
		///	<code>
		/// // SyncLock is acquired
		/// BeforeCompose()
		/// LastComposeErrors is set null
		/// GetTarget()
		/// WithNewTarget()
		/// HandleProvideParts() // Invokes parts participants
		/// ... IProvideParts callbacks are invoked
		/// WithAllParts()
		/// ... IBootstrap participants are invoked
		/// ... HandleComposed participants are invoked
		/// ... Remove any "oneTimeOnly" participants
		/// AfterComposed()
		/// // SyncLock is released here
		/// RaiseComposed() // Raises this Composed event
		/// </code>
		/// Note that this implementation aggregates the <see cref="LastComposeErrors"/>
		/// with each exception as it happens --- the <see cref="LastComposeErrors"/>
		/// exception itself will be set to a new instance with the caught exception
		/// inserted at the top of the <see cref="AggregateException.InnerExceptions"/>.
		/// </summary>
		/// <returns>Not null.</returns>
		protected virtual TTarget HandleCompose()
		{
			bool throwException = false;
			bool AggregateError(
					bool isParticipant,
					Exception exception,
					IComposerParticipant<TTarget> participant,
					object callbackDelegateTarget,
					string participantName)
			{
				TraceSources.For(GetType())
						.Error(
								exception,
								"Catching exception invoking {0}: '{1}', raised by: {2}.",
								participantName,
								exception.Message,
								participant ?? callbackDelegateTarget);
				lastComposeError
						= lastComposeError.AggregateError(
								(message, innerExceptions) => new AggregateComposerException(
										message,
										innerExceptions?.OfType<ComposerException>()
												.ToArray()
										?? new ComposerException[0]),
								exception is ComposerException composerException
										? composerException
										: new ParticipantException<TTarget>(
												exception,
												participant,
												callbackDelegateTarget),
								"One or more errors occurred during composition:"
								+ " the InnerExceptions holds the errors.");
				switch (composeExceptionPolicy) {
					case ComposerExceptionPolicy.ThrowUnhandledComposerException :
						if (isParticipant)
							return false;
						throwException = true;
						return true;
					case ComposerExceptionPolicy.ThrowParticipantException :
						if (!isParticipant)
							return false;
						throwException = true;
						return true;
					case ComposerExceptionPolicy.ThrowAny :
						throwException = true;
						return true;
					default :
						return false;
				}
			}
			using (TraceSources.For(GetType())
					.BeginScope(nameof(Composer<TTarget>.HandleCompose))) {
				lock (SyncLock) {
					if (IsDisposed)
						throw new ObjectDisposedException(ToString());
					TTarget target = default;
					bool hasTarget = false;
					ComposerEventArgs<TTarget> composerEventArgs = null;
					try {
						BeforeCompose();
						lastComposeError = null;
						TraceSources.For(GetType())
								.Verbose("{0} ...", nameof(Composer<TTarget>.GetTarget));
						target = GetTarget();
						TraceSources.For(GetType())
								.Verbose("Target: {0}", target);
						hasTarget = true;
						WithNewTarget(target);
						ProvidePartsEventArgs<TTarget> providePartsEventArgs
								= new ProvidePartsEventArgs<TTarget>(
										target,
										out Func<IReadOnlyCollection<Action<TTarget>>> providePartsCallbacks);
						foreach (IProvideParts<TTarget> provideParts
								in Participants.Select(tuple => tuple.participant)
										.OfType<IProvideParts<TTarget>>()) {
							TraceSources.For(GetType())
									.Verbose("Invoke {0} participant: {1}", nameof(IProvideParts<TTarget>), provideParts);
							try {
								provideParts.ProvideParts(providePartsEventArgs);
							} catch (Exception exception) {
								if (AggregateError(true, exception, provideParts, null, "ProvideParts Participant")
										|| failOnFirstParticipantException)
									goto FinishCompose;
							}
						}
						foreach (Action<TTarget> callback in providePartsCallbacks()) {
							try {
								callback(target);
							} catch (Exception exception) {
								if (AggregateError(
												true,
												exception,
												null,
												callback.Target,
												"ProvidePartsEventArgs callback")
										|| failOnFirstParticipantException)
									goto FinishCompose;
							}
						}
						TraceSources.For(GetType())
								.Verbose(nameof(Composer<TTarget>.WithAllParts));
						WithAllParts(target);
						composerEventArgs = new ComposerEventArgs<TTarget>(target);
						foreach (IBootstrap<TTarget> bootstrap
								in Participants.Select(tuple => tuple.participant)
										.OfType<IBootstrap<TTarget>>()) {
							TraceSources.For(GetType())
									.Verbose("Invoke {0} participant: {1}", nameof(IBootstrap<TTarget>), bootstrap);
							try {
								bootstrap.HandleBootstrap(composerEventArgs);
							} catch (Exception exception) {
								if (AggregateError(true, exception, bootstrap, null, "Bootstrap Participant")
										|| failOnFirstParticipantException)
									goto FinishCompose;
							}
						}
						TraceSources.For(GetType())
								.Verbose(nameof(Composer<TTarget>.BeforeHandleComposed));
						BeforeHandleComposed(target);
						foreach (IHandleComposed<TTarget> handleComposed
								in Participants.Select(tuple => tuple.participant)
										.OfType<IHandleComposed<TTarget>>()) {
							TraceSources.For(GetType())
									.Verbose("Invoke {0} participant: {1}", nameof(IHandleComposed<TTarget>), handleComposed);
							try {
								handleComposed.HandleComposed(composerEventArgs);
							} catch (Exception exception) {
								if (AggregateError(
												true,
												exception,
												handleComposed,
												null,
												"HandleComposed Participant")
										|| failOnFirstParticipantException)
									goto FinishCompose;
							}
						}
						RemoveOneTimeOnlyParticipants();
						TraceSources.For(GetType())
								.Verbose(nameof(Composer<TTarget>.AfterComposed));
						AfterComposed(target);
					} catch (Exception exception) {
						TraceSources.For(GetType())
								.Error(
										exception,
										"Catching unhandled Composer error in HandleComposed: '{0}'.",
										exception.Message);
						AggregateError(false, exception, null, this, "Unhandled Composer error in HandleComposed");
					}
					FinishCompose:
					if (lastComposeError != null) {
						TraceSources.For(GetType())
								.Error(
										lastComposeError,
										"Setting {0} non-null.",
										nameof(IComposer<TTarget>.LastComposeErrors));
					}
					if (throwException) {
						Debug.Assert(lastComposeError != null, "lastComposeError != null");
						throw lastComposeError;
					}
					if (hasTarget)
						RaiseComposed(composerEventArgs ?? new ComposerEventArgs<TTarget>(target));
					return target;
				}
			}
		}

		/// <summary>
		/// This method is responsible for removing all participants marked
		/// for "oneTimeOnly". This will be invoked under the <see cref="SyncLock"/>
		/// on each composition, before <see cref="RaiseComposed"/> is invoked.
		/// This implementation removes the participants added here: if you
		/// override this method, you must invoke base. Note that if you invoke this
		/// method directly, the <see cref="SyncLock"/> will not be held.
		/// </summary>
		protected virtual void RemoveOneTimeOnlyParticipants()
		{
			foreach ((IComposerParticipant<TTarget> participant, bool oneTimeOnly) in Participants.ToArray()) {
				if (oneTimeOnly)
					Remove(participant);
			}
		}

		/// <summary>
		/// This method is provided to be invoked before each composition is about
		/// to begin. This will be invoked under the
		/// <see cref="SyncLock"/>, before
		/// <see cref="GetTarget"/> is invoked; and also is
		/// invoked before this <see cref="LastComposeErrors"/> is set null (it
		/// may still already be null if there is no prior error).
		/// This implementation is empty. Note that if you invoke this method directly,
		/// the <see cref="SyncLock"/> will not be held.
		/// </summary>
		protected virtual void BeforeCompose() { }

		/// <summary>
		/// This method is responsible for constructing or fetching the
		/// <see cref="TTarget"/> that is passed to all handlers on each
		/// composition. This is invoked in <see cref="HandleCompose"/>
		/// (under the <see cref="SyncLock"/>). Note that this is
		/// invoked for each composition: this object constructs a new instance,
		/// or returns a singleton instance here, as defined by the implementation.
		/// This implementation invokes any delegate provided on
		/// construction; and otherwise will throw <see cref="NotImplementedException"/>.
		/// Note that if you invoke this method directly, the <see cref="SyncLock"/>
		/// will not be held.
		/// </summary>
		/// <returns>Not null.</returns>
		/// <exception cref="NotImplementedException"></exception>
		protected virtual TTarget GetTarget()
		{
			if (getTarget != null)
				return getTarget();
			throw new NotImplementedException(
					$"{GetType().GetFriendlyName()} has no {nameof(Composer<TTarget>.GetTarget)} delegate.");
		}

		/// <summary>
		/// This virtual method is invoked after <see cref="GetTarget"/> each time;
		/// with the new <see cref="TTarget"/> result. This implementation is empty.
		/// The <see cref="SyncLock"/> will
		/// be held. Note that if you invoke this method directly, the
		/// <see cref="SyncLock"/> will not be held.
		/// </summary>
		/// <param name="newTarget">Is the result from <see cref="GetTarget"/>.</param>
		protected virtual void WithNewTarget(TTarget newTarget) { }

		/// <summary>
		/// This protected virtual method is invoked on each composition, from
		/// <see cref="HandleCompose"/> (and is under
		/// the <see cref="SyncLock"/>).
		/// This is invoked after all <see cref="IProvideParts{TTarget}"/>
		/// participants have been invoked, and after those callbacks
		/// have been invoked --- your target has been configured by
		/// all parts participants; but no
		/// <see cref="IBootstrap{TTarget}"/> participants
		/// have yet been invoked. This implementation is empty.
		/// Note that if you invoke this
		/// method directly, the <see cref="SyncLock"/>
		/// will not be held.
		/// </summary>
		/// <param name="composedTarget">Not null.</param>
		protected virtual void WithAllParts(TTarget composedTarget) { }

		/// <summary>
		/// This protected virtual method is invoked on each composition, from
		/// <see cref="HandleCompose"/> (and is under
		/// the <see cref="SyncLock"/>).
		/// This is invoked after all <see cref="IBootstrap{TTarget}"/>
		/// participants have been invoked, and just before
		/// <see cref="IHandleComposed{TTarget}"/> participants run.
		/// Note that if you invoke this
		/// method directly, the <see cref="SyncLock"/>
		/// will not be held.
		/// </summary>
		/// <param name="composedTarget"></param>
		protected virtual void BeforeHandleComposed(TTarget composedTarget) { }

		/// <summary>
		/// This protected virtual method is invoked when each composition
		/// is complete; and is under the <see cref="SyncLock"/>.
		/// This is invoked after all participants have run, just before
		/// <see cref="RaiseComposed"/> is invoked (and the
		/// <see cref="SyncLock"/> is released).
		/// Note that if you invoke this
		/// method directly, the <see cref="SyncLock"/>
		/// will not be held.
		/// </summary>
		/// <param name="composedTarget"></param>
		protected virtual void AfterComposed(TTarget composedTarget) { }

		/// <summary>
		/// This protected virtual method raises the <see cref="Composed"/> event.
		/// Note that this is invoked after the composition has taken place, and
		/// after event callbacks have been called back. This DOES NOT invoke
		/// callbacks nor <see cref="IHandleComposed{TTarget}"/> handlers.
		/// No locks are held here. This will trace and rethrow any exception.
		/// </summary>
		/// <param name="eventArgs">The event with the completed target to raise.</param>
		protected virtual void RaiseComposed(ComposerEventArgs<TTarget> eventArgs)
		{
			TraceSources.For(GetType())
					.Verbose(nameof(Composer<TTarget>.RaiseComposed));
			try {
				Composed?.Invoke(this, eventArgs);
			} catch (Exception exception) {
				TraceSources.For(GetType())
						.Error(
								exception,
								"Exception invoking "
								+ nameof(Composer<TTarget>.Composed)
								+ ": {0}.",
								exception.Message);
				throw;
			}
		}


		public virtual IComposer<TTarget> Participate(
				IComposerParticipant<TTarget> participant,
				bool oneTimeOnly = false)
		{
			if (participant == null)
				throw new ArgumentNullException(nameof(participant));
			lock (SyncLock) {
				if (isDisposed)
					throw new ObjectDisposedException(ToString());
				if (Participants.Any(tuple => object.Equals(participant, tuple.participant)))
					return this;
				if (participant is IRequestComposition<TTarget> requestComposition) {
					requestComposition.CompositionRequested -= HandleCompositionRequested;
					requestComposition.CompositionRequested += HandleCompositionRequested;
				}
				Participants.Add((participant, oneTimeOnly));
			}
			return this;
		}

		public virtual void Remove(IComposerParticipant<TTarget> participant)
		{
			lock (SyncLock) {
				if (participant is IRequestComposition<TTarget> requestComposition)
					requestComposition.CompositionRequested -= HandleCompositionRequested;
				Participants.RemoveAll(tuple => object.Equals(participant, tuple.participant));
			}
		}

		public TTarget Compose()
			=> HandleCompose();

		public AggregateComposerException LastComposeErrors
		{
			get {
				lock (SyncLock) {
					return lastComposeError;
				}
			}
			protected set {
				lock (SyncLock) {
					lastComposeError = value;
				}
			}
		}


		/// <summary>
		/// Specifies how added participants are handled when
		/// this composer is disposed. Defaults to true.
		/// </summary>
		public bool DisposeProvidersWithThis
		{
			get {
				lock (SyncLock) {
					return disposeProvidersWithThis;
				}
			}
			set {
				lock (SyncLock) {
					disposeProvidersWithThis = value;
				}
			}
		}

		/// <summary>
		/// Defaults to <see cref="ComposerExceptionPolicy.ThrowUnhandledComposerException"/>:
		/// an unhandled exception raised in <see cref="Compose"/> will be logged
		/// and rethrown. Composition will halt at that exception. All exceptions
		/// are always aggregated into the <see cref="LastComposeErrors"/>; and
		/// this policy may specify that the exception is raised; or not.
		/// </summary>
		public ComposerExceptionPolicy ComposeExceptionPolicy
		{
			get {
				lock (SyncLock) {
					return composeExceptionPolicy;
				}
			}
			set {
				lock (SyncLock) {
					composeExceptionPolicy = value;
				}
			}
		}

		/// <summary>
		/// Defaults to false: this applies to exceptions raised by participants.
		/// Exceptions are always aggregated into the
		/// <see cref="LastComposeErrors"/>; and if this is set true, the
		/// <see cref="Compose"/> method will halt on the first error.
		/// When false, other participants are all invoked. Note that
		/// the exception policy defined by <see cref="ComposeExceptionPolicy"/>
		/// determines whether the method throws any exceptions.
		/// </summary>
		public bool FailOnFirstParticipantException
		{
			get {
				lock (SyncLock) {
					return failOnFirstParticipantException;
				}
			}
			set {
				lock (SyncLock) {
					failOnFirstParticipantException = value;
				}
			}
		}


		/// <summary>
		/// Returns the count of added <see cref="IComposerParticipant{TTarget}"/>
		/// participants.
		/// </summary>
		public int ParticipantCount
		{
			get {
				lock (SyncLock) {
					return Participants.Count;
				}
			}
		}

		/// <summary>
		/// Returns the count of added <see cref="IProvideParts{TTarget}"/>
		/// participants.
		/// </summary>
		public int ProvidePartsCount
		{
			get {
				lock (SyncLock) {
					return Participants
							.Count(tuple => tuple.participant is IProvideParts<TTarget>);
				}
			}
		}

		/// <summary>
		/// Returns the count of added <see cref="IBootstrap{TTarget}"/>
		/// participants.
		/// </summary>
		public int BootstrapCount
		{
			get {
				lock (SyncLock) {
					return Participants
							.Count(tuple => tuple.participant is IBootstrap<TTarget>);
				}
			}
		}

		/// <summary>
		/// Returns the count of added <see cref="IHandleComposed{TTarget}"/>
		/// participants.
		/// </summary>
		public int HandleComposedCount
		{
			get {
				lock (SyncLock) {
					return Participants
							.Count(tuple => tuple.participant is IHandleComposed<TTarget>);
				}
			}
		}

		/// <summary>
		/// Returns the count of added <see cref="IRequestComposition{TTarget}"/>
		/// participants.
		/// </summary>
		public int RequestCompositionCount
		{
			get {
				lock (SyncLock) {
					return Participants
							.Count(tuple => tuple.participant is IRequestComposition<TTarget>);
				}
			}
		}


		/// <summary>
		/// This event is raised at the completion of every composition event sequence,
		/// with the results of this composition.
		/// </summary>
		public event GenericEventHandler<IComposer<TTarget>, ComposerEventArgs<TTarget>> Composed;

		public event EventHandler Disposed;

		public bool IsDisposed
		{
			get {
				lock (SyncLock) {
					return isDisposed;
				}
			}
		}

		/// <summary>
		/// Invoked from <see cref="IDisposable.Dispose"/>. This sets <see cref="IsDisposed"/>,
		/// removes and optionally disposes all participants, and raises <see cref="Disposed"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from the finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			lock (SyncLock) {
				if (!isDisposing
						|| isDisposed)
					return;
				isDisposed = true;
				Composed = null;
				foreach ((IComposerParticipant<TTarget> participant, bool _) in Participants) {
					if (participant is IRequestComposition<TTarget> requestComposition)
						requestComposition.CompositionRequested -= HandleCompositionRequested;
					if (disposeProvidersWithThis)
						(participant as IDisposable)?.Dispose();
				}
				Participants.Clear();
			}
			Disposed?.Invoke(this, EventArgs.Empty);
			Disposed = null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		public override string ToString()
		{
			lock (SyncLock) {
				return $"{base.ToString()}"
						+ ", ["
						+ $"{nameof(Composer<TTarget>.IsDisposed)}: {IsDisposed}"
						+ $", {nameof(Composer<TTarget>.Participants)}"
						+ $"{Participants.ToStringCollection()}"
						+ $", {nameof(IComposer<TTarget>.LastComposeErrors)}: {LastComposeErrors}"
						+ "]";
			}
		}
	}
}
