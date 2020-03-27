using System;
using System.Threading;


namespace Sc.Abstractions.Threading.Timers
{
	/// <summary>
	/// A generic object holding a <see cref="State"/> that you provide,
	/// and running on a <see cref="Timer"/>.
	/// Instances are managed by <see cref="ICallbackTimerFactory"/>.
	/// This class is <see cref="IDisposable"/>: dispose the instance to
	/// terminate the <see cref="Timer"/>; and release references to any Parent.
	/// The Parent then releases any reference to this object.
	/// </summary>
	/// <typeparam name="TState"></typeparam>
	public interface ICallbackTimer<TState>
			: IDisposable
	{
		/// <summary>
		/// Will be true if this instance was created with an <c>"InvokeAndDispose"</c>
		/// method; and the Timer will call back only once --- and then this instance is Disposed.
		/// </summary>
		bool IsInvokeAndDispose { get; }

		/// <summary>
		/// The <see cref="Timer"/>. Not null. Notice that
		/// if this instance <see cref="IsInvokeAndDispose"/>, then the Parent will always Dispose this
		/// instance when the Timer has run for the first time: any changes made here will not change this
		/// behavior  --- this instance will always be disposed after the first callback.
		/// Otherwise the timer parameters can be changed here at any time.
		/// </summary>
		Timer Timer { get; }

		/// <summary>
		/// The state passed into the <see cref="ICallbackTimerFactory"/> <c>Create</c> method. May be null.
		/// This property is mutable.
		/// </summary>
		TState State { get; set; }

		/// <summary>
		/// MUST be invoked for any instance that is not started autoatically by the parent.
		/// A reference to this object is retained; and then <see cref="Timer"/>'s dueTime and period are
		/// changed to the arguments. This method will only succeed once: the <c>Timer</c> is started and
		/// added to the Parent. After this, use the <see cref="Timer"/> property to make any changes.
		/// If the Parent has not yet been initialized, this object will be enqueued, and
		/// will start with these parameters when the parent is Initialized.
		/// </summary>
		/// <param name="dueTime"><see cref="Timer"/> argument.</param>
		/// <param name="period"><see cref="Timer"/> argument.</param>
		/// <returns>THIS object.</returns>
		ICallbackTimer<TState> Start(TimeSpan dueTime, TimeSpan period);

		/// <summary>
		/// Returns the Utc time when this instance was first started;
		/// and null until then.
		/// </summary>
		DateTime? FirstStartUtc { get; }

		/// <summary>
		/// If this object has been Disposed.
		/// </summary>
		bool IsDisposed { get; }
	}
}
