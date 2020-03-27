using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Sc.Util.System;


namespace Sc.Util.Threading
{
	/// <summary>
	/// A thread-free reader/writer lock. This lock is NOT recursive.
	/// </summary>
	public sealed class AsyncReaderWriterLock
	{
		private static int cancellationPollWaitTime
			=> 15;


		private readonly object handle = new object();
		private int readerCount;
		private bool hasWriter;


		private bool enter(int millisecondsTimeout, CancellationToken cancellationToken, bool isWriteLock)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			bool IsTimedOut()
			{
				if (cancellationToken.IsCancellationRequested)
					return true;
				if (millisecondsTimeout < 0)
					return false;
				stopwatch.Stop();
				if (stopwatch.ElapsedMilliseconds >= millisecondsTimeout)
					return true;
				millisecondsTimeout -= (int)stopwatch.ElapsedMilliseconds;
				stopwatch.Restart();
				return false;
			}
			while (true) {
				bool gotLock = false;
				try {
					if (cancellationToken.CanBeCanceled) {
						Monitor.TryEnter(
								handle,
								millisecondsTimeout < 0
										? AsyncReaderWriterLock.cancellationPollWaitTime
										: Math.Min(
												AsyncReaderWriterLock.cancellationPollWaitTime,
												millisecondsTimeout),
								ref gotLock);
					} else
						Monitor.TryEnter(handle, millisecondsTimeout, ref gotLock);
					if (!gotLock
							|| hasWriter
							|| (isWriteLock
							&& (readerCount > 0))) {
						if (IsTimedOut())
							return false;
						continue;
					}
					if (isWriteLock)
						hasWriter = true;
					else
						++readerCount;
					return true;
				} finally {
					if (gotLock)
						Monitor.Exit(handle);
				}
			}
		}

		private void exit(bool isWriteLock)
		{
			lock (handle) {
				if (isWriteLock)
					hasWriter = false;
				else
					--readerCount;
				Monitor.PulseAll(handle);
			}
		}

		private Task<T> getExitContinuation<T>(Task<T> task, bool isWriteLock)
		{
			return task.ContinueWith(
					Continue,
					isWriteLock,
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
			T Continue(Task<T> argumentTask, object exitWriteLock)
			{
				exit((bool)exitWriteLock);
				return argumentTask.Result;
			}
		}


		/// <summary>
		/// Tries to enter the reader lock, and creates a disposable object that exits the lock
		/// when disposed.
		/// </summary>
		/// <param name="gotLock">Will be false if your attempt to acquire the lock fails: if false
		/// then you do not have the lock.</param>
		/// <param name="millisecondsTimeout">Optional timeout to wait for the lock. Notice that the
		/// default is not infinite: you may pass <see cref="Timeout.Infinite"/>.</param>
		/// <param name="cancellationToken">Optional token that will cancel the wait for the lock.</param>
		/// <returns>Not null.</returns>
		public IDisposable WithReaderLock(
				out bool gotLock,
				int millisecondsTimeout = 1000 * 60 * 3,
				CancellationToken cancellationToken = default)
		{
			DelegateDisposable<bool> result
					= DelegateDisposable.With(
							enter(millisecondsTimeout, cancellationToken, false), 
							Dispose);
			gotLock = result.State;
			return result;
			void Dispose(bool isGotLock)
			{
				if (isGotLock)
					exit(false);
			}
		}

		/// <summary>
		/// Tries to enter the reader lock, and creates a Task continuation that exits the lock when
		/// your Task is complete.
		/// </summary>
		/// <param name="task">Not null. This will only be invoked if the lock is acquired; and if
		/// so, then the returned task is a continuation of this one that invokes and returns
		/// this Task's result after exiting the lock.</param>
		/// <param name="millisecondsTimeout">Optional timeout to wait for the lock. Notice that the
		/// default is not infinite: you may pass <see cref="Timeout.Infinite"/>.</param>
		/// <param name="cancellationToken">Optional token that will cancel the wait for the lock.
		/// Notice that this token is not used on the Tasks.</param>
		/// <returns>You must test the result. The bool will be false if your attempt to acquire the lock
		/// fails: if false then you do not have the lock; and the returned task will not be null, and is
		/// canceled. If true then the returned task must be awaited for the lock exit and the completion
		/// of your task.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public (bool gotLock, Task<T> task) WithReaderLock<T>(
				Func<Task<T>> task,
				int millisecondsTimeout = 1000 * 60 * 3,
				CancellationToken cancellationToken = default)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			return enter(millisecondsTimeout, cancellationToken, false)
					? (true, getExitContinuation(task(), false))
					: (false, new Task<T>(WouldReturnDefault, new CancellationToken(true)));
			static T WouldReturnDefault()
				=> default;
		}


		/// <summary>
		/// Tries to enter the write lock, and creates a disposable object that exits the lock
		/// when disposed.
		/// </summary>
		/// <param name="gotLock">Will be false if your attempt to acquire the lock fails: if false
		/// then you do not have the lock.</param>
		/// <param name="millisecondsTimeout">Optional timeout to wait for the lock. Notice that the
		/// default is not infinite: you may pass <see cref="Timeout.Infinite"/>.</param>
		/// <param name="cancellationToken">Optional token that will cancel the wait for the lock.</param>
		/// <returns>Not null.</returns>
		public IDisposable WithWriteLock(
				out bool gotLock,
				int millisecondsTimeout = 1000 * 60 * 3,
				CancellationToken cancellationToken = default)
		{
			DelegateDisposable<bool> result
					= DelegateDisposable.With(
							enter(millisecondsTimeout, cancellationToken, true),
							Dispose);
			gotLock = result.State;
			return result;
			void Dispose(bool isGotLock)
			{
				if (isGotLock)
					exit(true);
			}
		}

		/// <summary>
		/// Tries to enter the write lock, and creates a Task continuation that exits the lock when
		/// your Task is complete.
		/// </summary>
		/// <param name="task">Not null. This will only be invoked if the lock is acquired; and if
		/// so, then the returned task is a continuation of this one that invokes and returns
		/// this Task's result after exiting the lock.</param>
		/// <param name="millisecondsTimeout">Optional timeout to wait for the lock. Notice that the
		/// default is not infinite: you may pass <see cref="Timeout.Infinite"/>.</param>
		/// <param name="cancellationToken">Optional token that will cancel the wait for the lock.
		/// Notice that this token is not used on the Tasks.</param>
		/// <returns>You must test the result. The bool will be false if your attempt to acquire the lock
		/// fails: if false then you do not have the lock; and the returned task will not be null, and is
		/// canceled. If true then the returned task must be awaited for the lock exit and the completion
		/// of your task.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public (bool gotLock, Task<T> task) WithWriteLock<T>(
				Func<Task<T>> task,
				int millisecondsTimeout = 1000 * 60 * 3,
				CancellationToken cancellationToken = default)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			return enter(millisecondsTimeout, cancellationToken, true)
					? (true, getExitContinuation(task(), true))
					: (false, new Task<T>(WouldReturnDefault, new CancellationToken(true)));
			static T WouldReturnDefault()
				=> default;
		}
	}
}
