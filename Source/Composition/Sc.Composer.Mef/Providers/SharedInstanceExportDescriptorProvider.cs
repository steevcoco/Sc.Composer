using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using Sc.Util.System;


namespace Sc.Composer.Mef.Providers
{
	/// <summary>
	/// Implements a Mef <see cref="ExportDescriptorProvider"/> that returns
	/// a singleton shared object for a given export contract type.
	/// </summary>
	public class SharedInstanceExportDescriptorProvider
			: ExportDescriptorProvider
	{
		/// <summary>
		/// Convenience method that will construct and return a new
		/// <see cref="SharedInstanceExportDescriptorProvider"/>
		/// for the contract type and shared instance.
		/// </summary>
		/// <typeparam name="TExport">The contract type.</typeparam>
		/// <param name="sharedInstance">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static SharedInstanceExportDescriptorProvider For<TExport>(TExport sharedInstance)
			=> new SharedInstanceExportDescriptorProvider(typeof(TExport), sharedInstance);


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="contractType">Required.</param>
		/// <param name="sharedInstance">Required.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If the <c>sharedInstance</c> does
		/// not extend the <c>contractType</c>.</exception>
		public SharedInstanceExportDescriptorProvider(Type contractType, object sharedInstance)
		{
			ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
			SharedInstance = sharedInstance ?? new ArgumentNullException(nameof(sharedInstance));
			if (!ContractType.IsInstanceOfType(SharedInstance)) {
				throw new ArgumentException(
						$"Shared instance '{SharedInstance.GetType().GetFriendlyFullName()}'"
						+ $" does not extend service contract type '{ContractType.GetFriendlyFullName()}'.");
			}
		}


		/// <summary>
		/// The export contract type
		/// </summary>
		public Type ContractType { get; }

		/// <summary>
		/// The singleton instance
		/// </summary>
		public object SharedInstance { get; }

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(
				CompositionContract contract,
				DependencyAccessor descriptorAccessor)
		{
			if (contract.ContractType == ContractType) {
				yield return new ExportDescriptorPromise(
						contract,
						contract.ContractType.FullName,
						true,
						ExportDescriptorProvider.NoDependencies,
						dependencies => ExportDescriptor.Create(
								(context, operation) => SharedInstance,
								ExportDescriptorProvider.NoMetadata));
			}
		}
	}
}
