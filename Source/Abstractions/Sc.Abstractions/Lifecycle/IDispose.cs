using System;


namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines an <see cref="IDisposable"/> object that is <see cref="IRaiseDisposed"/>.
	/// </summary>
	public interface IDispose
			: IDisposable,
					IRaiseDisposed { }
}
