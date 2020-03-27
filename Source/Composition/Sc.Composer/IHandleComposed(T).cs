namespace Sc.Composer
{
	/// <summary>
	/// Provides an interface for a component to handle the fully composed
	/// <see cref="IComposer{TTarget}"/> target.
	/// The <see cref="IHandleComposed{TTarget}"/> participant is invoked
	/// after all <see cref="IBootstrap{TTarget}"/> participants have run.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target handled
	/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
	public interface IHandleComposed<in TTarget>
			: IComposerParticipant<TTarget>
	{
		/// <summary>
		/// Handle the <see cref="IComposer{TTarget}"/> composed target.
		/// </summary>
		/// <param name="eventArgs">The event argument.</param>
		void HandleComposed<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget;
	}
}
