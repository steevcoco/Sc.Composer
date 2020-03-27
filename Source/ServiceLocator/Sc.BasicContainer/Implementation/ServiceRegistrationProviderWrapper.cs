using System;
using Sc.Diagnostics;
using Sc.Util.System;


namespace Sc.BasicContainer.Implementation
{
	/// <summary>
	/// Wraps an <see cref="IServiceProvider"/> and implements
	/// <see cref="IServiceRegistrationProvider"/>. Supports scoping.
	/// </summary>
	internal sealed class ServiceRegistrationProviderWrapper
			: IServiceRegistrationProvider
	{
		private readonly IServiceProvider serviceProvider;
		private readonly IServiceRegistrationProvider parentServiceRegistrationProvider;


		/// <summary>
		/// Constructor. NOTICE that the <paramref name="serviceProvider"/>
		/// MUST NOT implement <see cref="IServiceRegistrationProvider"/>.
		/// </summary>
		/// <param name="serviceProvider">Required; and MUST NOT implement
		/// <see cref="IServiceRegistrationProvider"/>.</param>
		/// <param name="parentServiceProvider">Optional parent for the
		/// <see cref="IServiceRegistrationProvider.ParentServiceProvider"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public ServiceRegistrationProviderWrapper(
				IServiceProvider serviceProvider,
				IServiceProvider parentServiceProvider = null)
		{
			if (serviceProvider is IServiceRegistrationProvider) {
				throw new ArgumentException(
						$"{nameof(ServiceRegistrationProviderWrapper)} {nameof(serviceProvider)}"
						+ $" must not implement {nameof(IServiceRegistrationProvider)}"
						+ $": '{serviceProvider.GetType().GetFriendlyFullName()}'.");
			}
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			ParentServiceProvider = parentServiceProvider;
			parentServiceRegistrationProvider = parentServiceProvider as IServiceRegistrationProvider;
			if (object.ReferenceEquals(serviceProvider, parentServiceProvider)) {
				throw new ArgumentException(
						$"Requested parent container is THIS: {parentServiceProvider}",
						nameof(parentServiceProvider));
			}
			IServiceProvider checkParent = parentServiceRegistrationProvider?.ParentServiceProvider;
			while (checkParent != null) {
				if (object.ReferenceEquals(checkParent, parentServiceProvider)
						|| object.ReferenceEquals(checkParent, serviceProvider)) {
					throw new ArgumentException(
							$"Container cannot be scoped twice: {parentServiceProvider}",
							nameof(parentServiceProvider));
				}
				checkParent = (checkParent as IServiceRegistrationProvider)?.ParentServiceProvider;
			}
		}


		private object tryGetService(
				Type serviceType,
				ServiceConstructorRequest serviceConstructorRequest)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));
			object service = serviceProvider.GetService(serviceType);
			return serviceType.IsInstanceOfType(service)
					? service
					: parentServiceRegistrationProvider != null
							? parentServiceRegistrationProvider.GetService(
									serviceType,
									serviceConstructorRequest
									?? new ServiceConstructorRequest(
											TraceSources.For<ServiceRegistrationProviderWrapper>()))
							: ParentServiceProvider?.GetService(serviceType);
		}


		public IServiceProvider ParentServiceProvider { get; }

		object IServiceProvider.GetService(Type serviceType)
			=> tryGetService(serviceType, null);

		object IServiceRegistrationProvider.GetService(
				Type serviceType,
				ServiceConstructorRequest serviceConstructorRequest)
			=> tryGetService(serviceType, serviceConstructorRequest);
	}
}
