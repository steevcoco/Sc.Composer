using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace Sc.IO
{
	/// <summary>
	/// IO helper methods.
	/// </summary>
	public static class IoHelper
	{
		/// <summary>
		/// Method to read a fixed-size buffer from the <see cref="Stream"/>. The read must return all bytes
		/// in the buffer's length; or else the <see cref="IoResult.Result"/> is
		/// <see cref="IoResultState.BadData"/>. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. The read operation updates the
		/// <see cref="IoResult.BytesReadOrWritten"/> property: if there is an incoming value, it is incremented.
		/// </summary>
		/// <param name="ioResult">Not null.</param>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="fixedBuffer">Not null.</param>
		/// <returns>Completes when the read completes.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static Task<IoResult> ReadAllAsync(this IoResult ioResult, Stream sourceStream, byte[] fixedBuffer)
		{
			if (ioResult == null)
				throw new ArgumentNullException(nameof(ioResult));
			return IoHelper.ReadAllAsync(sourceStream, fixedBuffer, ioResult);
		}

		/// <summary>
		/// Method to read to a buffer from the <see cref="Stream"/>. The read must return all bytes in the
		/// given <c>length</c>; or else the <see cref="IoResult.Result"/> is
		/// <see cref="IoResultState.BadData"/>. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. This method takes an <c>offset</c> and  <c>length</c> within
		/// the <c>buffer</c>; and updates the <see cref="IoResult.BytesReadOrWritten"/> property: notice that
		/// if there is an incoming value, it is incremented.
		/// </summary>
		/// <param name="ioResult">Not null.</param>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin reading.</param>
		/// <param name="length">Length within <c>buffer</c> to read.</param>
		/// <returns>Completes when the read completes.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<IoResult> ReadAllAsync(
				this IoResult ioResult,
				Stream sourceStream,
				byte[] buffer,
				int offset,
				int length)
		{
			if (ioResult == null)
				throw new ArgumentNullException(nameof(ioResult));
			return IoHelper.ReadAllAsync(sourceStream, buffer, offset, length, ioResult);
		}

		/// <summary>
		/// Method to read a fixed-size buffer from the <see cref="Stream"/>. The read must return all bytes
		/// in the buffer's length; or else the <see cref="IoResult.Result"/> is
		/// <see cref="IoResultState.BadData"/>. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. The read operation updates the
		/// <see cref="IoResult.BytesReadOrWritten"/> property: if there is an incoming value, it is incremented.
		/// </summary>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="fixedBuffer">Not null.</param>
		/// <param name="ioResult">Optional. If null, a new instance is created; with any given
		/// <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">Optional.</param>
		/// <returns>Not null. If you provided an argument, the same object is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static async Task<IoResult> ReadAllAsync(
				Stream sourceStream,
				byte[] fixedBuffer,
				IoResult ioResult = null,
				CancellationToken cancellationToken = default)
			=> await IoHelper.ReadAllAsync(
					sourceStream,
					fixedBuffer,
					0,
					fixedBuffer.Length,
					ioResult,
					cancellationToken);

		/// <summary>
		/// Method to read to a buffer from the <see cref="Stream"/>. The read must return all bytes in the
		/// given <c>length</c>; or else the <see cref="IoResult.Result"/> is
		/// <see cref="IoResultState.BadData"/>. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. This method takes an <c>offset</c> and  <c>length</c> within
		/// the <c>buffer</c>; and updates the <see cref="IoResult.BytesReadOrWritten"/> property: notice that
		/// if there is an incoming value, it is incremented.
		/// </summary>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin reading.</param>
		/// <param name="length">Length within <c>buffer</c> to read.</param>
		/// <param name="ioResult">Optional. If null, a new instance is created; with any given
		/// <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">Optional.</param>
		/// <returns>Not null. If you provided an argument, the same object is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static async Task<IoResult> ReadAllAsync(
				Stream sourceStream,
				byte[] buffer,
				int offset,
				int length,
				IoResult ioResult = null,
				CancellationToken cancellationToken = default)
		{
			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if ((offset < 0)
					|| (offset >= buffer.Length))
				throw new ArgumentOutOfRangeException(nameof(offset));
			if ((length < 0)
					|| (length > (buffer.Length - offset)))
				throw new ArgumentOutOfRangeException(nameof(length));
			if (ioResult == null)
				ioResult = new IoResult(cancellationToken);
			else if (!ioResult.IsSuccess)
				return ioResult;
			long targetCount = ioResult.BytesReadOrWritten + length;
			try {
				while (ioResult.BytesReadOrWritten < targetCount) {
					int bytesRead
							= await sourceStream.ReadAsync(
									buffer,
									offset,
									(int)targetCount - (int)ioResult.BytesReadOrWritten,
									ioResult.CancellationToken);
					ioResult.AddBytesReadOrWritten(bytesRead);
					if ((ioResult.BytesReadOrWritten == targetCount)
							|| ioResult.IsCancelled)
						break;
					if (bytesRead > 0) {
						offset += Math.Max(0, bytesRead);
						continue;
					}
					ioResult.BadData();
					break;
				}
			} catch (Exception exception) {
				ioResult.Fault(exception);
			}
			return ioResult;
		}


		/// <summary>
		/// Method to read to a buffer from the <see cref="Stream"/>. The read may return any length; and the
		/// result is updated by the the length. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. This method updates the <see cref="IoResult.BytesReadOrWritten"/>
		/// property: notice that if there is an incoming value, it is incremented.
		/// </summary>
		/// <param name="ioResult">Not null.</param>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin reading.</param>
		/// <param name="length">Length within <c>buffer</c> to read.</param>
		/// <returns>Completes when the indicated read completes.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<IoResult> ReadAsync(
				this IoResult ioResult,
				Stream sourceStream,
				byte[] buffer,
				int offset,
				int length)
		{
			if (ioResult == null)
				throw new ArgumentNullException(nameof(ioResult));
			return IoHelper.ReadAsync(sourceStream, buffer, offset, length, ioResult);
		}

		/// <summary>
		/// Method to read to a buffer from the <see cref="Stream"/>. The read may return any length; and the
		/// result is updated by the the length. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. This method updates the <see cref="IoResult.BytesReadOrWritten"/>
		/// property: notice that if there is an incoming value, it is incremented.
		/// </summary>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin reading.</param>
		/// <param name="length">Length within <c>buffer</c> to read.</param>
		/// <param name="ioResult">Optional. If null, a new instance is created; with any given
		/// <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">Optional.</param>
		/// <returns>Not null. If you provided an argument, the same object is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static async Task<IoResult> ReadAsync(
				Stream sourceStream,
				byte[] buffer,
				int offset,
				int length,
				IoResult ioResult = null,
				CancellationToken cancellationToken = default)
		{
			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if ((offset < 0)
					|| (offset >= buffer.Length))
				throw new ArgumentOutOfRangeException(nameof(offset));
			if ((length < 0)
					|| (length > (buffer.Length - offset)))
				throw new ArgumentOutOfRangeException(nameof(length));
			if (ioResult == null)
				ioResult = new IoResult(cancellationToken);
			else if (!ioResult.IsSuccess)
				return ioResult;
			try {
				ioResult.AddBytesReadOrWritten(
						await sourceStream.ReadAsync(
								buffer,
								offset,
								length,
								ioResult.CancellationToken));
			} catch (Exception exception) {
				ioResult.Fault(exception);
			}
			return ioResult;
		}


		/// <summary>
		/// Method to read from the source <see cref="Stream"/> and write to the sink. NOTICE: this method
		/// will ONLY update the <see cref="IoResult.BytesReadOrWritten"/> property if one of the given
		/// streams supports seeking. You must test <see cref="Stream.CanSeek"/> to know. Notice also that
		/// <see cref="IoResult.BytesReadOrWritten"/> may be 0 when the <see cref="IoResult.Result"/>
		/// is <see cref="IoResultState.Success"/>.
		/// </summary>
		/// <param name="ioResult">Not null.</param>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="bufferSize">Required.</param>
		/// <returns>Completes when the copy is coplete.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<IoResult> CopyToAsync(
				this IoResult ioResult,
				Stream sourceStream,
				Stream sinkStream,
				int bufferSize)
		{
			if (ioResult == null)
				throw new ArgumentNullException(nameof(ioResult));
			return IoHelper.CopyToAsync(sourceStream, sinkStream, bufferSize, ioResult);
		}

		/// <summary>
		/// Method to read from the source <see cref="Stream"/> and write to the sink. NOTICE: this method
		/// will ONLY update the <see cref="IoResult.BytesReadOrWritten"/> property if one of the given
		/// streams supports seeking. You must test <see cref="Stream.CanSeek"/> to know. Notice also that
		/// <see cref="IoResult.BytesReadOrWritten"/> may be 0 when the <see cref="IoResult.Result"/>
		/// is <see cref="IoResultState.Success"/>.
		/// </summary>
		/// <param name="sourceStream">Not null.</param>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="bufferSize">Required.</param>
		/// <param name="ioResult">Optional. If null, a new instance is created; with any given
		/// <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">Optional.</param>
		/// <returns>Not null. If you provided an argument, the same object is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static async Task<IoResult> CopyToAsync(
				Stream sourceStream,
				Stream sinkStream,
				int bufferSize,
				IoResult ioResult = null,
				CancellationToken cancellationToken = default)
		{
			if (sourceStream == null)
				throw new ArgumentNullException(nameof(sourceStream));
			if (sinkStream == null)
				throw new ArgumentNullException(nameof(sinkStream));
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(bufferSize), bufferSize.ToString());
			if (ioResult == null)
				ioResult = new IoResult(cancellationToken);
			else if (!ioResult.IsSuccess)
				return ioResult;
			try {
				long position = 0L;
				if (sourceStream.CanSeek)
					position = sourceStream.Position;
				else if (sinkStream.CanSeek)
					position = sinkStream.Position;
				await sourceStream.CopyToAsync(sinkStream, bufferSize, ioResult.CancellationToken);
				if (sourceStream.CanSeek)
					ioResult.AddBytesReadOrWritten(Math.Max(0L, sourceStream.Position - position));
				else if (sinkStream.CanSeek)
					ioResult.AddBytesReadOrWritten(Math.Max(0L, sinkStream.Position - position));
			} catch (Exception exception) {
				ioResult.Fault(exception);
			}
			return ioResult;
		}


		/// <summary>
		/// NOTICE: this method ALWAYS updates the <see cref="IoResult.BytesReadOrWritten"/> property by the buffer's
		/// length if the stream does not support seeking. This writes a fixed-size buffer to the sink
		/// <see cref="Stream"/>. If the stream can seek, the write must write all bytes in the given buffer's length;
		/// or else the <see cref="IoResult.Result"/> is <see cref="IoResultState.BadData"/>. Note that this
		/// method catches all exceptions and will <see cref="IoResult.Fault"/> the result. If the stream supports
		/// seeking, This method updates the <see cref="IoResult.BytesReadOrWritten"/> property based on the change in
		/// the stream's Position. Otherwise the value is always explicitly updated by the buffer's length: you must
		/// test the stream to know. If there is an incoming value, it is incremented. Otherwise, if the stream cannot
		/// seek, then one <see cref="Stream.WriteAsync(byte[],int,int,CancellationToken)"/> is invoked with the
		/// arguments; AND in this case, the <see cref="IoResult.Result"/> is never
		/// <see cref="IoResultState.BadData"/>.
		/// </summary>
		/// <param name="ioResult">Not null.</param>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="fixedBuffer">Not null.</param>
		/// <returns>Completes when the write completes.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<IoResult> WriteAllAsync(
				this IoResult ioResult,
				Stream sinkStream,
				byte[] fixedBuffer)
		{
			if (ioResult == null)
				throw new ArgumentNullException(nameof(ioResult));
			return IoHelper.WriteAllAsync(sinkStream, fixedBuffer, ioResult);
		}

		/// <summary>
		/// NOTICE: this method ALWAYS updates the <see cref="IoResult.BytesReadOrWritten"/> property by the given
		/// length if the stream does not support seeking. This writes a specified <c>length</c> from a buffer to the
		/// sink <see cref="Stream"/>. If the stream can seek, the write must write all bytes in the given length;
		/// or else the <see cref="IoResult.Result"/> is <see cref="IoResultState.BadData"/>. Note that this
		/// method catches all exceptions and will <see cref="IoResult.Fault"/> the result. If the stream supports
		/// seeking, This method updates the <see cref="IoResult.BytesReadOrWritten"/> property based on the change in
		/// the stream's Position. Otherwise the value is always explicitly updated by the given length: you must
		/// test the stream to know. If there is an incoming value, it is incremented. Otherwise, if the stream cannot
		/// seek, then one <see cref="Stream.WriteAsync(byte[],int,int,CancellationToken)"/> is invoked with the
		/// arguments; AND in this case, the <see cref="IoResult.Result"/> is never
		/// <see cref="IoResultState.BadData"/>.
		/// </summary>
		/// <param name="ioResult">Not null.</param>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin writing.</param>
		/// <param name="length">Length within <c>buffer</c> to write.</param>
		/// <returns>Completes when the write completes.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<IoResult> WriteAllAsync(
				this IoResult ioResult,
				Stream sinkStream,
				byte[] buffer,
				int offset,
				int length)
		{
			if (ioResult == null)
				throw new ArgumentNullException(nameof(ioResult));
			return IoHelper.WriteAllAsync(sinkStream, buffer, offset, length, ioResult);
		}

		/// <summary>
		/// NOTICE: this method ALWAYS updates the <see cref="IoResult.BytesReadOrWritten"/> property by the buffer's
		/// length if the stream does not support seeking. This writes a fixed-size buffer to the sink
		/// <see cref="Stream"/>. If the stream can seek, the write must write all bytes in the given buffer's length;
		/// or else the <see cref="IoResult.Result"/> is <see cref="IoResultState.BadData"/>. Note that this
		/// method catches all exceptions and will <see cref="IoResult.Fault"/> the result. If the stream supports
		/// seeking, This method updates the <see cref="IoResult.BytesReadOrWritten"/> property based on the change in
		/// the stream's Position. Otherwise the value is always explicitly updated by the buffer's length: you must
		/// test the stream to know. If there is an incoming value, it is incremented. Otherwise, if the stream cannot
		/// seek, then one <see cref="Stream.WriteAsync(byte[],int,int,CancellationToken)"/> is invoked with the
		/// arguments; AND in this case, the <see cref="IoResult.Result"/> is never
		/// <see cref="IoResultState.BadData"/>.
		/// </summary>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="fixedBuffer">Not null.</param>
		/// <param name="ioResult">Optional. If null, a new instance is created; with any given
		/// <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">Optional.</param>
		/// <returns>Not null. If you provided an argument, the same object is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static async Task<IoResult> WriteAllAsync(
				Stream sinkStream,
				byte[] fixedBuffer,
				IoResult ioResult = null,
				CancellationToken cancellationToken = default)
			=> await IoHelper.WriteAllAsync(
					sinkStream,
					fixedBuffer,
					0,
					fixedBuffer.Length,
					ioResult,
					cancellationToken);

		/// <summary>
		/// NOTICE: this method ALWAYS updates the <see cref="IoResult.BytesReadOrWritten"/> property by the given
		/// length if the stream does not support seeking. This writes a specified <c>length</c> from a buffer to the
		/// sink <see cref="Stream"/>. If the stream can seek, the write must write all bytes in the given length;
		/// or else the <see cref="IoResult.Result"/> is <see cref="IoResultState.BadData"/>. Note that this
		/// method catches all exceptions and will <see cref="IoResult.Fault"/> the result. If the stream supports
		/// seeking, This method updates the <see cref="IoResult.BytesReadOrWritten"/> property based on the change in
		/// the stream's Position. Otherwise the value is always explicitly updated by the given length: you must
		/// test the stream to know. If there is an incoming value, it is incremented. Otherwise, if the stream cannot
		/// seek, then one <see cref="Stream.WriteAsync(byte[],int,int,CancellationToken)"/> is invoked with the
		/// arguments; AND in this case, the <see cref="IoResult.Result"/> is never
		/// <see cref="IoResultState.BadData"/>.
		/// </summary>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin writing.</param>
		/// <param name="length">Length within <c>buffer</c> to write.</param>
		/// <param name="ioResult">Optional. If null, a new instance is created; with any given
		/// <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">Optional.</param>
		/// <returns>Not null. If you provided an argument, the same object is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static async Task<IoResult> WriteAllAsync(
				Stream sinkStream,
				byte[] buffer,
				int offset,
				int length,
				IoResult ioResult = null,
				CancellationToken cancellationToken = default)
		{
			if (sinkStream == null)
				throw new ArgumentNullException(nameof(sinkStream));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if ((offset < 0)
					|| (offset >= buffer.Length))
				throw new ArgumentOutOfRangeException(nameof(offset));
			if ((length < 0)
					|| (length > (buffer.Length - offset)))
				throw new ArgumentOutOfRangeException(nameof(length));
			if (ioResult == null)
				ioResult = new IoResult(cancellationToken);
			else if (!ioResult.IsSuccess)
				return ioResult;
			long targetCount = ioResult.BytesReadOrWritten + length;
			try {
				while (ioResult.BytesReadOrWritten < targetCount) {
					long position = sinkStream.CanSeek
							? sinkStream.Position
							: targetCount;
					await sinkStream.WriteAsync(
							buffer,
							offset,
							(int)targetCount - (int)ioResult.BytesReadOrWritten,
							ioResult.CancellationToken);
					long bytesWritten = sinkStream.CanSeek
							? Math.Max(0L, sinkStream.Position - position)
							: length;
					ioResult.AddBytesReadOrWritten(bytesWritten);
					if ((ioResult.BytesReadOrWritten == targetCount)
							|| ioResult.IsCancelled)
						break;
					if (bytesWritten > 0L) {
						offset += (int)bytesWritten;
						continue;
					}
					ioResult.BadData();
					break;
				}
			} catch (Exception exception) {
				ioResult.Fault(exception);
			}
			return ioResult;
		}


		/// <summary>
		/// NOTICE: this method ALWAYS updates the <see cref="IoResult.BytesReadOrWritten"/> property by the given
		/// length if the stream does not support seeking. This writes from a buffer to the sink <see cref="Stream"/>.
		/// The write may write any length. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. IF the stream supports seeking, this method updates the
		/// <see cref="IoResult.BytesReadOrWritten"/> property based on the change in the stream's Position. Otherwise
		/// the value is always explicitly updated by the given length: you must test the stream to know. If there is
		/// an incoming value, it is incremented.
		/// </summary>
		/// <param name="ioResult">Not null.</param>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin writing.</param>
		/// <param name="length">Length within <c>buffer</c> to write.</param>
		/// <returns>Completes when the indicated write completes.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static Task<IoResult> WriteAsync(
				this IoResult ioResult,
				Stream sinkStream,
				byte[] buffer,
				int offset,
				int length)
		{
			if (ioResult == null)
				throw new ArgumentNullException(nameof(ioResult));
			return IoHelper.WriteAsync(sinkStream, buffer, offset, length, ioResult);
		}

		/// <summary>
		/// NOTICE: this method ALWAYS updates the <see cref="IoResult.BytesReadOrWritten"/> property by the given
		/// length if the stream does not support seeking. This writes from a buffer to the sink <see cref="Stream"/>.
		/// The write may write any length. Note that this method catches all exceptions and will
		/// <see cref="IoResult.Fault"/> the result. IF the stream supports seeking, this method updates the
		/// <see cref="IoResult.BytesReadOrWritten"/> property based on the change in the stream's Position. Otherwise
		/// the value is always explicitly updated by the given length: you must test the stream to know. If there is
		/// an incoming value, it is incremented.
		/// </summary>
		/// <param name="sinkStream">Not null.</param>
		/// <param name="buffer">Not null.</param>
		/// <param name="offset">Offset within <c>buffer</c> to begin writing.</param>
		/// <param name="length">Length within <c>buffer</c> to write.</param>
		/// <param name="ioResult">Optional. If null, a new instance is created; with any given
		/// <see cref="CancellationToken"/>.</param>
		/// <param name="cancellationToken">Optional.</param>
		/// <returns>Not null. If you provided an argument, the same object is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static async Task<IoResult> WriteAsync(
				Stream sinkStream,
				byte[] buffer,
				int offset,
				int length,
				IoResult ioResult = null,
				CancellationToken cancellationToken = default)
		{
			if (sinkStream == null)
				throw new ArgumentNullException(nameof(sinkStream));
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if ((offset < 0)
					|| (offset >= buffer.Length))
				throw new ArgumentOutOfRangeException(nameof(offset));
			if ((length < 0)
					|| (length > (buffer.Length - offset)))
				throw new ArgumentOutOfRangeException(nameof(length));
			if (ioResult == null)
				ioResult = new IoResult(cancellationToken);
			else if (!ioResult.IsSuccess)
				return ioResult;
			try {
				long position = sinkStream.CanSeek
						? sinkStream.Position
						: 0L;
				await sinkStream.WriteAsync(
						buffer,
						offset,
						length,
						ioResult.CancellationToken);
				long bytesWritten = sinkStream.CanSeek
						? Math.Max(0L, sinkStream.Position - position)
						: length;
				ioResult.AddBytesReadOrWritten(bytesWritten);
			} catch (Exception exception) {
				ioResult.Fault(exception);
			}
			return ioResult;
		}
	}
}
