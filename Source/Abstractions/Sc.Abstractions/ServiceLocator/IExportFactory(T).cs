using System;


namespace Sc.Abstractions.ServiceLocator
{
	/// <summary>
	/// A service that constructs Export objects from registered Import types. Registrations
	/// are made with <see cref="IExportRegistry{TExport}"/>.
	/// Each request for an Export constructs a new instance by default, UNLESS the
	/// service defines otherwise --- it CAN define "singleton" services, or lookup
	/// cached services that are already constructed and should be shared at that time.
	/// The factory constructs requested Exports by fetching a registration made for
	/// a selected Import Type, and the invoker provides optional constructor
	/// dependencies to pass to the Export's constructor (or for method or
	/// property injection if supported). NOTICE ALSO that the provided
	/// dependencies CAN also be used to construct the needed Import at that time;
	/// and the invoker may ALSO pass the Import instance itself.
	/// </summary>
	/// <typeparam name="TExport">Base type implemented by all Exports.</typeparam>
	public interface IExportFactory<out TExport>
	{
		/// <summary>
		/// Constructs a new Export --- or if the Export is a singleton or
		/// is cached, finds any existing instance as the serviuce defines
		/// --- and returns the Export. Also returns the OPTIONAL Import
		/// instance as well: if the Export requires the Import, then
		/// this <paramref name="import"/> argument will be set to the
		/// instance --- which MAY also be constructed here now. If
		/// this factory supports it, and the Import is required by
		/// the Export, then it will be resolved or constructed now
		/// if needed.
		/// </summary>
		/// <param name="importType">Required registered Import type.</param>
		/// <param name="import">This will always be set to the Import
		/// that was requested by the Export if it does request it, and
		/// if the Import instance is not <see langword="null"/> (note
		/// that an Export CAN construct itself with no Import if
		/// it supports that). If the Export does not request the
		/// Import, this is null.</param>
		/// <param name="instanceProvider">Optional func that can provide the
		/// actiual Import instance, and/or any other constructor arguments for the Export;
		/// AND if the service implements, can also provide dependencies used
		/// to construct this Import now. When resolving, constructor arguments will first be passed to
		/// this Func for an instance; and if this returns null, then the container is queried.</param>
		/// <returns>MAY be null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		TExport GetExport(Type importType, out object import, Func<Type, object> instanceProvider = null);
	}
}
