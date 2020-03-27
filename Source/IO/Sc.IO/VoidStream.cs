using System;
using System.IO;


namespace Sc.IO
{
	/// <summary>
	/// A <see cref="Stream"/> that maintains a <see cref="Position"/> and <see cref="Length"/>;
	/// and has no data. <see cref="CanRead"/>, <see cref="CanWrite"/>, and <see cref="CanSeek"/>
	/// are true, and the stream will grow to any length. As with <see cref="MemoryStream"/>, if
	/// <see cref="Seek"/> extends beyond the current length, the <see cref="Position"/> will change,
	/// but the <see cref="Length"/> will not. There is no data. This class also provides
	/// <see cref="BytesWritten"/> and <see cref="BytesRead"/> properties, and protected methods.
	/// Notice that the properties are mutable: you must monitor read and write flow to be sure that
	/// if the stream is seeked that the values will return what you expect: if the stream seeks,
	/// the values do note change. This implementation sets them from the value returned by the
	/// <see cref="Stream"/> in <see cref="Read"/>; and the change to <see cref="Position"/> in
	/// <see cref="Write"/>.
	/// </summary>
	public class VoidStream
			: Stream
	{
		private long length;
		private long current;


		/// <summary>
		/// Counts bytes written. Is mutable.
		/// </summary>
		public long BytesWritten { get; set; }

		/// <summary>
		/// Counts bytes read. Is mutable.
		/// </summary>
		public long BytesRead { get; set; }


		public override void Flush() { }

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin) {
				case SeekOrigin.Begin :
					current = Math.Max(0L, offset);
					break;
				case SeekOrigin.Current :
					current = Math.Max(0L, current + offset);
					break;
				case SeekOrigin.End :
					current = Math.Max(0L, (length - 1L) + offset);
					break;
			}
			return current;
		}

		public override void SetLength(long value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), value.ToString());
			length = value;
		}

		public override int Read(byte[] buffer, int offset, int count)
			=> readWrite(true, buffer, offset, count);

		public override void Write(byte[] buffer, int offset, int count)
			=> readWrite(false, buffer, offset, count);

		private int readWrite(bool isRead, byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if ((offset < 0)
					|| (offset >= buffer.Length))
				throw new ArgumentException(nameof(offset));
			if ((count < 0)
					|| (count > (buffer.Length - offset)))
				throw new ArgumentException(nameof(count));
			if (current < length)
				count = (int)Math.Min(count, length - current);
			else
				count = 0;
			current += count;
			if (isRead)
				BytesRead += count;
			else
				BytesWritten += count;
			return count;
		}

		public override bool CanRead
			=> true;

		public override bool CanSeek
			=> true;

		public override bool CanWrite
			=> true;

		public override long Length
			=> length;

		public override long Position
		{
			get => current;
			set => Seek(value, SeekOrigin.Begin);
		}
	}
}
