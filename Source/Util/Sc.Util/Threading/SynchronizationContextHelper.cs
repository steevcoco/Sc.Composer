using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;


namespace Sc.Util.Threading
{
	/// <summary>
	/// Extension methods for <see cref="SynchronizationContext"/>.
	/// </summary>
	public static class SynchronizationContextHelper
	{
		private static bool checkContinuationTask<TResult>(this Task task, out Task<TResult> cancelledOrFaultedTask)
		{
			if (task.IsCanceled) {
				TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
				tcs.SetCanceled();
				cancelledOrFaultedTask = tcs.Task;
				return false;
			}
			if (task.IsFaulted) {
				TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
				tcs.SetException(
						(Exception)task.Exception
						?? new TargetException($"Task faulted with no provided exception: {task}"));
				cancelledOrFaultedTask = tcs.Task;
				return false;
			}
			cancelledOrFaultedTask = null;
			return true;
		}


		/// <summary>
		/// Provides a basic implementation to check if the Current
		/// <see cref="SynchronizationContext.Current"/>
		/// is equal to the given <paramref name="checkContext"/>.
		/// Note that the argument CAN be null.
		/// If the method returns true, then the Current context is
		/// considered equal to the <paramref name="checkContext"/>.
		/// If the Current context equals this argument, then it is considered
		/// safe to synchronously invoke some action now that is required to
		/// execute on this provided <paramref name="checkContext"/>; AND at the
		/// same time, CAN INDICATE that an attempt to await an activity
		/// on this <paramref name="checkContext"/> from the current
		/// context may never return.
		/// </summary>
		/// <param name="checkContext">CAN be null.</param>
		/// <returns>True if the current <see cref="SynchronizationContext.Current"/>
		/// is equal to the argument <paramref name="checkContext"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckInvoke(this SynchronizationContext checkContext)
			=> checkContext.AreEqual(SynchronizationContext.Current);

		/// <summary>
		/// Provides a basic implementation to check if the given
		/// <paramref name="synchronizationContext"/>
		/// is equal to the given <paramref name="other"/>.
		/// Note that either argument CAN be null.
		/// If the method returns true, then this context is
		/// considered equal to the <paramref name="other"/>.
		/// If this context equals this argument, then it is considered
		/// safe to synchronously invoke some action now that is required to
		/// execute on this provided <paramref name="other"/>; AND at the
		/// same time, CAN INDICATE that an attempt to await an activity
		/// on this <paramref name="other"/> from the current
		/// <paramref name="synchronizationContext"/> may never return.
		/// </summary>
		/// <param name="synchronizationContext">CAN be null.</param>
		/// <param name="other">CAN be null.</param>
		/// <returns>True if this <paramref name="synchronizationContext"/>
		/// is equal to the argument <paramref name="other"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreEqual(this SynchronizationContext synchronizationContext, SynchronizationContext other)
		{
			if (synchronizationContext == null)
				return other == null;
			if (other == null)
				return false;
			if (synchronizationContext.Equals(other))
				return true;
			if (synchronizationContext is ISynchronizeInvoke synchronizeInvoke)
				return !synchronizeInvoke.InvokeRequired;
			// HACK TODO
			return CheckField(synchronizationContext, null, out object d1, out string fieldName)
					? CheckField(other, fieldName, out object d2, out _)
							&& object.Equals(d1, d2)
					: CheckProperty(synchronizationContext, null, out d1, out string propertyName)
							? CheckProperty(other, propertyName, out d2, out _)
									&& object.Equals(d1, d2)
							: false;
			static bool CheckField(
					SynchronizationContext context,
					string memberName,
					out object value,
					out string name)
			{
				FieldInfo member = (memberName == null)
						? (context.GetType()
										.GetField("_dispatcher")
								?? context.GetType()
										.GetField("_thread")
								?? context.GetType()
										.GetField("dispatcher")
								?? context.GetType()
										.GetField("thread"))
						: context.GetType()
								.GetField(memberName);
				if (member == null) {
					value = null;
					name = null;
					return false;
				}
				value = member.GetValue(context);
				name = member.Name;
				return true;
			}
			static bool CheckProperty(
					SynchronizationContext context,
					string memberName,
					out object value,
					out string name)
			{
				PropertyInfo member = (memberName == null)
						? (context.GetType()
										.GetProperty("Dispatcher")
								?? context.GetType()
										.GetProperty("Thread"))
						: context.GetType()
							.GetProperty(memberName);
				if (member == null) {
					value = null;
					name = null;
					return false;
				}
				value = member.GetValue(context);
				name = member.Name;
				return true;
			}
		}

		/// <summary>
		/// Provides a basic implementation to check if the Current
		/// <see cref="SynchronizationContext.Current"/>
		/// is not equal to the given <paramref name="checkContext"/>; and
		/// returns true if the selected <paramref name="sendOrPostPolicy"/>
		/// would require Posting an action.
		/// If the method returns true, then the Current context is not
		/// considered equal to the <paramref name="checkContext"/>;
		/// OR the selected <paramref name="sendOrPostPolicy"/>
		/// otherwise specifies Posting to that context.
		/// </summary>
		/// <param name="checkContext">CAN be null.</param>
		/// <returns>True if an action would be Posted to the
		/// <paramref name="checkContext"/> based on the selected
		/// <paramref name="sendOrPostPolicy"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ShouldPost(this SynchronizationContext checkContext, SendOrPostPolicy sendOrPostPolicy)
		{
			switch (sendOrPostPolicy) {
				case SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown
						when (checkContext != null) && !checkContext.CheckInvoke():
				case SendOrPostPolicy.PostSafeAlwaysOrInvokeUnknown when checkContext != null:
				case SendOrPostPolicy.PostAlwaysSafeOrUnknown:
					return true;
				default:
					return false;
			}
		}


		/// <summary>
		/// Helper method will invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Send"/>.
		/// </summary>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void Send(this SynchronizationContext synchronizationContext, Action callback)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			synchronizationContext.Send(Callback, null);
			void Callback(object _)
				=> callback();
		}

		/// <summary>
		/// Helper method will invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Send"/>.
		/// </summary>
		/// <typeparam name="TState">Your delegate's argument type.</typeparam>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="asyncState">Not tested here: an argument for
		/// your <paramref name="callback"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void Send<TState>(
				this SynchronizationContext synchronizationContext,
				Action<TState> callback,
				TState asyncState)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			synchronizationContext.Send(Callback, asyncState);
			void Callback(object state)
				=> callback((TState)state);
		}

		/// <summary>
		/// Helper method will invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Send"/>; and return the result.
		/// </summary>
		/// <typeparam name="TResult">Your result type.</typeparam>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <returns>Your <paramref name="callback"/> result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TResult Send<TResult>(
				this SynchronizationContext synchronizationContext,
				Func<TResult> callback)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			object syncLock = new object();
			TResult result = default;
			synchronizationContext.Send(Callback, (syncLock, callback));
			lock (syncLock) {
				return result;
			}
			void Callback(object state)
			{
				(object monitor, Func<TResult> func) = ((object monitor, Func<TResult> func))state;
				lock (monitor) {
					result = func();
				}
			}
		}

		/// <summary>
		/// Helper method will invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Send"/>; and return the result.
		/// </summary>
		/// <typeparam name="TResult">Your result type.</typeparam>
		/// <typeparam name="T">Your delegate's argument type.</typeparam>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="asyncState">Not tested here: an argument for
		/// your <paramref name="callback"/>.</param>
		/// <returns>Your <paramref name="callback"/> result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TResult Send<T, TResult>(
				this SynchronizationContext synchronizationContext,
				Func<T, TResult> callback,
				T asyncState)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			object syncLock = new object();
			TResult result = default;
			synchronizationContext.Send(Callback, (syncLock, callback, asyncState));
			lock (syncLock) {
				return result;
			}
			void Callback(object state)
			{
				(object monitor, Func<T, TResult> func, T arg) = ((object monitor, Func<T, TResult> func, T arg))state;
				lock (monitor) {
					result = func(arg);
				}
			}
		}


		/// <summary>
		/// Helper method will invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Post"/>.
		/// </summary>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="cancellationToken">Optional: since a delegate is created
		/// here, then if this is provided, and your given <paramref name="callback"/>
		/// has not yet been invoked, then the returned Task
		/// will be cancelled if Token is cancelled.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task Post(
				this SynchronizationContext synchronizationContext,
				Action callback,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			synchronizationContext.Post(Callback, null);
			return tcs.Task;
			void Callback(object _)
			{
				cancellationTokenRegistration.Dispose();
				if (tcs.IsCompleteInAnyState())
					return;
				if (cancellationToken.IsCancellationRequested) {
					tcs.SetCanceled();
					return;
				}
				try {
					callback();
					tcs.SetResult(true);
				} catch (Exception exception) {
					tcs.TrySetException(exception);
				}
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}

		/// <summary>
		/// Helper method will invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Post"/>.
		/// </summary>
		/// <typeparam name="TState">Your delegate's argument type.</typeparam>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="asyncState">Not tested here: an argument for
		/// your <paramref name="callback"/>.</param>
		/// <param name="cancellationToken">Optional: since a delegate is created
		/// here, then if this is provided, and your given <paramref name="callback"/>
		/// has not yet been invoked, then the returned Task
		/// will be cancelled if Token is cancelled.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task Post<TState>(
				this SynchronizationContext synchronizationContext,
				Action<TState> callback,
				TState asyncState,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			synchronizationContext.Post(Callback, asyncState);
			return tcs.Task;
			void Callback(object state)
			{
				cancellationTokenRegistration.Dispose();
				if (tcs.IsCompleteInAnyState())
					return;
				if (cancellationToken.IsCancellationRequested) {
					tcs.SetCanceled();
					return;
				}
				try {
					callback((TState)state);
					tcs.SetResult(true);
				} catch (Exception exception) {
					tcs.TrySetException(exception);
				}
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}

		/// <summary>
		/// Helper method will begin invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Post"/>; and return the result.
		/// </summary>
		/// <typeparam name="TResult">Your result type.</typeparam>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="cancellationToken">Optional: since a delegate is created
		/// here, then if this is provided, and your given <paramref name="callback"/>
		/// has not yet been invoked, then the returned Task
		/// will be cancelled if Token is cancelled.</param>
		/// <returns>Completes with your <paramref name="callback"/> result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task<TResult> Post<TResult>(
				this SynchronizationContext synchronizationContext,
				Func<TResult> callback,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			synchronizationContext.Post(Callback, (tcs, callback));
			return tcs.Task;
			void Callback(object state)
			{
				cancellationTokenRegistration.Dispose();
				if (tcs.IsCompleteInAnyState())
					return;
				if (cancellationToken.IsCancellationRequested) {
					tcs.SetCanceled();
					return;
				}
				(TaskCompletionSource<TResult> completion, Func<TResult> func)
						= ((TaskCompletionSource<TResult> completion, Func<TResult> func))state;
				try {
					completion.SetResult(func());
				} catch (Exception exception) {
					completion.TrySetException(exception);
				}
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}

		/// <summary>
		/// Helper method will begin invoke your <paramref name="callback"/> on this
		/// <paramref name="synchronizationContext"/>, with
		/// <see cref="SynchronizationContext.Post"/>; and return the result.
		/// </summary>
		/// <typeparam name="TResult">Your result type.</typeparam>
		/// <typeparam name="T">Your delegate's argument type.</typeparam>
		/// <param name="synchronizationContext">Not null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="asyncState">Not tested here: an argument for
		/// your <paramref name="callback"/>.</param>
		/// <param name="cancellationToken">Optional: since a delegate is created
		/// here, then if this is provided, and your given <paramref name="callback"/>
		/// has not yet been invoked, then the returned Task
		/// will be cancelled if Token is cancelled.</param>
		/// <returns>Completes with your <paramref name="callback"/> result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task<TResult> Post<T, TResult>(
				this SynchronizationContext synchronizationContext,
				Func<T, TResult> callback,
				T asyncState,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			CancellationTokenRegistration cancellationTokenRegistration = default;
			if (cancellationToken.CanBeCanceled)
				cancellationTokenRegistration = cancellationToken.Register(Cancel);
			synchronizationContext.Post(Callback, (tcs, callback, asyncState));
			return tcs.Task;
			void Callback(object state)
			{
				cancellationTokenRegistration.Dispose();
				if (tcs.IsCompleteInAnyState())
					return;
				if (cancellationToken.IsCancellationRequested) {
					tcs.SetCanceled();
					return;
				}
				(TaskCompletionSource<TResult> completion, Func<T, TResult> func, T arg)
						= ((TaskCompletionSource<TResult> completion, Func<T, TResult> func, T arg))state;
				try {
					completion.SetResult(func(arg));
				} catch (Exception exception) {
					completion.TrySetException(exception);
				}
			}
			void Cancel()
			{
				cancellationTokenRegistration.Dispose();
				tcs.TrySetCanceled();
			}
		}


		/// <summary>
		/// This helper method will invoke your
		/// <paramref name="callback"/> by first checking the given
		/// <paramref name="checkContext"/> (which CAN be null)
		/// against the current <see cref="SynchronizationContext.Current"/>,
		/// and Posting, Sending, or Invoking the callback according to the
		/// <see cref="SendOrPostPolicy"/>.
		/// </summary>
		/// <param name="checkContext">The <see cref="SynchronizationContext"/> to check
		/// against the current <see cref="SynchronizationContext.Current"/>.
		/// CAN be null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="sendOrPostPolicy">Invoke selection. Defaults to
		/// <see cref="SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown"/>.</param>
		/// <returns>Returns a <see cref="Task"/> that completes when your callback
		/// is invoked.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task SendOrPost(
				this SynchronizationContext checkContext,
				Action callback,
				SendOrPostPolicy sendOrPostPolicy = SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			if (SynchronizationContextHelper.ShouldPost(checkContext, sendOrPostPolicy))
				return (checkContext ?? new SynchronizationContext()).Post(callback);
			callback();
			return Task.WhenAll();
		}

		/// <summary>
		/// This helper method will invoke your
		/// <paramref name="callback"/> by first checking the given
		/// <paramref name="checkContext"/> (which CAN be null)
		/// against the current <see cref="SynchronizationContext.Current"/>,
		/// and Posting, Sending, or Invoking the callback according to the
		/// <see cref="SendOrPostPolicy"/>. This method allows you to provide a
		/// typed async state object.
		/// </summary>
		/// <typeparam name="TState">Your async state type.</typeparam>
		/// <param name="checkContext">The <see cref="SynchronizationContext"/> to check
		/// against the current <see cref="SynchronizationContext.Current"/>.
		/// CAN be null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="state">Optional.</param>
		/// <param name="sendOrPostPolicy">Invoke selection. Defaults to
		/// <see cref="SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown"/>.</param>
		/// <returns>Returns a <see cref="Task"/> that completes when your callback
		/// is invoked.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task SendOrPost<TState>(
				this SynchronizationContext checkContext,
				Action<TState> callback,
				TState state = default,
				SendOrPostPolicy sendOrPostPolicy = SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			if (SynchronizationContextHelper.ShouldPost(checkContext, sendOrPostPolicy))
				return (checkContext ?? new SynchronizationContext()).Post(callback, state);
			callback(state);
			return Task.WhenAll();
		}

		/// <summary>
		/// This helper method will invoke your
		/// <paramref name="callback"/> by first checking the given
		/// <paramref name="checkContext"/> (which CAN be null)
		/// against the current <see cref="SynchronizationContext.Current"/>,
		/// and Posting, Sending, or Invoking the callback according to the
		/// <see cref="SendOrPostPolicy"/>. This method allows you to return a result.
		/// </summary>
		/// <typeparam name="TResult">Your async result type.</typeparam>
		/// <param name="checkContext">The <see cref="SynchronizationContext"/> to check
		/// against the current <see cref="SynchronizationContext.Current"/>.
		/// CAN be null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="sendOrPostPolicy">Invoke selection. Defaults to
		/// <see cref="SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown"/>.</param>
		/// <returns>Returns a <see cref="Task"/> that completes when your callback
		/// is invoked; and has your callback's result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<TResult> SendOrPost<TResult>(
				this SynchronizationContext checkContext,
				Func<TResult> callback,
				SendOrPostPolicy sendOrPostPolicy = SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			return SynchronizationContextHelper.ShouldPost(checkContext, sendOrPostPolicy)
					? (checkContext ?? new SynchronizationContext()).Post(callback)
					: Task.FromResult(callback());
		}

		/// <summary>
		/// This helper method will invoke your
		/// <paramref name="callback"/> by first checking the given
		/// <paramref name="checkContext"/> (which CAN be null)
		/// against the current <see cref="SynchronizationContext.Current"/>,
		/// and Posting, Sending, or Invoking the callback according to the
		/// <see cref="SendOrPostPolicy"/>. This method allows you to provide a
		/// typed async state object, and return a result.
		/// </summary>
		/// <typeparam name="TState">Your async state type.</typeparam>
		/// <typeparam name="TResult">Your async result type.</typeparam>
		/// <param name="checkContext">The <see cref="SynchronizationContext"/> to check
		/// against the current <see cref="SynchronizationContext.Current"/>.
		/// CAN be null.</param>
		/// <param name="callback">Not null.</param>
		/// <param name="state">Optional.</param>
		/// <param name="sendOrPostPolicy">Invoke selection. Defaults to
		/// <see cref="SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown"/>.</param>
		/// <returns>Returns a <see cref="Task"/> that completes when your callback
		/// is invoked; and has your callback's result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Task<TResult> SendOrPost<TState, TResult>(
				this SynchronizationContext checkContext,
				Func<TState, TResult> callback,
				TState state = default,
				SendOrPostPolicy sendOrPostPolicy = SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			return SynchronizationContextHelper.ShouldPost(checkContext, sendOrPostPolicy)
					? (checkContext ?? new SynchronizationContext()).Post(callback, state)
					: Task.FromResult(callback(state));
		}


		/// <summary>
		/// This method will invoke <see cref="Task.Delay(TimeSpan)"/> with a continuation
		/// that will then Post your <paramref name="callback"/> to this
		/// <see cref="SynchronizationContext"/>.
		/// </summary>
		/// <param name="synchronizationContext">Not null: the context to Post onto.</param>
		/// <param name="delay">The delay time.</param>
		/// <param name="callback">Required.</param>
		/// <param name="cancellationToken">Optional token that will cancel the task.</param>
		/// <returns>A Task that completes when your <paramref name="callback"/>
		/// has completed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task PostAfter(
				this SynchronizationContext synchronizationContext,
				TimeSpan delay,
				Action callback,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			return Task.Delay(delay, cancellationToken)
					.ContinueWith(DelayCallback, cancellationToken)
					.Unwrap();
			Task DelayCallback(Task task)
				=> task.IsCanceled || task.IsFaulted
						? task
						: synchronizationContext.SendOrPost(callback);
		}

		/// <summary>
		/// This method will invoke <see cref="Task.Delay(TimeSpan)"/> with a continuation
		/// that will then Post your <paramref name="callback"/> to this
		/// <see cref="SynchronizationContext"/>. This method allows you to pass
		/// a typed async state object.
		/// </summary>
		/// <typeparam name="TState">Your async state type.</typeparam>
		/// <param name="synchronizationContext">CAN be null: the context to Post onto.</param>
		/// <param name="delay">The delay time.</param>
		/// <param name="callback">Required.</param>
		/// <param name="asyncState">Note: not checked here: is always passed to your
		/// <paramref name="callback"/>.</param>
		/// <param name="cancellationToken">Optional token that will cancel the task.</param>
		/// <returns>A Task that completes when your <paramref name="callback"/>
		/// has completed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task PostAfter<TState>(
				this SynchronizationContext synchronizationContext,
				TimeSpan delay,
				Action<TState> callback,
				TState asyncState = default,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			return Task.Delay(delay, cancellationToken)
					.ContinueWith(DelayCallback, cancellationToken)
					.Unwrap();
			Task DelayCallback(Task task)
				=> task.IsCanceled || task.IsFaulted
						? task
						: synchronizationContext.SendOrPost(callback, asyncState);
		}

		/// <summary>
		/// This method will invoke <see cref="Task.Delay(TimeSpan)"/> with a continuation
		/// that will then Post your <paramref name="callback"/> to this
		/// <see cref="SynchronizationContext"/>. This method allows you to return a result.
		/// </summary>
		/// <typeparam name="TResult">Your async result type.</typeparam>
		/// <param name="synchronizationContext">CAN be null: the context to Post onto.</param>
		/// <param name="delay">The delay time.</param>
		/// <param name="callback">Required.</param>
		/// <param name="cancellationToken">Optional token that will cancel the task.</param>
		/// <returns>A Task that completes when your <paramref name="callback"/>
		/// has completed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<TResult> PostAfter<TResult>(
				this SynchronizationContext synchronizationContext,
				TimeSpan delay,
				Func<TResult> callback,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			return Task.Delay(delay, cancellationToken)
					.ContinueWith(DelayCallback, cancellationToken)
					.Unwrap();
			Task<TResult> DelayCallback(Task task)
				=> task.checkContinuationTask(out Task<TResult> cancelledOrFaultedTask)
						? synchronizationContext.SendOrPost(callback)
						: cancelledOrFaultedTask;
		}

		/// <summary>
		/// This method will invoke <see cref="Task.Delay(TimeSpan)"/> with a continuation
		/// that will then Post your <paramref name="callback"/> to this
		/// <see cref="SynchronizationContext"/>. This method allows you to pass
		/// a typed async state object and return a result.
		/// </summary>
		/// <typeparam name="TState">Your async state type.</typeparam>
		/// <typeparam name="TResult">Your async result type.</typeparam>
		/// <param name="synchronizationContext">CAN be null: the context to Post onto.</param>
		/// <param name="delay">The delay time.</param>
		/// <param name="callback">Required.</param>
		/// <param name="asyncState">Note: not checked here: is always passed to your
		/// <paramref name="callback"/>.</param>
		/// <param name="cancellationToken">Optional token that will cancel the task.</param>
		/// <returns>A Task that completes when your <paramref name="callback"/>
		/// has completed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<TResult> PostAfter<TState, TResult>(
				this SynchronizationContext synchronizationContext,
				TimeSpan delay,
				Func<TState, TResult> callback,
				TState asyncState = default,
				CancellationToken cancellationToken = default)
		{
			if (synchronizationContext == null)
				throw new ArgumentNullException(nameof(synchronizationContext));
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			return Task.Delay(delay, cancellationToken)
					.ContinueWith(DelayCallback, cancellationToken)
					.Unwrap();
			Task<TResult> DelayCallback(Task task)
				=> task.checkContinuationTask(out Task<TResult> cancelledOrFaultedTask)
						? synchronizationContext.SendOrPost(callback, asyncState)
						: cancelledOrFaultedTask;
		}
	}
}
