using System;
using System.IO;
using System.Security;
using Sc.Abstractions.Lifecycle;
using Sc.Diagnostics;
using Sc.Util.IO;
using Sc.Util.System;


namespace Sc.IO.Files
{
	/// <summary>
	/// <see cref="IDirectoryWatcher"/> implementation.
	/// </summary>
	public sealed class DirectoryWatcher
			: IDirectoryWatcher
	{
		private readonly object syncLock;
		private FileSystemWatcher fileSystemWatcher;
		private bool isDisposed;


		/// <summary>
		/// Constructor. Notice that the this will throw if for some reason this object cannot
		/// watch your folder nor ANY parent. To support deferred initialization you may set the
		/// <paramref name="beginWatchingNow"/> argument to false; and in this case, you MUST
		/// manually invoke <see cref="Refresh"/> to attempt to create the watcher
		/// --- you can immediately watch the <see cref="Changed"/> event,
		/// or check the <see cref="IsWatching"/> property to
		/// be sure that the watcher has been successfully created.
		/// </summary>
		/// <param name="path">Not null; and must be a valid directory path; but does not
		/// need to exist now.</param>
		/// <param name="filter">This is the <see cref="FileSystemWatcher.Filter"/> property. This
		/// defaults to "*.*" --- watches all FILES. And if your argument is null or whitespace, it
		/// is set to that filter. This ONLY applies at the
		/// <paramref name="path"/> and within</param>
		/// <param name="includeSubdirectories">Value used to construct the
		/// <see cref="FileSystemWatcher"/>: this ONLY applies at the
		/// <paramref name="path"/> and within.</param>
		/// <param name="beginWatchingNow">Optionally can be set false to defer creating the watcher
		/// until <see cref="IRefresh.Refresh"/> is MANUALLY invoked. You must then test the
		/// <see cref="IsWatching"/> property.</param>
		/// <param name="notifyFilter">This is the <see cref="FileSystemWatcher.NotifyFilter"/>
		/// property.</param>
		/// <exception cref="NotSupportedException">If <paramref name="beginWatchingNow"/> is true and this
		/// constructor cannot watch your folder nor any parent. This will have an Inner Exception with
		/// the cause. May ALSO be thrown by <see cref="System.IO.Path"/>
		/// <see cref="System.IO.Path.GetFullPath"/></exception>
		/// <exception cref="ArgumentNullException">For <paramref name="path"/>.</exception>
		/// <exception cref="ArgumentException">For <paramref name="path"/>.</exception>
		/// <exception cref="SecurityException">For <paramref name="path"/>.</exception>
		/// <exception cref="PathTooLongException">For <paramref name="path"/>.</exception>
		public DirectoryWatcher(
				string path,
				string filter = "*.*",
				bool includeSubdirectories = false,
				bool beginWatchingNow = true,
				NotifyFilters notifyFilter = NotifyFilters.LastWrite
						| NotifyFilters.FileName
						| NotifyFilters.DirectoryName)
		{
			syncLock = new object();
			Path = System.IO.Path.GetFullPath(PathHelper.RemoveTrailingSeparators(path, true, true));
			Filter = string.IsNullOrWhiteSpace(filter)
					? "*.*"
					: filter;
			IncludeSubdirectories = includeSubdirectories;
			NotifyFilter = notifyFilter;
			if (!beginWatchingNow)
				return;
			tryCreateFileSystemWatcher(out Exception exception);
			if (exception is NotSupportedException)
				throw exception;
		}


		private void handleFileSystemWatcherChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
		{
			TraceSources.For<DirectoryWatcher>()
					.Verbose("Refreshing {0}.", this);
			refresh(fileSystemEventArgs);
		}

		private void handleFileSystemWatcherError(object sender, ErrorEventArgs errorEventArgs)
		{
			Exception exception = errorEventArgs.GetException();
			TraceSources.For<DirectoryWatcher>()
					.Warning(
							exception,
							$"{nameof(FileSystemWatcher)} Error for {{0}}: {{1}}.",
							this,
							exception.Message);
			FileSystemWatcherError?.Invoke(this, errorEventArgs);
		}

		private void tryCreateFileSystemWatcher(out Exception exception)
		{
			try {
				string path = Path;
				do {
					if (Directory.Exists(path))
						break;
					path = System.IO.Path.GetDirectoryName(PathHelper.RemoveTrailingSeparators(path));
					if (!string.IsNullOrWhiteSpace(path))
						continue;
					throw new NotSupportedException($"Failed to create {nameof(FileSystemWatcher)} for {this}.");
				} while (true);
				fileSystemWatcher = PathHelper.IsSameFullPath(Path, path)
						? new FileSystemWatcher
						{
								Path = Path,
								Filter = Filter,
								IncludeSubdirectories = IncludeSubdirectories,
								NotifyFilter = NotifyFilter,
								EnableRaisingEvents = true
						}
						: new FileSystemWatcher
						{
								Path = path,
								Filter = "*",
								IncludeSubdirectories = true,
								NotifyFilter = NotifyFilters.LastWrite
										| NotifyFilters.DirectoryName,
								EnableRaisingEvents = true
						};
				fileSystemWatcher.Changed += handleFileSystemWatcherChanged;
				fileSystemWatcher.Created += handleFileSystemWatcherChanged;
				fileSystemWatcher.Deleted += handleFileSystemWatcherChanged;
				fileSystemWatcher.Renamed += handleFileSystemWatcherChanged;
				fileSystemWatcher.Error += handleFileSystemWatcherError;
				exception = null;
			} catch (Exception error) {
				TraceSources.For<DirectoryWatcher>()
						.Error(
								error,
								"Failed to refresh {0}: {1}. Not watching any folder.",
								this,
								error.Message);
				fileSystemWatcher?.Dispose();
				fileSystemWatcher = null;
				exception = error;
			}
		}

		private void disposeFileSystemWatcher()
		{
			if (fileSystemWatcher == null)
				return;
			try {
				fileSystemWatcher.Changed -= handleFileSystemWatcherChanged;
				fileSystemWatcher.Created -= handleFileSystemWatcherChanged;
				fileSystemWatcher.Deleted -= handleFileSystemWatcherChanged;
				fileSystemWatcher.Renamed -= handleFileSystemWatcherChanged;
				fileSystemWatcher.Error -= handleFileSystemWatcherError;
				fileSystemWatcher.Dispose();
			} catch {
				// Ignored.
			}
			fileSystemWatcher = null;
		}

		private void refresh(FileSystemEventArgs fileSystemEventArgs)
		{
			Exception exception;
			lock (syncLock) {
				if (isDisposed) {
					if (fileSystemEventArgs == null)
						throw new ObjectDisposedException(ToString());
					return;
				}
				disposeFileSystemWatcher();
				tryCreateFileSystemWatcher(out exception);
				if (exception != null) {
					TraceSources.For<DirectoryWatcher>()
							.Error("Local Error for {0}: {1}.", exception, this, exception.Message);
				} else if ((fileSystemEventArgs == null)
						|| !IsWatchingPath)
					return;
			}
			Changed?.Invoke(this, new DirectoryWatcherEventArgs(fileSystemEventArgs, exception));
		}


		public void Refresh()
			=> refresh(null);

		public string Path { get; }

		public string CurrentWatchedPath
		{
			get {
				lock (syncLock) {
					return fileSystemWatcher?.Path;
				}
			}
		}

		public bool IsWatching
		{
			get {
				lock (syncLock) {
					return fileSystemWatcher != null;
				}
			}
		}

		public bool IsWatchingPath
		{
			get {
				lock (syncLock) {
					try {
						return (fileSystemWatcher != null)
								&& PathHelper.IsSameFullPath(Path, fileSystemWatcher.Path);
					} catch {
						return false;
					}
				}
			}
		}

		public bool PathExists
			=> Directory.Exists(Path);

		public string Filter { get; }

		public bool IncludeSubdirectories { get; }

		public NotifyFilters NotifyFilter { get; }

		public bool IsDisposed
		{
			get {
				lock (syncLock) {
					return isDisposed;
				}
			}
		}

		public event EventHandler<DirectoryWatcherEventArgs> Changed;

		public event EventHandler<ErrorEventArgs> FileSystemWatcherError;


		public void Dispose()
		{
			lock (syncLock) {
				if (isDisposed)
					return;
				isDisposed = true;
				disposeFileSystemWatcher();
				Changed = null;
				FileSystemWatcherError = null;
			}
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $"'{Path}'"
					+ $", {Filter}"
					+ $", {NotifyFilter}"
					+ (IsDisposed
							? $", {nameof(DirectoryWatcher.IsDisposed)}"
							: $", {nameof(DirectoryWatcher.IsWatching)}={IsWatching}"
							+ $", {nameof(DirectoryWatcher.IsWatchingPath)}={IsWatchingPath}")
					+ "]";
	}
}
