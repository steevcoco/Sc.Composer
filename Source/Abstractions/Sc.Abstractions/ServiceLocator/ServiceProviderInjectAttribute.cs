using System;


namespace Sc.Abstractions.ServiceLocator
{
	/// <summary>
	/// Provides an attribute to annotate a Method or Property
	/// for <see cref="IServiceProvider"/> dependency injection.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class ServiceProviderInjectAttribute
			: Attribute { }
}
