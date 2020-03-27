using System;


namespace Sc.Util.Events
{
	/// <summary>
	/// Provides an <see cref="EventArgs"/> class that allows the sender
	///  to request one or more handlers to <see cref="Confirm"/>
	///  a simple action.
	/// </summary>
	public class ConfirmedEventArgs
			: EventArgs
	{
		/// <summary>
		/// The event result. The default value is <see langword="null"/>.
		/// If not handler confirms the event then the value remains
		/// null. If <see langword="abstract"/>handler invokes <see cref="Confirm"/>
		/// with <see langword="false"/>, then the value becomes false.
		/// If any handler passes <see langword="true"/> then the value will
		/// be true and will not be set false from any other handler.
		/// </summary>
		public bool? IsConfirmed { get; set; }

		/// <summary>
		/// Must be invoked by an event handler to set <see cref="IsConfirmed"/>.
		/// </summary>
		/// <param name="value">The value to set: see <see cref="IsConfirmed"/>.</param>
		public void Confirm(bool value)
		{
			if (value)
				IsConfirmed = true;
			else if (IsConfirmed != true)
				IsConfirmed = false;
		}
	}
}
