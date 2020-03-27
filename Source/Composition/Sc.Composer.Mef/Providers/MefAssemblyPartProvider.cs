using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;


namespace Sc.Composer.Mef.Providers
{
	/// <summary>
	/// An <see cref="IProvideParts{TTarget}"/> for Mef composition,
	/// that provides a fixed list of Assemblies.
	/// </summary>
	public class MefAssemblyPartProvider
			: IProvideParts<ContainerConfiguration>,
					IDisposable
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="providedAssemblies">May not be null or empty.</param>
		/// <param name="conventions">Optional conventions applied to all added
		/// Assemblies.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public MefAssemblyPartProvider(
				IEnumerable<Assembly> providedAssemblies,
				AttributedModelProvider conventions = null)
		{
			ProvidedAssemblies
					= providedAssemblies?.ToArray()
					?? throw new ArgumentNullException(nameof(providedAssemblies));
			if (ProvidedAssemblies.Count == 0)
				throw new ArgumentException(nameof(providedAssemblies));
			foreach (Assembly providedAssembly in ProvidedAssemblies) {
				if (providedAssembly == null)
					throw new ArgumentException(nameof(providedAssemblies));
			}
			Conventions = conventions;
		}


		/// <summary>
		/// Provided on Construction.
		/// </summary>
		public IReadOnlyCollection<Assembly> ProvidedAssemblies { get; private set; }

		/// <summary>
		/// Provided on Construction.
		/// </summary>
		public AttributedModelProvider Conventions { get; }

		/// <summary>
		/// This virtual method provides the implementation for
		/// <see cref="IProvideParts{TTarget}"/>.
		/// This adds all <see cref="ProvidedAssemblies"/>, with the optional
		/// <see cref="Conventions"/> if set.
		/// </summary>
		public virtual void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
				where T : ContainerConfiguration
		{
			if (Conventions != null)
				eventArgs.Target.WithAssemblies(ProvidedAssemblies, Conventions);
			else
				eventArgs.Target.WithAssemblies(ProvidedAssemblies);
		}


		/// <summary>
		/// Invoked <see cref="IDisposable.Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
				ProvidedAssemblies = new Assembly[0];
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}
	}
}
