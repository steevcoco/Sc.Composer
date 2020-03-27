using System;


namespace Sc.Abstractions.ServiceLocator
{
	/// <summary>
	/// Provides an attribute to annotate a constructor to be selected
	/// for <see cref="IServiceProvider"/> dependency injection.
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor)]
	public class ServiceProviderConstructorAttribute
			: Attribute { }
}
