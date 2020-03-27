using System;
using Sc.Util.System;


namespace Sc.Composer.Providers
{
	/// <summary>
	/// Implements <see cref="IProvideParts{TTarget}"/>, <see cref="IBootstrap{TTarget}"/>,
	/// <see cref="IHandleComposed{TTarget}"/>, and
	/// <see cref="IRequestComposition{TTarget}"/>, for a covariant
	/// type, using an actual participant defining a contravariant type.
	/// Note that this class implements <see cref="IDisposable"/>, and
	/// when disposed, if the participant is disposable, it is disposed.
	/// Implements <see cref="IEquatable{T}"/> in terms of the <see cref="Participant"/>.
	/// </summary>
	/// <typeparam name="TSuper">The delegate's actual (contravariant) type.</typeparam>
	/// <typeparam name="TTarget">This instance's implemented (covariant) type.</typeparam>
	public class VariantParticipant<TSuper, TTarget>
			: IProvideParts<TTarget>,
					IBootstrap<TTarget>,
					IHandleComposed<TTarget>,
					IRequestComposition<TTarget>,
					IEquatable<VariantParticipant<TSuper, TTarget>>,
					IEquatable<IComposerParticipant<TSuper>>,
					IDisposable
			where TTarget : TSuper
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="participant">Required.</param>
		public VariantParticipant(IComposerParticipant<TSuper> participant)
		{
			Participant = participant ?? throw new ArgumentNullException(nameof(participant));
			if (Participant is IRequestComposition<TTarget> requestComposition)
				requestComposition.CompositionRequested += handleCompositionRequested;
		}


		private void handleCompositionRequested(object sender, RequestCompositionEventArgs<TTarget> eventArgs)
			=> CompositionRequested?.Invoke(this, eventArgs);


		/// <summary>
		/// The actual delegate participant.
		/// </summary>
		public IComposerParticipant<TSuper> Participant { get; }


		public void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
				where T : TTarget
			=> (Participant as IProvideParts<TSuper>)?.ProvideParts(eventArgs);

		public void HandleBootstrap<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget
			=> (Participant as IBootstrap<TSuper>)?.HandleBootstrap(eventArgs);

		public void HandleComposed<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget
			=> (Participant as IHandleComposed<TSuper>)?.HandleComposed(eventArgs);

		public event EventHandler<RequestCompositionEventArgs<TTarget>> CompositionRequested;


		public void Dispose()
		{
			if (Participant is IRequestComposition<TTarget> requestComposition)
				requestComposition.CompositionRequested -= handleCompositionRequested;
			(Participant as IDisposable)?.Dispose();
		}


		public override int GetHashCode()
			=> Participant.GetHashCode();

		public override bool Equals(object obj)
			=> Equals(obj as VariantParticipant<TSuper, TTarget>)
					|| Equals(obj as IComposerParticipant<TSuper>)
					|| Equals(obj as IComposerParticipant<TTarget>);

		public bool Equals(IComposerParticipant<TTarget> other)
			=> Equals(other as VariantParticipant<TSuper, TTarget>);

		public bool Equals(IComposerParticipant<TSuper> other)
			=> (other != null)
					&& object.ReferenceEquals(Participant, other);

		public bool Equals(VariantParticipant<TSuper, TTarget> other)
			=> (other != null)
					&& object.ReferenceEquals(Participant, other.Participant);

		public override string ToString()
			=> $"{GetType().GetFriendlyName()}[{Participant}]";
	}
}
