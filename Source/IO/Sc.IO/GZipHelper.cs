using System;
using System.IO;
using System.IO.Compression;


namespace Sc.IO
{
	/// <summary>
	/// Static methods for GZip streams.
	/// </summary>
	public static class GZipHelper
	{
		/// <summary>
		/// Saves an uncompressed <c>sourceStream</c> to the specified file, in a GZip compressed
		/// file. If any file exists by this full name, it is overwritten. Does not close the
		/// <c>sourceStream</c>. Throws any exceptions raised by <see cref="Path.GetDirectoryName"/>,
		/// <see cref="Directory.CreateDirectory(string)"/>, <see cref="File.Create(string)"/>,
		/// <see cref="FileStream"/>, or <see cref="GZipStream"/>.
		/// </summary>
		/// <param name="sourceStream">Not null. The source.</param>
		/// <param name="filePath">Not null. The destination.</param>
		/// <exception cref="ArgumentNullException">If <c>sourceStream</c> or <c>filePath</c>
		/// is null or empty.</exception>
		public static void SaveGZipFile(Stream sourceStream, string filePath)
		{
			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			string directoryName = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directoryName))
				Directory.CreateDirectory(directoryName);
			using (FileStream fileOut = File.Create(filePath)) {
				using (GZipStream gZipOut = new GZipStream(fileOut, CompressionMode.Compress)) {
					sourceStream.CopyTo(gZipOut);
					gZipOut.Flush();
					fileOut.Flush();
				}
			}
		}

		/// <summary>
		/// Restores a GZip compressed file from the specified file. If a file by this full name does
		/// not exist, throws <see cref="FileNotFoundException"/> or <see cref="DirectoryNotFoundException"/>.
		/// Otherwise it opens the file and a <see cref="GZipStream"/> with <see cref="CompressionMode.Decompress"/>,
		/// and invokes <see cref="Stream.CopyTo(Stream)"/> --- which writes the uncompressed data to your
		/// <c>destinationStream</c>. If an empty file exists, copies nothing. Does not close the
		/// <c>destinationStream</c>. Throws any other exceptions raised by <see cref="File.Exists"/>, new
		/// <see cref="FileInfo"/>, <see cref="File.OpenRead"/>, <see cref="FileStream"/>, or <see cref="GZipStream"/>.
		/// </summary>
		/// <param name="filePath">Not null. The source.</param>
		/// <param name="destinationStream">Not null. The destination.</param>
		/// <exception cref="ArgumentNullException">If <c>destinationStream</c> or <c>filePath</c>
		/// is null or empty.</exception>
		public static void ReadGZipFile(string filePath, Stream destinationStream)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			if (destinationStream == null)
				throw new ArgumentNullException(nameof(destinationStream));
			if (!File.Exists(filePath)) {
				throw new FileNotFoundException(
						$"{nameof(GZipHelper)}.{nameof(GZipHelper.ReadGZipFile)}: file not found: \"{filePath}\".");
			}
			if (new FileInfo(filePath).Length == 0L)
				return;
			using (FileStream fileIn = File.OpenRead(filePath)) {
				GZipHelper.ReadGZipStream(fileIn, destinationStream);
			}
		}

		/// <summary>
		/// Restores a GZip compressed stream from the specified <c>sourceStream</c>. Opens a <see cref="GZipStream"/>
		/// with <see cref="CompressionMode.Decompress"/>, and invokes <see cref="Stream.CopyTo(Stream)"/> --- which
		/// writes the uncompressed data to your <c>destinationStream</c>. Does not close the <c>sourceStream</c> or
		/// <c>destinationStream</c>. Throws any other exceptions raised by <see cref="GZipStream"/>.
		/// </summary>
		/// <param name="sourceStream">Not null. The source.</param>
		/// <param name="destinationStream">Not null. The destination.</param>
		/// <exception cref="ArgumentNullException">If <c>sourceStream</c> or <c>destinationStream</c>
		/// is null.</exception>
		public static void ReadGZipStream(Stream sourceStream, Stream destinationStream)
		{
			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));
			if (destinationStream == null)
				throw new ArgumentNullException(nameof(destinationStream));
			using (GZipStream gZipIn = new GZipStream(sourceStream, CompressionMode.Decompress, true)) {
				gZipIn.CopyTo(destinationStream);
				sourceStream.Flush();
				gZipIn.Flush();
				destinationStream.Flush();
			}
		}

		/// <summary>
		/// Writes a GZip compressed stream to the specified <c>destinationStream</c>. Opens a <see cref="GZipStream"/>
		/// with <see cref="CompressionMode.Compress"/>, and invokes <see cref="Stream.CopyTo(Stream)"/> --- which
		/// compresses the data from your <c>sourceStream</c> and writes the compressed data to your
		/// <c>destinationStream</c>. Does not close the <c>sourceStream</c> or <c>destinationStream</c>.
		/// Throws any other exceptions raised by <see cref="GZipStream"/>.
		/// </summary>
		/// <param name="sourceStream">Not null. The source.</param>
		/// <param name="destinationStream">Not null. The destination.</param>
		/// <exception cref="ArgumentNullException">If <c>sourceStream</c> or <c>destinationStream</c>
		/// is null.</exception>
		public static void WriteGZipStream(Stream sourceStream, Stream destinationStream)
		{
			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));
			if (destinationStream == null)
				throw new ArgumentNullException(nameof(destinationStream));
			using (GZipStream gZipout = new GZipStream(destinationStream, CompressionMode.Compress, true)) {
				sourceStream.CopyTo(gZipout);
				sourceStream.Flush();
				gZipout.Flush();
				destinationStream.Flush();
			}
		}

		/// <summary>
		/// Writes a GZip compressed stream and returns the bytes. Opens a <see cref="GZipStream"/>
		/// with <see cref="CompressionMode.Compress"/>, and writes compressed data from your <c>sourceData</c>.
		/// Throws any other exceptions raised by <see cref="GZipStream"/>.
		/// </summary>
		/// <param name="sourceData">Not null.</param>
		/// <exception cref="ArgumentNullException">If <c>sourceData</c> is null.</exception>
		public static byte[] GZip(byte[] sourceData)
		{
			if (sourceData == null)
				throw new ArgumentNullException(nameof(sourceData));
			using (MemoryStream memStream = new MemoryStream((int)(sourceData.Length * .8D))) {
				using (GZipStream gZipout = new GZipStream(memStream, CompressionMode.Compress, false)) {
					gZipout.Write(sourceData, 0, sourceData.Length);
					gZipout.Flush();
					memStream.Flush();
					return memStream.ToArray();
				}
			}
		}

		/// <summary>
		/// Decompresses a GZip compressed stream and returns the bytes. Opens a <see cref="GZipStream"/>
		/// with <see cref="CompressionMode.Decompress"/>, and reads and decompresses the data.
		/// </summary>
		/// <param name="compressedData">Not null.</param>
		/// <exception cref="ArgumentNullException">If <c>compressedData</c> is null.</exception>
		public static byte[] UnZip(byte[] compressedData)
		{
			if (compressedData == null)
				throw new ArgumentNullException(nameof(compressedData));
			using (MemoryStream memStream = new MemoryStream(compressedData)) {
				using (GZipStream gZipIn = new GZipStream(memStream, CompressionMode.Decompress, false)) {
					using (MemoryStream outStream = new MemoryStream((int)(compressedData.Length * 1.5D))) {
						gZipIn.CopyTo(outStream);
						memStream.Flush();
						gZipIn.Flush();
						outStream.Flush();
						return outStream.ToArray();
					}
				}
			}
		}
	}
}
