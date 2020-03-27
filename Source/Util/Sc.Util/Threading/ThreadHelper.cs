using System;
using System.Diagnostics;
using System.Threading;
using Sc.Abstractions.Lifecycle;
using Sc.Util.System;


namespace Sc.Util.Threading
{
	/// <summary>
	/// Static helpers for Threads.
	/// </summary>
	public static class ThreadHelper
	{
		/// <summary>
		/// Params for the New Thread methods.
		/// </summary>
		/// <typeparam name="TResult">User <see cref="initializer"/> return type.</typeparam>
		private sealed class ThreadParams<TResult>
				: IInitialize,
						IDisposable
				where TResult : class
		{
			private Action<TResult> threadStart;
			private Func<TResult> initializer;
			private AutoResetEvent gate;
			private TResult result;
			private Exception error;


			/// <summary>
			/// Constructor. Creates the <see cref="Gate"/>.
			/// </summary>
			/// <param name="threadStart">Not null.</param>
			/// <param name="initializer">MAY be null.</param>
			public ThreadParams(Action<TResult> threadStart, Func<TResult> initializer)
			{
				this.threadStart = threadStart ?? throw new ArgumentNullException(nameof(threadStart));
				this.initializer = initializer;
				gate = new AutoResetEvent(false);
			}


			public void Initialize()
			{
				if (initializer != null) {
					try {
						Result = initializer();
					} catch (Exception exception) {
						Trace.TraceError(
								"{0}: Catching exception within ThreadStart Initializer"
								+ " (Thread will not run): {1}",
								nameof(ThreadHelper.NewThread),
								exception.Message);
						Trace.WriteLine(exception);
						Error = exception;
						Interlocked.Exchange(ref threadStart, null);
						return;
					} finally {
						Interlocked.Exchange(ref initializer, null);
						Gate.Set();
					}
				}
				Action<TResult> start = threadStart;
				Interlocked.Exchange(ref threadStart, null);
				start(Result);
			}


			/// <summary>
			/// Used to communicate between the invoker and the new Thread.
			/// </summary>
			public AutoResetEvent Gate
			{
				get {
					Interlocked.MemoryBarrier();
					return gate;
				}
				private set => Interlocked.Exchange(ref gate, value);
			}

			/// <summary>
			/// The returned user's Initializer Result.
			/// </summary>
			public TResult Result
			{
				get {
					Interlocked.MemoryBarrier();
					return result;
				}
				private set => Interlocked.Exchange(ref result, value);
			}

			/// <summary>
			/// Any error raised on the new Thread from the user's Initializer.
			/// </summary>
			public Exception Error
			{
				get {
					Interlocked.MemoryBarrier();
					return error;
				}
				private set => Interlocked.Exchange(ref error, value);
			}


			public void Dispose()
			{
				Gate.Dispose();
				Gate = null;
				Result = null;
				Error = null;
			}
		}


		/// <summary>
		/// Creates a new Thread. Allows invoking an <paramref name="onThreadStart"/>
		/// delegate on the thread, capturing and returning your result here
		/// from that delegate, which has been run on the new Thread.
		/// Then invokes your <paramref name="threadStart"/> to run the
		/// new Thread. Notice that if provided, your
		/// <paramref name="onThreadStart"/> is run ON the new
		/// Thread. Your delegate will be invoked in a catch block, and if it
		/// raises an error, this will return that Exception,
		/// and your <paramref name="threadStart"/> does not run, and the Thread exits.
		/// Your given <paramref name="threadStart"/> otherwise then runs the new Thread.
		/// You may also provide the <paramref name="newThreadInitializer"/> to set
		/// properties on the newly-constructed Thread before it is started,
		/// and before your <paramref name="onThreadStart"/> runs; and, that
		/// is invoked on this invoking Thread. The new Thread is set as a
		/// Background Thread by default. Before returning, if the Name is
		/// null, the Name will be set to
		/// <c>"ThreadHelper[TResult]-threadStart.GetHashCode()"</c>
		/// (other settings are all at defaults; and could be changed with the
		/// <paramref name="newThreadInitializer"/> or in your thread start).
		/// </summary>
		/// <typeparam name="TResult">Your <paramref name="onThreadStart"/>
		/// result type.</typeparam>
		/// <param name="threadStart">Action that runs the new Thread's ThreadStart.
		/// Receives any result returned from your optional
		/// <paramref name="onThreadStart"/> delegate.</param>
		/// <param name="onThreadStart">Optional initializer that will be run
		/// before the <paramref name="threadStart"/> is run. If given, the result from this
		/// Func --- even if null --- is passed to your <paramref name="threadStart"/>,
		/// and also returned from this method. Note that this runs on the new Thread.</param>
		/// <param name="newThreadInitializer">Optional constructor for the new Thread. If
		/// this is provided, it is invoked with the new Thread before it is ,
		/// and before your <paramref name="onThreadStart"/> runs; and,
		/// is invoked on this invoking Thread.</param>
		/// <param name="waitForThreadInitializer">A timeout for your invoking Thread
		/// to wait for the new Thread's <paramref name="onThreadStart"/> to
		/// complete, start the thread and return your result. Note that this
		/// will default to thirty seconds: if your initiaizer does not return, then
		/// the thread is NOT aborted, but the result from this method will
		/// be null: the thread will continue to try to start and eventually
		/// complete and return the result from your initializer --- you
		/// MAY get that result directly from that delegate, but this method
		/// will return null and let the thread continue to try to complete
		/// initialization. If this times out, then the returned Exception
		/// will be set to a <see cref="TimeoutException"/>.</param>
		/// <returns>Any result returned by your optional
		/// <paramref name="onThreadStart"/> delegate. Notice that if the
		/// returned Exception is not null, then this will be null.
		/// Also returns any Exception thrown by that delegate (from within the new
		/// Thread). And returns the thread.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static (TResult onThreadStartResult, Exception onThreadStartError, Thread thread) NewThread<TResult>(
				Action<TResult> threadStart,
				Func<TResult> onThreadStart = null,
				Action<Thread> newThreadInitializer = null,
				TimeSpan? waitForThreadInitializer = null)
				where TResult : class
		{
			using (ThreadParams<TResult> threadParams
					= new ThreadParams<TResult>(threadStart, onThreadStart)) {
				Thread newThread = new Thread(ThreadStart)
				{
					IsBackground = true,
				};
				newThreadInitializer?.Invoke(newThread);
				newThread.Start(threadParams);
				bool gotSignal = threadParams.Gate.WaitOne(
						waitForThreadInitializer ?? TimeSpan.FromSeconds(30D), false);
				Exception onThreadStartError = threadParams.Error;
				TResult onThreadStartResult = threadParams.Result;
				(TResult onThreadStartResult, Exception, Thread newThread) result
						= (onThreadStartResult,
								onThreadStartError
								?? (gotSignal
										? (Exception)null
										: new TimeoutException(
												$"{nameof(AutoResetEvent)}.{nameof(AutoResetEvent.WaitOne)}")),
								newThread);
				if ((onThreadStartError != null)
						|| (newThread.Name != null))
					return result;
				try {
					newThread.Name
							= $"{nameof(ThreadHelper)}"
							+ $"[{typeof(TResult).GetFriendlyName()}]"
							+ $"-{threadStart.GetHashCode()}";
				} catch {
					// Ignored
				}
				return result;
			}
			static void ThreadStart(object @params)
				=> ((ThreadParams<TResult>)@params).Initialize();
		}
	}
}
