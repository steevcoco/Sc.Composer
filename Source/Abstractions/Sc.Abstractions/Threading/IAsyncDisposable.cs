using System;
using System.Threading;


namespace Sc.Abstractions.Threading
{
	/// <summary>
	/// Provides an async disposal method for an object that is disposable and
	/// is managing some async resources that can be awaited before disposal.
	/// </summary>
	public interface IAsyncDisposable
	{
		/// <summary>
		/// This method provides a "safe" way to dispose this instance. This method
		/// will mark this object as disposed now; and this will check if any async
		/// actions are currently running: if so, then your callback will be
		/// cached, and it will be invoked when the actions return. Otherwise
		/// your callback will be invoked here now. The current
		/// <see cref="SynchronizationContext"/> will be captured, and if not null,
		/// your callback will be Posted back on that context; and otherwise it is
		/// invoked synchronously on the async Thread. The argument to your
		/// callback will be TRUE if it is invoked NOW; and if cached and invoked
		/// asynchronously, the argument will be false when invoked.
		/// </summary>
		/// <param name="onDisposed">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		void Dispose(Action<bool> onDisposed);
	}
}
