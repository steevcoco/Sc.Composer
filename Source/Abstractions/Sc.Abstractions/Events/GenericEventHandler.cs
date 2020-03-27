namespace Sc.Abstractions.Events
{
	/// <summary>
	/// A generic event handler delegate.
	/// </summary>
	/// <typeparam name="TSender">The sender's type.</typeparam>
	/// <typeparam name="TEventArgs">The event args type.</typeparam>
	/// <param name="sender">Not null.</param>
	/// <param name="eventArgs">Not null.</param>
	public delegate void GenericEventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs eventArgs);
}
