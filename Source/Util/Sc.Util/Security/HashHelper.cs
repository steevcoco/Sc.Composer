using System;
using Sc.Util.System;


namespace Sc.Util.Security
{
	/// <summary>
	/// Static helpers for hashing.
	/// </summary>
	public static class HashHelper
	{
		/// <summary>
		/// Produces the most rudimentary magic number hash.
		/// The returned byte value is composed in two parts: the lower
		/// seven bits, and the upper bit. First, the lower seven bits
		/// is set to the value of the count of the One-bits in
		/// this <paramref name="guid"/>, minus one (therefore, the
		/// lower seven bits in the returned byte is the count the one-bits,
		/// from zero to 127; and if there are either zero or one One-bits
		/// then that value is zero). If those lower seven bits are masked
		/// out, it yields the value, [0,127] that is the count of the
		/// One-bits, minus one. The last upper bit is set to one if
		/// the counr of the One-bits in the first eight bytes in this
		/// <paramref name="guid"/> is larger than the count of One-bits
		/// in the lower eight bytes --- and is zero if the counts are equal.
		/// </summary>
		/// <param name="i">Sixteen byte value to hash into one byte.</param>
		/// <returns>One byte composed from sixteen.</returns>
		public static byte SimpleMaskedByte(this Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			int bigCount = 0;
			int littleCount = 0;
			for (int i = 0; i < bytes.Length; ++i) {
				if (i < 8)
					littleCount += ByteFlags.CountBits(bytes[i]);
				else
					bigCount += ByteFlags.CountBits(bytes[i]);
			}
			int value = (bigCount + littleCount) - 1;
			if (bigCount > littleCount)
				value |= 128;
			return (byte)value;
		}

		/// <summary>
		/// Produces the most rudimentary magic number hash.
		/// The returned ushort value is composed in two parts: the lower
		/// eight bits, and the upper eight. First, the lower eight bits
		/// is simply set to the value of the count of the One-bits in
		/// this <paramref name="guid"/>. If those lower eight bits are
		/// masked out, it yields the value, [0,128] that is the count of the
		/// One-bits. The upper eight bits are set individually, by counting
		/// the One-bits in each two-byte pair of bytes from the
		/// <paramref name="guid"/>. The upper msb byte is set to one
		/// if the count of One-bits in the first two bytes from the Guid
		/// is larger than the count of zero-bits --- and is zero if the
		/// counts are equal. The following seven bits are then set from
		/// the foloowing counts of the two-byte pairs: the ushort byte
		/// is set to one if the count of One-bits in the two-byte pair
		/// from the Guid is larger than the count of zero-bits.
		/// </summary>
		/// <param name="i">Sixteen byte value to hash into two bytes.</param>
		/// <returns>Two bytes composed from sixteen.</returns>
		public static ushort SimpleMaskedShort(this Guid guid)
		{
			byte[] bytes = guid.ToByteArray();
			int count = 0;
			foreach (byte b in bytes) {
				count += ByteFlags.CountBits(b);
			}
			ByteFlags flags = new ByteFlags();
			for (int i = 0, j = 8; i < bytes.Length - 1; i+=2, --j) {
				if ((ByteFlags.CountBits(bytes[i])
								+ ByteFlags.CountBits(bytes[i + 1]))
						> 8) {
					flags.Set(j);
				}
			}
			return (ushort)(count | (flags.Flags << 8));
		}
	}
}
