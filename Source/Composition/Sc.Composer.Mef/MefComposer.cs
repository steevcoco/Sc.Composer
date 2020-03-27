using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Sc.Composer.Mef.Providers;
using Sc.Diagnostics;
using Sc.Util.Collections;
using Sc.Util.System;

namespace Sc.Composer.Mef
{
	/// <summary>
	/// A complete <see cref="IComposer{TTarget}"/> implementation
	/// for Mef composition, that can also be subclassed. This is a
	/// <see cref="Composer{TTarget}"/> for a <see cref="ContainerConfiguration"/>
	/// target. Participants configure the <see cref="ContainerConfiguration"/>
	/// --- which is then provided by the composer when composed.
	/// Static methods are provided to perform one-time compositions.
	/// </summary>
	public class MefComposer
			: Composer<ContainerConfiguration>
	{
		/// <summary>
		/// Import target.
		/// </summary>
		/// <typeparam name="TExport"></typeparam>
		/// <typeparam name="TMetadata"></typeparam>
		private sealed class Container<TExport, TMetadata>
		{
			[ImportMany]
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public ExportFactory<TExport, TMetadata>[] Exports { get; set; }


			public override string ToString()
				=> $"{GetType().GetFriendlyName()}"
						+ $"["
						+ $"{nameof(Exports)}: {Exports.ToStringCollection(1024)}"
						+ $"]";
		}


		/// <summary>
		/// Empty Mef meta data.
		/// </summary>
		private class EmptyMetaData { }


		private static List<(TExport export, TMetadata metadata)> getInstances<TExport, TMetadata>(
				bool? onlyDerivedTypesExclusively,
				Func<TMetadata, bool> metaDataPredicate,
				IEnumerable<IProvideParts<ContainerConfiguration>> partsProviders,
				Assembly[] assemblies)
		{
			using (MefComposer composer
					= new MefComposer(
							MefComposerHelper.CreateDefaultConventions(
									typeof(TExport),
									onlyDerivedTypesExclusively))) {
				if (partsProviders != null) {
					foreach (IProvideParts<ContainerConfiguration> partsProvider in partsProviders) {
						composer.Participate(partsProvider);
					}
				}
				if ((assemblies != null)
						&& (assemblies.Length != 0))
					composer.Participate(new MefAssemblyPartProvider(assemblies));
				using (CompositionHost compositionHost
						= composer.Compose()
								.CreateContainer()) {
					TraceSources.For<MefComposer>()
							.Verbose("Constructed new {0}: {1}", nameof(CompositionHost), compositionHost);
					Container<TExport, TMetadata> container = new Container<TExport, TMetadata>();
					compositionHost.SatisfyImports(container);
					TraceSources.For<MefComposer>()
							.Verbose("{0} for {1}: {2}",
							nameof(CompositionContextExtensions.SatisfyImports),
							typeof(TExport).GetFriendlyFullName(),
							container);
					return container.Exports
							.Where(export => metaDataPredicate?.Invoke(export.Metadata) ?? true)
							.Select(
									export
											=> (export.CreateExport()
															.Value,
													export.Metadata))
							.ToList();
				}
			}
		}


		/// <summary>
		/// Convenience method that creates a new <see cref="MefComposer"/> with a
		/// parts provider for your Assemblies, and an optional parts
		/// provider for your shared instances; performs one composition;
		/// and then returns the composed exports.
		/// Notice that this method wil propagate any Mef exceptions.
		/// All resources are disposed before returning.
		/// The composer composes only for the single type.
		/// If <paramref name="onlyDerivedTypesExclusively"/> is null, then both
		/// <see cref="ConventionBuilder.ForType"/> and
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> are applied.
		/// If If true, then only <see cref="ConventionBuilder.ForTypesDerivedFrom"/>
		/// is applied. And if false, then only <see cref="ConventionBuilder.ForType"/>
		/// is applied.
		/// </summary>
		/// <typeparam name="TExport">The single export type for the composer.</typeparam>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <param name="sharedInstances">Optional list of shared instances to
		/// provide for the composition.</param>
		/// <param name="assemblies">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If <c>assemblies</c> is
		/// empty or contains a null element.</exception>
		/// <exception cref="AggregateComposerException">If there is an unhandled
		/// <see cref="IComposer{TTarget}"/> error.</exception>
		public static List<TExport> GetInstances<TExport>(
				bool? onlyDerivedTypesExclusively,
				IEnumerable<(Type contractType, object sharedInstance)> sharedInstances,
				params Assembly[] assemblies)
		{
			if (assemblies == null)
				throw new ArgumentNullException(nameof(assemblies));
			if ((assemblies.Length == 0)
					|| assemblies.Any(assembly => assembly == null)) {
				throw new ArgumentException(
						assemblies.ToStringCollection()
								.ToString(),
						nameof(assemblies));
			}
			return sharedInstances == null
					? MefComposer.getInstances<TExport, EmptyMetaData>(
									onlyDerivedTypesExclusively,
									null,
									null,
									assemblies)
							.Select(tuple => tuple.export)
							.ToList()
					: MefComposer.getInstances<TExport, EmptyMetaData>(
									onlyDerivedTypesExclusively,
									null,
									ExportDescriptorPartProvider.ForSharedInstances(sharedInstances.ToArray())
											.AsSingle(),
									assemblies)
							.Select(tuple => tuple.export)
							.ToList();
		}

		/// <summary>
		/// Convenience method that creates a new <see cref="MefComposer"/> with a
		/// parts provider for your Assemblies, and an optional parts
		/// provider for your shared instances; performs one composition;
		/// and then returns the composed exports.
		/// Notice that this method wil propagate any Mef exceptions.
		/// All resources are disposed before returning.
		/// The composer composes only for the single type.
		/// If <paramref name="onlyDerivedTypesExclusively"/> is null, then both
		/// <see cref="ConventionBuilder.ForType"/> and
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> are applied.
		/// If If true, then only <see cref="ConventionBuilder.ForTypesDerivedFrom"/>
		/// is applied. And if false, then only <see cref="ConventionBuilder.ForType"/>
		/// is applied. This method allows you to specify
		/// <see cref="ExportMetadataAttribute"/> data to filter the results.
		/// </summary>
		/// <typeparam name="TExport">The single export type for the composer.</typeparam>
		/// <typeparam name="TMetadata">The <see cref="ExportMetadataAttribute"/>
		/// type.</typeparam>
		/// <param name="metaDataPredicate">Optional predicate for
		/// <see cref="ExportMetadataAttribute"/> metadata.</param>
		/// <param name="onlyDerivedTypesExclusively">Allows you to specify only
		/// <see cref="ConventionBuilder.ForTypesDerivedFrom"/> (if true), only
		/// <see cref="ConventionBuilder.ForType"/> (if false), or both (if null).
		/// If the type is abstract then ForType is not applied.</param>
		/// <param name="sharedInstances">Optional list of shared instances to
		/// provide for the composition.</param>
		/// <param name="assemblies">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If <c>assemblies</c> is
		/// empty or contains a null element.</exception>
		/// <exception cref="AggregateComposerException">If there is an unhandled
		/// <see cref="IComposer{TTarget}"/> error.</exception>
		public static List<(TExport, TMetadata)> GetInstances<TExport, TMetadata>(
				Func<TMetadata, bool> metaDataPredicate,
				bool? onlyDerivedTypesExclusively,
				IEnumerable<(Type contractType, object sharedInstance)> sharedInstances,
				params Assembly[] assemblies)
		{
			if (assemblies == null)
				throw new ArgumentNullException(nameof(assemblies));
			if ((assemblies.Length == 0)
					|| assemblies.Any(assembly => assembly == null)) {
				throw new ArgumentException(
						assemblies.ToStringCollection()
								.ToString(),
						nameof(assemblies));
			}
			return sharedInstances == null
					? MefComposer.getInstances<TExport, TMetadata>(
									onlyDerivedTypesExclusively,
									metaDataPredicate,
									null,
									assemblies)
							.ToList()
					: MefComposer.getInstances<TExport, TMetadata>(
									onlyDerivedTypesExclusively,
									metaDataPredicate,
									ExportDescriptorPartProvider.ForSharedInstances(sharedInstances.ToArray())
											.AsSingle(),
									assemblies)
							.ToList();
		}


		private AttributedModelProvider defaultConventions;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="defaultConventions">Optional conventions used on each composition
		/// to construct each <see cref="ContainerConfiguration"/>.</param>
		/// <param name="disposeProvidersWithThis">Specifies how added providers are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		public MefComposer(
				AttributedModelProvider defaultConventions = null,
				bool disposeProvidersWithThis = true)
				: base(disposeProvidersWithThis)
			=> this.defaultConventions = defaultConventions;


		/// <summary>
		/// This method is responsible for constructing the <see cref="ContainerConfiguration"/>
		/// that is passed to all handlers on each composition.
		/// This implementation constructs a
		/// new instance every time, using these optional <see cref="DefaultConventions"/>
		/// </summary>
		/// <returns>Not null.</returns>
		protected override ContainerConfiguration GetTarget()
		{
			ContainerConfiguration configuration = new ContainerConfiguration();
			if (DefaultConventions != null)
				configuration.WithDefaultConventions(DefaultConventions);
			return configuration;
		}


		/// <summary>
		/// The default conventions provided on construction. Also mutable!
		/// </summary>
		public virtual AttributedModelProvider DefaultConventions
		{
			get {
				lock (SyncLock) {
					return defaultConventions;
				}
			}
			set {
				lock (SyncLock) {
					defaultConventions = value;
				}
			}
		}
	}
}
