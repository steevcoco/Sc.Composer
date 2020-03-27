using System;
using System.Runtime.CompilerServices;
using Sc.Abstractions.Diagnostics;
using Sc.Abstractions.ServiceLocator;
using Sc.BasicContainer.Implementation;
using Sc.BasicContainer.Specialized;


namespace Sc.BasicContainer
{
	/// <summary>
	/// Static helpers for <see cref="IServiceProvider"/>.
	/// </summary>
	public static class ServiceProviderHelper
	{
		/// <summary>
		/// This static method provides an <see cref="IServiceProvider"/>
		/// implementation that will fully CONSTRUCT and resolve a type.
		/// NOTICE that the invoker IS ASSUMED to ba directly from a registered type
		/// --- meaning that the target type given here will NOT be resolved
		/// form the resolver, but constructed.
		/// NOTICE also that this method is intended for
		/// <see cref="IServiceProvider"/> implementation: any resolved
		/// dependencies are managed by the container, and are subject to
		/// the lifetime policies there: they will be disposed according to
		/// the container and may not have lifetime affinity with the constructed
		/// instance (this also applies recursively if arguments are
		/// constructed). Similarly, any constructed arguments may hide bugs
		/// if the instance was expecting only a service from the provider,
		/// but the object is instead constructed here on demand.
		/// All required constructor parameters
		/// will first try the optional Func, and then either the resolver only, or if
		/// <paramref name="tryConstructArguments"/> is true, they are also
		/// constructed recursively here. This method traces progress.
		/// This implementation allows you to specify if constructors will only
		/// be selected if they have a specified attribute; and otherwise,
		/// this implementation will attempt all constructors. In both cases,
		/// all selected constructors are attempted from the one
		/// with the most arguments first. This ALSO supports default
		/// argument values.
		/// </summary>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="targetType">This is the type to construct. Required.</param>
		/// <param name="service">Is set if the method returns true.</param>
		/// <param name="logger">Optional trace source used to log here.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for constructor argument values.</param>
		/// <param name="tryConstructArguments">If false, all
		/// constructor arguments must resolve from the resolver; or from the
		/// optional <paramref name="instanceProvider"/>. If set true, all constructors will
		/// attempt to construct arguments that do not resolve.</param>
		/// <param name="requireAttributes">Selects whether this method will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <paramref name="attributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments is tried first. If true, only constructors
		/// with attributes are selected; and again sorted by argument length.</param>
		/// <param name="attributeTypes">Applies if <paramref name="requireAttributes"/>
		/// is not false: this specifies the required or optional constructor
		/// attributes that will be considered. PLEASE NOTICE: if
		/// this argument is empty, then if <paramref name="requireAttributes"/> is not
		/// false, this method WILL ADD the <see cref="ServiceProviderConstructorAttribute"/>
		/// here.</param>
		/// <returns>True for success.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryConstruct(
				this IServiceProvider serviceProvider,
				Type targetType,
				out object service,
				ITrace logger = null,
				Func<Type, object> instanceProvider = null,
				bool tryConstructArguments = false,
				bool? requireAttributes = null,
				params Type[] attributeTypes)
		{
			IServiceRegistrationProvider serviceRegistrationProvider
					= serviceProvider as IServiceRegistrationProvider
					?? new ServiceRegistrationProviderWrapper(serviceProvider);
			return ServiceConstructorMethods.TryConstruct(
					targetType,
					out service,
					new ServiceConstructorRequest(logger),
					serviceRegistrationProvider,
					instanceProvider,
					tryConstructArguments,
					requireAttributes,
					attributeTypes);
		}


		/// <summary>
		/// This static method provides an <see cref="IServiceProvider"/>
		/// implementation that will fully resolve or construct a type.
		/// The target type given here will first be tried to be resolved
		/// form the resolver; and if it is not returned, it is constructed
		/// with <see cref="ServiceProviderHelper.TryConstruct"/>.
		/// Please see that method for important documentation.
		/// </summary>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="targetType">This is the type to construct. Required.</param>
		/// <param name="service">Is set if the method returns true.</param>
		/// <param name="logger">Optional trace source used to log here.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for constructor argument values.</param>
		/// <param name="tryConstructArguments">If false, all
		/// constructor arguments must resolve from the resolver; or from the
		/// optional <paramref name="instanceProvider"/>. If set true, all constructors will
		/// attempt to construct arguments that do not resolve.</param>
		/// <param name="requireConstructorAttributes">Selects whether this
		/// method will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <paramref name="constructorAttributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments is tried first. If true, only constructors
		/// with attributes are selected; and again sorted by argument length.</param>
		/// <param name="constructorAttributeTypes">Applies if
		/// <paramref name="requireConstructorAttributes"/>
		/// is not false: this specifies the required or optional constructor
		/// attributes that will be considered. PLEASE NOTICE: if
		/// this argument is empty, then if
		/// <paramref name="requireConstructorAttributes"/> is not
		/// false, this method WILL ADD the <see cref="ServiceProviderConstructorAttribute"/>
		/// here.</param>
		/// <returns>False if not successful.</returns>
		/// <see cref="ArgumentNullException"/>
		public static bool TryResolveOrConstruct(
				this IServiceProvider serviceProvider,
				Type targetType,
				out object service,
				ITrace logger = null,
				Func<Type, object> instanceProvider = null,
				bool tryConstructArguments = false,
				bool? requireConstructorAttributes = null,
				params Type[] constructorAttributeTypes)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			service = serviceProvider.GetService(targetType);
			return targetType.IsInstanceOfType(service)
					|| serviceProvider.TryConstruct(
							targetType,
							out service,
							logger,
							instanceProvider,
							tryConstructArguments,
							requireConstructorAttributes,
							constructorAttributeTypes);
		}

		/// <summary>
		/// This static method provides an <see cref="IServiceProvider"/>
		/// implementation that will fully resolve or construct a type;
		/// as with <see cref="TryResolveOrConstruct"/>: please see
		/// that method for important documentation.
		/// </summary>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="service">Is set if the method returns true.</param>
		/// <param name="logger">Optional trace source used to log here.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for constructor argument values.</param>
		/// <param name="tryConstructArguments">If false, all
		/// constructor arguments must resolve from the resolver; or from the
		/// optional <paramref name="instanceProvider"/>. If set true, all constructors will
		/// attempt to construct arguments that do not resolve.</param>
		/// <param name="requireConstructorAttributes">Selects whether this
		/// method will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <paramref name="constructorAttributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments is tried first. If true, only constructors
		/// with attributes are selected; and again sorted by argument length.</param>
		/// <param name="constructorAttributeTypes">Applies if
		/// <paramref name="requireConstructorAttributes"/>
		/// is not false: this specifies the required or optional constructor
		/// attributes that will be considered. PLEASE NOTICE: if
		/// this argument is empty, then if
		/// <paramref name="requireConstructorAttributes"/> is not
		/// false, this method WILL ADD the <see cref="ServiceProviderConstructorAttribute"/>
		/// here.</param>
		/// <returns>True if the service is set.</returns>
		public static bool TryResolveOrConstruct<TService>(
				this IServiceProvider serviceProvider,
				out TService service,
				ITrace logger = null,
				Func<Type, object> instanceProvider = null,
				bool tryConstructArguments = false,
				bool? requireConstructorAttributes = null,
				params Type[] constructorAttributeTypes)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (serviceProvider.TryResolveOrConstruct(
							typeof(TService),
							out object result,
							logger,
							instanceProvider,
							tryConstructArguments,
							requireConstructorAttributes,
							constructorAttributeTypes)
					&& result is TService serviceResult) {
				service = serviceResult;
				return true;
			}
			service = default;
			return false;
		}


		/// <summary>
		/// This static method provides an <see cref="IServiceProvider"/>
		/// implementation that will fully resolve and optionally construct services
		/// to be injected into a method on the <c>target</c> object that is marked with
		/// the specified <c>attributeType</c>.
		/// NOTICE also that this method is intended for
		/// <see cref="IServiceProvider"/> implementation: any resolved
		/// dependencies are managed by the container, and are subject to
		/// the lifetime policies there: they will be disposed according to
		/// the container and may not have lifetime affinity with the injected
		/// instance (this also applies recursively if arguments are
		/// constructed). Similarly, any constructed arguments may hide bugs
		/// if the instance was expecting only a service from the provider,
		/// but the object is instead constructed here on demand.
		/// The method must be an instance method, and can be any visibility.
		/// This implementation will attempt all methods with the attribute, from
		/// the one with the most arguments first; and will stop with the first
		/// successful method. This implementation ALSO supports default
		/// argument values.
		/// </summary>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="target">Is the target object to locate the method on.</param>
		/// <param name="attributeType">Specifies an Attribute type that must be
		/// present on a method.</param>
		/// <param name="logger">Optional trace source used to log here.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for method or constructor argument values.</param>
		/// <param name="tryConstructArguments">If false, all method arguments
		/// must resolve from the resolver; or from the optional <c>instanceProvider</c>.
		/// If set true, this will attempt to construct method arguments that do
		/// not resolve (and will recursively construct argument constructor values that
		/// require dependencies themselves).</param>
		/// <param name="requireConstructorAttributes">Applies only if
		/// <paramref name="tryConstructArguments"/> is true; and applies to any
		/// constructed argument values. Selects whether this method will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <paramref name="constructorAttributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments is tried first. If true, only constructors
		/// with attributes are selected; and again sorted by argument length.</param>
		/// <param name="constructorAttributeTypes">Applies if
		/// <paramref name="requireConstructorAttributes"/>
		/// is not false: this specifies the required or optional constructor
		/// attributes that will be considered. PLEASE NOTICE: if
		/// this argument is empty, then if
		/// <paramref name="requireConstructorAttributes"/> is not
		/// false, this method WILL ADD the
		/// <see cref="ServiceProviderConstructorAttribute"/>
		/// here.</param>
		/// <returns>True for success.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryInject(
				this IServiceProvider serviceProvider,
				object target,
				Type attributeType,
				ITrace logger = null,
				Func<Type, object> instanceProvider = null,
				bool tryConstructArguments = false,
				bool? requireConstructorAttributes = null,
				params Type[] constructorAttributeTypes)
			=> ServiceConstructorMethods.TryInject(
					serviceProvider as IServiceRegistrationProvider
					?? new ServiceRegistrationProviderWrapper(serviceProvider),
					target,
					attributeType,
					logger,
					instanceProvider,
					tryConstructArguments,
					requireConstructorAttributes,
					constructorAttributeTypes);

		/// <summary>
		/// This static method provides an <see cref="IServiceProvider"/>
		/// implementation that will fully resolve and optionally construct services
		/// to be injected into a method on the <c>target</c> object that is marked with
		/// the specified <c>attributeType</c>; as with <see cref="TryInject"/>:
		/// please see that method for important documentation.
		/// </summary>
		/// <typeparam name="TAttribute">Your attribute type: used to locate the method.</typeparam>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="target">Is the target object to locate the method on.</param>
		/// <param name="logger">Optional trace source used to log here.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for method or constructor argument values.</param>
		/// <param name="tryConstructArguments">If false, all method arguments
		/// must resolve from the resolver; or from the optional <c>instanceProvider</c>.
		/// If set true, this will attempt to construct method arguments that do
		/// not resolve (and will recursively construct argument constructor values that
		/// require dependencies themselves).</param>
		/// <param name="requireConstructorAttributes">Applies only if
		/// <paramref name="tryConstructArguments"/> is true; and applies to any
		/// constructed argument values. Selects whether this method will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <paramref name="constructorAttributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments is tried first. If true, only constructors
		/// with attributes are selected; and again sorted by argument length.</param>
		/// <param name="constructorAttributeTypes">Applies if
		/// <paramref name="requireConstructorAttributes"/>
		/// is not false: this specifies the required or optional constructor
		/// attributes that will be considered. PLEASE NOTICE: if
		/// this argument is empty, then if
		/// <paramref name="requireConstructorAttributes"/> is not
		/// false, this method WILL ADD the
		/// <see cref="ServiceProviderConstructorAttribute"/>
		/// here.</param>
		/// <returns>True if a method is found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryInject<TAttribute>(
				this IServiceProvider serviceProvider,
				object target,
				ITrace logger = null,
				Func<Type, object> instanceProvider = null,
				bool tryConstructArguments = false,
				bool? requireConstructorAttributes = null,
				params Type[] constructorAttributeTypes)
				where TAttribute : Attribute
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			return serviceProvider.TryInject(
					target,
					typeof(TAttribute),
					logger,
					instanceProvider,
					tryConstructArguments,
					requireConstructorAttributes,
					constructorAttributeTypes);
		}


		/// <summary>
		/// Returns a simple <see cref="IServiceProvider"/> implementation that invokes
		/// your Func.
		/// </summary>
		/// <param name="getService">Required.</param>
		/// <returns>Not null. Implements <see cref="IDisposable"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IServiceProvider AsServiceProvider(this Func<Type, object> getService)
			=> new DelegateServiceProvider(getService);


		/// <summary>
		/// Provides an implementation of <see cref="IServiceProvider"/> that will
		/// support scoping. Requests for types ON THIS
		/// <paramref name="scopedServiceProvider"/> will
		/// first check this container itself; and if not registered
		/// here, they will then check this <paramref name="parentServiceProvider"/>.
		/// Additionally, dependencies needed for services constructed HERE will first
		/// check this container, and if not resolved here, WILL check
		/// this <paramref name="parentServiceProvider"/>. Note that dependencies needed
		/// for Types registered on the <paramref name="parentServiceProvider"/> will NOT
		/// come from this or any other child containers --- and at the same
		/// time, ALL parents WILL be checked up the hieerarchy.
		/// NOTICE that this means that the <paramref name="parentServiceProvider"/>
		/// container MUST live at least as long as THIS container,
		/// AND all parents up the hierarchy must also --- otherwise dependencies
		/// resolved from a parent container could be disposed before
		/// a service that is using them.
		/// </summary>
		/// <param name="scopedServiceProvider">Not null.</param>
		/// <param name="parentServiceProvider">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IServiceProvider Scope(
				this IServiceProvider scopedServiceProvider,
				IServiceProvider parentServiceProvider)
		{
			if (scopedServiceProvider == null)
				throw new ArgumentNullException(nameof(scopedServiceProvider));
			if (parentServiceProvider == null)
				throw new ArgumentNullException(nameof(parentServiceProvider));
			if (scopedServiceProvider is BasicContainer basicContainer) {
				basicContainer.Scope(parentServiceProvider);
				return basicContainer;
			}
			if (scopedServiceProvider is IServiceRegistrationProvider) {
				throw new NotSupportedException(
						$"Cannot scope {nameof(IServiceRegistrationProvider)}: {scopedServiceProvider}");
			}
			return new ServiceRegistrationProviderWrapper(scopedServiceProvider, parentServiceProvider);
		}
	}
}
