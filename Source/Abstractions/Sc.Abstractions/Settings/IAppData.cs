using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Sc.Abstractions.Serialization;


namespace Sc.Abstractions.Settings
{
	/// <summary>
	/// Encapsulates persistence within user folders; or any abstracted storage location. Fundamentally,
	/// an <see cref="IAppData"/> instance points to a single folder, which is possbily nested. Each
	/// folder in some path is represented by a single <see cref="IAppData"/> instance. Instances can
	/// be nested, and scoped --- and nested folders create single <see cref="IAppData"/> instances
	/// for each subfolder. All persistence begins within a root instance, which is rooted inside
	/// the <see cref="StorageRoot"/> location, such as the <see cref="Environment.SpecialFolder.ApplicationData"/>.
	/// Within that root, each <c>IAppData</c> instance holds a mandatory <see cref="RootFolderName"/>; and
	/// all persistence begins within this folder. All subfolder instances in this path will hold the
	/// same <see cref="StorageRoot"/> and <see cref="RootFolderName"/>. Therefore, any instance is
	/// "scoped" within this <see cref="RootFolderName"/> folder. The root instance may persist
	/// directly within that <see cref="RootFolderName"/>. Further instances can then be created
	/// with a <see cref="ThisFolderName"/>. The first is directly within the <see cref="RootFolderName"/>;
	/// and further instances nest within the parent instance: the individual <see cref="ThisFolderName"/>
	/// is the single deepest folder for an instance, and is nested within the parent's
	/// <see cref="ThisFolderName"/> (or the <see cref="RootFolderName"/> for the first nested
	/// instance). The <see cref="GetFolderPath"/> method returns the full path to a given instance's
	/// folder. Subfolders can be created by convention based on a type name, assembly name, or namespace.
	/// You can get the root instance from <see cref="GetRootAppData"/>. Each instance also carries a
	/// <see cref="Serializer"/> for that instance: it wil be the same instance for all nested instances,
	/// unless a scoped instance is created with a new serializer. <see cref="IAppData"/> must be thread-safe.
	/// </summary>
	public interface IAppData
	{
		/// <summary>
		/// This returns the non-null <see cref="Serializer"/> that is used for all persistance
		/// on this instance --- and may differ from a pareent or child if an instance has
		/// been scoped with a new serializer.
		/// </summary>
		ISerializer Serializer { get; }


		/// <summary>
		/// All persistence begins within this folder; but no files are directly placed here: each
		/// instance must define the <see cref="RootFolderName"/>, and files begin persisting within
		/// that folder. Nested instances may then be created, and they define a nested
		/// <see cref="ThisFolderName"/>, where they persist. All nested instances rooted from the
		/// same root <see cref="IAppData"/> instance have the same <see cref="StorageRoot"/>
		/// and <see cref="RootFolderName"/>. This string is either an absolute path, or a
		/// folder name only or subfolder path  if the implementation will use an implicit file root.
		/// </summary>
		string StorageRoot { get; }

		/// <summary>
		/// The root writable folder where all data begins storage --- notice that this property
		/// is always the folder WITHIN the <see cref="StorageRoot"/> location. If this is a nested
		/// instance, then THIS folder is still ALWAYS the ROOT location. This string is a folder
		/// name only, and not a path.
		/// </summary>
		string RootFolderName { get; }

		/// <summary>
		/// If this instance is a nested instance, this is the name of the subfolder in which THIS
		/// instance persists. This is ultimately nested within the <see cref="RootFolderName"/>; and
		/// if this instance is nested within another, this is within that instance's
		/// <see cref="ThisFolderName"/> if it has one. This forms a full path from the
		/// <see cref="StorageRoot"/> to this folder (which can be obtained from
		/// <see cref="GetFolderPath"/>). This string is a folder name only, and not a path.
		/// Note also that only the root <see cref="RootFolderName"/> instance has a
		/// null <see cref="ThisFolderName"/> (and vice-versa: if this is null, then
		/// the <see cref="ParentAppData"/> is also null, and vice-versa).
		/// </summary>
		string ThisFolderName { get; }


		/// <summary>
		/// If this instance has been created as a nested instance by
		/// <see cref="SubFolder(string,ISerializer)"/>, then this is the FIRST parent
		/// <see cref="IAppData"/> instance that created this instance -- and otherwise is null. The
		/// <see cref="ThisFolderName"/> on THIS instance is nested within the <see cref="ThisFolderName"/> on
		/// this parent (or the parent's <see cref="RootFolderName"/> if the parent is not a
		/// subfolder). If not null, all persistence here is nested within this parent and all parents of
		/// this parent. Note also that only the root <see cref="RootFolderName"/> instance has a
		/// null <see cref="ParentAppData"/> (and vice-versa: if this is null, then
		/// the <see cref="ThisFolderName"/> is also null, and vice-versa).
		/// </summary>
		IAppData ParentAppData { get; }

		/// <summary>
		/// Always not null: returns the root <see cref="IAppData"/> instance:
		/// it is the instance whose <see cref="ThisFolderName"/> (and
		/// <see cref="ParentAppData"/>) is null.
		/// </summary>
		IAppData GetRootAppData();


		/// <summary>
		/// This method returns this instance's full folder path: this is a full path to the folder that is
		/// created by combining the <see cref="ThisFolderName"/> if any, plus the <see cref="ThisFolderName"/>
		/// on any parent instances, plus <see cref="RootFolderName"/>; and all within the <see cref="StorageRoot"/>.
		/// </summary>
		string GetFolderPath();

		/// <summary>
		/// This method will create a full file path to a nested file or folder within
		/// this folder. The path begins with the full <see cref="GetFolderPath"/>,
		/// and is combined with your argument --- which may be a file name only,
		/// or a further nested path; and, may point to a directory.
		/// </summary>
		/// <param name="fileName">Must not be null. Used as-is in <c>Path.Combine</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If <c>Path.Combine</c> does.</exception>
		string GetPath(string fileName);


		/// <summary>
		/// This method is used to create <see cref="IAppData"/> instances that begin to persist into a
		/// nested subfolder. The returned instance persists in a folder that is the argument combined with
		/// any possible <see cref="ThisFolderName"/> on this object, and all parent instances up to the
		/// <see cref="RootFolderName"/>. The <see cref="ThisFolderName"/> on the returned instance is
		/// this argument only. This string must be a folder name only, and not a path.
		/// </summary>
		/// <param name="subFolderName">Not null or empty.</param>
		/// <param name="serializer">Optional: if provided, then the returned instance uses this
		/// serializer.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		IAppData SubFolder(string subFolderName, ISerializer serializer = null);

		/// <summary>
		/// This method performs the same function as
		/// <see cref="SubFolder(string,ISerializer)"/>: this
		/// will create the <c>subFolderName</c> by convention, from the given type's <c>FullName</c>.
		/// Illegal file characters are replaced with "_".
		/// </summary>
		/// <param name="type">Not null.</param>
		/// <param name="serializer">Optional: if provided, then the returned instance uses this
		/// serializer.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		IAppData SubFolder(Type type, ISerializer serializer = null);

		/// <summary>
		/// This method performs the same function as
		/// <see cref="SubFolder(string,ISerializer)"/>: this
		/// will create the <c>subFolderName</c> by convention, from the given Assembly's <c>Name</c>.
		/// Illegal file characters are replaced with "_".
		/// </summary>
		/// <param name="assembly">Not null.</param>
		/// <param name="serializer">Optional: if provided, then the returned instance uses this
		/// serializer.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		IAppData SubFolder(Assembly assembly, ISerializer serializer = null);

		/// <summary>
		/// This method creates a new <see cref="StorageRoot"/> instance from the given argument,
		/// and re-creates the <see cref="RootFolderName"/>, all nested instances, and this
		/// instance to form the same path with a new root. The returned instance is the
		/// deepest NESTED instance --- that matches this one.
		/// </summary>
		/// <param name="storageRoot">Required; and must be a valid folder path.</param>
		/// <param name="serializer">Optional: if provided, then the returned instance and
		/// all instance in the path uses this serializer (it will be set on the root
		/// instance --- AND, otherwise, ALL instances use THIS instance's current
		/// <see cref="Serializer"/>.</param>
		/// <returns>Not null.</returns>
		IAppData NewStorageRoot(string storageRoot, ISerializer serializer = null);


		/// <summary>
		/// Convenience method that will return an <see cref="IEnumerable{T}"/> from
		/// <see cref="Directory"/>; and first check if the specified path exists --- and return
		/// an empty enumerable if so.
		/// </summary>
		/// <param name="searchPattern">Passed to
		/// <see cref="Directory.EnumerateFiles(string,string,SearchOption)"/>, or
		/// <see cref="Directory.EnumerateDirectories(string,string,SearchOption)"/>, or
		/// <see cref="Directory.EnumerateFileSystemEntries(string,string,SearchOption)"/>.</param>
		/// <param name="enumerateFilesOrDirectories">Pass true to enumerate files, false to
		/// enumerate directories, and null to enumerate all file system entries.</param>
		/// <param name="searchOption">Passed to <see cref="Directory"/>.</param>
		/// <param name="subfolderPath">Can be null: if so, then <see cref="GetFolderPath"/> is
		/// enumerated. Otherwise this is passed to <see cref="GetPath"/>.</param>
		/// <returns>Not null.</returns>
		IEnumerable<string> TryEnumerate(
				string searchPattern,
				bool? enumerateFilesOrDirectories = true,
				SearchOption searchOption = SearchOption.TopDirectoryOnly,
				string subfolderPath = null);

		/// <summary>
		/// Convenience method that will check if the file exists, and if so, open a <see cref="FileStream"/>
		/// in read mode, and invoke your Func within a using block on that stream.
		/// </summary>
		/// <typeparam name="TResult">Your own result.</typeparam>
		/// <param name="fileSubPath">Will be passed to <see cref="GetPath"/>.</param>
		/// <param name="usingFileIn">If the file exists, will be invoked with the stream.</param>
		/// <param name="ifNotExists">Will be invoked and this result is returned
		/// if the file does not exist.</param>
		/// <returns>Your Func result if the file exists; or your default result.</returns>
		TResult TryOpenRead<TResult>(
				string fileSubPath,
				Func<FileStream, TResult> usingFileIn,
				Func<TResult> ifNotExists = null);

		/// <summary>
		/// Convenience method that will create all folders, and open a <see cref="FileStream"/>
		/// in write mode, and invoke your Func within a using block on that stream.
		/// The file will be overwritten.
		/// </summary>
		/// <typeparam name="TResult">Your own reult.</typeparam>
		/// <param name="fileSubPath">Will be passed to <see cref="GetPath"/>.</param>
		/// <param name="usingFileOut">Will be invoked with the stream.</param>
		/// <returns>Your Func result.</returns>
		TResult OpenWrite<TResult>(string fileSubPath, Func<FileStream, TResult> usingFileOut);
	}
}
