using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using Sc.Util.Collections;


namespace Sc.Composer.Mef.Providers
{
	/// <summary>
	/// Implements an <see cref="IProvideParts{TTarget}"/> for Mef composition,
	/// that provides a fixed list of
	/// <see cref="ExportDescriptorProvider"/> instances.
	/// </summary>
	public class ExportDescriptorPartProvider
			: IProvideParts<ContainerConfiguration>,
					IDisposable
	{
		/// <summary>
		/// Convenience method that will construct and return a new
		/// <see cref="ExportDescriptorPartProvider"/>,
		/// that provides a single
		/// <see cref="SharedInstanceExportDescriptorProvider"/>, that
		/// provides your <c>sharedInstance</c> object.
		/// </summary>
		/// <param name="sharedInstance">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static ExportDescriptorPartProvider ForSharedInstance<TExport>(TExport sharedInstance)
			=> new ExportDescriptorPartProvider(
					SharedInstanceExportDescriptorProvider.For(sharedInstance)
							.AsSingle());

		/// <summary>
		/// Convenience method that will construct and return a new
		/// <see cref="ExportDescriptorPartProvider"/>,
		/// that provides a single
		/// <see cref="NonSharedExportDescriptorProvider"/>, that
		/// provides your <c>factory</c> object.
		/// </summary>
		/// <param name="factory">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static ExportDescriptorPartProvider ForNonSharedInstance<TExport>(Func<object> factory)
			=> new ExportDescriptorPartProvider(
					new NonSharedExportDescriptorProvider(typeof(TExport), factory)
							.AsSingle());

		/// <summary>
		/// Convenience method that will construct and return a new
		/// <see cref="ExportDescriptorPartProvider"/>,
		/// that provides a list of
		/// <see cref="SharedInstanceExportDescriptorProvider"/> instances, that
		/// provide your <c>sharedInstances</c> objects.
		/// </summary>
		/// <param name="sharedInstances">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If a <c>sharedInstance</c> does
		/// not extend the <c>contractType</c>.</exception>
		public static ExportDescriptorPartProvider ForSharedInstances(
				params (Type contractType, object sharedInstance)[] sharedInstances)
			=> new ExportDescriptorPartProvider(
					sharedInstances?.Select(
							sharedInstance => new SharedInstanceExportDescriptorProvider(
									sharedInstance.contractType,
									sharedInstance.sharedInstance))
					?? throw new ArgumentNullException(nameof(sharedInstances)));

		/// <summary>
		/// Convenience method that will construct and return a new
		/// <see cref="ExportDescriptorPartProvider"/>,
		/// that provides a list of
		/// <see cref="NonSharedExportDescriptorProvider"/> instances, that
		/// provide your <c>factories</c> objects.
		/// </summary>
		/// <param name="factories">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If a <c>sharedInstance</c> does
		/// not extend the <c>contractType</c>.</exception>
		public static ExportDescriptorPartProvider ForNonSharedInstances(
				params (Type contractType, Func<object> factory)[] factories)
			=> new ExportDescriptorPartProvider(
					factories?.Select(
							factory => new NonSharedExportDescriptorProvider(
									factory.contractType,
									factory.factory))
					?? throw new ArgumentNullException(nameof(factories)));


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="exportDescriptorProviders">Required.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException">If <c>exportDescriptorProviders</c> is
		/// empty or contains a null element.</exception>
		public ExportDescriptorPartProvider(IEnumerable<ExportDescriptorProvider> exportDescriptorProviders)
		{
			ExportDescriptorProviders
					= exportDescriptorProviders?.ToArray()
					?? throw new ArgumentNullException(nameof(exportDescriptorProviders));
			if ((ExportDescriptorProviders.Count == 0)
					|| ExportDescriptorProviders.Any(provider => provider == null)) {
				throw new ArgumentException(
						ExportDescriptorProviders.ToStringCollection()
								.ToString(),
						nameof(exportDescriptorProviders));
			}
		}


		/// <summary>
		/// The list of providers.
		/// </summary>
		public IReadOnlyList<ExportDescriptorProvider> ExportDescriptorProviders { get; private set; }

		/// <summary>
		/// This virtual method provides the implementation for
		/// <see cref="IProvideParts{TTarget}"/>.
		/// This adds all <see cref="ExportDescriptorProviders"/>.
		/// </summary>
		public virtual void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
				where T : ContainerConfiguration
		{
			foreach (ExportDescriptorProvider exportDescriptorProvider in ExportDescriptorProviders) {
				eventArgs.Target.WithProvider(exportDescriptorProvider);
			}
		}


		/// <summary>
		/// Invoked <see cref="IDisposable.Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!isDisposing)
				return;
			IReadOnlyList<ExportDescriptorProvider> dispose = ExportDescriptorProviders;
			ExportDescriptorProviders = new ExportDescriptorProvider[0];
			foreach (IDisposable disposable in dispose.OfType<IDisposable>()) {
				disposable.Dispose();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}
	}
}
