using System;


namespace Sc.Util.System
{
	/// <summary>
	/// Static helpers for Enums.
	/// </summary>
	public static class EnumHelper
	{
		/// <summary>
		/// Helper method returns a typed array of all <see cref="Enum"/>
		/// values for the enum type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">Must be an Enum type.</typeparam>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentException"><typeparamref name="T"/>
		/// is not an <see cref="Enum"/>.</exception>
		/// <exception cref="InvalidOperationException">The method is invoked by
		/// reflection in a reflection-only context, -or- <typeparamref name="T"/>
		/// is a type from an assembly loaded in a reflection-only context.</exception>
		public static T[] GetValues<T>()
				where T : struct, Enum
		{
			Array array = Enum.GetValues(typeof(T));
			if (array is T[] a)
				return a;
			a = new T[array.Length];
			Buffer.BlockCopy(array, 0, a, 0, Buffer.ByteLength(array));
			return a;
		}


		/// <summary>
		/// Convenience method returns <see cref="EnumValueRange{T}.Get"/>
		/// for <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The Enum type.</typeparam>
		/// <returns>Not null.</returns>
		public static EnumValueRange<T>.Set GetValueRanges<T>()
				where T : struct, Enum
			=> EnumValueRange<T>.Get();

		/// <summary>
		/// Please notice: this is a convenience method that tries to fetch
		/// the next-higher defined Enum value from this given
		/// <paramref name="value"/>. This method first gets the
		/// <see cref="EnumValueRange{T}.Set"/> for <typeparamref name="T"/>
		/// --- which is a sorted set of ranges for
		/// all defined values --- and then searches for the next-higher
		/// value. This guarantees that all duplicate values are
		/// sorted; and the returned range includes any duplicate values.
		/// Since the method contructs and sorts the list, it is not
		/// guaranteed to be performance-critical
		/// </summary>
		/// <typeparam name="T">The Enum type.</typeparam>
		/// <param name="value">This value to locate.</param>
		/// <param name="nextHigherValue">The range containing the next-higher enum value.</param>
		/// <returns>True if found.</returns>
		public static bool TryGetNextHigherValue<T>(this T value, out EnumValueRange<T> nextHigherValue)
				where T : struct, Enum
			=> EnumValueRange<T>.Get()
					.TryGetNextHigherValue(value, out nextHigherValue);

		/// <summary>
		/// Please notice: this is a convenience method that tries to fetch
		/// the next-lower defined Enum value from this given
		/// <paramref name="value"/>. This method first gets the
		/// <see cref="EnumValueRange{T}.Set"/> for <typeparamref name="T"/>
		/// --- which is a sorted set of ranges for
		/// all defined values --- and then searches for the next-lower
		/// value. This guarantees that all duplicate values are
		/// sorted; and the returned range includes any duplicate values.
		/// Since the method contructs and sorts the list, it is not
		/// guaranteed to be performance-critical
		/// </summary>
		/// <typeparam name="T">The Enum type.</typeparam>
		/// <param name="value">This value to locate.</param>
		/// <param name="nextHigherValue">The range containing the next-lower enum value.</param>
		/// <returns>True if found.</returns>
		public static bool TryGetNextLowerValue<T>(this T value, out EnumValueRange<T> nextLowerValue)
				where T : struct, Enum
			=> EnumValueRange<T>.Get()
					.TryGetNextLowerValue(value, out nextLowerValue);


		/// <summary>
		/// Returns the minimum and maximum underlying primitive values
		/// of this <typeparamref name="T"/> <see cref="Enum"/> Type.
		/// </summary>
		/// <typeparam name="T">The Enum type.</typeparam>
		/// <param name="min">The minimum underlying primitive value.</param>
		/// <param name="max">The maximum underlying primitive value.</param>
		public static void MinMaxValues<T>(out long min, out ulong max)
				where T : struct, Enum
		{
			switch (default(T).GetTypeCode()) {
				case TypeCode.SByte:
					min = sbyte.MinValue;
					max = (ulong)sbyte.MaxValue;
					break;
				case TypeCode.Byte:
					min = byte.MinValue;
					max = byte.MaxValue;
					break;
				case TypeCode.Int16:
					min = short.MinValue;
					max = (ulong)short.MaxValue;
					break;
				case TypeCode.UInt16:
					min = ushort.MinValue;
					max = ushort.MaxValue;
					break;
				case TypeCode.Int32:
					min = int.MinValue;
					max = int.MaxValue;
					break;
				case TypeCode.UInt32:
					min = uint.MinValue;
					max = uint.MaxValue;
					break;
				case TypeCode.Int64:
					min = long.MinValue;
					max = long.MaxValue;
					break;
				case TypeCode.UInt64:
					min = (long)ulong.MinValue;
					max = ulong.MaxValue;
					break;
				default:
					throw new NotSupportedException(
					 $"Unexpected constrained '{nameof(Enum)}' Type: {typeof(T)}.");
			}
		}
	}
}
