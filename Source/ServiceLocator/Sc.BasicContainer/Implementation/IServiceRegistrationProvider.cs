using System;


namespace Sc.BasicContainer.Implementation
{
	/// <summary>
	/// This is an <see cref="IServiceProvider"/> implementation interface for
	/// <see cref="ServiceConstructorMethods"/>
	/// and <see cref="ServiceRegistration"/> implementations.
	/// </summary>
	internal interface IServiceRegistrationProvider
			: IServiceProvider
	{
		/// <summary>
		/// This implementation method must perform as with
		/// <see cref="IServiceProvider"/> <see cref="IServiceProvider.GetService"/>:
		/// if the requested service is registered, it must be returned by
		/// invoking this <see cref="ServiceRegistration"/> WITH the
		/// provided <paramref name="serviceConstructorRequest"/>
		/// argument --- which the <see cref="ServiceConstructorMethods"/>
		/// is using to track an ongoing construction. The given argument
		/// MUST be passed as-is if not null; and otherwise, this
		/// service provider implementation must construct a new request
		/// and pass that to the <see cref="ServiceRegistration"/>
		/// --- which then becomes the top-level request for this
		/// operation (this method can be invoked as the implementation
		/// for <see cref="IServiceProvider.GetService"/>, by creating
		/// a new request now).
		/// </summary>
		/// <param name="serviceType">Required service Type to resolve
		/// or construct.</param>
		/// <param name="serviceConstructorRequest">This argument must be propagated
		/// as-is if not null.</param>
		/// <returns>As with <see cref="IServiceProvider"/>.</returns>
		object GetService(Type serviceType, ServiceConstructorRequest serviceConstructorRequest);

		/// <summary>
		/// If this instance has been added to another as a scoped instance,
		/// then this is the parent <see cref="IServiceRegistrationProvider"/>
		/// to which this has been directly added.
		/// </summary>
		IServiceProvider ParentServiceProvider { get; }
	}
}
