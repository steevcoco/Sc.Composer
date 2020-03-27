using System;
using System.Collections.Generic;
using System.IO;
using Sc.Util.Collections;


namespace Sc.Util.IO
{
	/// <summary>
	/// Static helpers for <see cref="File"/>.
	/// </summary>
	public static class FileHelper
	{
		/// <summary>
		/// Convenience method that will return an <see cref="IEnumerable{T}"/> from
		/// <see cref="Directory"/>; and first check if the specified path exists --- and return
		/// and empty enumerable if so.
		/// </summary>
		/// <param name="path">Passed to
		/// <see cref="Directory.EnumerateFiles(string,string,SearchOption)"/>, or
		/// <see cref="Directory.EnumerateDirectories(string,string,SearchOption)"/>, or
		/// <see cref="Directory.EnumerateFileSystemEntries(string,string,SearchOption)"/>.</param>
		/// <param name="searchPattern">Passed to <see cref="Directory"/>.</param>
		/// <param name="enumerateFilesOrDirectories">Pass true to enumerate files, false to
		/// enumerate directories, and null to enumerate all file system entries.</param>
		/// <param name="searchOption">Passed to <see cref="Directory"/>.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<string> TryEnumerate(
				string path,
				string searchPattern,
				bool? enumerateFilesOrDirectories = true,
				SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			return !Directory.Exists(path)
				? EnumerableHelper.EmptyEnumerator<string>()
						.AsEnumerable()
				: (enumerateFilesOrDirectories switch
				{
					true
						=> Directory.EnumerateFiles(path, searchPattern, searchOption),
					false
						=> Directory.EnumerateDirectories(path, searchPattern, searchOption),
					_
						=> Directory.EnumerateFileSystemEntries(path, searchPattern, searchOption),
				});
		}


		/// <summary>
		/// Convenience method that will check if the file exists,
		/// and if so, open a <see cref="FileStream"/>
		/// in read mode, and invoke your Func within a using block on that stream.
		/// </summary>
		/// <typeparam name="TResult">Your own result.</typeparam>
		/// <param name="filePath">Will be passed to <see cref="File"/>.</param>
		/// <param name="usingFileIn">If the file exists, will be invoked with the stream.</param>
		/// <param name="ifNotExists">Will be returned if the file does not exist.</param>
		/// <returns>Your Func result if the file exists; or your default result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TResult TryOpenRead<TResult>(
				string filePath,
				Func<FileStream, TResult> usingFileIn,
				TResult ifNotExists = default)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			if (!File.Exists(filePath))
				return ifNotExists;
			using (FileStream fileIn = File.OpenRead(filePath)) {
				return usingFileIn(fileIn);
			}
		}

		/// <summary>
		/// Convenience method that will check if the file exists,
		/// and if so, open a <see cref="FileStream"/>
		/// in read mode, and invoke your Func within a using block on that stream.
		/// </summary>
		/// <typeparam name="TResult">Your own result.</typeparam>
		/// <param name="filePath">Will be passed to <see cref="File"/>.</param>
		/// <param name="usingFileIn">If the file exists, will be invoked with the stream.</param>
		/// <param name="ifNotExists">Will be invoked and this result is returned
		/// if the file does not exist.</param>
		/// <returns>Your Func result if the file exists; or your default result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TResult TryOpenRead<TResult>(
				string filePath,
				Func<FileStream, TResult> usingFileIn,
				Func<TResult> ifNotExists = default)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			if (!File.Exists(filePath)) {
				return ifNotExists != null
						? ifNotExists()
						: default;
			}
			using (FileStream fileIn = File.OpenRead(filePath)) {
				return usingFileIn(fileIn);
			}
		}

		/// <summary>
		/// Convenience method that will create all folders, and open a <see cref="FileStream"/>
		/// in write mode, and invoke your Func within a using block on that stream. The
		/// file will be overwritten.
		/// </summary>
		/// <typeparam name="TResult">Your own result.</typeparam>
		/// <param name="filePath">Will be passed to <see cref="File"/>.</param>
		/// <param name="usingFileOut">Will be invoked with the stream.</param>
		/// <returns>Your Func result.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TResult OpenWrite<TResult>(string filePath, Func<FileStream, TResult> usingFileOut)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			string directoryName = Path.GetDirectoryName(filePath);
			if (string.IsNullOrEmpty(directoryName))
				throw new ArgumentException(filePath, nameof(filePath));
			Directory.CreateDirectory(directoryName);
			using (FileStream fileOut = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				return usingFileOut(fileOut);
			}
		}
	}
}
