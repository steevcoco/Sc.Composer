using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Security;
using Sc.Diagnostics;
using Sc.IO.Files;


namespace Sc.Composer.Mef.Providers
{
	/// <summary>
	/// An <see cref="IProvideParts{TTarget}"/> for Mef composition,
	/// that wraps a <see cref="IO.Files.DirectoryWatcher"/> for
	/// the directory, and contributes all Assemblies in the directory.
	/// This class also
	/// implements <see cref="IRequestComposition{TTarget}"/>
	/// and will raise that event when the
	/// <see cref="DirectoryWatcher"/> raises a change.
	/// </summary>
	public class MefDirectoryPartWatcher
			: IProvideParts<ContainerConfiguration>,
					IRequestComposition<ContainerConfiguration>,
					IDisposable
	{
		/// <summary>
		/// Holds a list of provided Assemblies, to support
		/// <see cref="ProvideAssembliesOneTimeOnly"/>. Must be locked
		/// for all access. This is ONLY populated when the property is true.
		/// </summary>
		protected readonly List<Assembly> ComposedAssemblies = new List<Assembly>();

		private readonly Func<string, Assembly> loadAssembly;


		/// <summary>
		/// This constructor creates a new <see cref="IO.Files.DirectoryWatcher"/>.
		/// This constructs the instance with your <c>path</c>, and sets the file filter to
		/// <c>"*.dll"</c>. The default <see cref="NotifyFilters"/> are used,
		/// and subdirectories are not included by default.
		/// </summary>
		/// <param name="path">Required: must be a valid directory path..</param>
		/// <param name="searchOption">Optional search option to locate Assemblies: defaults to
		/// <see cref="System.IO.SearchOption.TopDirectoryOnly"/>.</param>
		/// <param name="conventions">Optional conventions applied to all added
		/// Assemblies.</param>
		/// <exception cref="ArgumentNullException">For <c>path</c>.</exception>
		/// <exception cref="ArgumentException">For <c>path</c>.</exception>
		/// <exception cref="SecurityException">For <c>path</c>.</exception>
		/// <exception cref="ArgumentNullException">For <c>path</c>.</exception>
		/// <exception cref="NotSupportedException">For <c>path</c>.</exception>
		/// <exception cref="PathTooLongException">For <c>path</c>.</exception>
		public MefDirectoryPartWatcher(
				string path,
				SearchOption searchOption = SearchOption.TopDirectoryOnly,
				AttributedModelProvider conventions = null)
				: this(
						null,
						false,
						new DirectoryWatcher(path, "*.dll", searchOption == SearchOption.AllDirectories),
						searchOption,
						conventions) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="directoryWatcher">Not null.</param>
		/// <param name="searchOption">Optional search option to locate Assemblies: defaults to
		/// <see cref="System.IO.SearchOption.TopDirectoryOnly"/>.</param>
		/// <param name="conventions">Optional conventions applied to all added
		/// Assemblies.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public MefDirectoryPartWatcher(
				DirectoryWatcher directoryWatcher,
				SearchOption searchOption = SearchOption.TopDirectoryOnly,
				AttributedModelProvider conventions = null)
				: this(
						null,
						false,
						directoryWatcher,
						searchOption,
						conventions) { }

		/// <summary>
		/// Constructor. Allows passing a delegate that is used to load each Assembly,
		/// </summary>
		/// <param name="loadAssembly">Not null. This Func is passed the full path to each Assembly
		/// to load. You may return null if the Assembly cannot be loaded; and exceptions will
		/// be caught and traced.</param>
		/// <param name="directoryWatcher">Not null.</param>
		/// <param name="searchOption">Optional search option to locate Assemblies: defaults to
		/// <see cref="System.IO.SearchOption.TopDirectoryOnly"/>.</param>
		/// <param name="conventions">Optional conventions applied to all added
		/// Assemblies.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public MefDirectoryPartWatcher(
				Func<string, Assembly> loadAssembly,
				DirectoryWatcher directoryWatcher,
				SearchOption searchOption = SearchOption.TopDirectoryOnly,
				AttributedModelProvider conventions = null)
				: this(
						loadAssembly,
						true,
						directoryWatcher,
						searchOption,
						conventions) { }

		private MefDirectoryPartWatcher(
				Func<string, Assembly> loadAssembly,
				bool requireLoadAssembly,
				DirectoryWatcher directoryWatcher,
				SearchOption searchOption,
				AttributedModelProvider conventions)
		{
			if (requireLoadAssembly
					&& (loadAssembly == null))
				throw new ArgumentNullException(nameof(loadAssembly));
			this.loadAssembly = loadAssembly;
			DirectoryWatcher = directoryWatcher ?? throw new ArgumentNullException(nameof(directoryWatcher));
			SearchOption = searchOption;
			Conventions = conventions;
			DirectoryWatcher.Changed += HandleDirectoryWatcherChanged;
		}


		/// <summary>
		/// The <see cref="IO.Files.DirectoryWatcher"/> provided or
		/// created on construction.
		/// </summary>
		public DirectoryWatcher DirectoryWatcher { get; }

		/// <summary>
		/// The <see cref="System.IO.SearchOption"/> given on construction.
		/// </summary>
		public SearchOption SearchOption { get; }

		/// <summary>
		/// The <see cref="AttributedModelProvider"/> given on construction.
		/// </summary>
		public AttributedModelProvider Conventions { get; }

		/// <summary>
		/// Defaults to false.
		/// If set true, this provider will track each Assembly added on each
		/// event, and will not add the same assembly twice.
		/// </summary>
		public bool ProvideAssembliesOneTimeOnly { get; set; }


		/// <summary>
		/// This protected virtual handler is invoked by the <see cref="DirectoryWatcher"/> on all
		/// changes. This method invokes <see cref="RaiseCompositionRequested"/>.
		/// </summary>
		/// <param name="sender">The <see cref="DirectoryWatcher"/> <c>sender</c>.</param>
		/// <param name="directoryWatcherEventArgs">The <see cref="DirectoryWatcher"/> <c>event</c>.</param>
		protected virtual void HandleDirectoryWatcherChanged(
				object sender,
				DirectoryWatcherEventArgs directoryWatcherEventArgs)
			=> RaiseCompositionRequested();

		/// <summary>
		/// This virtual method provides the implementation for
		/// <see cref="IProvideParts{TTarget}"/>.
		/// This enumerates the files on the <see cref="DirectoryWatcher"/> with the
		/// <see cref="SearchOption"/>, and loads each path with <see cref="TryLoadAssembly"/>.
		/// This method catches and traces all exceptions; and also provides support for
		/// <see cref="ProvideAssembliesOneTimeOnly"/>.
		/// </summary>
		public virtual void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
				where T : ContainerConfiguration
		{
			List<Assembly> assemblies = new List<Assembly>();
			if (DirectoryWatcher.PathExists) {
				try {
					foreach (string filePath in Directory.EnumerateFiles(
							DirectoryWatcher.Path,
							DirectoryWatcher.Filter,
							SearchOption)) {
						if (TryLoadAssembly(filePath, out Assembly assembly))
							assemblies.Add(assembly);
					}
				} catch (Exception exception) {
					TraceSources.For<MefDirectoryPartWatcher>()
							.Warning(
									exception,
									"Catching exception enumerating files in the watched directory: '{0}'.",
									exception.Message);
				}
			}
			if (assemblies.Count == 0)
				return;
			lock (ComposedAssemblies) {
				if (ProvideAssembliesOneTimeOnly) {
					foreach (Assembly assembly in new List<Assembly>(assemblies)) {
						if (!ComposedAssemblies.Contains(assembly))
							ComposedAssemblies.Add(assembly);
						else
							assemblies.Remove(assembly);
					}
					if (assemblies.Count == 0)
						return;
				}
			}
			if (Conventions != null)
				eventArgs.Target.WithAssemblies(assemblies, Conventions);
			else
				eventArgs.Target.WithAssemblies(assemblies);
		}

		/// <summary>
		/// This protected virtual method loads each Assembly. If a delegate was provided, that
		/// is invoked here; and otherwise, this loads the Assembly with <c>Assembly.LoadFrom</c>.
		/// This is invoked in <see cref="ProvideParts{T}"/>. This method catches and traces all
		/// exceptions.
		/// </summary>
		/// <param name="filePath">Not null.</param>
		/// <param name="assembly">Not null if the method returns true.</param>
		/// <returns>True if the out argument is not null.</returns>
		protected virtual bool TryLoadAssembly(string filePath, out Assembly assembly)
		{
			try {
				assembly = loadAssembly != null
						? loadAssembly(filePath)
						: Assembly.LoadFrom(filePath);
			} catch (Exception exception) {
				TraceSources.For<MefDirectoryPartWatcher>()
						.Warning(
								exception,
								"Failed to load Assembly from '{0}': {1}.",
								filePath,
								exception.Message);
				assembly = null;
			}
			return assembly != null;
		}

		/// <summary>
		/// This protected virtual method raises the
		/// <see cref="IRequestComposition{TTarget}.CompositionRequested"/>
		/// event.
		/// </summary>
		/// <param name="participant">Optional parameter for a new
		/// <see cref="RequestCompositionEventArgs{TTarget}"/>: notice that if this is not null,
		/// the <paramref name="request"/> must be set..</param>
		/// <param name="request">Optional parameter for a new
		/// <see cref="RequestCompositionEventArgs{TTarget}"/>.</param>
		protected virtual void RaiseCompositionRequested(
				IComposerParticipant<ContainerConfiguration> participant = null,
				RequestCompositionEventArgs<ContainerConfiguration>.ParticipantRequest request
						= RequestCompositionEventArgs<ContainerConfiguration>.ParticipantRequest.None)
			=> CompositionRequested?.Invoke(
					this,
					participant != null
							? new RequestCompositionEventArgs<ContainerConfiguration>(participant, request)
							: new RequestCompositionEventArgs<ContainerConfiguration>());

		public event EventHandler<RequestCompositionEventArgs<ContainerConfiguration>> CompositionRequested;


		/// <summary>
		/// Invoked <see cref="IDisposable.Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!isDisposing)
				return;
			CompositionRequested = null;
			DirectoryWatcher.Changed -= HandleDirectoryWatcherChanged;
			DirectoryWatcher.Dispose();
			lock (ComposedAssemblies) {
				ComposedAssemblies.Clear();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}
	}
}
