using System;
using System.Collections.Generic;


namespace Sc.Abstractions.ServiceLocator
{
	/// <summary>
	/// Provides a Type registration interface that registers
	/// single "Import" types for "Export" types.
	/// The types do not need to be variant. The factory
	/// constructs requested Exports by fetching a registration made for a selected
	/// Import Type. The factory's invoker may provide optional constructor
	/// dependencies to pass to the Export's constructor, and may also provide
	/// the Import instance itself at that time; and in addition,
	/// provided dependencies MAY be used to construct the
	/// Import on demand at that time.
	/// </summary>
	/// <typeparam name="TExport">Base type implemented by all Exports.</typeparam>
	public interface IExportRegistry<in TExport>
	{
		/// <summary>
		/// Registers an Export type for the Import type.
		/// </summary>
		/// <param name="importType">The Import type.</param>
		/// <param name="exportType">The Export type.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If the <paramref name="exportType"/>
		/// is not assignable to <typeparamref name="TExport"/>.</exception>
		/// <exception cref="InvalidOperationException">If the registration
		/// already exists.</exception>
		void RegisterExport(Type importType, Type exportType);

		/// <summary>
		/// This method returns all registrations: the Keys are
		/// Import types, and Values are Export types.
		/// </summary>
		IEnumerable<KeyValuePair<Type, Type>> GetAllRegistrations();
	}
}
