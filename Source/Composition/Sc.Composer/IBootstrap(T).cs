namespace Sc.Composer
{
	/// <summary>
	/// Provides an interface for a component to get a callback from an
	/// <see cref="IComposer{TTarget}"/> after all
	/// <see cref="IProvideParts{TTarget}"/> participants have completed,
	/// and before <see cref="IHandleComposed{TTarget}"/>
	/// participants run. The <see cref="IBootstrap{TTarget}"/> participant
	/// is invoked after <see cref="IProvideParts{TTarget}"/> participants have
	/// all run, and also after any callbacks added by those participants.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target handled
	/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
	public interface IBootstrap<in TTarget>
			: IComposerParticipant<TTarget>
	{
		/// <summary>
		/// Event handler for the <see cref="IComposer{TTarget}"/>
		/// Bootstrap event.
		/// </summary>
		/// <param name="eventArgs">The result of this composition.</param>
		void HandleBootstrap<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget;
	}
}
