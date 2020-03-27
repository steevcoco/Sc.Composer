using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sc.Abstractions.Lifecycle;


namespace Sc.Util.System
{
	/// <summary>
	/// A thread-safe <see cref="IDisposable"/> class that invokes an
	/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/>,
	/// and provides some options. This class will not invoke your
	/// dispose delegate twice. The returned implementation also implements
	/// <see cref="IDispose"/>; which raises The <see cref="IRaiseDisposed.Disposed"/>
	/// event, and provides the <see cref="IRaiseDisposed.IsDisposed"/>
	/// property. An implementation is also provided that implements
	/// a finalizer that will also invoke your delegate (only once).
	/// Lastly, a generic version also provides an attached
	/// <see cref="DelegateDisposable{TState}.State"/> property
	/// that you can provide. Use the static
	/// <see cref="DelegateDisposable"/> methods to construct an instance.
	/// </summary>
	public abstract class DelegateDisposable
			: IDispose
	{
		/// <summary>
		/// No op implementation.
		/// </summary>
		private sealed class NoOpDisposable
				: IDisposable
		{
			public void Dispose() { }
		}


		/// <summary>
		/// Action implementation.
		/// </summary>
		private sealed class NoState
				: DelegateDisposable
		{
			/// <summary>
			/// Action implementation with a finalizer.
			/// </summary>
			internal sealed class Finalizing
					: DelegateDisposable
			{
				private Action<bool> dispose;


				/// <summary>
				/// Constructor.
				/// </summary>
				/// <param name="dispose">Not null.</param>
				internal Finalizing(Action<bool> dispose)
				{
					lock (SyncLock) {
						this.dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
					}
				}
				

				public override bool IsDisposed
				{
					get {
						lock (SyncLock) {
							return dispose == null;
						}
					}
				}

				public override event EventHandler Disposed;

				protected override void Dispose(bool isDisposing)
				{
					Action<bool> action;
					EventHandler eventHandler;
					lock (SyncLock) {
						action = dispose;
						if (action == null)
							return;
						dispose = null;
						eventHandler = Disposed;
						Disposed = null;
					}
					action(isDisposing);
					if (isDisposing)
						eventHandler?.Invoke(this, EventArgs.Empty);
				}

				~Finalizing()
					=> Dispose(false);
			}


			private Action dispose;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="dispose">Required.</param>
			internal NoState(Action dispose)
			{
				lock (SyncLock) {
					this.dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
				}
			}


			public override bool IsDisposed
			{
				get {
					lock (SyncLock) {
						return dispose == null;
					}
				}
			}

			public override event EventHandler Disposed;

			protected override void Dispose(bool isDisposing)
			{
				if (!isDisposing)
					return;
				Action action;
				EventHandler eventHandler;
				lock (SyncLock) {
					action = dispose;
					if (action == null)
						return;
					dispose = null;
					eventHandler = Disposed;
					Disposed = null;
				}
				action();
				eventHandler?.Invoke(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// State implementation.
		/// </summary>
		/// <typeparam name="TState">Your state type.</typeparam>
		private sealed class WithState<TState>
				: DelegateDisposable<TState>
		{
			/// <summary>
			/// State implementation with a finalizer.
			/// </summary>
			internal sealed class Finalizing
					: DelegateDisposable<TState>
			{
				private Action<TState, bool> dispose;


				/// <summary>
				/// Constructor.
				/// </summary>
				/// <param name="state">Optional.</param>
				/// <param name="dispose">Not null.</param>
				internal Finalizing(TState state, Action<TState, bool> dispose)
				{
					lock (SyncLock) {
						this.dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
						StateValue = state;
					}
				}


				public override bool IsDisposed
				{
					get {
						lock (SyncLock) {
							return dispose == null;
						}
					}
				}

				public override event EventHandler Disposed;

				protected override void Dispose(bool isDisposing)
				{
					Action<TState, bool> action;
					TState state;
					EventHandler eventHandler;
					lock (SyncLock) {
						action = dispose;
						if (action == null)
							return;
						dispose = null;
						state = StateValue;
						StateValue = default;
						eventHandler = Disposed;
						Disposed = null;
					}
					action(state, isDisposing);
					if (isDisposing)
						eventHandler?.Invoke(this, EventArgs.Empty);
				}

				~Finalizing()
					=> Dispose(false);
			}


			private Action<TState> dispose;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="state">optional.</param>
			/// <param name="dispose">Required.</param>
			internal WithState(TState state, Action<TState> dispose)
			{
				lock (SyncLock) {
					this.dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
					StateValue = state;
				}
			}


			public override bool IsDisposed
			{
				get {
					lock (SyncLock) {
						return dispose == null;
					}
				}
			}

			public override event EventHandler Disposed;

			protected override void Dispose(bool isDisposing)
			{
				if (!isDisposing)
					return;
				Action<TState> action;
				TState state;
				EventHandler eventHandler;
				lock (SyncLock) {
					action = dispose;
					if (action == null)
						return;
					dispose = null;
					state = StateValue;
					StateValue = default;
					eventHandler = Disposed;
					Disposed = null;
				}
				action(state);
				eventHandler?.Invoke(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// Returns an <see cref="IDisposable"/> that does nothing.
		/// </summary>
		/// <returns>Not null.</returns>
		public static IDisposable NoOp()
			=> new NoOpDisposable();


		/// <summary>
		/// Static constructor method. Invokes your <see cref="Action"/> in
		/// <see cref="IDisposable.Dispose"/>. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="dispose">Not null.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable With(Action dispose)
			=> new NoState(dispose);

		/// <summary>
		/// Static constructor method for an instance that implements a finalizer. Invokes your
		/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/> or the finalizer. Will not
		/// invoke the delegate twice.
		/// </summary>
		/// <param name="dispose">Not null. The argument is true if invoked from Dispose;
		/// and false if invoked from the finalizer.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable With(Action<bool> dispose)
			=> new NoState.Finalizing(dispose);

		/// <summary>
		/// Static constructor method. This generic class adds a <see cref="DelegateDisposable{T}.State"/>
		/// object that you can provide. Invokes your
		/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/>. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="state">NOTICE: CAN be null.</param>
		/// <param name="dispose">Not null.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable<TState> With<TState>(TState state, Action<TState> dispose)
			=> new WithState<TState>(state, dispose);

		/// <summary>
		/// Static constructor method for an instance that implements a finalizer.This generic class adds a
		/// <see cref="DelegateDisposable{T}.State"/> object that you can provide.
		/// Invokes your <see cref="Action"/> in <see cref="IDisposable.Dispose"/>
		/// or the finalizer. Will not invoke the delegate twice.
		/// </summary>
		/// <param name="state">NOTICE: CAN be null.</param>
		/// <param name="dispose">Not null. The bool argument is true if invoked from Dispose;
		/// and false if invoked from the finalizer.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable<TState> With<TState>(TState state, Action<TState, bool> dispose)
			=> new WithState<TState>.Finalizing(state, dispose);


		/// <summary>
		/// Static constructor method will dispose a list of
		/// <see cref="IDisposable"/> instances. Will not dispose the objects twice.
		/// </summary>
		/// <param name="dispose">Not null.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable ForAll(IEnumerable<IDisposable> dispose)
		{
			if (dispose == null)
				throw new ArgumentNullException(nameof(dispose));
			return new NoState(Dispose);
			void Dispose()
			{
				// ReSharper disable once PossibleMultipleEnumeration
				foreach (IDisposable disposable in dispose) {
					disposable.Dispose();
				}
			}
		}

		/// <summary>
		/// Static constructor method. Creates a new <see cref="Task"/> that will complete when the
		/// returned object is <see cref="IDisposable.Dispose"/>. Also will cancel the task if you
		/// provide a token; and allows a Task result.
		/// </summary>
		/// <param name="task">The task that will complete when the result is disposed.</param>
		/// <param name="taskResult">Note that this is optional. This will set the result of
		/// the completed <c>task</c> when the object is disposed. If null, the result
		/// is default <typeparamref name="T"/>.</param>
		/// <param name="cancellationToken">Optional token that can cancel the returned <c>task</c>.</param>
		/// <returns>Not null.</returns>
		public static DelegateDisposable When<T>(
				out Task<T> task,
				Func<T> taskResult = default,
				CancellationToken cancellationToken = default)
		{
			TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
			task = taskCompletionSource.Task;
			if (cancellationToken.CanBeCanceled) {
				cancellationToken.Register(TokenCallback, taskCompletionSource);
				if (cancellationToken.IsCancellationRequested)
					taskCompletionSource.TrySetCanceled();
			}
			return DelegateDisposable.With(
					(taskCompletionSource, taskResult ?? (() => default), cancellationToken),
					Dispose);
			static void Dispose((TaskCompletionSource<T> tcs, Func<T> taskResult, CancellationToken token) tuple)
			{
				if (tuple.token.IsCancellationRequested)
					tuple.tcs.TrySetCanceled();
				else
					tuple.tcs.TrySetResult(tuple.taskResult());
			}
			static void TokenCallback(object tcs)
				=> ((TaskCompletionSource<T>)tcs).TrySetCanceled();
		}


		/// <summary>
		/// Monitor for state.
		/// </summary>
		protected readonly object SyncLock = new object();


		public abstract bool IsDisposed { get; }

		public abstract event EventHandler Disposed;

		/// <summary>
		/// Invoked from <see cref="Dispose"/>.;
		/// </summary>
		/// <param name="isDisposing">True if invoked from Dispose; else a finalizer.</param>
		protected abstract void Dispose(bool isDisposing);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}


	/// <summary>
	/// Simple thread-safe <see cref="IDisposable"/> class that invokes an
	/// <see cref="Action"/> in <see cref="IDisposable.Dispose"/>. Will not invoke the
	/// delegate twice. This generic class adds a <see cref="State"/> object that you
	/// can provide. Use the static
	/// <see cref="DelegateDisposable"/> methods to construct an instance.
	/// </summary>
	/// <typeparam name="TState">Your state type.</typeparam>
	public abstract class DelegateDisposable<TState>
			: DelegateDisposable
	{
		/// <summary>
		/// The <see cref="State"/>: MUST be protected by the
		/// <see cref="DelegateDisposable.SyncLock"/>.
		/// </summary>
		protected TState StateValue;


		/// <summary>
		/// Your arbitrary state.
		/// Set in the constructor; and set to default in dispose.
		/// </summary>
		public TState State
		{
			get {
				lock (SyncLock) {
					return StateValue;
				}
			}
			set {
				lock (SyncLock) {
					StateValue = value;
				}
			}
		}
	}
}
