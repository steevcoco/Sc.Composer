using Sc.Abstractions.Lifecycle;


namespace Sc.Abstractions.Threading
{
	/// <summary>
	/// Defines a factory or locator for services that will manage async resources.
	/// The services can be notified that async resources should be suspended.
	/// This implements <see cref="ISuspendable"/>.
	/// </summary>
	/// <typeparam name="TService">The service type.</typeparam>
	/// <typeparam name="TConfiguration">Configuration type for each service instance.</typeparam>
	public interface IAsyncServiceProvider<out TService, in TConfiguration>
			: ISuspendable
	{
		/// <summary>
		/// Constructs, or locates an existing service, with the
		/// <paramref name="configuration"/>; and returns it.
		/// </summary>
		/// <param name="configuration">May be optional if the service supports it.</param>
		/// <returns>Not null..</returns>
		TService Get(TConfiguration configuration);
	}
}
