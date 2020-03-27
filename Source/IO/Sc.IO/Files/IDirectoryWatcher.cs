using System;
using System.IO;
using Sc.Abstractions.Lifecycle;


namespace Sc.IO.Files
{
	/// <summary>
	/// Wraps a <see cref="FileSystemWatcher"/> for a directory and a specified file name filter, and
	/// supports watching parent directories if your target <see cref="Path"/> does not currently exist.
	/// The object is disposable, and watches a specified folder. You may manually invoke a refresh with
	/// <see cref="IRefresh.Refresh"/>, which will try to ensure the <see cref="FileSystemWatcher"/> is
	/// watching for changes; and that may destroy and re-create the watcher every time. That method
	/// will also always run synchronously --- allowing you to check errors. This object will
	/// raise the <see cref="Changed"/> event if it is watching your target <see cref="Path"/> (and not
	/// some parent) and changes are made in the <see cref="Path"/> with your <see cref="Filter"/> and
	/// <see cref="NotifyFilter"/>. That event will be synchronously raised with a manual
	/// <see cref="IRefresh.Refresh"/> (again, allowing you to check for errors, which will appear on
	/// the raised event). You may handle errors raised by the <see cref="FileSystemWatcher"/>
	/// itself with <see cref="FileSystemWatcherError"/>. If the constructor is not able to watch your
	/// <see cref="Path"/> or some parent, then it may throw. AND, on ANY <see cref="Changed"/> event,
	/// if there is an error re-creating the watcher, then the event will hold an Exception, and you
	/// must test the <see cref="IsWatching"/> property: when that property is FALSE, NO file system
	/// watcher has been created, and this object CANNOT recover without a MANUAL
	/// <see cref="IRefresh.Refresh"/>. When <see cref="IRefresh.Refresh"/> is invoked, it will run
	/// synchronously, and you may re-test. The object will otherwise watch parent directories if your
	/// target directory does not currently exist, and watch for new subdirectories, and re-create the
	/// watcher until your target directory exists. It only monitors for your <see cref="Filter"/> within
	/// your target directory, and the <see cref="Changed"/> event will only be raised for your target
	/// <see cref="Path"/> and <see cref="Filter"/>.
	/// </summary>
	public interface IDirectoryWatcher
			: IRefresh,
					IDisposable
	{
		/// <summary>
		/// The full path to the innermost target folder to be watched. Immutable.
		/// The <see cref="Changed"/> event is only raised for events in this directory
		/// --- or also within subdirectories here if <see cref="IncludeSubdirectories"/>
		/// is true. This <see cref="IDirectoryWatcher"/> is only watching this path if
		/// <see cref="IsWatchingPath"/> is true. See also <see cref="CurrentWatchedPath"/>.
		/// </summary>
		string Path { get; }

		/// <summary>
		/// Can be null.
		/// This property returns the full path of the directory currently being watched.
		/// This will be some parent of the <see cref="Path"/> until that directory
		/// exists. When that does exist, this will match the <see cref="Path"/>.
		/// This returns null if <see cref="IsWatching"/> is false.
		/// </summary>
		string CurrentWatchedPath { get; }

		/// <summary>
		/// See also <see cref="IsWatchingPath"/>.
		/// This property returns true if the <see cref="FileSystemWatcher"/> has been created for the
		/// <see cref="Path"/> OR any parent. On any event, you may test this property: when this
		/// property is false, NO file system watcher has been re-created; and this object cannot
		/// recover without a MANUAL <see cref="IRefresh.Refresh"/>.
		/// </summary>
		bool IsWatching { get; }

		/// <summary>
		/// This property only returns true if <see cref="IsWatching"/> is true,
		/// and, this is now watching your <see cref="Path"/> and not some parent.
		/// </summary>
		bool IsWatchingPath { get; }

		/// <summary>
		/// Simply returns true if the <see cref="Path"/> exists.
		/// </summary>
		bool PathExists { get; }

		/// <summary>
		/// The <see cref="FileSystemWatcher"/> <see cref="FileSystemWatcher.Filter"/> property.
		/// This ONLY applies at the <see cref="Path"/> and within.
		/// </summary>
		string Filter { get; }

		/// <summary>
		/// The <see cref="FileSystemWatcher"/> <see cref="FileSystemWatcher.IncludeSubdirectories"/> property.
		/// This ONLY applies at the <see cref="Path"/> and within.
		/// </summary>
		bool IncludeSubdirectories { get; }

		/// <summary>
		/// The <see cref="FileSystemWatcher"/> <see cref="FileSystemWatcher.NotifyFilter"/> property.
		/// This ONLY applies at the <see cref="Path"/> and within.
		/// </summary>
		NotifyFilters NotifyFilter { get; }

		/// <summary>
		/// Returns true when this has been disposed.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// This event will be raised when the <see cref="FileSystemWatcher"/> raises an update, or when
		/// <see cref="IRefresh.Refresh"/> is invoked directly. The event's <c>sender</c> is this object.
		/// The event also contains any errors raised during the update. If this is a
		/// <see cref="FileSystemWatcher"/> event, then this event's payload is that event. Note that
		/// you must test the <see cref="IsWatching"/> property on this event: if this object has
		/// failed to re-create a file system watcher, it cannot recover without a MANUAL
		/// <see cref="IRefresh.Refresh"/>.
		/// </summary>
		event EventHandler<DirectoryWatcherEventArgs> Changed;

		/// <summary>
		/// This event will be raised from the <see cref="FileSystemWatcher"/>
		/// <see cref="FileSystemWatcher.Error"/> event. The sender is this object, and the event args
		/// is the <see cref="FileSystemWatcher"/> error event.
		/// </summary>
		event EventHandler<ErrorEventArgs> FileSystemWatcherError;
	}
}
