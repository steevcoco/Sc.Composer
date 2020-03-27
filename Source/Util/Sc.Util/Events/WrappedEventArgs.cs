using System;


namespace Sc.Util.Events
{
	/// <summary>
	/// An <see cref="EventArgs"/> class that wraps another <see cref="Event"/>.
	/// </summary>
	/// <typeparam name="TEvent">The wrapped event type.</typeparam>
	/// <typeparam name="TSender">The wrapped event sender type.</typeparam>
	public class WrappedEventArgs<TEvent, TSender>
			: EventArgs
			where TEvent : EventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="eventArgs">Not null.</param>
		/// <param name="sender">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public WrappedEventArgs(TEvent eventArgs, TSender sender)
		{
			if (sender == null)
				throw new ArgumentNullException(nameof(sender));
			Event = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
			Sender = sender;
		}


		/// <summary>
		/// The wrapped event.
		/// </summary>
		public TEvent Event { get; }

		/// <summary>
		/// The sender of the wrapped <see cref="Event"/>.
		/// </summary>
		public TSender Sender { get; }
	}
}
