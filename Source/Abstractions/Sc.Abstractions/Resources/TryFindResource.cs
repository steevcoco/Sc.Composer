using System;


namespace Sc.Abstractions.Resources
{
	/// <summary>
	/// Defines a delegate that finds resources. The delegate receives
	/// the resource key to find, and returns the resource if found;
	/// or null.
	/// </summary>
	/// <param name="resourceKey">The resource key to find.</param>
	/// <returns>The resource if found; or null.</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public delegate object TryFindResource(object resourceKey);
}
