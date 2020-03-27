using System;
using System.IO;


namespace Sc.Abstractions.Application
{
	/// <summary>
	/// An object that defines the system-wide scope of an application instance.
	/// </summary>
	public interface IAppScope
	{
		/// <summary>
		/// The system-wide unique ID for the application. Single instance support
		/// may bve implemented with this value.
		/// </summary>
		string AppGuid { get; }

		/// <summary>
		/// The user-visible friendly name of the application.
		/// </summary>
		string AppName { get; }

		/// <summary>
		/// The application Version.
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// This must return the user-visible sub-folder path for storing the
		/// application settings. This path will point to a subfolder within
		/// the ApplicationData folder(s); and must not be absolute.
		/// Normally, this is a single folder named for the application.
		/// NOTICE: if this returns a path, it MUST RETURN a path using
		/// THIS PLATFORM'S specific <see cref="Path.DirectorySeparatorChar"/>.
		/// Can be a single folder name, or a nested subfolder path.
		/// </summary>
		string GetAppDataFolderPath();

		/// <summary>
		/// This method defaults to null, and if this returns a non-null
		/// string, this can specify an alternate root folder path that roots
		/// the required <see cref="GetAppDataFolderPath"/> folder. If null,
		/// the container implementation is assumed to default to the
		/// <see cref="Environment.SpecialFolder.ApplicationData"/>
		/// folder. If not null, then this can specify the root.
		/// Notice that this may be absolute OR relative if the
		/// container supports creating a relative storage root.
		/// </summary>
		string GetAppDataRootFolder();
	}
}
