using System;
using System.Collections.Generic;
using System.Linq;
using Sc.Diagnostics;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Composer.Providers
{
	/// <summary>
	/// A simple <see cref="IComposerParticipant{TTarget}"/> implementation
	/// that takes a list of <see cref="IComposerParticipant{TTarget}"/>
	/// participants, and invokes all. Can be subclassed. This provider
	/// can be added and removed alone to the host to participate all
	/// delegates; and, "oneTimeOnly" will apply to all delegates here.
	/// This invokes all supported interfaces on the delegates.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target handled
	/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
	public class AggregateComposerParticipant<TTarget>
			: IProvideParts<TTarget>,
					IBootstrap<TTarget>,
					IHandleComposed<TTarget>,
					IRequestComposition<TTarget>,
					IDisposable
	{
		/// <summary>
		/// The actual list of participants.
		/// Notice: all access must lock this object.
		/// </summary>
		protected readonly List<IComposerParticipant<TTarget>> Participants
				= new List<IComposerParticipant<TTarget>>(8);

		private bool disposeParticipantsWithThis = true;
		private bool isDisposed;


		/// <summary>
		/// Default constructor.
		/// </summary>
		public AggregateComposerParticipant() { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="participants">Required.</param>
		/// <exception cref="ArgumentNullException"/>
		public AggregateComposerParticipant(IEnumerable<IComposerParticipant<TTarget>> participants)
		{
			if (participants == null)
				throw new ArgumentNullException(nameof(participants));
			AddRange(participants);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="participants">Required.</param>
		/// <exception cref="ArgumentNullException"/>
		public AggregateComposerParticipant(params IComposerParticipant<TTarget>[] participants)
			=> AddRange(participants);


		private void checkDisposed()
		{
			if (isDisposed)
				throw new ObjectDisposedException(ToString());
		}


		/// <summary>
		/// This handler is added to all <see cref="IRequestComposition{TTarget}"/>
		/// participants here, and will invoke <see cref="RaiseCompositionRequested"/>
		/// to raise this actual event here.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="participantEvent">Sender's event.</param>
		protected virtual void HandleParticipantCompositionRequested(
				object sender,
				RequestCompositionEventArgs<TTarget> participantEvent)
			=> RaiseCompositionRequested(participantEvent);

		/// <summary>
		/// Raises this <see cref="CompositionRequested"/> event with the given
		/// <paramref name="participantEvent"/>. Note that this method is invoked by
		/// <see cref="HandleParticipantCompositionRequested"/>, which will invoke this
		/// with the actual event raised by a participant added here; and such an
		/// event also has the optional callback from the participant.
		/// </summary>
		/// <param name="participantEvent">An event raised by the participant here;
		/// or, any new event to raise now.</param>
		protected virtual void RaiseCompositionRequested(RequestCompositionEventArgs<TTarget> participantEvent)
		{
			lock (Participants) {
				if (isDisposed)
					return;
			}
			CompositionRequested?.Invoke(this, participantEvent);
		}

		/// <summary>
		/// Will be invoked with each added participant, while holding the sync lock.
		/// </summary>
		/// <param name="addedParticipant">Not null.</param>
		protected virtual void HandleParticipantAdded(IComposerParticipant<TTarget> addedParticipant) { }

		/// <summary>
		/// Will be invoked with each removed participant, while holding the sync lock.
		/// </summary>
		/// <param name="removedParticipant">Not null.</param>
		protected virtual void HandleParticipantRemoved(IComposerParticipant<TTarget> removedParticipant) { }


		/// <summary>
		/// Defaults to <see langword="true"/>: participants will be disposed when
		/// this provider is disposed.
		/// </summary>
		public bool DisposeParticipantsWithThis
		{
			get {
				lock (Participants) {
					return disposeParticipantsWithThis;
				}
			}
			set {
				lock (Participants) {
					disposeParticipantsWithThis = value;
				}
			}
		}

		/// <summary>
		/// Adds the participant.
		/// </summary>
		/// <param name="participant">Not null.</param>
		/// <returns>False if the argument is already added.</returns>
		/// <exception cref="ArgumentNullException"/>
		public bool Add(IComposerParticipant<TTarget> participant)
		{
			if (participant == null)
				throw new ArgumentNullException(nameof(participant));
			lock (Participants) {
				checkDisposed();
				if (Participants.Contains(participant))
					return false;
				Participants.Add(participant);
				if (participant is IRequestComposition<TTarget> requestComposition) {
					requestComposition.CompositionRequested -= HandleParticipantCompositionRequested;
					requestComposition.CompositionRequested += HandleParticipantCompositionRequested;
				}
				HandleParticipantAdded(participant);
				return true;
			}
		}

		/// <summary>
		/// Adds all of the participants that are not already added.
		/// </summary>
		/// <param name="participants">Not null.</param>
		/// <returns>The count of participants added.</returns>
		/// <exception cref="ArgumentNullException"/>
		public int AddRange(IEnumerable<IComposerParticipant<TTarget>> participants)
		{
			if (participants == null)
				throw new ArgumentNullException(nameof(participants));
			return participants.Count(Add);
		}

		/// <summary>
		/// Removes the participant.
		/// </summary>
		/// <param name="participant">Not null.</param>
		/// <returns>False if the argument is not found.</returns>
		/// <exception cref="ArgumentNullException"/>
		public bool Remove(IComposerParticipant<TTarget> participant)
		{
			if (participant == null)
				throw new ArgumentNullException(nameof(participant));
			lock (Participants) {
				if (participant is IRequestComposition<TTarget> requestComposition)
					requestComposition.CompositionRequested -= HandleParticipantCompositionRequested;
				if (!Participants.Remove(participant))
					return false;
				HandleParticipantRemoved(participant);
				return true;
			}
		}


		/// <summary>
		/// This method is virtual and implements
		/// <see cref="IProvideParts{TTarget}.ProvideParts{T}"/>.
		/// This implementation invokes all participants added here.
		/// Notice that each is invoked in a catch block, and this will trace exceptions,
		/// AND re-throw a new <see cref="AggregateException"/> if there are
		/// any exceptions.
		/// catch this exception and set the <see cref="IComposer{TTarget}.LastComposeErrors"/>.
		/// </summary>
		/// <param name="eventArgs"><see cref="IProvideParts{TTarget}"/> argument.</param>
		/// <exception cref="AggregateException"></exception>
		public virtual void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
				where T : TTarget
		{
			lock (Participants) {
				checkDisposed();
				List<Exception> exceptions = new List<Exception>(Participants.Count);
				foreach (IProvideParts<TTarget> participant
						in Participants.OfType<IProvideParts<TTarget>>()) {
					try {
						participant.ProvideParts(eventArgs);
					} catch (Exception exception) {
						TraceSources.For(GetType())
								.Error(
										exception,
										"Participant exception invoking ProvideParts: {0}, '{1}'.",
										participant,
										exception.Message);
						exceptions.Add(exception);
					}
				}
				if (exceptions.Count != 0) {
					throw new AggregateException(
							$"One or more exceptions was raised by a {GetType().GetFriendlyName()} participant in"
							+ $" {nameof(IProvideParts<TTarget>.ProvideParts)}.",
							exceptions.EnumerateInReverse());
				}
			}
		}

		/// <summary>
		/// This method is virtual and implements
		/// <see cref="IBootstrap{TTarget}.HandleBootstrap{T}"/>.
		/// This implementation invokes all participants added here.
		/// Notice that each is invoked in a catch block, and this will trace exceptions,
		/// AND re-throw a new <see cref="AggregateException"/> if there are
		/// any exceptions.
		/// </summary>
		/// <param name="eventArgs"><see cref="IBootstrap{TTarget}"/> argument.</param>
		/// <exception cref="AggregateException"></exception>
		public virtual void HandleBootstrap<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget
		{
			lock (Participants) {
				checkDisposed();
				List<Exception> exceptions = new List<Exception>(Participants.Count);
				foreach (IBootstrap<TTarget> participant
						in Participants.OfType<IBootstrap<TTarget>>()) {
					try {
						participant.HandleBootstrap(eventArgs);
					} catch (Exception exception) {
						TraceSources.For(GetType())
								.Error(
										exception,
										"Participant exception invoking HandleBootstrap: {0}, '{1}'.",
										participant,
										exception.Message);
						exceptions.Add(exception);
					}
				}
				if (exceptions.Count != 0) {
					throw new AggregateException(
							$"One or more exceptions was raised by a {GetType().GetFriendlyName()} participant in"
							+ $" {nameof(IBootstrap<TTarget>.HandleBootstrap)}.",
							exceptions.EnumerateInReverse());
				}
			}
		}

		/// <summary>
		/// This method is virtual and implements
		/// <see cref="IHandleComposed{TTarget}.HandleComposed{T}"/>.
		/// This implementation invokes all participants added here.
		/// Notice that each is invoked in a catch block, and this will trace exceptions,
		/// AND re-throw a new <see cref="AggregateException"/> if there are
		/// any exceptions.
		/// </summary>
		/// <param name="eventArgs"><see cref="IHandleComposed{TTarget}"/> argument.</param>
		/// <exception cref="AggregateException"></exception>
		public void HandleComposed<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget
		{
			lock (Participants) {
				checkDisposed();
				List<Exception> exceptions = new List<Exception>(Participants.Count);
				foreach (IHandleComposed<TTarget> participant
						in Participants.OfType<IHandleComposed<TTarget>>()) {
					try {
						participant.HandleComposed(eventArgs);
					} catch (Exception exception) {
						TraceSources.For(GetType())
								.Error(
										exception,
										"Participant exception invoking HandleComposed: {0}, '{1}'.",
										participant,
										exception.Message);
						exceptions.Add(exception);
					}
				}
				if (exceptions.Count != 0) {
					throw new AggregateException(
							$"One or more exceptions was raised by a {GetType().GetFriendlyName()} participant in"
							+ $" {nameof(IHandleComposed<TTarget>.HandleComposed)}.",
							exceptions.EnumerateInReverse());
				}
			}
		}

		public event EventHandler<RequestCompositionEventArgs<TTarget>> CompositionRequested;


		/// <summary>
		/// Invoked from <see cref="IDisposable.Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!isDisposing)
				return;
			lock (Participants) {
				isDisposed = true;
				CompositionRequested = null;
				foreach (IComposerParticipant<TTarget> participant in Participants.ToArray()) {
					Remove(participant);
					if (disposeParticipantsWithThis)
						(participant as IDisposable)?.Dispose();
				}
				Participants.Clear();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}
	}
}
