using System;
using System.ComponentModel;


namespace Sc.Abstractions.Events
{
	/// <summary>
	/// Defines an object that raises the <see cref="StateChanged"/> event. Unlike
	/// <see cref="INotifyPropertyChanged"/>, which notifies single property changes, this event can
	/// be used to convey an atomic change signal for an object that may make several property changes:
	/// the listener should be notified only through the single event so that changes are seen
	/// atomically.
	/// </summary>
	public interface IRaiseStateChanged
	{
		/// <summary>
		/// Signals that this object's state has changed.
		/// </summary>
		event EventHandler StateChanged;
	}
}
