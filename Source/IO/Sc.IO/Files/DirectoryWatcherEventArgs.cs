﻿using System;
using System.IO;
using Sc.Abstractions.Lifecycle;


namespace Sc.IO.Files
{
	/// <summary>
	/// Event args raised by <see cref="IDirectoryWatcher.Changed"/>. This event is raised
	/// for two reasons: if the <see cref="FileSystemWatcherEvent"/> is not null, then that
	/// <see cref="FileSystemWatcher"/> event generated this event --- this was generated
	/// on a file change event, which then invokes an automatic <see cref="IRefresh.Refresh"/>;
	/// and is raised whether or not there is also an error.
	/// Otherwise, this event was generated from a manual <see cref="IRefresh.Refresh"/>,
	/// and in that case, this is only raised if there is an error re-creating
	/// the <see cref="FileSystemWatcher"/>. If <see cref="HasRefreshError"/> is true, the
	/// <see cref="RefreshError"/> is not null; and holds an error from trying to recreate
	/// the file system watcher; whether this event was a manual refresh or a file system
	/// watcher change event. Note that this <see cref="RefreshError"/> will
	/// ONLY contain an error generated by this class
	/// if trying to re-create the <see cref="FileSystemWatcher"/> failed:
	/// if the <see cref="FileSystemWatcher"/>
	/// <see cref="FileSystemWatcher.Error"/> event is raised, then the
	/// <see cref="DirectoryWatcher.FileSystemWatcherError"/> event is raised; and not this.
	/// (You may test the <see cref="IDirectoryWatcher.IsWatching"/> property on any event or error).
	/// </summary>
	public class DirectoryWatcherEventArgs
			: EventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemWatcherEvent">One must be non-null.</param>
		/// <param name="refreshError">One must be non-null.</param>
		/// <exception cref="ArgumentException"></exception>
		public DirectoryWatcherEventArgs(FileSystemEventArgs fileSystemWatcherEvent, Exception refreshError)
		{
			if ((fileSystemWatcherEvent == null)
					&& (refreshError == null)) {
				throw new ArgumentException(
						$"{nameof(fileSystemWatcherEvent)} is null && {nameof(refreshError)} is null");
			}
			FileSystemWatcherEvent = fileSystemWatcherEvent;
			RefreshError = refreshError;
		}


		/// <summary>
		/// Returns true if this event has a non-null <see cref="RefreshError"/>.
		/// </summary>
		public bool HasRefreshError
			=> RefreshError != null;

		/// <summary>
		/// The optional <see cref="FileSystemWatcher"/> event:
		/// only non-null IF this event was raised
		/// on a <see cref="FileSystemWatcher"/> event
		/// --- Otherwise this was raised by a manual <see cref="IRefresh.Refresh"/>.
		/// </summary>
		public FileSystemEventArgs FileSystemWatcherEvent { get; }

		/// <summary>
		/// This property contains ONLY a locally-generated error during an update: an error was
		/// raised while trying to re-create the <see cref="FileSystemWatcher"/>. This property
		/// is otherwise null. If the <see cref="DirectoryWatcher"/> was created without
		/// immediately watching the directory; and the <see cref="Path"/> does not exist
		/// and the <see cref="DirectoryWatcher"/> cannot find
		/// ANY parent directory to watch, then it raises <see cref="NotSupportedException"/>
		/// on a <see cref="IRefresh.Refresh"/> attempt.
		/// </summary>
		public Exception RefreshError { get; }
	}
}
