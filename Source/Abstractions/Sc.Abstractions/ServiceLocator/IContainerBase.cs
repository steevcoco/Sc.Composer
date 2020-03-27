using System;
using System.Collections.Generic;


namespace Sc.Abstractions.ServiceLocator
{
	/// <summary>
	/// Basic service container interface. Extends <see cref="IServiceProvider"/> to
	/// add registration methods.
	/// </summary>
	public interface IContainerBase
			: IServiceProvider
	{
		/// <summary>
		/// Registers a type mapping with the container.
		/// The container constructs the service and injects constructor dependencies.
		/// </summary>
		/// <param name="serviceType">The exposed service type.</param>
		/// <param name="implementationType">The concrete type.</param>
		/// <param name="isSingleton">The registration mode: if true the type is registered as a
		/// singleton instance.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If the <c>implementationType</c>
		/// does not extend the <c>serviceType</c>.</exception>
		void RegisterType(Type serviceType, Type implementationType, bool isSingleton = true);

		/// <summary>
		/// Register a type mapping with the container.
		/// Your <paramref name="factory"/> constructs the service, and the
		/// <see cref="IServiceProvider"/> is provided to resolve dependencies.
		/// </summary>
		/// <typeparam name="TService">The exposed service type.</typeparam>
		/// <typeparam name="TImplementation">The concrete type.</typeparam>
		/// <param name="factory">The factory required to construct the object when resolved.</param>
		/// <param name="isSingleton">The registration mode: if true the type is registered as a
		/// singleton; otherwise as a transient object.</param>
		/// <param name="disposeIfSingleton">This is an optional property for the container:
		/// if the mapping is for a singleton, this specifies if the container will
		/// dispose the singleton instance with other registrations. If null, the
		/// container's default policy is used --- note that this registration differs
		/// in that the container will not explicitly construct the service since
		/// your factory does.</param>
		/// <exception cref="ArgumentNullException"/>
		void RegisterType<TService, TImplementation>(
				Func<IServiceProvider, TImplementation> factory,
				bool isSingleton = true,
				bool? disposeIfSingleton = null)
				where TImplementation : TService;

		/// <summary>
		/// Registers a type mapping with the container as a singleton.
		/// </summary>>
		/// <param name="serviceType">The exposed service type.</param>
		/// <param name="service">The registered object.</param>
		/// <param name="disposeSingleton">This is an optional property for the container:
		/// since the mapping is for a singleton, this specifies if the container will
		/// dispose the singleton instance with other registrations. If null, the
		/// container's default policy is used --- note that this registration differs
		/// in that the container does not explicitly construct the service.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If the <c>service</c>
		/// does not extend the <c>serviceType</c>.</exception>
		void RegisterInstance(Type serviceType, object service, bool? disposeSingleton = null);

		/// <summary>
		/// Yields an enumeration of all registered service types
		/// </summary>
		/// <returns>Not null.</returns>
		IEnumerable<Type> GetRegisteredServiceTypes();
	}
}
