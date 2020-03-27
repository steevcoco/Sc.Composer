using System;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Sc.Util.Threading
{
	/// <summary>
	/// Implements a disposable <see cref="ReaderWriterLockSlim"/> handler --- which returns
	/// <see cref="IDisposable"/> objects to use in <see langword="using"/>
	/// blocks for locking. NOTICE that THIS object IMPLEMENTS
	/// <see cref="IDisposable"/>: disposing THIS object DISPOSES THE
	/// <see cref="ReaderWriterLock"/> instance: you MUST NEVER expose this object
	/// to any invokers acquiring a lock.
	/// </summary>
	public sealed class DisposableReaderWriterLockSlim
			: IDisposable
	{
		/// <summary>
		/// Defines the object returned by <see cref="TryEnterUpgradeableReadLock"/>,
		/// which supports downgrading to read mode.
		/// </summary>
		public interface IDowngrade
				: IDisposable
		{
			/// <summary>
			/// This method can be used to perform a downgrade for a thread that has
			/// entered the lock with <see cref="TryEnterUpgradeableReadLock"/>. This
			/// tries to acquire the read lock now; and if successful, this
			/// exits the upgradeable lock now, and this disposable will now
			/// exit the read lock instead. The disposable MUST be the object
			/// returned by <see cref="TryEnterUpgradeableReadLock"/>.
			/// See that method for an example.
			/// </summary>
			/// <param name="lockTaken">MUST ALWAYS be tested before acting on critical code.
			/// This will be false if the read lock is not acquired now.</param>
			/// <param name="millisecondsTimeout">Optional timeout to wait for the lock.</param>
			/// <exception cref="LockRecursionException"></exception>
			/// <exception cref="ArgumentOutOfRangeException"></exception>
			/// <exception cref="ObjectDisposedException"></exception>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void TryDowngrade(out bool lockTaken, int millisecondsTimeout = Timeout.Infinite);
		}


		/// <summary>
		/// Returned when the lock is not acquired.
		/// </summary>
		private sealed class NoOpReleaser
				: IDowngrade
		{
			/// <summary>
			/// Singleton instance.
			/// </summary>
			public static readonly IDisposable Instance = new NoOpReleaser();


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void TryDowngrade(out bool lockTaken, int millisecondsTimeout = Timeout.Infinite)
				=> lockTaken = false;


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose() { }
		}


		/// <summary>
		/// Returned when the read or write lock is acquired.
		/// </summary>
		private sealed class ReadWriteReleaser
				: IDisposable
		{
			private readonly bool isWriter;
			private readonly ReaderWriterLockSlim readerWriterLock;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="isWriter">TRUE if this releases the WRITE lock.
			/// False to release the read lock.</param>
			/// <param name="readerWriterLock">Required.</param>
			public ReadWriteReleaser(bool isWriter, ReaderWriterLockSlim readerWriterLock)
			{
				this.isWriter = isWriter;
				this.readerWriterLock
						= readerWriterLock
						?? throw new ArgumentNullException(nameof(readerWriterLock));
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				if (isWriter)
					readerWriterLock.ExitWriteLock();
				else
					readerWriterLock.ExitReadLock();
			}
		}


		/// <summary>
		/// Returned when the upgradeable read lock is acquired.
		/// Implements downgrading.
		/// </summary>
		private sealed class UpgradeableReleaser
				: IDowngrade
		{
			private readonly ReaderWriterLockSlim readerWriterLock;
			private bool isDowngraded;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="readerWriterLock">Required.</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public UpgradeableReleaser(ReaderWriterLockSlim readerWriterLock)
				=> this.readerWriterLock
						= readerWriterLock
						?? throw new ArgumentNullException(nameof(readerWriterLock));


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void TryDowngrade(out bool lockTaken, int millisecondsTimeout = Timeout.Infinite)
			{
				lockTaken = readerWriterLock.TryEnterReadLock(millisecondsTimeout);
				if (!lockTaken)
					return;
				readerWriterLock.ExitUpgradeableReadLock();
				isDowngraded = true;
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				if (isDowngraded)
					readerWriterLock.ExitReadLock();
				else
					readerWriterLock.ExitUpgradeableReadLock();
			}
		}


		private readonly IDisposable releaseWriteLock;
		private readonly IDisposable releaseReadLock;


		/// <summary>
		/// Constructor.
		/// </summary>
		public DisposableReaderWriterLockSlim(LockRecursionPolicy lockRecursionPolicy = LockRecursionPolicy.NoRecursion)
		{
			ReaderWriterLock = new ReaderWriterLockSlim(lockRecursionPolicy);
			releaseWriteLock = new ReadWriteReleaser(true, ReaderWriterLock);
			releaseReadLock = new ReadWriteReleaser(false, ReaderWriterLock);
		}


		/// <summary>
		/// This is the actual lock object: this is exposed to allow using other
		/// <see cref="ReaderWriterLockSlim"/> properties; but you
		/// MUST carefully consider how the lock has been acquired and will be
		/// released before using direct methods here --- you SHOULD NOT
		/// acquire or release locks directly with this Lock.
		/// </summary>
		public ReaderWriterLockSlim ReaderWriterLock
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
		}


		/// <summary>
		/// Enters the write lock; and returns an object that can ALWAYS be disposed
		/// to safely release that lock. The returned object will not attempt to release
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
		/// <exception cref="LockRecursionException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDisposable TryEnterWriteLock(out bool lockTaken, int millisecondsTimeout = Timeout.Infinite)
		{
			lockTaken = ReaderWriterLock.TryEnterWriteLock(millisecondsTimeout);
			return lockTaken
					? releaseWriteLock
					: NoOpReleaser.Instance;
		}

		/// <summary>
		/// Enters the upgradeable read lock; and returns an object that can ALWAYS be disposed
		/// to safely release that lock. The returned object will not attempt to release
		/// the lock if this attempt to acquire does not succeed. In addition,
		/// if the lock IS acquired here, this Thread can downgrade to read mode:
		/// this method returns an instance of <see cref="IDowngrade"/>, and this thread
		/// can invoke <see cref="IDowngrade.TryDowngrade"/> --- AND STILL, the single disposable
		/// returned here should be disposed: it will also be "downgraded" to
		/// then release the Reader lock when disposed.
		/// You should simply always use the result in a <see langword="using"/>
		/// block since it is safe to do so; and in any case, you MUST ALWAYS
		/// test the out <paramref name="lockTaken"/> argument before acting
		/// on critical code. Example:
		/// <code>
		/// using (IDowngrade downgrade = myLock.TryEnterUpgradeableReadLock(out bool lockTaken)) {
		///     if (!lockTaken)
		///         return null;
		///     object myValue = myData.Read();
		///     if (!myValue.IsDirty)
		///         return myValue.Value;
		///     using (myLock.TryEnterWriteLock(out lockTaken)) {
		///         if (!lockTaken)
		///             return myValue.Value;
		///         myData.Write(myValue.NewValue);
		///     }
		///     downgrade.TryDowngrade(out lockTaken);
		///     if (!lockTaken)
		///         return null;
		///     return myData.Read();
		/// }
		/// </code>
		/// </summary>
		/// <param name="lockTaken">MUST ALWAYS be tested before acting on critical code.
		/// This will be false if the lock is not acquired now.</param>
		/// <param name="millisecondsTimeout">Optional timeout to wait for the lock.</param>
		/// <returns>Not null; and can safely always be disposed --- YET STILL
		/// you MUST ALWAYS test the out <paramref name="lockTaken"/> argument
		/// before acting on critical code.</returns>
		/// <exception cref="LockRecursionException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDowngrade TryEnterUpgradeableReadLock(
				out bool lockTaken,
				int millisecondsTimeout = Timeout.Infinite)
		{
			lockTaken = ReaderWriterLock.TryEnterUpgradeableReadLock(millisecondsTimeout);
			return lockTaken
					? new UpgradeableReleaser(ReaderWriterLock)
					: (IDowngrade)NoOpReleaser.Instance;
		}

		/// <summary>
		/// Enters the read lock; and returns an object that can ALWAYS be disposed
		/// to safely release that lock. The returned object will not attempt to
		/// release the lock if this attempt to acquire does not succeed.
		/// You should simply always use the result in a <see langword="using"/>
		/// block since it is safe to do so; and in any case, you MUST ALWAYS
		/// test the out <paramref name="lockTaken"/> argument before acting
		/// on critical code.
		/// </summary>
		/// <returns>Not null; and can safely always be disposed --- YET STILL
		/// you MUST ALWAYS test the out <paramref name="lockTaken"/> argument
		/// before acting on critical code.</returns>
		/// <exception cref="LockRecursionException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ObjectDisposedException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IDisposable TryEnterReadLock(out bool lockTaken, int millisecondsTimeout = Timeout.Infinite)
		{
			lockTaken = ReaderWriterLock.TryEnterReadLock(millisecondsTimeout);
			return lockTaken
					? releaseReadLock
					: NoOpReleaser.Instance;
		}


		/// <summary>
		/// NOTICE that this method DISPOSES the underlying <see cref="ReaderWriterLockSlim"/>.
		/// This method IS NOT used to exit a lock.
		/// </summary>
		public void Dispose()
			=> ReaderWriterLock.Dispose();
	}
}
