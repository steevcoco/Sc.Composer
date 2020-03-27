using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Diagnostics;
using Sc.Abstractions.ServiceLocator;
using Sc.Diagnostics;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.BasicContainer.Implementation
{
	/// <summary>
	/// Provides <see cref="IServiceRegistrationProvider"/> implementations
	/// to fully construct and resolve types.
	/// </summary>
	internal static class ServiceConstructorMethods
	{
		/// <summary>
		/// This static method provides an <see cref="IServiceProvider"/>
		/// implementation that will fully CONSTRUCT and resolve a type.
		/// NOTICE that the
		/// invoker IS ASSUMED to ba directly from a registered type
		/// --- meaning that the requested service type will NOT be resolved
		/// form the resolver, but constructed. All parameters will first try
		/// the optional <c>Func</c>, and then either the resolver only, or if
		/// <c>TryConstructArguments</c> is true, they are also
		/// constructed recursively.
		/// This implementation allows you to specify if constructors will only
		/// be selected if they have a specified attribute; and otherwise,
		/// this implementation will attempt all constructors. In both cases,
		/// all selected constructors are attempted from the one
		/// with the most arguments first. This ALSO supports default
		/// argument values. NOTICE that this is intended for
		/// <see cref="IServiceProvider"/> implementation: any resolved
		/// dependencies are managed by the container, and are subject to
		/// the lifetime policies there: they will be disposed according to
		/// the container and may not have lifetime affinity with the constructed
		/// instance (this also applies recursively if arguments are
		/// constructed).
		/// </summary>
		/// <param name="targetType">Required concrete service type to be constructed.</param>
		/// <param name="service">Is set if the method returns true.</param>
		/// <param name="request">Required: this will hold state if
		/// constructors must be recursively invoked. A new instance must be
		/// created for a top-level request ONLY.</param>
		/// <param name="serviceProvider">Required.</param>
		/// <param name="instanceProvider">Optional service instance provider that
		/// will first be checked for constructor argument values.</param>
		/// <param name="tryConstructArguments">If false, all
		/// constructor arguments must resolve from the resolver; or from the
		/// optional <paramref name="instanceProvider"/>. If set true, all constructors
		/// will attempt to construct arguments that do not resolve.</param>
		/// <param name="requireConstructorAttributes">Selects whether the
		/// <see cref="ServiceConstructorMethods"/> will only select
		/// constructors with specified attributes. If this is false, then all
		/// constructors are selected, and attempted with the most arguments first.
		/// If null, then constructors with any <paramref name="constructorAttributeTypes"/>
		/// attributes will be attempted first; and among the double sort, the
		/// ones with the most arguments are tried first. If true, only constructors
		/// with attributes are selected; and again sorted by argument length.</param>
		/// <param name="constructorAttributeTypes">Applies if
		/// <paramref name="requireConstructorAttributes"/>
		/// is not false: this specifies the required or optional constructor
		/// attributes that will be considered. PLEASE NOTICE: if
		/// this argument is empty, then if <paramref name="requireConstructorAttributes"/>
		/// is not false, this WILL ALWAYS ADD the
		/// <see cref="ServiceProviderConstructorAttribute"/>.</param>
		/// <returns>True for success.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryConstruct(
				Type targetType,
				out object service,
				ServiceConstructorRequest request,
				IServiceRegistrationProvider serviceProvider,
				Func<Type, object> instanceProvider = null,
				bool tryConstructArguments = false,
				bool? requireConstructorAttributes = null,
				IReadOnlyCollection<Type> constructorAttributeTypes = null)
		{
			if (targetType == null)
				throw new ArgumentNullException(nameof(targetType));
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if ((constructorAttributeTypes == null)
					|| (constructorAttributeTypes.Count == 0))
				constructorAttributeTypes = new[] { typeof(ServiceProviderConstructorAttribute) };
			if (request.ConstructingTypes.Contains(targetType)) {
				request.TraceStack.Add(
						"Found a recursive constructor dependency"
						+ $": '{targetType.GetFriendlyFullName()}'"
						+ " ... cannot continue.");
				service = null;
				return false;
			}
			request.ConstructingTypes.Add(targetType);
			request.Logger.Verbose("Constructing {0}.", targetType.GetFriendlyFullName());
			if (ServiceConstructorMethods.invokeMember(
					ServiceConstructorMethods.findConstructors(
							targetType,
							requireConstructorAttributes,
							constructorAttributeTypes,
							request.TraceStack),
					false,
					null,
					out service,
					request,
					serviceProvider,
					instanceProvider,
					tryConstructArguments,
					requireConstructorAttributes,
					constructorAttributeTypes,
					false)) {
				request.Logger.Verbose(
						"Construct result for '{0}' is true: '{1}'.",
						targetType.GetFriendlyFullName(),
						service.GetType().GetFriendlyFullName());
				request.ConstructingTypes.Remove(targetType);
				return true;
			}
			request.Logger.Warning("Construct result for '{0}' is false.", targetType.GetFriendlyFullName());
			request.ConstructingTypes.Remove(targetType);
			return false;
		}

		/// <summary>
		/// This static method provides an <see cref="IServiceProvider"/>
		/// implementation that will fully resolve and optionally construct services
		/// to be injected into a method on the <paramref name="target"/>
		/// object that is marked with
		/// the specified <paramref name="attributeType"/>.
		/// The method can be any visibility, and is an instance method.
		/// This implementation will attempt all methods with the attribute, from
		/// the one with the most arguments first; and will stop with the first
		/// successful method. This implementation also supports default
		/// argument values. NOTICE that this method is intended for
		/// <see cref="IServiceProvider"/> implementation: any resolved
		/// dependencies are managed by the container, and are subject to
		/// the lifetime policies there: they will be disposed according to
		/// the container and may not have lifetime affinity with the injected
		/// instance (this also applies recursively if arguments are
		/// constructed).
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
				IServiceRegistrationProvider serviceProvider,
				object target,
				Type attributeType,
				ITrace logger = null,
				Func<Type, object> instanceProvider = null,
				bool tryConstructArguments = false,
				bool? requireConstructorAttributes = null,
				IReadOnlyCollection<Type> constructorAttributeTypes = null)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (attributeType == null)
				throw new ArgumentNullException(nameof(attributeType));
			if ((constructorAttributeTypes == null)
					|| (constructorAttributeTypes.Count == 0))
				constructorAttributeTypes = new[] { typeof(ServiceProviderConstructorAttribute) };
			ServiceConstructorRequest request
					= new ServiceConstructorRequest(logger ?? TraceSources.For(typeof(ServiceConstructorMethods)));
			request.Logger.Verbose(
					"Injecting '{0}' on '{1}'.",
					attributeType.GetFriendlyFullName(),
					target.GetType().GetFriendlyFullName());
			if (ServiceConstructorMethods.invokeMember(
					ServiceConstructorMethods.findMethods(target, attributeType, request.TraceStack),
					true,
					target,
					out _,
					request,
					serviceProvider,
					instanceProvider,
					tryConstructArguments,
					requireConstructorAttributes,
					constructorAttributeTypes,
					false)) {
				request.Logger.Verbose(
						"Inject result for '{0}' on '{1}' is true.",
						attributeType.GetFriendlyFullName(),
						target.GetType().GetFriendlyFullName());
				return true;
			}
			request.Logger.Info(
					"Inject result for '{0}' on '{1}' is false.",
					attributeType.GetFriendlyFullName(),
					target.GetType().GetFriendlyFullName());
			return false;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool invokeMember(
				IEnumerable<MethodBase> members,
				bool isMethod,
				object methodTarget,
				out object newInstance,
				ServiceConstructorRequest request,
				IServiceRegistrationProvider serviceProvider,
				Func<Type, object> instanceProvider,
				bool tryConstructArguments,
				bool? requireConstructorAttributes,
				IReadOnlyCollection<Type> constructorAttributeTypes,
				bool isRecursed)
		{
			newInstance = null;
			bool success = false;
			foreach (MethodBase methodBase in members) {
				ParameterInfo[] parameters = methodBase.GetParameters();
				if (parameters.Length == 0) {
					success = isMethod
							? ServiceConstructorMethods.tryInvokeMethod(
									methodTarget,
									(MethodInfo)methodBase,
									new object[0],
									request.TraceStack)
							: ServiceConstructorMethods.tryInvokeConstructor(
									(ConstructorInfo)methodBase,
									new object[0],
									out newInstance,
									request.TraceStack);
					break;
				}
				List<object> arguments = new List<object>(parameters.Length);
				foreach (ParameterInfo parameterInfo in parameters) {
					if (ServiceConstructorMethods.tryResolve(
							parameterInfo.ParameterType,
							out object parameterResult,
							request,
							serviceProvider,
							instanceProvider,
							!tryConstructArguments)) {
						arguments.Add(parameterResult);
						continue;
					}
					if (tryConstructArguments) {
						if (request.ConstructingTypes.Contains(parameterInfo.ParameterType)) {
							request.TraceStack.Add(
									"Found a recursive constructor dependency"
									+ $": '{parameterInfo.ParameterType.GetFriendlyFullName()}'"
									+ " ... cannot continue.");
							break;
						}
						request.ConstructingTypes.Add(parameterInfo.ParameterType);
						bool argumentSuccess
								= ServiceConstructorMethods.invokeMember(
										ServiceConstructorMethods.findConstructors(
												parameterInfo.ParameterType,
												requireConstructorAttributes,
												constructorAttributeTypes,
												request.TraceStack),
										false,
										null,
										out object argumentResult,
										request,
										serviceProvider,
										instanceProvider,
										true,
										requireConstructorAttributes,
										constructorAttributeTypes,
										true);
						request.ConstructingTypes.Remove(parameterInfo.ParameterType);
						if (argumentSuccess) {
							arguments.Add(argumentResult);
							continue;
						}
					}
					if (parameterInfo.HasDefaultValue) {
						request.TraceStack.Add(
								$"Parameter '{parameterInfo.ParameterType.GetFriendlyFullName()}' has default value.");
						arguments.Add(parameterInfo.DefaultValue);
					} else
						break;
				}
				if ((arguments.Count != parameters.Length)
						|| (isMethod
								? !ServiceConstructorMethods.tryInvokeMethod(
										methodTarget,
										(MethodInfo)methodBase,
										arguments.ToArray(),
										request.TraceStack)
								: !ServiceConstructorMethods.tryInvokeConstructor(
										(ConstructorInfo)methodBase,
										arguments.ToArray(),
										out newInstance,
										request.TraceStack)))
					continue;
				request.Dependencies.TryAddRange(
						methodBase.DeclaringType,
						parameters.Select(parameter => parameter.ParameterType));
				success = true;
				break;
			}
			if (isRecursed)
				return success;
			if (success) {
				if (request.Logger.IsVerbose())
					request.Logger.Verbose(GetTraceMessage());
			} else {
				if (request.Logger.IsInfo())
					request.Logger.Info(GetTraceMessage());
			}
			string GetTraceMessage()
			{
				string separator = $"{Environment.NewLine}    ";
				StringBuilder sb = request.TraceStack.ToConcatenatedString(null, separator);
				if (sb.Length != 0)
					sb.Insert(0, "    ");
				return sb.ToString();
			}
			return success;
		}

		/// <summary>
		/// Utility method resolves the type and logs and returns success.
		/// </summary>
		private static bool tryResolve(
				Type targetType,
				out object result,
				ServiceConstructorRequest request,
				IServiceRegistrationProvider serviceProvider,
				Func<Type, object> instanceProvider,
				bool logMustResolve)
		{
			bool CheckResult(object service, string sourceName, out object successResult)
			{
				if (targetType.IsInstanceOfType(service)) {
					request.TraceStack.Add($"{sourceName} resolved '{targetType.GetFriendlyFullName()}'.");
					successResult = service;
					return true;
				}
				successResult = null;
				return false;
			}
			if (instanceProvider != null) {
				if (CheckResult(instanceProvider(targetType), "InstanceProvider Func", out result))
					return true;
			}
			if (CheckResult(serviceProvider.GetService(targetType, request), nameof(IServiceProvider), out result))
				return true;
			if (serviceProvider.ParentServiceProvider
					is IServiceRegistrationProvider parentServiceRegistrationProvider) {
				if (CheckResult(
						parentServiceRegistrationProvider.GetService(targetType, request),
						nameof(IServiceRegistrationProvider.ParentServiceProvider),
						out result))
					return true;
			} else {
				if (CheckResult(
						serviceProvider.ParentServiceProvider?.GetService(targetType),
						nameof(IServiceRegistrationProvider.ParentServiceProvider),
						out result))
					return true;
			}
			if (logMustResolve)
				request.TraceStack.Add($"Did not resolve '{targetType.GetFriendlyFullName()}'.");
			return false;
		}

		/// <summary>
		/// Utility method invokes the constructor, logs the attempt, and any exception.
		/// </summary>
		private static bool tryInvokeConstructor(
				ConstructorInfo constructor,
				object[] args,
				out object newInstance,
				ISequence<object> traceStack)
		{
			traceStack.Add(
					$"Trying constructor '{constructor}'"
					+ $" for: '{constructor.ReflectedType?.GetFriendlyFullName()}'.");
			try {
				newInstance = constructor.Invoke(args);
				Debug.Assert(constructor.ReflectedType != null, "constructor.ReflectedType != null");
				return constructor.ReflectedType.IsInstanceOfType(newInstance);
			} catch (Exception exception) {
				traceStack.Add(exception);
				newInstance = null;
				return false;
			}
		}

		/// <summary>
		/// Utility method invokes the method, logs the attempt, and any exception.
		/// </summary>
		private static bool tryInvokeMethod(
				object target,
				MethodInfo method,
				object[] args,
				ISequence<object> traceStack)
		{
			traceStack.Add($"Trying method '{method}' for: '{target.GetType().GetFriendlyFullName()}'.");
			try {
				method.Invoke(target, args);
				return true;
			} catch (Exception exception) {
				traceStack.Add(exception);
				return false;
			}
		}

		/// <summary>
		/// Utility method returns all Public and NonPublic constructors,
		/// sorted with the most parameters first.
		/// </summary>
		private static IEnumerable<ConstructorInfo> findConstructors(
				Type type,
				bool? requireAttributes,
				IReadOnlyCollection<Type> attributeTypes,
				ISequence<object> traceStack)
		{
			List<ConstructorInfo> constructors
					= type.GetConstructors(
									BindingFlags.Instance
									| BindingFlags.Public
									| BindingFlags.NonPublic)
							.ToList();
			if (constructors.Count == 0)
				traceStack.Add($"No constructors found for '{type.GetFriendlyFullName()}'.");
			if (requireAttributes == false) {
				constructors.Sort(ServiceConstructorMethods.attributeParameterCountSort<ConstructorInfo>(attributeTypes));
				return constructors;
			}
			if ((requireAttributes == true)
					|| (constructors.FindIndex(
									ServiceConstructorMethods.hasAttributePredicate<ConstructorInfo>(attributeTypes))
							>= 0))
				constructors.RemoveAll(ServiceConstructorMethods.hasAttributePredicate<ConstructorInfo>(attributeTypes, true));
			if (constructors.Count == 0) {
				traceStack.Add(
						$"No selected constructors found for '{type.GetFriendlyFullName()}'"
						+ $" --- {attributeTypes.ToStringCollection(256, t => t.GetFriendlyFullName())}.");
			}
			constructors.Sort(
					ServiceConstructorMethods
							.attributeParameterCountSort<ConstructorInfo>(attributeTypes));
			return constructors;
		}

		/// <summary>
		/// Utility method returns all Public and NonPublic instance methods
		/// with the <paramref name="attributeType"/>,
		/// sorted with the most parameters first.
		/// </summary>
		private static IEnumerable<MethodInfo> findMethods(
				object target,
				Type attributeType,
				ISequence<object> traceStack)
		{
			MethodInfo[] methods
					= target.GetType()
							.GetMethods(
									BindingFlags.Instance
									| BindingFlags.Public
									| BindingFlags.NonPublic)
							.Where(method => method.GetCustomAttribute(attributeType) != null)
							.ToArray();
			Array.Sort(methods, ServiceConstructorMethods.parameterCountSort);
			if (methods.Length == 0) {
				traceStack.Add(
						$"No methods found with '{attributeType.GetFriendlyFullName()}'"
						+ $" on '{target.GetType().GetFriendlyFullName()}'.");
			}
			return methods;
		}


		private static int parameterCountSort<T>(T x, T y)
				where T : MethodBase
			=> -x.GetParameters().Length.CompareTo(y.GetParameters().Length);

		private static Comparison<T> attributeParameterCountSort<T>(IReadOnlyCollection<Type> attributeTypes)
				where T : MethodBase
		{
			int Sort(T x, T y)
				=> ServiceConstructorMethods.hasAttributePredicate<T>(attributeTypes)(x)
						? ServiceConstructorMethods.hasAttributePredicate<T>(attributeTypes)(y)
								? -x.GetParameters().Length.CompareTo(y.GetParameters().Length)
								: -1
						: ServiceConstructorMethods.hasAttributePredicate<T>(attributeTypes)(y)
								? 1
								: -x.GetParameters().Length.CompareTo(y.GetParameters().Length);
			return Sort;
		}

		private static Predicate<T> hasAttributePredicate<T>(IReadOnlyCollection<Type> attributeTypes, bool not = false)
				where T : MethodBase
		{
			bool Predicate(T member)
				=> attributeTypes.ContainsAny(member.CustomAttributes.Select(info => info.AttributeType));
			bool NotPredicate(T member)
				=> !attributeTypes.ContainsAny(member.CustomAttributes.Select(info => info.AttributeType));
			return not
					? (Predicate<T>)NotPredicate
					: Predicate;
		}
	}
}
