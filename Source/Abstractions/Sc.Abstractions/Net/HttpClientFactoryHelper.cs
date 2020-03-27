using System;
using System.Net.Http;


namespace Sc.Abstractions.Net
{
	/// <summary>
	/// Static helpers for <see cref="IHttpClientFactory"/>.
	/// </summary>
	public static class HttpClientFactoryHelper
	{
		/// <summary>
		/// This static convenience method gets or creates an <see cref="HttpClient"/>
		/// instance using the configuration that corresponds to the full name
		/// of the type specified by <typeparamref name="T"/>. Please see
		/// <see cref="IHttpClientFactory.GetOrCreateClient"/>.
		/// </summary>
		/// <param name="httpClientFactory">Required.</param>
		/// <returns>A new or cached <see cref="HttpClient"/> instance.</returns>
		public static HttpClient GetOrCreateClient<T>(this IHttpClientFactory httpClientFactory)
		{
			if (httpClientFactory == null)
				throw new ArgumentNullException(nameof(httpClientFactory));
			return httpClientFactory.GetOrCreateClient(typeof(T).FullName);
		}
	}
}
