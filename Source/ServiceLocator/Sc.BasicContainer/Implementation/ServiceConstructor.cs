using System;
using System.Collections.Generic;
using System.Linq;
using Sc.Abstractions.ServiceLocator;
using Sc.Diagnostics;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.BasicContainer.Implementation
{
	/// <summary>
	/// Provides an implementation to construct service objects by type.
	/// You invoke this object to construct an object on demand; and this
	/// implementation will optionally perform constructor argument injection
	/// from an <see cref="IServiceProvider"/>. This class also implements
	/// protected methods that can be overridden to implement service caching.
	/// NOTICE that this object is intended for
	/// <see cref="IServiceProvider"/>-level implementation, or in a scope
	/// where the lifetime of the container is known: any resolved
	/// dependencies are managed by the container, and are subject to
	/// the lifetime policies there: they will be disposed according to
	/// the container and may not have lifetime affinity with the constructed
	/// instances (this also applies recursively if arguments are
	/// constructed).
	/// </summary>
	public class ServiceConstructor
	{
		/// <summary>
		/// This object is used only to synchronize all operations here;
		/// and is held while invoking the protected caching methods.
		/// </summary>
		protected readonly object SyncLock = new object();

		private bool allowConstructingArguments;
		private bool? requireConstructorAttributes;
		private Type[] constructorAttributeTypes;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serviceProvider">Optional.</param>
		public ServiceConstructor(IServiceProvider serviceProvider = null)
			=> ServiceProvider = serviceProvider;


		/// <summary>
		/// The OPTIONAL <see cref="IServiceProvider"/> used to resolve or
		/// construct services and constructor arguments.
		/// </summary>
		protected IServiceProvider ServiceProvider { get; }


		/// <summary>
		/// This protected virtual method is provided to implement any caching here.
		/// This will be invoked from <see cref="TryResolveOrConstruct"/> for each request:
		/// if there is an existing instance for this service it must be returned here.
		/// This implementation always returns false. This will be invoked while
		/// the <see cref="SyncLock"/> is held.
		/// </summary>
		/// <param name="serviceType">The public argument.</param>
		/// <param name="service">The public result.</param>
		/// <returns>True if the result is set.</returns>
		protected virtual bool TryGetExistingService(Type serviceType, out object service)
		{
			service = default;
			return false;
		}

		/// <summary>
		/// This protected virtual method is provided to implement any caching here.
		/// This will be invoked with each newly-constructed service
		/// from <see cref="TryConstruct"/>, and the <paramref name="wasResolved"/>
		/// argument is false then. THis will also be invoked when services are
		/// resolved from <see cref="TryResolve"/>; and <paramref name="wasResolved"/>
		/// will be true. This implementation is empty.
		/// This will be invoked while the <see cref="SyncLock"/> is held.
		/// </summary>
		/// <param name="serviceType">The public argument.</param>
		/// <param name="service">The newly-constructed service.</param>
		/// <param name="wasResolved">True if this is invoked from <see cref="TryResolve"/>;
		/// and this service has been resolved from the service provider, or instance
		/// provider now. False if this is invoked from <see cref="TryConstruct"/>;
		/// and the service has been constructed now.</param>
		protected virtual void HandleNewService(Type serviceType, object service, bool wasResolved) { }


		/// <summary>
		/// This defaults to false: when an instance is constructed with constructor
		/// argument injection,
		/// the service's constructor arguments must resolve and will not be
		/// constructed here on demand. If this is set true, then arguments WILL
		/// be constructed here on demand if they do not resolve.
		/// </summary>
		public bool AllowConstructingArguments
		{
			get {
				lock (SyncLock) {
					return allowConstructingArguments;
				}
			}
			set {
				lock (SyncLock) {
					allowConstructingArguments = value;
				}
			}
		}

		/// <summary>
		/// Defaults to null.
		/// Selects whether this implementation will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <see cref="ConstructorAttributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments are tried first. If true, ONLY constructors
		/// with attributes are selected; and again sorted by argument length.
		/// </summary>
		public bool? RequireConstructorAttributes
		{
			get {
				lock (SyncLock) {
					return requireConstructorAttributes;
				}
			}
			set {
				lock (SyncLock) {
					requireConstructorAttributes = value;
				}
			}
		}

		/// <summary>
		/// Can provide optional or required attributes when selecting constructors
		/// for service construction. See <see cref="RequireConstructorAttributes"/>.
		/// Can be null. NOTE: if this argument is null or empty, then if
		/// <see cref="RequireConstructorAttributes"/>
		/// is not false, this WILL ALWAYS ADD the
		/// <see cref="ServiceProviderConstructorAttribute"/>.
		/// </summary>
		public IReadOnlyCollection<Type> ConstructorAttributeTypes
		{
			get {
				lock (SyncLock) {
					return constructorAttributeTypes;
				}
			}
			set {
				lock (SyncLock) {
					constructorAttributeTypes = value?.ToArray();
				}
			}
		}


		/// <summary>
		/// Resolves or constructs the <paramref name="serviceType"/>.
		/// This first tries <see cref="TryResolve"/>; and then tries
		/// <see cref="TryConstruct"/>.
		/// </summary>
		/// <param name="serviceType">Required concrete type to resolve or construct now.</param>
		/// <param name="service">The result if the method returns true.</param>
		/// <param name="instanceProvider">Optional: can provide the instance now,
		/// from <see cref="TryResolve"/>; or can provide constructor arguments in
		/// <see cref="TryConstruct"/>.</param>
		/// <returns>False if the service can't be resolved nor constructed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual bool TryResolveOrConstruct(
				Type serviceType,
				out object service,
				Func<Type, object> instanceProvider = null)
		{
			lock (SyncLock) {
				return TryResolve(serviceType, out service, instanceProvider)
						|| TryConstruct(serviceType, out service, instanceProvider);
			}
		}

		/// <summary>
		/// This method constructs each new service. NOTICE that this method will
		/// NOT resolve the service from either the <see cref="IServiceProvider"/>
		/// nor the <paramref name="instanceProvider"/>: this method always constructs
		/// this service now; YET also, it this implementation supports service caching,
		/// a previously-constructed instance may be returned:.
		/// This implementation tries to construct the service as follows.
		/// First this tries to construct an instance with the <see cref="IServiceProvider"/>
		/// as a constructor injector by invoking <see cref="ServiceProviderHelper"/>
		/// <see cref="ServiceProviderHelper.TryConstruct"/> --- and will include
		/// the <paramref name="instanceProvider"/> if not null, which can provide
		/// constructor arguments.
		/// Next, this will perform constructor injection with the
		/// <paramref name="instanceProvider"/> alone if given.
		/// Lastly, this tries to construct with <see cref="Activator"/> only.
		/// This implementation ALSO supports default
		/// argument values.
		/// </summary>
		/// <param name="serviceType">The concrete type to construct.</param>
		/// <param name="service">The newly-constructed service if the method returns true.</param>
		/// <param name="instanceProvider">Optional: can provide constructor arguments
		/// for the constructed instance now; and is checked first.</param>
		/// <returns>False if the service cannot be constructed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual bool TryConstruct(
				Type serviceType,
				out object service,
				Func<Type, object> instanceProvider = null)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			lock (SyncLock) {
				service = default;
				if (ServiceProvider?.TryConstruct(
								serviceType,
								out service,
								TraceSources.For<ServiceConstructor>(),
								instanceProvider,
								allowConstructingArguments,
								requireConstructorAttributes,
								constructorAttributeTypes)
						?? false) {
					HandleNewService(serviceType, service, false);
					return true;
				}
				if (instanceProvider?.AsServiceProvider()
								.TryConstruct(
										serviceType,
										out service,
										TraceSources.For<ServiceConstructor>(),
										null,
										allowConstructingArguments,
										requireConstructorAttributes,
										constructorAttributeTypes)
						?? false) {
					HandleNewService(serviceType, service, false);
					return true;
				}
				if (allowConstructingArguments) {
					if (ServiceProviderHelper.AsServiceProvider(EmptyProvider)
							.TryConstruct(
									serviceType,
									out service,
									TraceSources.For<ServiceConstructor>(),
									null,
									true,
									requireConstructorAttributes,
									constructorAttributeTypes)) {
						HandleNewService(serviceType, service, false);
						return true;
					}
				}
				try {
					service = Activator.CreateInstance(serviceType, true);
					if (serviceType.IsInstanceOfType(service)) {
						HandleNewService(serviceType, service, false);
						return true;
					}
					(service as IDisposable)?.Dispose();
					service = null;
					return false;
				} catch {
					TraceSources.For<ServiceConstructor>()
							.Warning("Failing to construct requested service: {0}", serviceType);
					return false;
				}

				static object EmptyProvider(Type _)
					=> null;
			}
		}

		/// <summary>
		/// Resolves the <paramref name="serviceType"/>: it will be attempted to be
		/// resolved from the <paramref name="instanceProvider"/>
		/// or the <see cref="IServiceProvider"/>
		/// only --- if the service doesn't resolve, it
		/// will NOT be constructed here.
		/// </summary>
		/// <param name="serviceType">Required concrete type to resolve.</param>
		/// <param name="service">The result if the method returns true.</param>
		/// <param name="instanceProvider">Optional: can provide the instance now;
		/// and is checked first.</param>
		/// <returns>False if the service can't be resolved.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual bool TryResolve(Type serviceType, out object service, Func<Type, object> instanceProvider = null)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			lock (SyncLock) {
				if (TryGetExistingService(serviceType, out service))
					return true;
				service = instanceProvider?.Invoke(serviceType);
				if (serviceType.IsInstanceOfType(service)) {
					HandleNewService(serviceType, service, true);
					return true;
				}
				service = ServiceProvider?.GetService(serviceType);
				if (!serviceType.IsInstanceOfType(service))
					return false;
				HandleNewService(serviceType, service, true);
				return true;
			}
		}

		/// <summary>
		/// This method provides an implementation that will fully
		/// resolve and optionally construct services
		/// to be injected into a method on the <paramref name="target"/>
		/// object that is marked with
		/// the specified <paramref name="attributeType"/>.
		/// The method must be an instance
		/// method, and can be any visibility.
		/// This implementation will attempt all methods with the attribute, from
		/// the one with the most arguments first; and will stop with the first
		/// successful method. This implementation ALSO supports default
		/// argument values.
		/// </summary>
		/// <param name="target">Is the target object to locate the method on.</param>
		/// <param name="attributeType">Specifies an Attribute type that must be
		/// present on a method.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for method or constructor argument values.</param>
		/// <returns>True for success.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual bool TryInject(
				object target,
				Type attributeType,
				Func<Type, object> instanceProvider = null)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (attributeType == null)
				throw new ArgumentNullException(nameof(attributeType));
			lock (SyncLock) {
				if (ServiceProvider != null) {
					if (ServiceProvider.TryInject(
							target,
							attributeType,
							TraceSources.For<ServiceConstructor>(),
							instanceProvider,
							allowConstructingArguments,
							requireConstructorAttributes,
							constructorAttributeTypes)) {
						return true;
					}
				}
				if (instanceProvider != null) {
					if (instanceProvider.AsServiceProvider()
							.TryInject(
									target,
									attributeType,
									TraceSources.For<ServiceConstructor>(),
									instanceProvider,
									allowConstructingArguments,
									requireConstructorAttributes,
									constructorAttributeTypes)) {
						return true;
					}
				}
				return allowConstructingArguments
					? ServiceProviderHelper.AsServiceProvider(EmptyProvider)
							.TryInject(
									target,
									attributeType,
									TraceSources.For<ServiceConstructor>(),
									null,
									true,
									requireConstructorAttributes,
									constructorAttributeTypes)
					: false;
			}
			static object EmptyProvider(Type _)
				=> null;
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $", {nameof(ServiceConstructor.ServiceProvider)}: {ServiceProvider}"
					+ $", {nameof(ServiceConstructor.AllowConstructingArguments)}: {AllowConstructingArguments}"
					+ $", {nameof(ServiceConstructor.RequireConstructorAttributes)}: {RequireConstructorAttributes}"
					+ $", {nameof(ServiceConstructor.ConstructorAttributeTypes)}{ConstructorAttributeTypes?.ToStringCollection()}"
					+ "]";
	}
}
