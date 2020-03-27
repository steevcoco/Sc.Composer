using System;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Sc.Util.Threading
{
	/// <summary>
	/// Implements a disposable plain <see cref="Monitor"/> lock handler --- which
	/// returns an <see cref="IDisposable"/> object to use in <see langword="using"/>
	/// blocks for locking.
	/// </summary>
	public sealed class DisposableMonitor
	{
		/// <summary>
		/// Returned when the lock is not acquired.
		/// </summary>
		private sealed class NoOpReleaser
				: IDisposable
		{
			/// <summary>
			/// Singleton instance.
			/// </summary>
			public static readonly IDisposable Instance = new NoOpReleaser();


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose() { }
		}


		/// <summary>
		/// Returned when the lock is acquired.
		/// </summary>
		private sealed class Releaser
				: IDisposable
		{
			private readonly object syncLock;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="syncLock">Required.</param>
			public Releaser(object syncLock)
				=> this.syncLock = syncLock ?? throw new ArgumentNullException(nameof(syncLock));


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
				=> Monitor.Exit(syncLock);
		}


		private readonly object syncLock;
		private readonly Releaser releaser;


		/// <summary>
		/// Constructor.
		/// </summary>
		public DisposableMonitor()
		{
			syncLock = new object();
			releaser = new Releaser(syncLock);
		}


		/// <summary>
		/// Enters the single <see cref="Monitor"/> lock; and returns an object
		/// that can ALWAYS be disposed to safely release that lock.
		/// </summary>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDisposable Enter()
		{
			Monitor.Enter(syncLock);
			return releaser;
		}

		/// <summary>
		/// Enters the single <see cref="Monitor"/> lock; and returns an object
		/// that can ALWAYS be disposed to safely release that lock.
		/// The returned object will not attempt to release
		/// the lock if this attempt to acquire does not succeed.
		/// You should simply always use the result in a <see langword="using"/>
		/// block since it is safe to do so; and in any case, you MUST ALWAYS
		/// test the out <paramref name="lockTaken"/> argument before acting
		/// on critical code.
		/// </summary>
		/// <param name="lockTaken">MUST ALWAYS be tested before acting on critical code.
		/// This will be false if the lock is not acquired now.</param>
		/// <param name="millisecondsTimeout">Optional timeout to wait for the lock.</param>
		/// <returns>Not null; and can safely always be disposed --- YET STILL
		/// you MUST ALWAYS test the out <paramref name="lockTaken"/> argument
		/// before acting on critical code.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDisposable TryEnter(out bool lockTaken, TimeSpan timeout)
		{
			lockTaken = false;
			Monitor.TryEnter(syncLock, timeout, ref lockTaken);
			return lockTaken
					? releaser
					: NoOpReleaser.Instance;
		}


		/// <summary>
		/// Returns the result of <see cref="Monitor.IsEntered"/> on this underlying lock.
		/// </summary>
		public bool IsEntered
			=> Monitor.IsEntered(syncLock);

		/// <summary>
		/// Invokes <see cref="Monitor.Pulse"/> on this underlying lock.
		/// </summary>
		public void Pulse()
			=> Monitor.Pulse(syncLock);

		/// <summary>
		/// Invokes <see cref="Monitor.PulseAll"/> on this underlying lock.
		/// </summary>
		public void PulseAll()
			=> Monitor.PulseAll(syncLock);

		/// <summary>
		/// Invokes <see cref="Monitor.Wait(object,int,bool)"/> on this underlying lock.
		/// </summary>
		/// <param name="timeout">A <see cref="T:System.TimeSpan" /> representing the amount of
		/// time to wait before the thread enters the ready queue.</param>
		/// <param name="exitContext"><see langword="true" /> to exit and reacquire the
		/// synchronization domain for the context (if in a synchronized context)
		/// before the wait; otherwise, <see langword="false" />.</param>
		/// <returns> <see langword="true" /> if the lock was reacquired before the specified
		/// time elapsed; <see langword="false" /> if the lock was reacquired after the
		/// specified time elapsed. The method does not return until the lock is reacquired.</returns>
		/// <exception cref="SynchronizationLockException"></exception>
		/// <exception cref="ThreadInterruptedException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public bool Wait(TimeSpan timeout, bool exitContext)
			=> Monitor.Wait(syncLock, timeout, exitContext);

		/// <summary>
		/// Invokes <see cref="Monitor.Wait(object,TimeSpan)"/> on this underlying lock.
		/// </summary>
		/// <param name="timeout">A <see cref="T:System.TimeSpan" /> representing the amount of
		/// time to wait before the thread enters the ready queue.</param>
		/// <returns><see langword="true" /> if the lock was reacquired before the specified
		/// time elapsed; <see langword="false" /> if the lock was reacquired after the
		/// specified time elapsed. The method does not return until the lock is reacquired.</returns>
		/// <exception cref="SynchronizationLockException"></exception>
		/// <exception cref="ThreadInterruptedException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public bool Wait(TimeSpan timeout)
			=> Monitor.Wait(syncLock, timeout);

		/// <summary>
		/// Invokes <see cref="Monitor.Wait(object)"/> on this underlying lock.
		/// </summary>
		/// <returns><see langword="true" /> if the call returned because the caller reacquired
		/// the lock for the specified object. This method does not return if the lock is
		/// not reacquired.</returns>
		/// <exception cref="SynchronizationLockException"></exception>
		/// <exception cref="ThreadInterruptedException"></exception>
		public bool Wait()
			=> Monitor.Wait(syncLock);
	}
}
