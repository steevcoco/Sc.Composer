using System.Threading;


namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines a task that provides a <see cref="CancellationToken"/> that is used
	/// by this task to monitor its cancellation.
	/// </summary>
	public interface ICancellable
	{
		/// <summary>
		/// Holds the actual <see cref="CancellationToken"/> that is monitored by this task.
		/// You may monitor or propagate this object.
		/// </summary>
		CancellationToken CancellationToken { get; }
	}
}
