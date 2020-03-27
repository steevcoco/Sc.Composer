using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;


namespace Sc.Util.IO
{
	/// <summary>
	/// Static File utilities.
	/// </summary>
	public static class PathHelper
	{
		private static readonly object syncLock = new object();


		/// <summary>
		/// This method does similar work to <see cref="Path.Combine(string,string)"/>, but this
		/// method checks the first characters in the <c>relativePath</c>, removing those characters
		/// while the string starts with <see cref="Path.DirectorySeparatorChar"/>,
		/// <see cref="Path.AltDirectorySeparatorChar"/>, <see cref="Path.PathSeparator"/>, or
		/// <see cref="Path.VolumeSeparatorChar"/>. --- For use when <c>Path.Combine</c> does not
		/// return the expected absolute path if the relative portion begins with a DirectorySeparator.
		/// </summary>
		/// <param name="rootPath">The first path to combine: not null, but can be empty.</param>
		/// <param name="relativePath">The second path to combine.</param>
		/// <param name="throwIfRelativePathNull">Defaults to <c>false</c>: if <c>relativePath</c> is
		/// <c>null</c> then <see cref="string.Empty"/> is returned. If true, the method throws.</param>
		/// <param name="throwIfRelativePathEmpty">Defaults to <c>false</c>: if <c>relativePath</c> is empty,
		/// OR if after stripping the invalid characters, it BECOMES empty, then <see cref="string.Empty"/>
		/// is returned. If true, the method throws.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException">If <c>rootPath</c> is null, or if <c>relativePath</c>
		/// is null and the method is specified to throw.</exception>
		/// <exception cref="ArgumentException">If <c>relativePath</c> is empty or becomes
		/// empty after removing invalid characters, and the method is specified to throw.
		/// Or if a path contains invalid characters.</exception>
		public static string PathCombine(
				string rootPath,
				string relativePath,
				bool throwIfRelativePathNull = false,
				bool throwIfRelativePathEmpty = false)
		{
			if (rootPath == null)
				throw new ArgumentNullException(nameof(rootPath));
			return Path.Combine(
					rootPath,
					PathHelper.RemoveLeadingSeparators(
							relativePath,
							throwIfRelativePathNull,
							throwIfRelativePathEmpty));
		}

		/// <summary>
		/// This method prepares paths that will be used with <see cref="Path.Combine(string,string)"/>,
		/// by checking the first characters in the <c>path</c>, and removing those characters
		/// while the string starts with <see cref="Path.DirectorySeparatorChar"/>,
		/// <see cref="Path.AltDirectorySeparatorChar"/>, <see cref="Path.PathSeparator"/>, or
		/// <see cref="Path.VolumeSeparatorChar"/>. --- For use when <c>Path.Combine</c> does not
		/// return the expected absolute path if the relative portion begins with a DirectorySeparator.
		/// </summary>
		/// <param name="path">The relative portion of a full path.</param>
		/// <param name="throwIfNull">Defaults to <c>false</c>: if <c>path</c> is
		/// <c>null</c> then <see cref="string.Empty"/> is returned. If true, the method throws.</param>
		/// <param name="throwIfEmpty">Defaults to <c>false</c>: if <c>path</c> is empty,
		/// OR if after stripping the invalid characters, it BECOMES empty, then <see cref="string.Empty"/>
		/// is returned. If true, the method throws.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException">If the argument is null, and the method is specified
		/// to throw.</exception>
		/// <exception cref="ArgumentException">If the argument is empty or becomes empty after
		/// removing invalid characters, and the method is specified to throw.</exception>
		public static string RemoveLeadingSeparators(string path, bool throwIfNull = false, bool throwIfEmpty = false)
		{
			if (path == null) {
				if (throwIfNull)
					throw new ArgumentNullException(nameof(path));
				return string.Empty;
			}
			do {
				if (string.IsNullOrEmpty(path)) {
					if (throwIfEmpty)
						throw new ArgumentException(nameof(path));
					return string.Empty;
				}
				// Typically removing only zero or one character: no StringBuilder is used
				if ((path[0] == Path.DirectorySeparatorChar)
						|| (path[0] == Path.AltDirectorySeparatorChar)
						|| (path[0] == Path.PathSeparator)
						|| (path[0] == Path.VolumeSeparatorChar)) {
					path = path.Substring(1);
				} else {
					return path;
				}
			} while (true);
		}

		/// <summary>
		/// This method prepares a path by checking the last characters in the <c>path</c>, and removing
		/// those characters while the string ends with <see cref="Path.DirectorySeparatorChar"/>,
		/// <see cref="Path.AltDirectorySeparatorChar"/>, <see cref="Path.PathSeparator"/>, or
		/// <see cref="Path.VolumeSeparatorChar"/>. --- For use when <c>Path.GetDirectoryName</c> does
		/// not return the expected parent if the path ends with a DirectorySeparator.
		/// </summary>
		/// <param name="path">The root portion of a full path.</param>
		/// <param name="throwIfNull">Defaults to <c>false</c>: if <c>path</c> is
		/// <c>null</c> then <see cref="string.Empty"/> is returned. If true, the method throws.</param>
		/// <param name="throwIfEmpty">Defaults to <c>false</c>: if <c>path</c> is empty,
		/// OR if after stripping the invalid characters, it BECOMES empty, then <see cref="string.Empty"/>
		/// is returned. If true, the method throws.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException">If the argument is null, and the method is specified
		/// to throw.</exception>
		/// <exception cref="ArgumentException">If the argument is whitespace or becomes empty after
		/// removing invalid characters, and the method is specified to throw.</exception>
		public static string RemoveTrailingSeparators(string path, bool throwIfNull = false, bool throwIfEmpty = false)
		{
			if (path == null) {
				if (throwIfNull)
					throw new ArgumentNullException(nameof(path));
				return string.Empty;
			}
			do {
				if (string.IsNullOrEmpty(path)) {
					if (throwIfEmpty)
						throw new ArgumentException(nameof(path));
					return string.Empty;
				}
				// Typically removing only zero or one character: no StringBuilder is used
				if ((path[path.Length - 1] == Path.DirectorySeparatorChar)
						|| (path[path.Length - 1] == Path.AltDirectorySeparatorChar)
						|| (path[path.Length - 1] == Path.PathSeparator)
						|| (path[path.Length - 1] == Path.VolumeSeparatorChar)) {
					path = path.Substring(0, path.Length - 1);
				} else {
					return path;
				}
			} while (true);
		}

		/// <summary>
		/// This method ensures that the <paramref name="path"/>
		/// ends with a <see cref="Path.DirectorySeparatorChar"/>
		/// --- and checks ONLY that last character now..
		/// </summary>
		/// <param name="path">Not null or empty.</param>
		/// <returns>Not null or empty.</returns>
		/// \<exception cref="ArgumentNullException"></exception>
		public static string EnsureTrailingSeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			return path[path.Length - 1] == Path.DirectorySeparatorChar
					? path
					: path + Path.DirectorySeparatorChar;
		}


		/// <summary>
		/// Checks if two paths are the same path. This method removes trailing separator characters
		/// from each path with <see cref="RemoveTrailingSeparators"/>, then gets each path's
		/// full path, and compares those with the given <see cref="StringComparison"/>.
		/// </summary>
		/// <param name="a">Not null; may be empty.</param>
		/// <param name="b">Not null; may be empty.</param>
		/// <param name="stringComparison">Defaults to
		/// <see cref="StringComparison.InvariantCultureIgnoreCase"/>.</param>
		/// <returns>True if the paths are the same full path.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If a path contains invalid characters.</exception>
		/// <exception cref="SecurityException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		public static bool IsSameFullPath(
				string a,
				string b,
				StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
			=> string.Equals(
					Path.GetFullPath(PathHelper.RemoveTrailingSeparators(a, true)),
					Path.GetFullPath(PathHelper.RemoveTrailingSeparators(b, true)),
					stringComparison);


		/// <summary>
		/// Tests that the string is valid for a file name. It must not be a path; and may or may not
		/// contain an extension. This method acquires a mutex, and creates a new <see cref="FileInfo"/>
		/// at a private subfolder path within the <see cref="Path.GetTempPath"/> folder. --- It creates
		/// the FileInfo only. Returns 0 for success.
		/// </summary>
		/// <param name="fileName">The tested name.</param>
		/// <param name="throwIfInvalid">Defaults to false: the method will catch exceptions and
		/// return a code. If set true, the method throws the documented exception; or returns
		/// 0 for success.</param>
		/// <returns>Code:
		/// <ul>
		/// <li>0:  Valid.</li>
		/// <li>1:  The argument is null; or <see cref="FileInfo"/> <see cref="ArgumentException"/>:
		/// The file name is empty, contains only white spaces, or contains invalid characters.</li>
		/// <li>2:  <see cref="FileInfo"/> <see cref="PathTooLongException"/>: The specified path,
		/// file name, or both exceed the system-defined maximum length.</li>
		/// <li>3:  <see cref="FileInfo"/> <see cref="SecurityException"/>: The caller does not
		/// have the required permission.</li>
		/// <li>4:  <see cref="FileInfo"/> <see cref="NotSupportedException"/>: fileName contains
		/// a colon (:) in the middle of the string.</li>
		/// </ul>
		/// </returns>
		/// <exception cref="ArgumentNullException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		/// <exception cref="ArgumentException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		/// <exception cref="PathTooLongException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		/// <exception cref="SecurityException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		/// <exception cref="NotSupportedException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		public static int IsValidFileName(string fileName, bool throwIfInvalid = false)
		{
			if (fileName == null) {
				if (throwIfInvalid)
					throw new ArgumentNullException(nameof(fileName));
				return 1;
			}
			lock (PathHelper.syncLock) {
				try {
					int hashCode
							= new FileInfo(
									Path.Combine(
											Path.Combine(
													Path.GetTempPath(),
													"SCFUTIL"),
											fileName)).GetHashCode();
					hashCode = Math.Abs(hashCode);
					hashCode -= hashCode;
					return hashCode;
				} catch (ArgumentNullException) when (!throwIfInvalid) {
					return 1;
				} catch (ArgumentException) when (!throwIfInvalid) {
					return 1;
				} catch (PathTooLongException) when (!throwIfInvalid) {
					return 2;
				} catch (SecurityException) when (!throwIfInvalid) {
					return 3;
				} catch (NotSupportedException) when (!throwIfInvalid) {
					return 4;
				}
			}
		}

		/// <summary>
		/// Tests that the string is valid for a directory name. It must not be a path. This method
		/// acquires a mutex, and creates a new <see cref="DirectoryInfo"/> at a private subfolder
		/// path within the <see cref="Path.GetTempPath"/> folder. --- It creates the DirectoryInfo
		/// only. Returns 0 for success.
		/// </summary>
		/// <param name="directoryName">The tested name.</param>
		/// <param name="throwIfInvalid">Defaults to false: the method will catch exceptions and
		/// return a code. If set true, the method throws the documented exception; or returns
		/// 0 for success.</param>
		/// <returns>Code:
		/// <ul>
		/// <li>0:  Valid.</li>
		/// <li>1:  The argument is null; or <see cref="DirectoryInfo"/> <see cref="ArgumentException"/>:
		/// The directory name contains invalid characters.</li>
		/// <li>2:  <see cref="DirectoryInfo"/> <see cref="PathTooLongException"/>: The specified path,
		/// file name, or both exceed the system-defined maximum length.</li>
		/// <li>3:  <see cref="DirectoryInfo"/> <see cref="SecurityException"/>: The caller does not
		/// have the required permission.</li>
		/// </ul>
		/// </returns>
		/// <exception cref="ArgumentNullException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		/// <exception cref="ArgumentException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		/// <exception cref="SecurityException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		/// <exception cref="PathTooLongException">As documented, if <c>throwIfInvalid</c>
		/// is true.</exception>
		public static int IsValidDirectoryName(string directoryName, bool throwIfInvalid = false)
		{
			if (directoryName == null) {
				if (throwIfInvalid)
					throw new ArgumentNullException(nameof(directoryName));
				return 1;
			}
			lock (PathHelper.syncLock) {
				try {
					int hashCode
							= new DirectoryInfo(
									Path.Combine(
											Path.Combine(
													Path.GetTempPath(),
													"SCFUTIL"),
											directoryName)).GetHashCode();
					hashCode = Math.Abs(hashCode);
					hashCode -= hashCode;
					return hashCode;
				} catch (ArgumentNullException) when (!throwIfInvalid) {
					return 1;
				} catch (ArgumentException) when (!throwIfInvalid) {
					return 1;
				} catch (PathTooLongException) when (!throwIfInvalid) {
					return 2;
				} catch (SecurityException) when (!throwIfInvalid) {
					return 3;
				}
			}
		}


		/// <summary>
		/// Determines if this <paramref name="path"/> is a valid file
		/// or directory path: this will invoke <see cref="Path.GetFullPath"/>
		/// to check the path; and will propagate a <see cref="SecurityException"/>
		/// only.
		/// </summary>
		/// <param name="path">THe path to check.</param>
		/// <returns>True if the path is valid.</returns>
		/// <exception cref="SecurityException"></exception>
		public static bool IsValidPath(string path)
		{
			try {
				path = Path.GetFullPath(path);
				return true;
			} catch (SecurityException) {
				throw;
			} catch {
				return false;
			}
		}


		/// <summary>
		/// This method will replace all characters from <see cref="Path.GetInvalidFileNameChars"/>
		/// in the argument; and then test the result with <see cref="IsValidFileName"/> or
		/// <see cref="IsValidDirectoryName"/>. You may also provides Func to provide a fallback name
		/// if the method cannot construct a valid file name. NOTICE that the method always returns
		/// its own result: if it is unable to construct a name, then it will invoke your Func and
		/// set the out argument from your delegate; and still returns the non-zero error code from
		/// its own attempt. If the method returns a non-zero non-null result, then your Func has been
		/// invoked; and otherwise your Func was not invoked. If your Func is invoked, this will not
		/// test that result; and always returns the error code. The return value is null if the
		/// incoming name needs no changes; zero if this method successfully transforms the out
		/// argument; and otherwise as specified below (if you do not provide a fallback, the out
		/// result is not valid; and otherwise has simply been set to your result).
		/// </summary>
		/// <param name="fileName">NOTICE: can be null --- the method will return <c>1</c>.</param>
		/// <param name="name">Always set to the name with any invalid characters replaced,
		/// after this method's attempt; or else your Func's result.</param>
		/// <param name="invalidCharReplacement">Specifies the replacement for each invalid character.
		/// Defaults to <c>"_"</c>. You may pass null to remove all invalid characters.</param>
		/// <param name="isDirectoryName">Set this to true if this is a candidate directory name:
		/// the method tests the result with <see cref="IsValidDirectoryName"/>.</param>
		/// <param name="fallback">Optional: if provided, this is invoked if the method cannot construct
		/// a valid name --- and if invoked, the method always returns a non-zero, non-null result,
		/// and does not test the result of this delegate.</param>
		/// <returns>The method returns null if the incoming value is valid; and the out argument has
		/// no changes. Otherwise: if the result is zero, then the method successfully transformed the
		/// input, and the <c>name</c> is now valid; and your delegate was not invoked. Otherwise, the
		/// result is the non-zero, non-null result of <see cref="IsValidFileName"/> or
		/// <see cref="IsValidDirectoryName"/>, invoked on the result of this method's attempt; and,
		/// then if you provided a delegate, then the <c>name</c> has been set to your result, and this
		/// method's result is not available --- the returned error code is the code from this method's
		/// attempt, and your delegate's result is not tested here. If you do not provide a delegate,
		/// then the <c>name</c> is set to this method's attempt, and the result code is for that
		/// value.</returns>
		public static int? GetValidFileName(
				string fileName,
				out string name,
				string invalidCharReplacement = "_",
				bool isDirectoryName = false,
				Func<string, string> fallback = null)
		{
			if (fileName == null) {
				name = fallback?.Invoke(null);
				return 1;
			}
			StringBuilder sb = new StringBuilder(fileName);
			if ((invalidCharReplacement == null)
					|| (invalidCharReplacement.Length == 0)) {
				foreach (char invalidChar in Path.GetInvalidFileNameChars()) {
					sb.Replace($"{invalidChar}", string.Empty);
				}
			} else {
				char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
				foreach (char c in invalidCharReplacement) {
					if (invalidFileNameChars.Contains(c))
						throw new ArgumentException(invalidCharReplacement, nameof(invalidCharReplacement));
				}
				if (invalidCharReplacement.Length == 1) {
					foreach (char invalidChar in invalidFileNameChars) {
						sb.Replace(invalidChar, invalidCharReplacement[0]);
					}
				} else {
					foreach (char invalidChar in invalidFileNameChars) {
						sb.Replace($"{invalidChar}", invalidCharReplacement);
					}
				}
			}
			name = sb.ToString();
			if (string.Equals(fileName, name, StringComparison.Ordinal))
				return null;
			int result = isDirectoryName
					? PathHelper.IsValidDirectoryName(name)
					: PathHelper.IsValidFileName(name);
			if ((result == 0)
					|| (fallback == null)) {
				return result;
			}
			name = fallback.Invoke(null);
			return result;
		}
	}
}
