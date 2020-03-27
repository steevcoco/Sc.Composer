using System;
using System.Threading;
using Sc.Abstractions.Lifecycle;


namespace Sc.Abstractions.Threading.Timers
{
	/// <summary>
	/// Provides a factory and manager for <see cref="IWindowTimer"/>
	/// instances. See <see cref="IWindowTimer"/> for more. This class is used
	/// to instantiate and manage the timers. Notice that this interface
	/// extends <see cref="IInitialize"/>: it is provided to support deferred
	/// initialization. Before Initialize is invoked, Timers will not run,
	/// but will be enqueued to run when initialized. This factory also
	/// implements <see cref="ISuspendable"/>: when suspended, the factory
	/// continues to create and manage timers, but the actions do not run
	/// until the factory is resumed.
	/// </summary>
	public interface IWindowTimerFactory
			: IInitialize,
					ISuspendable
	{
		/// <summary>
		/// Returns true after <see cref="IInitialize.Initialize"/>.
		/// Notice that this property remains true after this is Disposed.
		/// </summary>
		bool IsInitialized { get; }

		/// <summary>
		/// Constructor for a new <see cref="IWindowTimer"/>.
		/// </summary>
		/// <param name="window">The <see cref="IWindowTimer.Window"/>.</param>
		/// <param name="eventAction">The <see cref="IWindowTimer"/> Action.</param>
		/// <param name="synchronizationContext">This is optional: if this is not null,
		/// then your Action will run on this context, according to the
		/// <see cref="IWindowTimer.SynchronizationContextSendPriority"/>.</param>
		/// <param name="synchronizationContextSendPriority">Defaults to false: the
		/// <see cref="IWindowTimer.SynchronizationContextSendPriority"/>.</param>
		/// <param name="defeatThrottling">Defaults to false: sets
		/// <see cref="IWindowTimer.DefeatThrottling"/>.</param>
		/// <param name="windowThrottleFactor">Defaults to <c>1.75D</c>: sets
		/// <see cref="IWindowTimer.WindowThrottleFactor"/>.</param>
		/// <param name="maxWindowThrottleFactor">Defaults to <c>10D</c>: sets
		/// <see cref="IWindowTimer.MaxWindowThrottleFactor"/>.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		IWindowTimer Create(
				TimeSpan window,
				Action eventAction,
				SynchronizationContext synchronizationContext = null,
				bool synchronizationContextSendPriority = false,
				bool defeatThrottling = false,
				double windowThrottleFactor = 1.75D,
				double maxWindowThrottleFactor= 10D);
	}
}
