using System;
using System.Collections.Generic;
using System.Linq;
using Sc.Abstractions.ServiceLocator;
using Sc.BasicContainer.Implementation;
using Sc.BasicContainer.Specialized;
using Sc.Diagnostics;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.BasicContainer
{
	/// <summary>
	/// Simple <see cref="IContainerBase"/> implementation that uses
	/// the <see cref="ServiceConstructorMethods"/> implementation methods.
	/// </summary>
	public sealed class BasicContainer
			: IContainerBase,
					IServiceRegistrationProvider,
					IDisposable
	{
		private readonly Dictionary<Type, ServiceRegistration> registrations
				= new Dictionary<Type, ServiceRegistration>();

		private IServiceProvider parentServiceProvider;
		private IServiceRegistrationProvider parentServiceRegistrationProvider;

		private bool throwOnDuplicateRegistrations = true;
		private bool disposeRegistrationsWithContainer = true;
		private bool? requireConstructorAttributes;
		private IReadOnlyCollection<Type> constructorAttributeTypes;


		/// <summary>
		/// Constructor.
		/// </summary>
		public BasicContainer()
			=> AsReadOnly = new ReadOnlyServiceProvider(this);


		private void addRegistration(
				ServiceRegistration serviceRegistration,
				bool isTryRegisterType = false,
				bool? isThrowOnDuplicateRegistrations = null,
				bool? disposeExistingInstance = null)
		{
			lock (registrations) {
				if (registrations.TryGetValue(
						serviceRegistration.ServiceType,
						out ServiceRegistration existingRegistration)) {
					if (isTryRegisterType)
						return;
					if (isThrowOnDuplicateRegistrations is true
							|| ((isThrowOnDuplicateRegistrations == null)
									&& ThrowOnDuplicateRegistrations)) {
						throw new InvalidOperationException(
								$"Duplicate registration is not allowed:"
								+ $" {serviceRegistration.ServiceType}.");
					}
					registrations.Remove(serviceRegistration.ServiceType);
					if (disposeExistingInstance is true
							|| ((disposeExistingInstance == null)
									&& DisposeRegistrationsWithContainer))
						existingRegistration?.Dispose();
				}
				registrations[serviceRegistration.ServiceType] = serviceRegistration;
			}
		}

		private ServiceRegistration newServiceRegistration(
				Type serviceType,
				Type implementationType,
				bool isSingleton,
				Func<IServiceProvider, object> factory = null,
				bool? isImplementationContainerCreated = null)
			=> new ServiceRegistration(
					this,
					serviceType,
					implementationType,
					isSingleton,
					factory,
					isImplementationContainerCreated)
			{
				RequireConstructorAttributes = RequireConstructorAttributes,
				ConstructorAttributeTypes = ConstructorAttributeTypes,
			};

		private ServiceConstructorRequest newServiceConstructorRequest()
			=> new ServiceConstructorRequest(TraceSources.For<BasicContainer>());


		/// <summary>
		/// Provides a singleton read only <see cref="IServiceProvider"/> wrapper for
		/// this instance.
		/// </summary>
		/// <returns></returns>
		public IServiceProvider AsReadOnly { get; }

		/// <summary>
		/// Defaults to TRUE.
		/// Defines whether the container will throw an exception on an
		/// attempt to register the same service type twice. If this is
		/// set FALSE, then a new registration will REPLACE the existing,
		/// and the existing will or will not be disposed according to
		/// <see cref="DisposeRegistrationsWithContainer"/>.
		/// </summary>
		public bool ThrowOnDuplicateRegistrations
		{
			get {
				lock (registrations) {
					return throwOnDuplicateRegistrations;
				}
			}
			set {
				lock (registrations) {
					throwOnDuplicateRegistrations = value;
				}
			}
		}

		/// <summary>
		/// Defaults to TRUE.
		/// Defines whether the container will dispose singleton
		/// implementations when the container is disposed.
		/// </summary>
		public bool DisposeRegistrationsWithContainer
		{
			get {
				lock (registrations) {
					return disposeRegistrationsWithContainer;
				}
			}
			set {
				lock (registrations) {
					disposeRegistrationsWithContainer = value;
				}
			}
		}

		/// <summary>
		/// Selects whether the service constructors will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <see cref="ConstructorAttributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments are tried first. If true, only constructors
		/// with attributes are selected; and again sorted by argument length.
		/// </summary>
		public bool? RequireConstructorAttributes
		{
			get {
				lock (registrations) {
					return requireConstructorAttributes;
				}
			}
			set {
				lock (registrations) {
					requireConstructorAttributes = value;
				}
			}
		}

		/// <summary>
		/// Optional or required attributes for the selected constructors. This
		/// property applies if <see cref="RequireConstructorAttributes"/> is
		/// not false. This specifies the required OR optional constructor
		/// attributes that will be considered. PLEASE NOTICE: if this list
		/// is null or empty, and if <see cref="RequireConstructorAttributes"/>
		/// is not false, then on construction, this WILL ALWAYS ADD the
		/// <see cref="ServiceProviderConstructorAttribute"/> as a selection.
		/// </summary>
		public IReadOnlyCollection<Type> ConstructorAttributeTypes
		{
			get {
				lock (registrations) {
					return constructorAttributeTypes;
				}
			}
			set {
				lock (registrations) {
					constructorAttributeTypes = value?.ToArray();
				}
			}
		}

		/// <summary>
		/// If this instance has been scoped --- in <see cref="Scope"/>
		/// --- then this will return the parent <see cref="IServiceProvider"/>.
		/// </summary>
		/// <returns>May be null.</returns>
		public IServiceProvider TryGetParentServiceProvider()
		{
			lock (registrations) {
				return parentServiceProvider;
			}
		}


		IServiceProvider IServiceRegistrationProvider.ParentServiceProvider
		{
			get {
				lock (registrations) {
					return parentServiceProvider;
				}
			}
		}

		/// <summary>
		/// Sets the given <paramref name="parent"/> as this instance's
		/// <see cref="IServiceRegistrationProvider.ParentServiceProvider"/>.
		/// Requests for types HERE will
		/// first check this container itself; and if not registered
		/// here, they will then check this parent. Additionally,
		/// dependencies needed for services constructed HERE will first
		/// check this container, and if not resolved here, WILL check
		/// this <paramref name="parent"/>. Note that dependencies needed
		/// for Types registered on the <paramref name="parent"/> will NOT
		/// come from this or any other child containers --- and at the same
		/// time, ALL parents WILL be checked up the hierarchy.
		/// NOTICE that this means that the <paramref name="parent"/>
		/// container MUST live at least as long as THIS container,
		/// AND all parents up the hierarchy must also --- otherwise dependencies
		/// resolved from a parent container could be disposed before
		/// a service that is using them.
		/// </summary>
		/// <param name="parent">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">The <paramref name="parent"/>
		/// cannot be cyclically scoped, AND THIS
		/// <see cref="IServiceRegistrationProvider.ParentServiceProvider"/>
		/// must be null at this time.</exception>
		internal void Scope(IServiceProvider parent)
		{
			if (parent == null)
				throw new ArgumentNullException(nameof(parent));
			if (object.ReferenceEquals(this, parent))
				throw new ArgumentException($"Requested parent container is THIS: {parent}", nameof(parent));
			lock (registrations) {
				if (parentServiceProvider != null)
					throw new ArgumentException($"Container cannot be scoped twice: {this}", nameof(parent));
				IServiceProvider checkParent = (parent as IServiceRegistrationProvider)?.ParentServiceProvider;
				while (checkParent != null) {
					if (object.ReferenceEquals(checkParent, parent)
							|| object.ReferenceEquals(checkParent, this))
						throw new ArgumentException($"Container cannot be scoped twice: {parent}", nameof(parent));
					checkParent = (checkParent as IServiceRegistrationProvider)?.ParentServiceProvider;
				}
				parentServiceProvider = parent;
				parentServiceRegistrationProvider = parent as IServiceRegistrationProvider;
			}
		}


		/// <summary>
		/// Removes the registration if present. Will optionally dispose that first.
		/// </summary>
		/// <param name="serviceType">Required.</param>
		/// <param name="disposeExistingInstance">Three-state: if this is null, then
		/// any removed registration will or will not be disposed according to
		/// <see cref="DisposeRegistrationsWithContainer"/>. Otherwise this explicitly
		/// selects whether the existing instance is or is not disposed now.</param>
		/// <returns>True if found and removed; and optionally disposed.</returns>
		public bool Remove(Type serviceType, bool? disposeExistingInstance = null)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			lock (registrations) {
				if (!registrations.TryGetValue(
						serviceType,
						out ServiceRegistration existingRegistration))
					return false;
				registrations.Remove(serviceType);
				if (disposeExistingInstance is true
						|| ((disposeExistingInstance == null)
								&& DisposeRegistrationsWithContainer))
					existingRegistration?.Dispose();
				return true;
			}
		}

		/// <summary>
		/// Returns a list of current service type registrations.
		/// </summary>
		/// <returns>Not null.</returns>
		public IEnumerable<Type> List()
		{
			Type[] result;
			lock (registrations) {
				result = registrations.Keys.ToArray();
			}
			return result;
		}


		public object GetService(Type serviceType)
			=> tryGetService(serviceType, null);

		object IServiceRegistrationProvider.GetService(
				Type serviceType,
				ServiceConstructorRequest serviceConstructorRequest)
			=> tryGetService(serviceType, serviceConstructorRequest);

		private object tryGetService(Type serviceType, ServiceConstructorRequest serviceConstructorRequest)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			lock (registrations) {
				if (serviceConstructorRequest == null)
					serviceConstructorRequest = newServiceConstructorRequest();
				return registrations.TryGetValue(serviceType, out ServiceRegistration registration)
						? registration.Get(serviceConstructorRequest)
						: parentServiceRegistrationProvider != null
								? parentServiceRegistrationProvider.GetService(serviceType, serviceConstructorRequest)
								: parentServiceProvider?.GetService(serviceType);
			}
		}


		public void RegisterType(Type serviceType, Type implementationType, bool isSingleton = true)
			=> addRegistration(newServiceRegistration(serviceType, implementationType, isSingleton));

		public void RegisterType<TService, TImplementation>(
				Func<IServiceProvider, TImplementation> factory,
				bool isSingleton = true,
				bool? disposeIfSingleton = null)
				where TImplementation : TService
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			addRegistration(
					newServiceRegistration(
							typeof(TService),
							typeof(TImplementation),
							isSingleton,
							Factory,
							disposeIfSingleton));
			object Factory(IServiceProvider serviceProvider)
				=> factory(serviceProvider);
		}

		public void RegisterInstance(Type serviceType, object service, bool? disposeSingleton = null)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			if (service == null)
				throw new ArgumentNullException(nameof(service));
			if (!serviceType.IsInstanceOfType(service)) {
				throw new ArgumentException(
						$"Service instance '{service.GetType().GetFriendlyFullName()}'"
						+ $" is not assignable to service type '{serviceType.GetFriendlyFullName()}'",
						nameof(service));
			}
			addRegistration(newServiceRegistration(serviceType, service.GetType(), true, Factory, disposeSingleton));
			object Factory(IServiceProvider _)
				=> service;
		}

		public IEnumerable<Type> GetRegisteredServiceTypes()
		{
			lock (registrations) {
				return registrations.Keys.ToArray();
			}
		}


		public void Dispose()
		{
			List<ServiceRegistration> disposeRegistrations;
			lock (registrations) {
				disposeRegistrations = DisposeRegistrationsWithContainer
						? registrations.Values.ToList()
						: null;
				registrations.Clear();
				parentServiceRegistrationProvider = null;
				parentServiceProvider = null;
			}
			if (disposeRegistrations == null)
				return;
			disposeRegistrations.Sort(DateComparer);
			disposeRegistrations.Sort(DependencyComparer);
			disposeRegistrations.Sort(DependantComparer);
			foreach (ServiceRegistration disposeRegistration in disposeRegistrations) {
				disposeRegistration.Dispose();
			}

			static int DateComparer(ServiceRegistration x, ServiceRegistration y)
				=> !x.ConstructedAt.HasValue
						? y.ConstructedAt.HasValue
								? -1
								: 0
						: !y.ConstructedAt.HasValue
								? 1
								: -x.ConstructedAt.Value.CompareTo(y.ConstructedAt.Value);
			static int DependencyComparer(ServiceRegistration x, ServiceRegistration y)
				=> -x.Dependencies.Count.CompareTo(y.Dependencies.Count);
			int DependantComparer(ServiceRegistration x, ServiceRegistration y)
				=> registrations.Values.Count(
								registration => registration.Dependencies.ContainsValue(x.ServiceType)
										|| registration.Dependencies.ContainsValue(x.ImplementationType))
						.CompareTo(
								registrations.Values.Count(
										registration => registration.Dependencies.ContainsValue(y.ServiceType)
												|| registration.Dependencies.ContainsValue(y.ImplementationType)));
		}


		public override string ToString()
		{
			lock (registrations) {
				return $"{GetType().GetFriendlyName()}"
						+ $"{registrations.ToStringCollection(0)}"
						+ "["
						+ $"{nameof(IServiceRegistrationProvider.ParentServiceProvider)}"
						+ $": {parentServiceProvider?.ToString() ?? Convert.ToString(null)}"
						+ "]";
			}
		}
	}
}
