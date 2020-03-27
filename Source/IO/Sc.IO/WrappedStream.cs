using System;
using System.IO;


namespace Sc.IO
{
	/// <summary>
	/// Wraps a <see cref="Stream"/> and provides <see cref="BytesWritten"/> and <see cref="BytesRead"/>
	/// properties, and protected methods. Notice that the properties are mutable: you must monitor read
	/// and write flow to be sure that if the stream is seeked that the values will return what you expect:
	/// if the stream seeks, the values do not change. This implementation sets them from the value
	/// returned by the <see cref="Stream"/> in <see cref="Read"/>; and the change to <see cref="Position"/>
	/// in <see cref="Write"/>.
	/// </summary>
	/// <typeparam name="TStream">Your wrapped Stream's type.</typeparam>
	public class WrappedStream<TStream>
			: Stream
			where TStream : Stream
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stream">Not null.</param>
		public WrappedStream(TStream stream)
			=> Stream = stream ?? throw new ArgumentNullException(nameof(stream));


		/// <summary>
		/// The wrapped Stream.
		/// </summary>
		public TStream Stream { get; }

		/// <summary>
		/// Counts bytes written. Is mutable.
		/// </summary>
		public long BytesWritten { get; set; }

		/// <summary>
		/// Counts bytes read. Is mutable.
		/// </summary>
		public long BytesRead { get; set; }


		public override bool CanRead
			=> Stream.CanRead;

		public override bool CanWrite
			=> Stream.CanWrite;

		public override bool CanSeek
			=> Stream.CanSeek;

		public override long Length
			=> Stream.Length;

		public override long Position
		{
			get => Stream.Position;
			set => Stream.Position = value;
		}

		public override void Flush()
			=> Stream.Flush();

		public override long Seek(long offset, SeekOrigin origin)
			=> Stream.Seek(offset, origin);

		public override void SetLength(long value)
			=> Stream.SetLength(value);

		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesRead = Stream.Read(buffer, offset, count);
			BytesRead += bytesRead;
			OnRead(buffer, offset, count, bytesRead);
			return bytesRead;
		}

		/// <summary>
		/// This method will be invoked from <see cref="Read(byte[],int,int)"/> with each result from the
		/// <see cref="Stream"/>. The <c>buffer</c>, <c>offset</c>, and <c>count</c> arguments are as passed
		/// to <c>Read</c>. The <c>bytesRead</c> argument is the value actually returned by the <see cref="Stream"/>.
		/// Notice that <see cref="BytesRead"/> will be updated before this is invoked.
		/// </summary>
		/// <param name="buffer">The <see cref="Read"/> argument.</param>
		/// <param name="offset">The <see cref="Read"/> argument.</param>
		/// <param name="count">The <see cref="Read"/> argument.</param>
		/// <param name="bytesRead">The actual value returned by the <see cref="Stream"/> after the read.</param>
		/// <returns>The return value is not used here: you may return any value that is useful to your own
		/// implementation. This returns <c>bytesRead</c>.</returns>
		protected virtual int OnRead(byte[] buffer, int offset, int count, int bytesRead)
			=> bytesRead;

		public override void Write(byte[] buffer, int offset, int count)
		{
			long position = Position;
			Stream.Write(buffer, offset, count);
			long bytesWritten = Math.Max(0L, Position - position);
			BytesWritten += bytesWritten;
			OnWrite(buffer, offset, count, (int)bytesWritten);
		}

		/// <summary>
		/// This method will be invoked from <see cref="Write(byte[],int,int)"/> with each result from the
		/// <see cref="Stream"/>. The <c>buffer</c>, <c>offset</c>, and <c>count</c> arguments are as passed
		/// to <c>Write</c>. The <c>bytesWritten</c> argument is the value actually computed by the change in
		/// the <see cref="Stream"/> <see cref="Position"/>.
		/// </summary>
		/// <param name="buffer">The <see cref="Write"/> argument.</param>
		/// <param name="offset">The <see cref="Write"/> argument.</param>
		/// <param name="count">The <see cref="Write"/> argument.</param>
		/// <param name="bytesWritten">The actual value computed by the change in the <see cref="Stream"/>
		/// <see cref="Position"/> after the write.</param>
		/// <returns>The return value is not used here: you may return any value that is useful to your own
		/// implementation. This returns <c>bytesWritten</c>.</returns>
		protected virtual int OnWrite(byte[] buffer, int offset, int count, int bytesWritten)
			=> bytesWritten;


		public override void Close()
			=> Stream.Close();
	}
}
