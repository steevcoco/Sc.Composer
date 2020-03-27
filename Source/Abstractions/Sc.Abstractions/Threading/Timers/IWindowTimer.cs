using System;
using System.Threading;
using Sc.Abstractions.Lifecycle;


namespace Sc.Abstractions.Threading.Timers
{
	/// <summary>
	/// A Timer that runs an action at a timed interval, requiring
	/// "Refresh" events to signal the action to run. You provide an
	/// <see cref="Action"/> that runs based on the update policy set here.
	/// Invoke <see cref="IRefresh.Refresh"/> to post a request for the Action to
	/// run. With at least one Refresh request,
	/// a timer is set for the interval defined by <see cref="Window"/>,
	/// and the action is run at this interval. Therefore, the object ensures
	/// that the Action is invoked at least once after no more than
	/// <see cref="Window"/>, if at least one Refresh request has been posted.
	/// The timer stops after the Action runs, until another Refresh is posted;
	/// and any Refreshes that arrive while the action is running are handled
	/// according to <see cref="DiscardUpdatesWhileRunning"/>. The
	/// <see cref="IWindowTimer"/> can also be Paused.
	/// <see cref="IWindowTimer"/> ensures that your Action is not invoked
	/// concurrently by this class. The timer is a <see cref="Timer"/>;
	/// and so runs on ThreadPool Thread(s), and has Timer's resolution.
	/// This also supports <see cref="ISynchronizationContextAware"/>,
	/// and if a <see cref="SynchronizationContext"/> is set, the
	/// timer update action will be marshalled onto that context:
	/// you can set <see cref="SynchronizationContextSendPriority"/>
	/// to control the invoke at that time. Implements <see cref="IDisposable"/>
	/// and <see cref="IAsyncDisposable"/>.
	/// </summary>
	public interface IWindowTimer
			: IRefresh,
					ISynchronizationContextAware,
					IDisposable,
					IAsyncDisposable
	{
		/// <summary>
		/// The time span to wait after an Update request before the
		/// timer will invoke the Action. Note that since the window
		/// only applies when the timer has returned, this can be
		/// set to zero.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Cannot be negative.</exception>
		TimeSpan Window { get; set; }

		/// <summary>
		/// True while the action is running.
		/// </summary>
		bool IsActionRunning { get; }

		/// <summary>
		/// True if paused. Note that the Action may currently be
		/// running; and will pause when complete if so.
		/// </summary>
		bool IsPaused { get; }

		/// <summary>
		/// Returns an average <see cref="TimeSpan"/> that the action
		/// takes to run.
		/// </summary>
		/// <returns>Running average duration.</returns>
		TimeSpan GetAverageActionTime();


		/// <summary>
		/// Defines how the <see cref="IWindowTimer"/> behaves if a Refresh
		/// is posted while the Action is running. This defaults to false: when
		/// the Action completes, any posted updates are merged into 1, and the
		/// Timer is set to tick again at the <see cref="Window"/> from there.
		/// If set to true, then the timer is cleared at that time,
		/// and the Timer STOPS until another Refresh.
		/// </summary>
		bool DiscardUpdatesWhileRunning { get; set; }

		/// <summary>
		/// Defaults to false: if timer ticks overlap while the action is running,
		/// then the <see cref="Window"/> will be throttled up by the
		/// <see cref="WindowThrottleFactor"/> each time
		/// there is an overlap, until the action runs without an overlapped tick.
		/// If set false, then if a timer tick overlaps, the timer is always set
		/// to tick again at the <see cref="Window"/> from there.
		/// </summary>
		bool DefeatThrottling { get; set; }

		/// <summary>
		/// Applies if <see cref="DefeatThrottling"/> is false --- the default.
		/// </summary>
		double WindowThrottleFactor { get; set; }

		/// <summary>
		/// Applies if the <see cref="WindowThrottleFactor"/> is used.
		/// Specifies the maximum value that the throttle factor can reach.
		/// </summary>
		double MaxWindowThrottleFactor { get; set; }

		/// <summary>
		/// Returns true if the timer throttling is currently active.
		/// </summary>
		bool IsThrottled { get; }

		/// <summary>
		/// Applies if an instance is marshaling onto a <see cref="SynchronizationContext"/>:
		/// if true, the action is invoked as Send, and if false, Posted.
		/// Defaults to false (Post).
		/// </summary>
		bool SynchronizationContextSendPriority { get; set; }


		/// <summary>
		/// Triggers the timer to handle any pending Updates ASAP. If there are no
		/// pending updates (or if Paused) then nothing happens. Otherwise this
		/// sets the Timer to tick immediately.
		/// </summary>
		/// <param name="blockUntilActionReturns">Defaults to false. If set true, your
		/// Thread will wait until the action runs and returns --- if there is an
		/// update pending now, OR if the action is running now.</param>
		/// <param name="timeout">Provides a timeout for your thread
		/// if it waits for the action to run and complete.</param>
		void Flush(bool blockUntilActionReturns = false, TimeSpan? timeout = null);

		/// <summary>
		/// Stops the timer, clears the updates, and returns with the Action
		/// not having been run --- unless it is currently running.
		/// </summary>
		void Clear();

		/// <summary>
		/// Pauses the <see cref="IWindowTimer"/>. The timer is stopped. Any current
		/// updates can be discarded or honored when Resumed.
		/// Note that the Action may currently be
		/// running; and will pause when complete if so.
		/// </summary>
		void Pause();

		/// <summary>
		/// Resumes the <see cref="IWindowTimer"/> after <see cref="Pause"/>.
		/// If the argument is true, then all updates are discarded and the timer
		/// begins waiting for an update --- as if by <see cref="Clear"/>.
		/// If false --- the default --- then any updates posted while
		/// paused are merged into 1, and the Timer is set to tick again at the
		/// <see cref="Window"/> from here.
		/// </summary>
		/// <param name="clearUpdates">Applies only if there have
		/// been any updates while paused.</param>
		void Resume(bool clearUpdates = false);


		/// <summary>
		/// Returns true when Disposed.
		/// </summary>
		bool IsDisposed { get; }
	}
}
