namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines a service that can be notified that activities should be suspended.
	/// </summary>
	public interface ISuspendable
	{
		/// <summary>
		/// Toggles true when <see cref="Suspend"/> is invoked.
		/// </summary>
		bool IsSuspended { get; }

		/// <summary>
		/// This method notifies the service that it must be
		/// suspended now. <see cref="IsSuspended"/> will become true.
		/// </summary>
		void Suspend();

		/// <summary>
		/// This method notifies the service that it may
		/// resume now. <see cref="IsSuspended"/> will become false.
		/// </summary>
		void Resume();
	}
}
