using System;
using System.Threading;
using System.Threading.Tasks;


namespace Sc.Util.Threading
{
	/// <summary>
	/// Static helpers for <see cref="Task"/>.
	/// </summary>
	public static class TaskHelper
	{
		/// <summary>
		/// Creates a new <see cref="Task{TResult}"/> that is delayed for
		/// <paramref name="delay"/>, and then returns your <paramref name="result"/>.
		/// </summary>
		/// <typeparam name="TResult">Your Task result type.</typeparam>
		/// <param name="delay">The Task delay fro the result.</param>
		/// <param name="result">The result for the Task.</param>
		/// <param name="cancellationToken">Optional token that can cancel the task.</param>
		/// <returns>Not null.</returns>
		public static Task<TResult> FromResultAfter<TResult>(
				TimeSpan delay,
				TResult result,
				CancellationToken cancellationToken)
		{
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			Task.Delay(delay, cancellationToken)
					.ContinueWith(Continuation, (tcs, cancellationToken, result), cancellationToken);
			return tcs.Task;
			static void Continuation(Task task, object asyncState)
			{
				(TaskCompletionSource<TResult> completionSource,
								CancellationToken token,
								TResult r)
						= ((TaskCompletionSource<TResult> completionSource,
								CancellationToken token,
								TResult r))asyncState;
				if (token.IsCancellationRequested)
					completionSource.TrySetCanceled();
				else
					completionSource.TrySetResult(r);
			}
		}


		/// <summary>
		/// Utility method will check this given <paramref name="task"/> first if it
		/// is cancelled; and try to transition this <paramref name="taskCompletionSource"/>
		/// to cancelled. Else if the Task is faulted, AND has an Exception, then
		/// this tries to transition the completion source with that Exception.
		/// Lastly, this will try to set the Task's Result; and if fetching
		/// that raises an exception, then the exception is set on the
		/// completion source.
		/// </summary>
		/// <typeparam name="T">The task result type.</typeparam>
		/// <param name="taskCompletionSource">Not null: if already complete,
		/// then the method returns false.</param>
		/// <param name="task">Not null: if not yet complete in any state, then
		/// the method returns false.</param>
		/// <returns>True if the <paramref name="task"/> is complete in any state,
		/// and the <paramref name="taskCompletionSource"/> was
		/// transitioned to any completed state. False if the task is not
		/// complete now, or the completion source is complete now.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryCopyResultFrom<T>(this TaskCompletionSource<T> taskCompletionSource, Task<T> task)
		{
			if (taskCompletionSource == null)
				throw new ArgumentNullException(nameof(taskCompletionSource));
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			if (!task.IsCompleteInAnyState()
					|| taskCompletionSource.Task.IsCompleteInAnyState())
				return false;
			if (task.IsCanceled) {
				if (taskCompletionSource.TrySetCanceled())
					return true;
			}
			if (task.IsFaulted) {
				if ((task.Exception != null)
						&& taskCompletionSource.TrySetException(task.Exception))
					return true;
			}
			try {
				taskCompletionSource.SetResult(task.Result);
			} catch (Exception exception) {
				taskCompletionSource.TrySetException(exception);
			}
			return true;
		}


		/// <summary>
		/// Returns true if this <paramref name="taskCompletionSource"/>'s
		/// <see cref="Task"/> is completed, cancelled, or faulted.
		/// </summary>
		/// <typeparam name="T">Completion result type.</typeparam>
		/// <param name="taskCompletionSource">Not null.</param>
		/// <returns>If false, the completion source may still be transitioned
		/// into some completed state --- and if true it already has.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool IsCompleteInAnyState<T>(this TaskCompletionSource<T> taskCompletionSource)
		{
			if (taskCompletionSource == null)
				throw new ArgumentNullException(nameof(taskCompletionSource));
			return taskCompletionSource.Task.IsCompleteInAnyState();
		}

		/// <summary>
		/// Returns true if this <paramref name="task"/>
		/// is completed, cancelled, or faulted.
		/// </summary>
		/// <param name="task">Not null.</param>
		/// <returns>If false, the Task may still transition
		/// into some completed state --- and if true it already has.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool IsCompleteInAnyState(this Task task)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			return task.IsCompleted
					|| task.IsCanceled
					|| task.IsFaulted;
		}


		/// <summary>
		/// Returns this <paramref name="task"/> <see cref="Task{TResult}.Result"/>
		/// if it is completed, and not cancelled nor faulted.
		/// </summary>
		/// <param name="task">Not null.</param>
		/// <param name="result">The result if the method returns true.</param>
		/// <returns>If false, the Task MAY still transition
		/// into some completed state --- and if true then the
		/// <paramref name="result"/> is set.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryGetResult<T>(this Task<T> task, out T result)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			if (task.IsCompleted
					&& !task.IsCanceled
					&& !task.IsFaulted) {
				result = task.Result;
				return true;
			}
			result = default;
			return false;
		}


		/// <summary>
		/// This helper method creates a new <see cref="TaskCompletionSource{TResult}"/>,
		/// and returns a delegate that will complete the Task when the delegate is invoked.
		/// </summary>
		/// <param name="callback">Not null: when invoked, the returned Task
		/// is completed.</param>
		/// <param name="cancellationToken">Optional: if provided, then the returned Task
		/// will be attempted to be cancelled if Token is cancelled.</param>
		/// <returns>Not null: completes when the <paramref name="callback"/>
		/// is invoked; or cancels with the <paramref name="cancellationToken"/>.</returns>
		public static Task NewCompletionSource(out Action callback, CancellationToken cancellationToken = default)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			callback = Callback;
			return tcs.Task;
			void Callback()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetResult(true);
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}

		/// <summary>
		/// This helper method creates a new <see cref="TaskCompletionSource{TResult}"/>
		/// for your <typeparamref name="T"/> argument type, and returns a
		/// delegate that will complete the Task when the delegate is invoked.
		/// </summary>
		/// <typeparam name="T">The Task result type.</typeparam>
		/// <param name="callback">Not null: when invoked, the returned Task
		/// is completed with the argument.</param>
		/// <param name="cancellationToken">Optional: if provided, then the returned Task
		/// will be attempted to be cancelled if Token is cancelled.</param>
		/// <returns>Not null: completes when the <paramref name="callback"/>
		/// is invoked; and returns the callback's argument.</returns>
		public static Task<T> NewCompletionSource<T>(
				out Action<T> callback,
				CancellationToken cancellationToken = default)
		{
			TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			callback = Callback;
			return tcs.Task;
			void Callback(T result)
			{
				cancellationTokenRegistration.Dispose();
				tcs.SetResult(result);
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}

		/// <summary>
		/// This helper method creates a new <see cref="TaskCompletionSource{TResult}"/>
		/// for your argument type, and returns a
		/// delegate that will complete the Task when the delegate is invoked.
		/// </summary>
		/// <typeparam name="T1">The Task's first result type.</typeparam>
		/// <typeparam name="T2">The Task's second result type.</typeparam>
		/// <param name="callback">Not null: when invoked, the returned Task
		/// is completed with the argument.</param>
		/// <param name="cancellationToken">Optional: if provided, then the returned Task
		/// will be attempted to be cancelled if Token is cancelled.</param>
		/// <returns>Not null: completes when the <paramref name="callback"/>
		/// is invoked; and returns the callback's arguments.</returns>
		public static Task<(T1 arg1, T2 arg2)> NewCompletionSource<T1, T2>(
				out Action<T1, T2> callback,
				CancellationToken cancellationToken = default)
		{
			TaskCompletionSource<(T1, T2)> tcs = new TaskCompletionSource<(T1, T2)>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			callback = Callback;
			return tcs.Task;
			void Callback(T1 arg1, T2 arg2)
			{
				cancellationTokenRegistration.Dispose();
				tcs.SetResult((arg1, arg2));
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}

		/// <summary>
		/// This helper method creates a new <see cref="TaskCompletionSource{TResult}"/>
		/// for your argument type, and returns a
		/// delegate that will complete the Task when the delegate is invoked.
		/// </summary>
		/// <typeparam name="T1">The Task's first result type.</typeparam>
		/// <typeparam name="T2">The Task's second result type.</typeparam>
		/// <typeparam name="T3">The Task's third result type.</typeparam>
		/// <param name="callback">Not null: when invoked, the returned Task
		/// is completed with the argument.</param>
		/// <param name="cancellationToken">Optional: if provided, then the returned Task
		/// will be attempted to be cancelled if Token is cancelled.</param>
		/// <returns>Not null: completes when the <paramref name="callback"/>
		/// is invoked; and returns the callback's arguments.</returns>
		public static Task<(T1 arg1, T2 arg2, T3 arg3)> NewCompletionSource<T1, T2, T3>(
				out Action<T1, T2, T3> callback,
				CancellationToken cancellationToken = default)
		{
			TaskCompletionSource<(T1, T2, T3)> tcs = new TaskCompletionSource<(T1, T2, T3)>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			callback = Callback;
			return tcs.Task;
			void Callback(T1 arg1, T2 arg2, T3 arg3)
			{
				cancellationTokenRegistration.Dispose();
				tcs.SetResult((arg1, arg2, arg3));
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}


		/// <summary>
		/// Helper method that will create a delegate continuation for your
		/// <paramref name="task"/>, which when run will then check your
		/// <paramref name="synchronizationContext"/>, and if not null,
		/// posts the continuation onto that context. Therefore,
		/// your <paramref name="continuation"/> runs on your
		/// <paramref name="synchronizationContext"/> if not null
		/// --- and the returned <see cref="Task"/> completes on that
		/// context. Note that the <paramref name="synchronizationContext"/>
		/// CAN be null; and then then continuation runs synchronously
		/// on the Tasks's continuing context.
		/// </summary>
		/// <param name="task">Not null.</param>
		/// <param name="synchronizationContext">CAN be null.</param>
		/// <param name="continuation">Not null.</param>
		/// <returns>Completes when your <paramref name="continuation"/>
		/// has run.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task ContinueOn(
				this Task task,
				SynchronizationContext synchronizationContext,
				Action continuation)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			if (continuation == null)
				throw new ArgumentNullException(nameof(continuation));
			return task.ContinueWith(GetCompletion)
					.Unwrap();
			Task GetCompletion(Task _)
				=> synchronizationContext.SendOrPost(continuation);
		}

		/// <summary>
		/// Helper method that will create a delegate continuation for your
		/// <paramref name="task"/>, which when run will then check your
		/// <paramref name="synchronizationContext"/>, and if not null,
		/// posts the continuation onto that context. Therefore,
		/// your <paramref name="continuation"/> runs on your
		/// <paramref name="synchronizationContext"/> if not null
		/// --- and the returned <see cref="Task"/> completes on that
		/// context. Note that the <paramref name="synchronizationContext"/>
		/// CAN be null; and then then continuation runs synchronously
		/// on the Tasks's continuing context.
		/// </summary>
		/// <typeparam name="TState">Argument type for your
		/// <paramref name="continuation"/>.</typeparam>
		/// <param name="task">Not null.</param>
		/// <param name="synchronizationContext">CAN be null.</param>
		/// <param name="continuation">Not null.</param>
		/// <returns>Completes when your <paramref name="continuation"/>
		/// has run.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task ContinueOn<TState>(
				this Task task,
				SynchronizationContext synchronizationContext,
				Action<TState> continuation,
				TState asyncState)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			if (continuation == null)
				throw new ArgumentNullException(nameof(continuation));
			return task.ContinueWith(GetCompletion)
					.Unwrap();
			Task GetCompletion(Task _)
				=> synchronizationContext.SendOrPost(s => continuation(s), asyncState);
		}

		/// <summary>
		/// Helper method that will create a delegate continuation for your
		/// <paramref name="task"/>, which when run will then check your
		/// <paramref name="synchronizationContext"/>, and if not null,
		/// posts the continuation onto that context. Therefore,
		/// your <paramref name="continuation"/> runs on your
		/// <paramref name="synchronizationContext"/> if not null
		/// --- and the returned <see cref="Task"/> completes on that
		/// context. Note that the <paramref name="synchronizationContext"/>
		/// CAN be null; and then then continuation runs synchronously
		/// on the Tasks's continuing context.
		/// </summary>
		/// <typeparam name="TState">Argument type for your
		/// <paramref name="continuation"/>.</typeparam>
		/// <typeparam name="TResult">Result type for your
		/// <paramref name="continuation"/>.</typeparam>
		/// <param name="task">Not null.</param>
		/// <param name="synchronizationContext">CAN be null.</param>
		/// <param name="continuation">Not null.</param>
		/// <returns>Completes when your <paramref name="continuation"/>
		/// has run.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task<TResult> ContinueOn<TState, TResult>(
				this Task task,
				SynchronizationContext synchronizationContext,
				Func<TState, TResult> continuation,
				TState asyncState)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			if (continuation == null)
				throw new ArgumentNullException(nameof(continuation));
			return task.ContinueWith(GetCompletion)
					.Unwrap();
			Task<TResult> GetCompletion(Task _)
				=> synchronizationContext.SendOrPost(s => continuation(s), asyncState);
		}
	}
}
