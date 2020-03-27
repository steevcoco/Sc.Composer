namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines a task that provides a <see cref="Cancel"/> method.
	/// </summary>
	public interface ICancel
	{
		/// <summary>
		/// If this task is currently executing, this will request a cancellation.
		/// </summary>
		void Cancel();
	}
}
