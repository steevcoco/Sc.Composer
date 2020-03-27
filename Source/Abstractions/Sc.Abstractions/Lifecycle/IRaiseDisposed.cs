using System;


namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines an object that reports <see cref="IsDisposed"/>, and
	/// raises <see cref="Disposed"/>.
	/// </summary>
	public interface IRaiseDisposed
	{
		/// <summary>
		/// Becomes true when the object is disposed.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// This event is raised after the instance is disposed.
		/// </summary>
		event EventHandler Disposed;
	}
}
