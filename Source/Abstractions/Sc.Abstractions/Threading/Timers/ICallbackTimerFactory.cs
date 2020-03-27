using System;
using System.Threading;
using Sc.Abstractions.Lifecycle;


namespace Sc.Abstractions.Threading.Timers
{
	/// <summary>
	/// Implements a factory and manager for <see cref="ICallbackTimer{TState}"/>
	/// instances. This factory is thread safe. The <see cref="ICallbackTimer{TState}"/>
	/// can be used like InvokeAsync callbacks; but your state is typed, and the
	/// object allows you make ongoing changes to the timer and/or state.
	/// Timers can tick and run at an interval; until you dispose the Timer. Usage:
	/// <code>
	/// ICallbackTimerFactory myTimers = new CallbackTimerFactory();
	/// ICallbackTimer&lt;string> myTimer
	///         = myTimers.Create(
	///                 "Hello, from Timer",
	///                 callbackTimer =>
	///                 {
	///                     System.Console.WriteLine(callbackTimer.State); // "Hello, from Timer"
	///                     callbackTimer.Dispose(); // Invoke the ICallbackTimer method to ensure references are released
	///                 })
	/// myTimer.Start(TimeSpan.FromSeconds(1D), Timeout.InfiniteTimeSpan);
	/// </code>
	/// You can keep a reference to the timer, and access your state object, and the Timer.
	/// The Create methods return the <see cref="ICallbackTimer{TState}"/>
	/// (the object holding your typed state object, and the
	/// reference to the Timer); and the InvokeAndDispose methods automatically dispose
	/// the instance after the Timer runs only once. You should always Dispose the
	/// <see cref="ICallbackTimer{TState}"/> object itself when
	/// you have a reference, to ensure all state is cleared.
	/// All instances are disposed when the Manager is Disposed.
	/// Timers can also call back on a given <see cref="SynchronizationContext"/>.
	/// Notice that this interface
	/// extends <see cref="IInitialize"/>: it is provided to support deferred
	/// initialization. Before Initialize is invoked, Timers will not run,
	/// but will be enqueued to run when initialized. This factory also
	/// implements <see cref="ISuspendable"/>: when suspended, the factory
	/// continues to create and manage timers, but the actions do not run
	/// until the factory is resumed.
	/// </summary>
	public interface ICallbackTimerFactory
			: IInitialize,
					ISuspendable
	{
		/// <summary>
		/// Creates a new <see cref="ICallbackTimer{TState}"/> object containing your state and a new Timer
		/// that will use the callback and state. NOTE: the Timer will initially NOT be started. The returned
		/// object will be passed into the callback as its argument. Start the Timer by invoking
		/// <see cref="ICallbackTimer{TState}.Start"/>. Dispose the Timer, and release references
		/// to it by disposing the returned callback. No references are held here on
		/// the callback until Start is invoked.
		/// </summary>
		/// <typeparam name="TState">Your state type.</typeparam>
		/// <param name="state">Optional</param>
		/// <param name="callback">Required.</param>
		/// <param name="synchronizationContext">This is optional: if this is not null, then your Action will
		/// run on this context, according to <paramref name="dispatchAsync"/>.</param>
		/// <param name="dispatchAsync">Used only if the <see cref="SynchronizationContext"/> is not null.
		/// If this is true, then your Action is Posted to the context; and otherwise Sent.</param>
		/// <returns>Not null; and NOT YET started.</returns>
		ICallbackTimer<TState> Create<TState>(
				TState state,
				Action<ICallbackTimer<TState>> callback,
				SynchronizationContext synchronizationContext = null,
				bool dispatchAsync = true);

		/// <summary>
		/// As with the Create method, but this creates an <see cref="ICallbackTimer{TState}"/> that will
		/// invoke the callback once, and then Dispose itself immediately after. This method immediately
		/// starts the timer with the dueTime, and a Timeout.InfiniteTimeSpan period. The factory holds a
		/// reference until it is disposed.
		/// </summary>
		/// <typeparam name="TState">Your state type.</typeparam>
		/// <param name="dueTime">Required timer due time.</param>
		/// <param name="state">Optional</param>
		/// <param name="callback">Required.</param>
		/// <param name="synchronizationContext">This is optional: if this is not null, the your Action will
		/// run on this context, according to <paramref name="dispatchAsync"/>.</param>
		/// <param name="dispatchAsync">Used only if the <see cref="SynchronizationContext"/> is not null.
		/// If this is true, then your Action is Posted to the context; and otherwise Sent.</param>
		/// <returns>Not null; and HAS BEEN started.</returns>
		void InvokeAndDispose<TState>(
				TimeSpan dueTime,
				TState state,
				Action<ICallbackTimer<TState>> callback,
				SynchronizationContext synchronizationContext = null,
				bool dispatchAsync = true);
	}
}
