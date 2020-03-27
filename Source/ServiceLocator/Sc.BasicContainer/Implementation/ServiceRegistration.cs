using System;
using System.Collections.Generic;
using Sc.Abstractions.Lifecycle;
using Sc.Collections;
using Sc.Util.System;


namespace Sc.BasicContainer.Implementation
{
	/// <summary>
	/// A service registration implementation for
	/// <see cref="IServiceRegistrationProvider"/> registrations.
	/// This class uses <see cref="ServiceConstructorMethods"/>
	/// methods to construct and resolve the service. Synchronized.
	/// </summary>
	internal sealed class ServiceRegistration
			: IRaiseDisposed,
					IDisposable
	{
		private readonly object syncLock = new object();
		private readonly IServiceRegistrationProvider serviceProvider;
		private readonly Func<IServiceProvider, object> factory;
		private bool? requireConstructorAttributes;
		private IReadOnlyCollection<Type> constructorAttributeTypes;
		private object singleton;
		private bool isDisposed;
		private bool isSingletonSet;
		private DateTime? constructedAt;
		private MultiDictionary<Type, Type> dependencies;


		/// <summary>
		/// Constructor. Notice that the <paramref name="factory"/> is optional:
		/// <see cref="IsImplementationContainerCreated"/> is
		/// set based on this argument by default.
		/// </summary>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="serviceType">Required.</param>
		/// <param name="implementationType">Required.</param>
		/// <param name="isSingleton">Required.</param>
		/// <param name="factory">OPTIONAL: if null, the <see cref="ServiceConstructorMethods"/>
		/// constructs the object. Note also that <see cref="IsImplementationContainerCreated"/>
		/// is set true by default if this is null; and is set false by default
		/// if this is not null.</param>
		/// <param name="isImplementationContainerCreated">Specifies the value
		/// for <see cref="IsImplementationContainerCreated"/>;
		/// which determines the disposal policy: this class ONLY disposes singletons
		/// marked as <c>IsImplementationContainerCreated</c> (and always disposes
		/// such instances). This value is nullable: if null, it will be set true
		/// if the <c>factory</c> is null; and otherwise false.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If the <paramref name="implementationType"/>
		/// does not extend the <paramref name="serviceType"/></exception>
		public ServiceRegistration(
				IServiceRegistrationProvider serviceProvider,
				Type serviceType,
				Type implementationType,
				bool isSingleton,
				Func<IServiceProvider, object> factory = null,
				bool? isImplementationContainerCreated = null)
		{
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
			ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
			if (!serviceType.IsAssignableFrom(implementationType)) {
				throw new ArgumentException(
						$"Implementation type '{implementationType.GetFriendlyFullName()}'"
						+ $" is not assignable to service type '{serviceType.GetFriendlyFullName()}'",
						nameof(implementationType));
			}
			IsSingleton = isSingleton;
			this.factory = factory;
			IsImplementationContainerCreated = isImplementationContainerCreated ?? (factory == null);
		}


		private void checkIsDisposedUnsafe()
		{
			if (isDisposed)
				throw new ObjectDisposedException(ToString());
		}


		/// <summary>
		/// If attributes are required for the selected constructors.
		/// </summary>
		public bool? RequireConstructorAttributes
		{
			get {
				lock (syncLock) {
					return requireConstructorAttributes;
				}
			}
			set {
				lock (syncLock) {
					requireConstructorAttributes = value;
				}
			}
		}

		/// <summary>
		/// Optional required attributes for the selected constructors.
		/// </summary>
		public IReadOnlyCollection<Type> ConstructorAttributeTypes
		{
			get {
				lock (syncLock) {
					return constructorAttributeTypes;
				}
			}
			set {
				lock (syncLock) {
					constructorAttributeTypes = value;
				}
			}
		}


		/// <summary>
		/// Used to track when a singleton instance is created: this implementation
		/// sets this true if this definition is a singleton, and the service
		/// has been created.
		/// </summary>
		public bool IsSingletonSet
		{
			get {
				lock (syncLock) {
					return isSingletonSet;
				}
			}
			private set {
				lock (syncLock) {
					isSingletonSet = value;
				}
			}
		}

		/// <summary>
		/// This will be set to the Utc time when this instance is
		/// constructed: if this is a singleton, this will be set once when
		/// the singleton is constructed; and otherwise this is set each time
		/// a transient instance is constructed. Null until set the first time.
		/// </summary>
		public DateTime? ConstructedAt
		{
			get {
				lock (syncLock) {
					return constructedAt;
				}
			}
			private set {
				lock (syncLock) {
					constructedAt = value;
				}
			}
		}

		/// <summary>
		/// True if the registration was for a singleton.
		/// </summary>
		public bool IsSingleton { get; }

		/// <summary>
		/// The public service registration type.
		/// </summary>
		public Type ServiceType { get; }

		/// <summary>
		/// The concrete implementation type.
		/// </summary>
		public Type ImplementationType { get; }

		/// <summary>
		/// Is true if the container constructs the implementation; and if true,
		/// and if this is a singleton, then the singleton will be disposed
		/// with this registration.
		/// </summary>
		public bool IsImplementationContainerCreated { get; }

		/// <summary>
		/// Non-null list of all dependencies under this constructed instance.
		/// </summary>
		public MultiDictionary<Type, Type> Dependencies
		{
			get {
				lock (syncLock) {
					return dependencies != null
							? new MultiDictionary<Type, Type>(dependencies, dependencies.Comparer)
							: new MultiDictionary<Type, Type>(null, 0);
				}
			}
		}

		public bool IsDisposed
		{
			get {
				lock (syncLock) {
					return isDisposed;
				}
			}
		}


		/// <summary>
		/// The implementation method that constructs and/or returns the
		/// implementation instance.
		/// </summary>
		/// <param name="serviceConstructorRequest">This argument is required here.</param>
		/// <returns>Should be null only if the service is not constructed successfully:
		/// note that this may result in a resolver or constructor error.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		internal object Get(ServiceConstructorRequest serviceConstructorRequest)
		{
			if (serviceConstructorRequest == null)
				throw new ArgumentNullException(nameof(serviceConstructorRequest));
			lock (syncLock) {
				checkIsDisposedUnsafe();
				if (!IsSingleton) {
					ConstructedAt = DateTime.UtcNow;
					object instance = InvokeFactory(serviceConstructorRequest);
					if (dependencies != null)
						dependencies.TryAddRange(serviceConstructorRequest.Dependencies);
					else {
						dependencies
								= new MultiDictionary<Type, Type>(
										serviceConstructorRequest.Dependencies,
										serviceConstructorRequest.Dependencies.Comparer);
					}
					return instance;
				}
				if (IsSingletonSet)
					return singleton;
				ConstructedAt = DateTime.UtcNow;
				singleton = InvokeFactory(serviceConstructorRequest);
				IsSingletonSet = true;
				dependencies
						= new MultiDictionary<Type, Type>(
								serviceConstructorRequest.Dependencies,
								serviceConstructorRequest.Dependencies.Comparer);
				return singleton;
			}
			object InvokeFactory(ServiceConstructorRequest request)
				=> factory != null
						? factory(serviceProvider)
						: ServiceConstructorMethods.TryConstruct(
								ImplementationType,
								out object result,
								request,
								serviceProvider,
								null,
								false,
								RequireConstructorAttributes,
								ConstructorAttributeTypes)
								? result
								: null;
		}


		public event EventHandler Disposed;

		public void Dispose()
		{
			IDisposable dispose;
			lock (syncLock) {
				dispose = IsImplementationContainerCreated
						? singleton as IDisposable
						: null;
				singleton = null;
				IsSingletonSet = false;
				isDisposed = true;
			}
			dispose?.Dispose();
			Disposed?.Invoke(this, EventArgs.Empty);
			Disposed = null;
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $"{ServiceType.GetFriendlyFullName()} - {ImplementationType.GetFriendlyFullName()}"
					+ $"{(IsSingleton ? $", {nameof(ServiceRegistration.IsSingleton)}" : string.Empty)}"
					+ $"{(IsImplementationContainerCreated ? $", {nameof(ServiceRegistration.IsImplementationContainerCreated)}" : string.Empty)}"
					+ $"{(IsSingleton && IsSingletonSet ? $", {nameof(ServiceRegistration.IsSingletonSet)}={IsSingletonSet}" : string.Empty)}"
					+ "]";
	}
}
