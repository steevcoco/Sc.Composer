using System;
using Sc.Abstractions.ServiceLocator;
using Sc.BasicContainer.Implementation;
using Sc.Diagnostics;
using Sc.Util.System;


namespace Sc.BasicContainer
{
	/// <summary>
	/// Complete <see cref="IExportFactory{TExport}"/> implementation.
	/// A <see cref="ServiceConstructor"/> is used to construct all services.
	/// NOTICE that this implementation WILL construct an Import for an Export
	/// constructor on demand: if the provided Import is null, and the Export
	/// requests the Import, the service constructor will be used
	/// to construct one. You may instrument further options by controlling
	/// the provided <see cref="ServiceConstructor"/>; and also override
	/// this <see cref="OnGetService(Type, Type, Func{Type, object})"/> method.
	/// </summary>
	/// <typeparam name="TExport">Base type implemented by all Exports.</typeparam>
	public class ExportFactory<TExport>
			: IExportFactory<TExport>,
					IDisposable
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="exportRegistry">Required.</param>
		/// <param name="serviceProvider">NOTICE: is optional.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ExportFactory(IExportRegistry<TExport> exportRegistry, IServiceProvider serviceProvider)
		{
			ExportRegistry = exportRegistry ?? throw new ArgumentNullException(nameof(exportRegistry));
			ServiceProvider = serviceProvider ?? ServiceProviderHelper.AsServiceProvider(EmptyProvider);
			ServiceConstructor = new ServiceConstructor(serviceProvider);
			static object EmptyProvider(Type _)
				=> null;
		}


		/// <summary>
		/// Type registry.
		/// </summary>
		protected IExportRegistry<TExport> ExportRegistry { get; }

		/// <summary>
		/// Used to construct all services.
		/// </summary>
		protected ServiceConstructor ServiceConstructor { get; }

		/// <summary>
		/// The optional provider given on construction: note: if none
		/// was provided, then a non-null "no-op" is set here.
		/// </summary>
		protected IServiceProvider ServiceProvider { get; }


		/// <summary>
		/// Provided as a convenience for subclasses: this is invoked from
		/// <see cref="GetExport"/> for all requests: that method has been provided
		/// the given <paramref name="importType"/> IMPORT Type, and has fetched the given
		/// <paramref name="exportType"/> EXPORT Type for that Import type.
		/// This method must construct the Export.
		/// This implementation invokes the <see cref="ServiceConstructor"/>
		/// with the <paramref name="instanceProvider"/>; AND, provides a delegate
		/// instance provider that will NOW construct the Import on demand
		/// if requested.
		/// </summary>
		/// <param name="exportType">The fetched EXPORT type. Not null.</param>
		/// <param name="importType">The provided IMPORT type. Not null.</param>
		/// <param name="instanceProvider">This is ONLY the optional func passed to the
		/// <see cref="IExportFactory{TExport}"/> method. This
		/// method implementation provides an additional delegate to construct
		/// the Import if requested and not resolved elsewhere.</param>
		/// <returns>Can be null.</returns>
		protected virtual TExport HandleGetExport(
				Type exportType,
				Type importType,
				out object import,
				Func<Type, object> instanceProvider = null)
		{
			object importInstance = default;
			if (ServiceConstructor.TryResolveOrConstruct(exportType, out object service, Provider)
					&& service is TExport export) {
				import = importInstance;
				return export;
			}
			import = importInstance;
			return default;
			object Provider(Type type)
			{
				if (type != importType)
					return instanceProvider?.Invoke(type);
				importInstance = instanceProvider?.Invoke(type);
				if (!type.IsInstanceOfType(importInstance))
					ServiceConstructor.TryResolveOrConstruct(type, out importInstance, instanceProvider);
				return importInstance;
			}
		}


		public TExport GetExport(Type importType, out object import, Func<Type, object> instanceProvider = null)
		{
			if (importType == null)
				throw new ArgumentNullException(nameof(importType));
			if (!ExportRegistry.TryFindExportType(importType, out Type exportType)) {
				import = default;
				return default;
			}
			TraceSources.For(GetType())
					.Verbose(
							$"{nameof(IExportFactory<TExport>.GetExport)}:"
							+ " '{0} for {1}'.",
							exportType.GetFriendlyFullName(),
							importType.GetFriendlyFullName());
			return HandleGetExport(exportType, importType, out import, instanceProvider);
		}


		/// <summary>
		/// Invoked from <see cref="IDisposable.Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing) { }

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}
	}
}
