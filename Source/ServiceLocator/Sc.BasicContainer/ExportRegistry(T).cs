using System;
using System.Collections.Generic;
using System.Linq;
using Sc.Abstractions.ServiceLocator;
using Sc.Diagnostics;
using Sc.Util.System;


namespace Sc.BasicContainer
{
	/// <summary>
	/// Complete <see cref="IExportRegistry{TExport}"/> implementation.
	/// Can be subclassed to inspect each registration before it is added;
	/// and provides the static <see cref="For{TImport}"/> helper method
	/// to construct an <see cref="ImportType{TImport}"/> instance that
	/// restricts the Import registration types.
	/// </summary>
	/// <typeparam name="TExport">Base type implemented by all Exports
	/// --- can be an interface.</typeparam>
	public class ExportRegistry<TExport>
			: IExportRegistry<TExport>,
					IDisposable
	{
		/// <summary>
		/// Implements an <see cref="ExportRegistry{TExport}"/> that restricts the
		/// registered import types to extend the <typeparamref name="TImport"/> type.
		/// Raises <see cref="InvalidOperationException"/> for rejected registrations.
		/// </summary>
		/// <typeparam name="TImport">The required base import type.</typeparam>
		public class ImportType<TImport>
				: ExportRegistry<TExport>
		{
			protected override bool OnRegisterExport(Type importType, Type exportType)
			{
				if (typeof(TImport).IsAssignableFrom(importType))
					return true;
				throw new InvalidOperationException(
						$"Import type must be assignable to"
								+ $" {typeof(TImport).GetFriendlyFullName()}"
								+ $": '{importType.GetFriendlyFullName()}'.");
			}
		}


		/// <summary>
		/// Static convenience method constructs an
		/// <see cref="ExportRegistry{TExport}"/> that restricts the
		/// registered import types to extend the <typeparamref name="TImport"/> type.
		/// Raises <see cref="InvalidOperationException"/> for rejected registrations.
		/// </summary>
		/// <typeparam name="TImport">The required base import type.</typeparam>
		/// <returns>Not null.</returns>
		public static ExportRegistry<TExport> For<TImport>()
			=> new ImportType<TImport>();


		/// <summary>
		/// The actual list of registrations: all access to this list
		/// must lock this object. Keys are Import types; values are Export types.
		/// </summary>
		protected readonly Dictionary<Type, Type> Registrations = new Dictionary<Type, Type>(8);


		/// <summary>
		/// This protected <see langword="virtual"/> method is invoked from
		/// <see cref="RegisterExport(Type, Type)"/> before each registration is
		/// added to the <see cref="Registrations"/>; and is provided for
		/// your subclass to implement a simple validation. If this returns
		/// false then this registration will not be added. Note that no
		/// exception is raised; but you may raise one yourself.
		/// </summary>
		/// <param name="importType">The public caller's Import type.</param>
		/// <param name="exportType">The public caller's Export type.</param>
		/// <returns>If false then this registration is not added.</returns>
		protected virtual bool OnRegisterExport(Type importType, Type exportType)
			=> true;


		public void RegisterExport(Type importType, Type exportType)
		{
			if (importType == null)
				throw new ArgumentNullException(nameof(importType));
			if (exportType == null)
				throw new ArgumentNullException(nameof(exportType));
			if (!typeof(TExport).IsAssignableFrom(exportType))
				throw new ArgumentException(typeof(TExport).GetFriendlyFullName(), nameof(exportType));
			lock (Registrations) {
				if (Registrations.ContainsKey(importType)) {
					throw new InvalidOperationException(
							$"Cannot add duplicate registration for '{importType.GetFriendlyFullName()}'.");
				}
				if (!OnRegisterExport(importType, exportType))
					return;
				TraceSources.For(GetType())
						.Verbose(
								$"{nameof(ExportRegistry<TExport>.RegisterExport)}:"
								+ " '{0}' for '{1}'.",
								exportType.GetFriendlyFullName(),
								importType.GetFriendlyFullName());
				Registrations[importType] = exportType;
			}
		}

		public IEnumerable<KeyValuePair<Type, Type>> GetAllRegistrations()
		{
			lock (Registrations) {
				return Registrations.ToArray();
			}
		}


		/// <summary>
		/// Invoked from <see cref="Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing) {
				lock(Registrations) {
					Registrations.Clear();
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
