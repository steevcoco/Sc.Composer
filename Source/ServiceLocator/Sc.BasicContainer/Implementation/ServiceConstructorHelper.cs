using System;


namespace Sc.BasicContainer.Implementation
{
	/// <summary>
	/// Static helpers for <see cref="ServiceConstructor"/>.
	/// </summary>
	public static class ServiceConstructorHelper
	{
		/// <summary>
		/// As with <see cref="ServiceConstructor.TryResolve"/>, but this method
		/// allows you to pass your requested generic service type.
		/// </summary>
		/// <typeparam name="TService">The concrete type to resolve.</typeparam>
		/// <param name="serviceConstructor">Not null.</param>
		/// <param name="service">The result if the method returns true.</param>
		/// <param name="instanceProvider">Optional: can provide the instance now;
		/// and is checked first.</param>
		/// <returns>False if the service can't be resolved.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryResolve<TService>(
				this ServiceConstructor serviceConstructor,
				out TService service,
				Func<Type, object> instanceProvider = null)
		{
			if (serviceConstructor == null)
				throw new ArgumentNullException(nameof(serviceConstructor));
			if (serviceConstructor.TryResolve(
							typeof(TService),
							out object resolved,
							instanceProvider)
					&& resolved is TService tService) {
				service = tService;
				return true;
			}
			service = default;
			return false;
		}

		/// <summary>
		/// As with <see cref="ServiceConstructor.TryResolve"/>, but this method
		/// allows you to pass your requested generic service type; and will
		/// returns the service or default.
		/// </summary>
		/// <typeparam name="TService">The concrete type to resolve.</typeparam>
		/// <param name="serviceConstructor">Not null.</param>
		/// <param name="instanceProvider">Optional: can provide the instance now;
		/// and is checked first.</param>
		/// <returns>The resolved service, or default if the service can't be resolved.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TService ResolveOrDefault<TService>(
				this ServiceConstructor serviceConstructor,
				Func<Type, object> instanceProvider = null)
		{
			serviceConstructor.TryResolve(out TService service, instanceProvider);
			return service;
		}

		/// <summary>
		/// As with <see cref="ServiceConstructor.TryConstruct"/>, but this method
		/// allows you to pass your requested generic service type.
		/// </summary>
		/// <typeparam name="TService">The concrete type to construct.</typeparam>
		/// <param name="serviceConstructor">Not null.</param>
		/// <param name="service">The newly-constructed service if the method returns true.</param>
		/// <param name="instanceProvider">Optional: can provide constructor arguments
		/// for the constructed instance now; and is checked first.</param>
		/// <returns>False if the service cannot be constructed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryConstruct<TService>(
				this ServiceConstructor serviceConstructor,
				out TService service,
				Func<Type, object> instanceProvider = null)
		{
			if (serviceConstructor == null)
				throw new ArgumentNullException(nameof(serviceConstructor));
			if (serviceConstructor.TryConstruct(
							typeof(TService),
							out object constructed,
							instanceProvider)
					&& constructed is TService tService) {
				service = tService;
				return true;
			}
			service = default;
			return false;
		}

		/// <summary>
		/// As with <see cref="ServiceConstructor.TryResolveOrConstruct"/>, but this method
		/// allows you to pass your requested generic service type.
		/// </summary>
		/// <typeparam name="TService">The concrete type to construct.</typeparam>
		/// <param name="serviceConstructor">Not null.</param>
		/// <param name="service">The newly-constructed service if the method returns true.</param>
		/// <param name="instanceProvider">Optional: can provide constructor arguments
		/// for the constructed instance now; and is checked first.</param>
		/// <returns>False if the service can't be resolved nor constructed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryResolveOrConstruct<TService>(
				this ServiceConstructor serviceConstructor,
				out TService service,
				Func<Type, object> instanceProvider = null)
		{
			if (serviceConstructor == null)
				throw new ArgumentNullException(nameof(serviceConstructor));
			if (serviceConstructor.TryResolveOrConstruct(
							typeof(TService),
							out object constructed,
							instanceProvider)
					&& constructed is TService tService) {
				service = tService;
				return true;
			}
			service = default;
			return false;
		}

		/// <summary>
		/// As with <see cref="ServiceConstructor.TryInject"/>, but this method
		/// allows you to pass your requested generic service type.
		/// </summary>
		/// <typeparam name="TAttribute">Specifies an Attribute type that must be
		/// present on a method.</typeparam>
		/// <param name="serviceConstructor">Not null.</param>
		/// <param name="target">Is the target object to locate the method on.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for method or constructor argument values.</param>
		/// <returns>True for success.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryInject<TAttribute>(
				this ServiceConstructor serviceConstructor,
				object target,
				Func<Type, object> instanceProvider = null)
		{
			if (serviceConstructor == null)
				throw new ArgumentNullException(nameof(serviceConstructor));
			return serviceConstructor.TryInject(target, typeof(TAttribute), instanceProvider);
		}
	}
}
