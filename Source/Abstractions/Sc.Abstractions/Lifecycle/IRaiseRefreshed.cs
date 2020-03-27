using System;


namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines an object that raises the <see cref="Refreshed"/> event when its state is refreshed.
	/// </summary>
	public interface IRaiseRefreshed
	{
		/// <summary>
		/// This event will be raised after affected state is refreshed.
		/// </summary>
		event EventHandler Refreshed;
	}
}
