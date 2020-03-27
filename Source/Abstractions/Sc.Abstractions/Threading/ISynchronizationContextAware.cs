using System.Threading;


namespace Sc.Abstractions.Threading
{
	/// <summary>
	/// Defines an object that receives a <see cref="SynchronizationContext"/>.
	/// </summary>
	public interface ISynchronizationContextAware
			: IHasSynchronizationContext
	{
		/// <summary>
		/// Sets this <see cref="SynchronizationContext"/>.
		/// </summary>
		/// <param name="synchronizationContext">CAN be null.</param>
		void SetSynchronizationContext(SynchronizationContext synchronizationContext);
	}
}
