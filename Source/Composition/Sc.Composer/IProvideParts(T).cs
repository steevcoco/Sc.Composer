namespace Sc.Composer
{
	/// <summary>
	/// Defines an object that provides Parts for composition to an
	/// <see cref="IComposer{TTarget}"/>.
	/// </summary>
	/// <typeparam name="TTarget">The (contravariant) type of the target handled
	/// by this object.</typeparam>
	public interface IProvideParts<in TTarget>
			: IComposerParticipant<TTarget>
	{
		/// <summary>
		/// Invoked by the <see cref="IComposer{TTarget}"/>
		/// when a composition event sequence has begun.
		/// This method provides the implementation for this provider to
		/// configure the given target now. You may also add an action
		/// here that will be called back when all <see cref="IProvideParts{TTarget}"/>
		/// participants have completed --- and before
		/// <see cref="IBootstrap{TTarget}"/> participants are invoked.
		/// </summary>
		/// <typeparam name="T">Is the covariant actual type of the invoking
		/// <see cref="IComposer{TTarget}"/>.</typeparam>
		/// <param name="eventArgs">This composition event target.</param>
		void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
				where T : TTarget;
	}
}
