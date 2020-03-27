using System;
using System.Net.Http;


namespace Sc.Abstractions.Net
{
	/// <summary>
	/// A factory abstraction for a component that can create <see cref="HttpClient"/>
	/// instances with configuration selected for a given logical name.
	/// </summary>
	public interface IHttpClientFactory
	{
		/// <summary>
		/// Configures and returns an <see cref="HttpClient"/> instance using the
		/// configuration that corresponds to the logical name specified by
		/// <paramref name="name"/>. Note that the name is required: by convention, you
		/// may pass a full Type name.
		/// </summary>
		/// <param name="name">Required: the logical name of the client to fetch.</param>
		/// <returns>A new or cached <see cref="HttpClient"/> instance.</returns>
		/// <remarks>
		/// <para>
		/// Each call to <see cref="GetOrCreateClient(string)"/> is not guaranteed
		/// to return a new <see cref="HttpClient"/> instance: the configured instance
		/// may be cached here according to a keep alive policy for this name.
		/// Callers may cache the returned <see cref="HttpClient"/> instance indefinitely;
		/// or, surround its use in a <see langword="using"/> block, or otherwise
		/// dispose it when desired.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"></exception>
		HttpClient GetOrCreateClient(string name);
	}
}
