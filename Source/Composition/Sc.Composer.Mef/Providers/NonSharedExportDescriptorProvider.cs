using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;


namespace Sc.Composer.Mef.Providers
{
	/// <summary>
	/// Implements a Mef <see cref="ExportDescriptorProvider"/> that returns
	/// a non-shared object for a given export contract type, from a provided
	/// factory.
	/// </summary>
	public class NonSharedExportDescriptorProvider
			: ExportDescriptorProvider
	{
		/// <summary>
		/// Convenience method that will construct and return a new
		/// <see cref="NonSharedExportDescriptorProvider"/>
		/// for the contract type and non-shared instance factory.
		/// </summary>
		/// <typeparam name="TExport">The contract type.</typeparam>
		/// <param name="factory">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static NonSharedExportDescriptorProvider For<TExport>(Func<object> factory)
			=> new NonSharedExportDescriptorProvider(typeof(TExport), factory);


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="contractType">Required.</param>
		/// <param name="factory">Required.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If the <c>sharedInstance</c> does
		/// not extend the <c>contractType</c>.</exception>
		public NonSharedExportDescriptorProvider(Type contractType, Func<object> factory)
		{
			ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}


		private object activator(LifetimeContext context, CompositionOperation operation)
			=> Factory();


		/// <summary>
		/// The export contract type
		/// </summary>
		public Type ContractType { get; }

		/// <summary>
		/// The non-shared instance factory.
		/// </summary>
		public Func<object> Factory { get; }

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(
				CompositionContract contract,
				DependencyAccessor descriptorAccessor)
		{
			if (contract.ContractType == ContractType) {
				yield return new ExportDescriptorPromise(
						contract,
						contract.ContractType.FullName,
						false,
						ExportDescriptorProvider.NoDependencies,
						dependencies => ExportDescriptor.Create(activator, ExportDescriptorProvider.NoMetadata));
			}
		}
	}
}
