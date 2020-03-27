using System;


namespace Sc.Abstractions.ServiceLocator
{
	/// <summary>
	/// Provides a collection for scoped services.
	/// Service instances are added under two keys:
	/// each instances is first added under a specific Scope <see cref="Type"/>;
	/// and then under each unique Scope Type, each service is added
	/// under a given Scope object key (of that Type).
	/// Service instances can then be fetched by passing the
	/// Scope object key.
	/// </summary>
	public interface IServiceScopeManager
	{
		/// <summary>
		/// Tries to locate the added <paramref name="service"/> for
		/// the given <paramref name="scope"/> key; which is resolved both by the
		/// specific given <typeparamref name="TScope"/> Type, and the
		/// <paramref name="scope"/> value. The requested <paramref name="service"/>
		/// is only located if it has been added for the specific
		/// given <typeparamref name="TService"/> Type and the specific
		/// <paramref name="scope"/> instance.
		/// </summary>
		/// <typeparam name="TScope">The scope key type.</typeparam>
		/// <typeparam name="TService">The service type.</typeparam>
		/// <param name="scope">Required scope key.</param>
		/// <param name="service">The result.</param>
		/// <returns>True if located.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		bool TryGet<TScope, TService>(TScope scope, out TService service);

		/// <summary>
		/// Tries to first locate the added <paramref name="service"/>
		/// for the given <paramref name="scope"/> and
		/// <typeparamref name="TScope"/> type; and if none is found, 
		/// invokes your <paramref name="serviceFactory"/>, adds the
		/// service now; and returns it. The service
		/// is resolved both by the
		/// specific given <typeparamref name="TScope"/> Type, and the
		/// <paramref name="scope"/> value. The requested <paramref name="service"/>
		/// is only located if it has been added for the specific
		/// given <typeparamref name="TService"/> Type and the
		/// <paramref name="scope"/> instance. This method returns an
		/// <see cref="IDisposable"/> object that will remove the
		/// service when disposed. Please notice that first,
		/// this manager will not dispose the service itself; and, if you
		/// are not the only invoker that may fetch this service,
		/// then if the service is disposed, any clients that MAY have
		/// fetched the service must be able to expect that at that time.
		/// </summary>
		/// <typeparam name="TScope">The scope key type.</typeparam>
		/// <typeparam name="TService">The service type.</typeparam>
		/// <param name="scope">Required.</param>
		/// <param name="service">The result.</param>
		/// <param name="wasAdded">Set true if this service is added now.</param>
		/// <param name="serviceFactory">Required factory will construct
		/// or return the service now if not found.</param>
		/// <returns>An <see cref="IDisposable"/> object that
		/// will remove the service when disposed. The returned object
		/// will only run one time to remove the service. Note again that this
		/// does not dispose the service nor the scope --- the IDisposable
		/// will remove the service.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If the <paramref name="serviceFactory"/>
		/// fails.</exception>
		IDisposable GetOrAdd<TScope, TService>(
				TScope scope,
				out TService service,
				out bool wasAdded,
				Func<TScope, TService> serviceFactory);
	}
}
