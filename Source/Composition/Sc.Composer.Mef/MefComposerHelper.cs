using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Sc.Abstractions.ServiceLocator;
using Sc.Composer.Mef.Providers;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Composer.Mef
{
	/// <summary>
	/// Static helper methods for <see cref="MefComposer"/>.
	/// </summary>
	public static class MefComposerHelper
	{
		/// <summary>
		/// Static helper method creates a new <see cref="ConventionBuilder"/>, and Exports
		/// the <paramref name="exportType"/>; applying
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/>,
		/// <see cref="ConventionBuilder.ForType"/>, or both according to
		/// <paramref name="onlyDerivedTypesExclusively"/>.
		/// </summary>
		/// <param name="exportType">Not null.</param>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).</param>
		/// <returns>Not null.</returns>
		internal static ConventionBuilder CreateDefaultConventions(
				Type exportType,
				bool? onlyDerivedTypesExclusively)
			=> new ConventionBuilder()
					.ApplyConventions(exportType, onlyDerivedTypesExclusively);

		/// <summary>
		/// Static helper method Exports the <paramref name="exportType"/> on the given
		/// <paramref name="conventions"/>; applying
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/>,
		/// <see cref="ConventionBuilder.ForType"/>, or both according to
		/// <paramref name="onlyDerivedTypesExclusively"/>.
		/// </summary>
		/// <param name="conventions">Not null.</param>
		/// <param name="exportType">Not null.</param>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <returns>Returns the given <paramref name="conventions"/>
		/// for chaining.</returns>
		internal static ConventionBuilder ApplyConventions(
				this ConventionBuilder conventions,
				Type exportType,
				bool? onlyDerivedTypesExclusively)
		{
			if (conventions == null)
				throw new ArgumentNullException(nameof(conventions));
			if (exportType == null)
				throw new ArgumentNullException(nameof(exportType));
			switch (onlyDerivedTypesExclusively) {
				case true :
				case false when exportType.IsAbstract :
				case false when exportType.IsInterface :
					conventions.ForTypesDerivedFrom(exportType)
							.Export();
					break;
				case false :
					conventions.ForType(exportType)
							.Export();
					break;
				default :
					if (!exportType.IsInterface
							&& !exportType.IsAbstract) {
						conventions.ForType(exportType)
								.Export();
					}
					conventions.ForTypesDerivedFrom(exportType)
							.Export();
					break;
			}
			return conventions;
		}


		/// <summary>
		/// This helper method returns all non-abstract types in this
		/// <paramref name="assembly"/> that have an <see cref="ExportAttribute"/>
		/// whose <see cref="ExportAttribute.ContractType"/> is <typeparamref name="TExport"/>,
		/// and where that type does also extend <typeparamref name="TExport"/>.
		/// The type and all defined <see cref="ExportMetadataAttribute"/> attributes
		/// on it are returned. Does not return abstract types nor interfaces.
		/// </summary>
		/// <typeparam name="TExport">The export type to locate.</typeparam>
		/// <param name="assembly">Required.</param>
		/// <returns>Not null.</returns>
		public static IEnumerable<(Type exportType, ExportMetadataAttribute[] metaData)> FindExportTypes<TExport>(
				this Assembly assembly)
		{
			return assembly.GetTypes()
					.Where(TypePredicate)
					.Select(
							type => (type,
									type.GetCustomAttributes<ExportMetadataAttribute>()
											.ToArray()));
			bool TypePredicate(Type type)
				=> !type.IsAbstract
						&& !type.IsInterface
						&& typeof(TExport).IsAssignableFrom(type)
						&& (type.GetCustomAttribute<ExportAttribute>()
										?.ContractType
								== typeof(TExport));
		}


		/// <summary>
		/// Convenience method that creates a new <see cref="MefComposer"/> with a
		/// parts provider for your Assemblies, and an optional list of parts providers
		/// for your optional shared instances; and performs one composition;
		/// and then returns the composed exports.
		/// Notice that this method wil propagate any Mef exceptions.
		/// All resources are disposed before returning.
		/// The composer composes only for the single type:
		/// if <paramref name="onlyDerivedTypesExclusively"/> is false, then only
		/// <see cref="ConventionBuilder.ForType"/> is applied; and if null, then both
		/// <see cref="ConventionBuilder.ForType"/> and
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> are applied; and if true,
		/// then only <see cref="ConventionBuilder.ForTypesDerivedFrom"/> is applied.
		/// </summary>
		/// <typeparam name="TExport">The single export type for the composer.</typeparam>
		/// <param name="assembly">Required.</param>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <param name="sharedInstances">Optional.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="AggregateComposerException">If there is an unhandled
		/// <see cref="IComposer{TTarget}"/> error.</exception>
		public static List<TExport> GetInstances<TExport>(
				this Assembly assembly,
				bool? onlyDerivedTypesExclusively = null,
				IEnumerable<(Type contractType, object sharedInstance)> sharedInstances = null)
			=> MefComposer.GetInstances<TExport>(
					onlyDerivedTypesExclusively,
					sharedInstances,
					new[]
					{
						assembly ?? throw new ArgumentNullException(nameof(assembly))
					});

		/// <summary>
		/// Convenience method that creates a new <see cref="MefComposer"/> with a
		/// parts provider for your Assemblies, and an optional list of parts providers
		/// for your optional shared instances; and performs one composition;
		/// and then returns the composed exports.
		/// Notice that this method wil propagate any Mef exceptions.
		/// All resources are disposed before returning.
		/// The composer composes only for the single type:
		/// if <paramref name="onlyDerivedTypesExclusively"/> is false, then only
		/// <see cref="ConventionBuilder.ForType"/> is applied; and if null, then both
		/// <see cref="ConventionBuilder.ForType"/> and
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> are applied; and if true,
		/// then only <see cref="ConventionBuilder.ForTypesDerivedFrom"/> is applied.
		/// This method allows you to specify <see cref="ExportMetadataAttribute"/> data
		/// to filter the results.
		/// </summary>
		/// <typeparam name="TExport">The single export type for the composer.</typeparam>
		/// <typeparam name="TMetadata">The <see cref="ExportMetadataAttribute"/>
		/// type.</typeparam>
		/// <param name="assembly">Required.</param>
		/// <param name="metaDataPredicate">Optional.</param>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <param name="sharedInstances">Optional.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If <c>assemblies</c> is
		/// empty or contains a null element.</exception>
		/// <exception cref="AggregateComposerException">If there is an unhandled
		/// <see cref="IComposer{TTarget}"/> error.</exception>
		public static List<(TExport, TMetadata)> GetInstances<TExport, TMetadata>(
				this Assembly assembly,
				bool? onlyDerivedTypesExclusively = null,
				Func<TMetadata, bool> metaDataPredicate = null,
				IEnumerable<(Type contractType, object sharedInstance)> sharedInstances = null)
			=> MefComposer.GetInstances<TExport, TMetadata>(
					metaDataPredicate,
					onlyDerivedTypesExclusively,
					sharedInstances,
					new[]
					{
						assembly ?? throw new ArgumentNullException(nameof(assembly))
					});


		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="SharedInstanceExportDescriptorProvider"/>
		/// to this <see cref="IComposer{TTarget}"/>;
		/// which provides your <c>sharedInstance</c>.
		/// </summary>
		/// <typeparam name="TExport">The export contract type.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="sharedInstance">Required.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/> argument value.</param>
		/// <returns>The new <see cref="ExportDescriptorPartProvider"/> for
		/// further configuration.</returns>
		public static ExportDescriptorPartProvider WithSharedInstance<TExport>(
				this IComposer<ContainerConfiguration> composer,
				TExport sharedInstance,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			ExportDescriptorPartProvider exportDescriptorPartProvider
					= ExportDescriptorPartProvider.ForSharedInstance(sharedInstance);
			composer.Participate(exportDescriptorPartProvider, oneTimeOnly);
			return exportDescriptorPartProvider;
		}

		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="SharedInstanceExportDescriptorProvider"/>
		/// to this <see cref="IComposer{TTarget}"/>;
		/// which provides your <c>sharedInstances</c>.
		/// </summary>
		/// <param name="composer">Required.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/> argument value.</param>
		/// <param name="sharedInstances">Required.</param>
		/// <returns>The new <see cref="ExportDescriptorPartProvider"/> for
		/// further configuration.</returns>
		public static ExportDescriptorPartProvider WithSharedInstances(
				this IComposer<ContainerConfiguration> composer,
				bool oneTimeOnly = false,
				params (Type contractType, object instances)[] sharedInstances)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			ExportDescriptorPartProvider exportDescriptorPartProvider
					= ExportDescriptorPartProvider.ForSharedInstances(sharedInstances);
			composer.Participate(exportDescriptorPartProvider, oneTimeOnly);
			return exportDescriptorPartProvider;
		}


		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="SharedInstanceExportDescriptorProvider"/>
		/// to this <see cref="IComposer{TTarget}"/>;
		/// which provides your <c>factory</c>.
		/// </summary>
		/// <typeparam name="TExport">The export contract type.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="factory">Required.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/> argument value.</param>
		/// <returns>The new <see cref="ExportDescriptorPartProvider"/> for
		/// further configuration.</returns>
		public static ExportDescriptorPartProvider WithNonSharedInstance<TExport>(
				this IComposer<ContainerConfiguration> composer,
				Func<object> factory,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			ExportDescriptorPartProvider exportDescriptorPartProvider
					= ExportDescriptorPartProvider.ForNonSharedInstance<TExport>(factory);
			composer.Participate(exportDescriptorPartProvider, oneTimeOnly);
			return exportDescriptorPartProvider;
		}

		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="NonSharedExportDescriptorProvider"/>
		/// to this <see cref="IComposer{TTarget}"/>;
		/// which provides your <c>factories</c>.
		/// </summary>
		/// <param name="composer">Required.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/> argument value.</param>
		/// <param name="factories">Required.</param>
		/// <returns>The new <see cref="ExportDescriptorPartProvider"/> for
		/// further configuration.</returns>
		public static ExportDescriptorPartProvider WithNonSharedInstances(
				this IComposer<ContainerConfiguration> composer,
				bool oneTimeOnly = false,
				params (Type contractType, Func<object> factory)[] factories)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			ExportDescriptorPartProvider exportDescriptorPartProvider
					= ExportDescriptorPartProvider.ForNonSharedInstances(factories);
			composer.Participate(exportDescriptorPartProvider, oneTimeOnly);
			return exportDescriptorPartProvider;
		}


		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="MefAssemblyPartProvider"/> to this
		/// <see cref="IComposer{TTarget}"/>;
		/// which provides your list of assemblies.
		/// </summary>
		/// <param name="composer">Required.</param>
		/// <param name="assemblies">Required.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/>
		/// argument value.</param>
		/// <param name="conventions">Optional: sets
		/// <see cref="MefAssemblyPartProvider.Conventions"/>; and CAN be null.</param>
		/// <returns>The new <see cref="MefAssemblyPartProvider"/> for
		/// further configuration.</returns>
		public static MefAssemblyPartProvider WithAssemblies(
				this IComposer<ContainerConfiguration> composer,
				IEnumerable<Assembly> assemblies,
				bool oneTimeOnly = false,
				AttributedModelProvider conventions = null)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			MefAssemblyPartProvider assemblyPartProvider
					= new MefAssemblyPartProvider(assemblies, conventions);
			composer.Participate(assemblyPartProvider, oneTimeOnly);
			return assemblyPartProvider;
		}

		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="MefAssemblyPartProvider"/> to this
		/// <see cref="IComposer{TTarget}"/>;
		/// which provides your list of assemblies.
		/// </summary>
		/// <param name="composer">Required.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/>
		/// argument value.</param>
		/// <param name="conventions">Optional: sets
		/// <see cref="MefAssemblyPartProvider.Conventions"/>; and CAN be null.</param>
		/// <param name="assemblies">Required.</param>
		/// <returns>The new <see cref="MefAssemblyPartProvider"/> for
		/// further configuration.</returns>
		public static MefAssemblyPartProvider WithAssemblies(
				this IComposer<ContainerConfiguration> composer,
				bool oneTimeOnly,
				AttributedModelProvider conventions,
				params Assembly[] assemblies)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			MefAssemblyPartProvider assemblyPartProvider
					= new MefAssemblyPartProvider(assemblies, conventions);
			composer.Participate(assemblyPartProvider, oneTimeOnly);
			return assemblyPartProvider;
		}

		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="MefAssemblyPartProvider"/> to this
		/// <see cref="IComposer{TTarget}"/>;
		/// which provides your list of assemblies.
		/// </summary>
		/// <param name="composer">Required.</param>
		/// <param name="assembly">Required.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/>
		/// argument value.</param>
		/// <param name="conventions">Optional: sets
		/// <see cref="MefAssemblyPartProvider.Conventions"/>; and CAN be null.</param>
		/// <returns>The new <see cref="MefAssemblyPartProvider"/> for
		/// further configuration.</returns>
		public static MefAssemblyPartProvider WithAssembly(
				this IComposer<ContainerConfiguration> composer,
				Assembly assembly,
				bool oneTimeOnly = false,
				AttributedModelProvider conventions = null)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));
			MefAssemblyPartProvider assemblyPartProvider
					= new MefAssemblyPartProvider(assembly.AsSingle(), conventions);
			composer.Participate(assemblyPartProvider, oneTimeOnly);
			return assemblyPartProvider;
		}


		/// <summary>
		/// Convenience method that will construct and add a new
		/// <see cref="MefDirectoryPartWatcher"/> to this
		/// <see cref="IComposer{TTarget}"/>;
		/// which watches your directory <c>path</c> for assemblies.
		/// </summary>
		/// <param name="composer">Required.</param>
		/// <param name="path">Required directory path to watch.</param>
		/// <param name="provideAssembliesOneTimeOnly">Sets
		/// <see cref="MefDirectoryPartWatcher.ProvideAssembliesOneTimeOnly"/>.</param>
		/// <param name="searchOption">Sets
		/// <see cref="MefDirectoryPartWatcher.SearchOption"/>.</param>
		/// <param name="conventions">Optional: sets
		/// <see cref="MefDirectoryPartWatcher.Conventions"/>; and CAN be null.</param>
		/// <param name="oneTimeOnly"><see cref="IComposer{TTarget}"/>
		/// argument value.</param>
		/// <returns>The new <see cref="MefDirectoryPartWatcher"/> for
		/// further configuration.</returns>
		public static MefDirectoryPartWatcher WatchDirectory(
				this IComposer<ContainerConfiguration> composer,
				string path,
				bool provideAssembliesOneTimeOnly = false,
				SearchOption searchOption = SearchOption.TopDirectoryOnly,
				AttributedModelProvider conventions = null,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			MefDirectoryPartWatcher directoryPartWatcher
					= new MefDirectoryPartWatcher(path, searchOption, conventions)
					{
						ProvideAssembliesOneTimeOnly = provideAssembliesOneTimeOnly
					};
			composer.Participate(directoryPartWatcher, oneTimeOnly);
			return directoryPartWatcher;
		}


		/// <summary>
		/// Provides a helper method for an
		/// <see cref="IExportRegistry{TExport}"/>, that will fetch all
		/// exported <see cref="ExportAttribute"/> types from the
		/// <paramref name="assemblies"/> that you provide, that export
		/// your <typeparamref name="TExport"/> base
		/// Type (exactly); and registers the Import and Export
		/// types in your <paramref name="exportRegistry"/>. The type must
		/// include an <see cref="ExportMetadataAttribute"/> that will
		/// specify the registered Import type. The located Export Types
		/// are the registered <typeparamref name="TExport"/> implementations.
		/// The type must define an <see cref="ExportMetadataAttribute"/>
		/// that will define the Import type for which it will be
		/// registered. The meta data attribute's Name must
		/// be the short name of the base Export Type
		/// <typeparamref name="TExport"/>, and the attribute's Value
		/// must be the registered Import Type. E.G.:
		/// <code>
		/// IExportRegistry&lt;IView&gt; { }
		/// 
		/// [Export(typeof(IView))]
		/// [ExportMetadata(nameof(IView), typeof(ThisViewsViewModel))]
		/// public class MyView : IView {}
		/// </code>
		/// The registration made is:
		/// Import Type = typeof(ThisViewsViewModel) (from the Meta Data),
		/// and Export Type = typeof(MyView) (this Export's actual type,
		/// located by the Export attribute ON this type).
		/// This method searches each Assembly for all Exports and registers
		/// each in the registry. An Export may declare more than one
		/// ExportMetadata registration type.
		/// </summary>
		/// <typeparam name="TExport">The registry's Export type.</typeparam>
		/// <param name="exportRegistry">Not null.</param>
		/// <param name="assemblies">Not null or empty.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="InvalidOperationException">If a registration
		/// already exists.</exception>
		public static void RegisterExports<TExport>(
				this IExportRegistry<TExport> exportRegistry,
				params Assembly[] assemblies)
		{
			if (exportRegistry == null)
				throw new ArgumentNullException(nameof(exportRegistry));
			if (assemblies == null)
				throw new ArgumentNullException(nameof(assemblies));
			if (assemblies.Length == 0)
				throw new ArgumentException(nameof(Array.Length), nameof(assemblies));
			foreach ((Type exportType, ExportMetadataAttribute[] metaData)
					in assemblies
							.NotNull()
							.SelectMany(assembly => assembly.FindExportTypes<TExport>())) {
				foreach (Type importType
						in metaData.Where(attribute => (attribute.Value is Type) && IsExportName(attribute.Name))
								.Select(attribute => attribute.Value as Type)) {
					exportRegistry.RegisterExport(importType, exportType);
				}
			}
			bool IsExportName(string name)
				=> (name == typeof(TExport).Name)
						|| (name == typeof(TExport).GetFriendlyName(true))
						|| (name == typeof(TExport).GetFriendlyName(false));
		}
	}
}
