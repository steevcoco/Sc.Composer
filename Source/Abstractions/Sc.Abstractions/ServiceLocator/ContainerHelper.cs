using System;
using System.Linq;
using Sc.Abstractions.Internal;


namespace Sc.Abstractions.ServiceLocator
{
	/// <summary>
	/// Static helpers for <see cref="IServiceProvider"/>,
	/// <see cref="IContainerBase"/>.
	/// </summary>
	public static class ContainerHelper
	{
		/// <summary>
		/// Resolves an instance of the requested type from the container.
		/// This method allows you to pass the requested serviuce type
		/// in the generic argument; and returns the given type.
		/// </summary>
		/// <typeparam name="TService"><see cref="Type"/> of the service to get from the
		/// container.</typeparam>
		/// <param name="serviceProvider">Required.</param>
		/// <returns>The retrieved service; or null if there is none.</returns>
		public static TService GetService<TService>(this IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			object result = serviceProvider.GetService(typeof(TService));
			return result is TService service
					? service
					: default;
		}

		/// <summary>
		/// Tries to resolve an instance of the requested type from the container.
		/// </summary>
		/// <param name="serviceType"><see cref="Type"/> of the service to get from the
		/// container.</param>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="service">The result if the method returns true.</param>
		/// <returns>True if the <paramref name="serviceProvider"/> retrieved the service.</returns>
		public static bool TryGetService(this IServiceProvider serviceProvider, Type serviceType, out object service)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			service = serviceProvider.GetService(serviceType);
			return serviceType.IsInstanceOfType(service);
		}

		/// <summary>
		/// Tries to resolve an instance of the requested type from the container.
		/// This method allows you to pass the requested serviuce type
		/// in the generic argument; and returns the given type.
		/// </summary>
		/// <typeparam name="TService"><see cref="Type"/> of the service to get from the
		/// container.</typeparam>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="service">The result if the method returns true.</param>
		/// <returns>True if the <paramref name="serviceProvider"/> retrieved the service.</returns>
		public static bool TryGetService<TService>(this IServiceProvider serviceProvider, out TService service)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (serviceProvider.GetService(typeof(TService)) is TService result) {
				service = result;
				return true;
			}
			service = default;
			return false;
		}


		/// <summary>
		/// Registers a type mapping with the container.
		/// </summary>
		/// <typeparam name="TService">The exposed type.</typeparam>
		/// <typeparam name="TImplementation">The concrete type.</typeparam>
		/// <param name="container">Required.</param>
		/// <param name="isSingleton">The registration mode: if true the type is registered as a
		/// singleton instance.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void RegisterType<TService, TImplementation>(
				this IContainerBase container,
				bool isSingleton = true)
				where TImplementation : TService
		{
			if (container == null)
				throw new ArgumentNullException(nameof(container));
			container.RegisterType(typeof(TService), typeof(TImplementation), isSingleton);
		}

		/// <summary>
		/// Registers a type mapping with the container as a singleton.
		/// </summary>>
		/// <typeparam name="TService">The exposed service type.</typeparam>
		/// <param name="container">Required.</param>
		/// <param name="service">The registered object.</param>
		/// <param name="disposeSingleton">This is an optional property for the container:
		/// since the mapping is for a singleton, this specifies if the container will
		/// dispose the singleton instance with other registrations. If null, the
		/// container's default policy is used --- note that this registration differs
		/// in that the container does not explicitly construct the service.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If the <c>service</c>
		/// does not extend the <c>serviceType</c>.</exception>
		public static void RegisterInstance<TService>(
				this IContainerBase container,
				TService service,
				bool? disposeSingleton = null)
		{
			if (container == null)
				throw new ArgumentNullException(nameof(container));
			if (service == null)
				throw new ArgumentNullException(nameof(service));
			container.RegisterInstance(typeof(TService), service, disposeSingleton);
		}


		/// <summary>
		/// Static helper method will check if this <paramref name="container"/>
		/// contains the registered <paramref name="serviceType"/> service type.
		/// </summary>
		/// <param name="container">Not null.</param>
		/// <param name="serviceType">Not null.</param>
		/// <returns>True if returned in <see cref="IContainerBase.GetRegisteredServiceTypes"/>.</returns>
		public static bool IsRegistered(this IContainerBase container, Type serviceType)
		{
			if (container == null)
				throw new ArgumentNullException(nameof(container));
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			return container.GetRegisteredServiceTypes().Contains(serviceType);
		}

		/// <summary>
		/// Static helper method will check if this <paramref name="container"/>
		/// contains the registered <typeparamref name="TService"/> service type.
		/// </summary>
		/// <param name="container">Not null.</param>
		/// <returns>True if returned in <see cref="IContainerBase.GetRegisteredServiceTypes"/>.</returns>
		public static bool IsRegistered<TService>(this IContainerBase container)
			=> container.IsRegistered(typeof(TService));


		/// <summary>
		/// Tries to register a type mapping with the container, only if one does not
		/// already exist. NOTICE that this implementation is not thread safe:
		/// the operation here is performed with two operations on the container.
		/// </summary>
		/// <param name="container">Required.</param>
		/// <param name="serviceType">The exposed type.</param>
		/// <param name="implementationType">The concrete type.</param>
		/// <param name="isSingleton">The registration mode: if true the type is registered as a
		/// singleton instance.</param>
		/// <returns>True if this registration was added; false if one is
		/// already present.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryRegisterType(
				this IContainerBase container,
				Type serviceType,
				Type implementationType,
				bool isSingleton = true)
		{
			if (container == null)
				throw new ArgumentNullException(nameof(container));
			if (!serviceType.IsAssignableFrom(implementationType)) {
				throw new ArgumentException(
						$"Implementation type '{implementationType.GetFriendlyFullName()}'"
						+ $" is not assignable to service type '{serviceType.GetFriendlyFullName()}'",
						nameof(implementationType));
			}
			if (container.IsRegistered(serviceType))
				return false;
			container.RegisterType(serviceType, implementationType, isSingleton);
			return true;
		}

		/// <summary>
		/// Tries to register a type mapping with the container, only if one does not
		/// already exist. NOTICE that this implementation is not thread safe:
		/// the operation here is performed with two operations on the container.
		/// </summary>
		/// <typeparam name="TService">The exposed type.</typeparam>
		/// <typeparam name="TImplementation">The concrete type.</typeparam>
		/// <param name="container">Required.</param>
		/// <param name="isSingleton">The registration mode: if true the type is registered as a
		/// singleton instance.</param>
		/// <returns>True if this registration was added; false if one is
		/// already present.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static void TryRegisterType<TService, TImplementation>(
				this IContainerBase container,
				bool isSingleton = true)
				where TImplementation : TService
			=> container.TryRegisterType(
					typeof(TService),
					typeof(TImplementation),
					isSingleton);

		/// <summary>
		/// Tries to register a type mapping with the container, only if one does not
		/// already exist. NOTICE that this implementation is not thread safe:
		/// the operation here is performed with two operations on the container.
		/// </summary>
		/// <typeparam name="TService">The exposed type.</typeparam>
		/// <typeparam name="TImplementation">The concrete type.</typeparam>
		/// <param name="container">Required.</param>>
		/// <param name="factory">The factory used to construct the object when resolved.</param>
		/// <param name="isSingleton">The registration mode: if true the type is registered as a
		/// singleton; otherwise as a transient object.</param>
		/// <param name="disposeIfSingleton">This is an optional property for the container:
		/// if the mapping is for a singleton, this specifies if the container will
		/// dispose the singleton instance with other registrations. If null, the
		/// container's default policy is used --- note that this registration differs
		/// in that the container will not explicitly construct the service since
		/// your factory does.</param>
		/// <returns>True if this registration was added; false if one is
		/// already present.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryRegisterType<TService, TImplementation>(
				this IContainerBase container,
				Func<IServiceProvider, TImplementation> factory,
				bool isSingleton = true,
				bool? disposeIfSingleton = null)
				where TImplementation : TService
		{
			if (container == null)
				throw new ArgumentNullException(nameof(container));
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			if (container.IsRegistered(typeof(TService)))
				return false;
			container.RegisterType<TService, TImplementation>(factory, isSingleton, disposeIfSingleton);
			return true;
		}

		/// <summary>
		/// Tries to register a type mapping with the container as a singleton,
		/// only if one does not already exist. NOTICE that this implementation is not thread safe:
		/// the operation here is performed with two operations on the container.
		/// </summary>
		/// <param name="container">Required.</param>
		/// <param name="serviceType">The exposed type.</param>
		/// <param name="service">The registered object.</param>
		/// <param name="disposeSingleton">This is an optional property for the container:
		/// since the mapping is for a singleton, this specifies if the container will
		/// dispose the singleton instance with other registrations. If null, the
		/// container's default policy is used --- note that this registration differs
		/// in that the container does not explicitly construct the service.</param>
		/// <returns>True if this registration was added; false if one is
		/// already present.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryRegisterInstance(
				this IContainerBase container,
				Type serviceType,
				object service,
				bool? disposeSingleton = null)
		{
			if (container == null)
				throw new ArgumentNullException(nameof(container));
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			if (service == null)
				throw new ArgumentNullException(nameof(service));
			if (!serviceType.IsInstanceOfType(service)) {
				throw new ArgumentException(
						$"Service instance '{service.GetType().FullName}'"
						+ $" is not assignable to service type '{serviceType.FullName}'",
						nameof(service));
			}
			if (container.IsRegistered(serviceType))
				return false;
			container.RegisterInstance(serviceType, service, disposeSingleton);
			return true;
		}

		/// <summary>
		/// Tries to register a type mapping with the container as a singleton,
		/// only if one does not already exist. NOTICE that this implementation is not thread safe:
		/// the operation here is performed with two operations on the container.
		/// </summary>
		/// <typeparam name="TService">The exposed service type.</typeparam>
		/// <param name="container">Required.</param>
		/// <param name="service">The registered object.</param>
		/// <param name="disposeSingleton">This is an optional property for the container:
		/// since the mapping is for a singleton, this specifies if the container will
		/// dispose the singleton instance with other registrations. If null, the
		/// container's default policy is used --- note that this registration differs
		/// in that the container does not explicitly construct the service.</param>
		/// <returns>True if this registration was added; false if one is
		/// already present.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryRegisterInstance<TService>(
				this IContainerBase container,
				TService service,
				bool? disposeSingleton = null)
		{
			if (container == null)
				throw new ArgumentNullException(nameof(container));
			if (service == null)
				throw new ArgumentNullException(nameof(service));
			if (container.IsRegistered(typeof(TService)))
				return false;
			container.RegisterInstance(typeof(TService), service, disposeSingleton);
			return true;
		}


		/// <summary>
		/// Registers an Export type for the Import type.
		/// </summary>
		/// <typeparam name="TImport">The registered Import type.</typeparam>
		/// <typeparam name="TExport">The registered Import type.</typeparam>
		/// <exception cref="InvalidOperationException">If the registration
		/// already exists.</exception>
		public static void RegisterExport<TImport, TExport>(this IExportRegistry<TExport> exportRegistry)
		{
			if (exportRegistry == null)
				throw new ArgumentNullException(nameof(exportRegistry));
			exportRegistry.RegisterExport(typeof(TImport), typeof(TExport));
		}

		/// <summary>
		/// Static helper method tries to get the Export for your
		/// requested <typeparamref name="TImport"/> type.
		/// </summary>
		/// <typeparam name="TImport">The registered Import type.
		/// Notice that this must be the actual registered Import Type.</typeparam>
		/// <param name="exportFactory">Not null.</param>
		/// <param name="import">As with <see cref="IExportFactory"/>.</param>
		/// <param name="instanceProvider">As with <see cref="IExportFactory"/>.</param>
		/// <returns>The Export: as with <see cref="IExportFactory"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TExport GetExport<TImport, TExport>(
				this IExportFactory<TExport> exportFactory,
				out TImport import,
				Func<Type, object> instanceProvider = null)
		{
			if (exportFactory == null)
				throw new ArgumentNullException(nameof(exportFactory));
			try {
				TExport export = exportFactory.GetExport(typeof(TImport), out object importObject, instanceProvider);
				import = importObject is TImport tImport
						? tImport
						: default;
				return export;
			} catch {
				import = default;
				return default;
			}
		}

		/// <summary>
		/// This method returns the Export registration
		/// that was made for this Import type.
		/// </summary>
		/// <param name="importType">Must exactly match the registration.</param>
		/// <param name="exportType">The located Export type.</param>
		/// <returns>False if not found exactly.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryFindExportType<TExport>(
				this IExportRegistry<TExport> exportRegistry,
				Type importType,
				out Type exportType)
		{
			if (exportRegistry == null)
				throw new ArgumentNullException(nameof(exportRegistry));
			if (importType == null)
				throw new ArgumentNullException(nameof(importType));
			exportType = exportRegistry.GetAllRegistrations()
					.FirstOrDefault(kv => kv.Key == importType)
					.Value;
			return exportType != null;
		}

		/// <summary>
		/// This method tries to find the IMPORT registration Type
		/// that was made for this EXPORT type.
		/// </summary>
		/// <param name="exportType">Must exactly match the registration.</param>
		/// <param name="importType">The located IMPORT type.</param>
		/// <returns>False if not found exactly.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryFindImportType<TExport>(
				this IExportRegistry<TExport> exportRegistry,
				Type exportType,
				out Type importType)
		{
			if (exportRegistry == null)
				throw new ArgumentNullException(nameof(exportRegistry));
			if (exportType == null)
				throw new ArgumentNullException(nameof(exportType));
			importType = exportRegistry.GetAllRegistrations()
					.FirstOrDefault(kv => kv.Value == exportType)
					.Key;
			return importType != null;
		}
	}
}
