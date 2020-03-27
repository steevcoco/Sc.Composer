using System;


namespace Sc.Util.System
{
	/// <summary>
	/// Static helpers for <see cref="BitConverter"/>.
	/// </summary>
	public static class BitConverterHelper
	{
		/// <summary>
		/// The argument is ASSUMED to be bytes from a primitive structure.
		/// This will reverse the bytes if this platform is little endian.
		/// </summary>
		/// <param name="bytes">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static byte[] ToNetworkBytes(this byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			return bytes;
		}

		/// <summary>
		/// The argument is ASSUMED to be bytes from a primitive structure,
		/// in Big-Endian order --- as if from <see cref="ToNetworkBytes"/>.
		/// This will reverse the bytes if this platform is little endian.
		/// </summary>
		/// <param name="bytes">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static byte[] FromNetworkBytes(this byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			return bytes;
		}
	}
}
