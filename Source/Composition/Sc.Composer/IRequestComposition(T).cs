using System;


namespace Sc.Composer
{
	/// <summary>
	/// An object that requests composition by defining the
	/// <see cref="CompositionRequested"/> event.
	/// </summary>
	public interface IRequestComposition<TTarget>
			: IComposerParticipant<TTarget>
	{
		/// <summary>
		/// This event is raised when composition is requested by this object.
		/// </summary>
		event EventHandler<RequestCompositionEventArgs<TTarget>> CompositionRequested;
	}
}
