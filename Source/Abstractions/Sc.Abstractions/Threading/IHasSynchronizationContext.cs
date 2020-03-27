using System.Threading;


namespace Sc.Abstractions.Threading
{
	/// <summary>
	/// Defines an object with a <see cref="SynchronizationContext"/>.
	/// </summary>
	public interface IHasSynchronizationContext
    {
		/// <summary>
		/// The current SynchronizationContext.
		/// </summary>
		SynchronizationContext SynchronizationContext { get; }
	}
}
