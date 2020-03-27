using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace Sc.Util.System
{
	/// <summary>
	/// A <see langword="struct"/> holding <see langword="byte"/> bit <see cref="Flags"/>.
	/// Flags are numbered from one to eight, from right-to-left in the
	/// <see langword="byte"/> value --- but the abstraction allows you to
	/// ignore that as an implementation detail. This struct implements
	/// methods to set and clear flags, and set and clear flags from other
	/// instances. This also implements implicit conversions to and from
	/// <see langword="byte"/> --- and therefore you may invoke methods with
	/// <see langword="byte"/> values. This is also
	/// <see cref="IXmlSerializable"/> and <see cref="ICloneable"/>.
	/// </summary>
	[Serializable]
	public sealed class ByteFlags
			: ICloneable,
					IXmlSerializable
	{
		/// <summary>
		/// Counts the bits that are set on the argument.
		/// </summary>
		/// <param name="flags">Arbitrary.</param>
		/// <returns>A count of bits that are set.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CountBits(byte flags)
		{
			int result = 0;
			for (int i = 0; i < 8; ++i) {
				if ((flags & 1) == 1)
					++result;
				flags >>= 1;
			}
			return result;
		}


		/// <summary>
		/// Returns the <see langword="byte"/> VALUE of the SINGLE
		/// highest bit that is set on the argument.
		/// </summary>
		/// <param name="flags">Arbitrary.</param>
		/// <returns>The value of the single highest bit that is set.
		/// Returns zero if no bits are set.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte GetHighestBitValue(byte flags)
		{
			int highestBit = ByteFlags.GetHighestBitPosition(flags);
			return highestBit == 0
					? (byte)0
					: (byte)(1 << (highestBit - 1));
		}

		/// <summary>
		/// Returns the <see langword="byte"/> VALUE of the SINGLE
		/// lowest bit that is set on the argument.
		/// </summary>
		/// <param name="flags">Arbitrary.</param>
		/// <returns>The value of the single lowest bit that is set.
		/// Returns zero if no bits are set.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte GetLowestBitValue(byte flags)
		{
			int lowestBit = ByteFlags.GetLowestBitPosition(flags);
			return lowestBit == 0
					? (byte)0
					: (byte)(1 << (lowestBit - 1));
		}


		/// <summary>
		/// Returns the position of highest bit that is set on the argument:
		/// where the right-most bit is position one; and the left-most is eight.
		/// </summary>
		/// <param name="flags">Arbitrary.</param>
		/// <returns>The position of the highest bit that is set.
		/// Returns zero if no bits are set.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetHighestBitPosition(byte flags)
		{
			if (flags == 0)
				return 0;
			for (int i = 7; i >= 0; --i) {
				if (((flags >> i) & 1) == 1)
					return i + 1;
			}
			Debug.Fail($"Value is '{flags}' but iteration failed to find a set bit.");
			return 0;
		}

		/// <summary>
		/// Returns the position of lowest bit that is set on the argument:
		/// where the right-most bit is position one; and the left-most is eight.
		/// </summary>
		/// <param name="flags">Arbitrary.</param>
		/// <returns>The position of the lowest bit that is set.
		/// Returns zero if no bits are set.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetLowestBitPosition(byte flags)
		{
			if (flags == 0)
				return 0;
			for (int i = 0; i < 8; ++i) {
				if (((flags >> i) & 1) == 1)
					return i + 1;
			}
			Debug.Fail($"Value is '{flags}' but iteration failed to find a set bit.");
			return 0;
		}

		/// <summary>
		/// Returns a new value from the <paramref name="flags"/>
		/// with the <paramref name="predicate"/> removed.
		/// </summary>
		/// <param name="flags">Arbitrary.</param>
		/// <param name="predicate">Arbitrary.</param>
		/// <returns>Returns <paramref name="flags"/> <c>&</c> the
		/// complement of the <paramref name="predicate"/>.</returns>
		public static byte Excluding(byte flags, byte predicate)
			=> (byte)(flags & ~predicate);


		/// <summary>
		/// Returns true if the <paramref name="source"/> has ANY of the bits that
		/// are set on the <paramref name="flags"/>. Notice that if the
		/// <paramref name="flags"/> are zero, this will return false.
		/// </summary>
		/// <param name="source">The source flags.
		/// If zero, this will return false.</param>
		/// <param name="flags">Arbitrary bits to search for.
		/// If zero, this will return false.</param>
		/// <returns>True if ANY <The name="flags"/> are present on the
		/// <paramref name="source"/> (and at least one flags bit is set).</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasAnyBits(byte source, byte flags)
			=> (source & flags) != 0;

		/// <summary>
		/// Returns true if the <paramref name="source"/> has ALL of the bits that
		/// are set on the <paramref name="flags"/>. Notice that if the
		/// <paramref name="flags"/> are zero, this will return false.
		/// </summary>
		/// <param name="source">The source flags.
		/// If zero, this will return false.</param>
		/// <param name="flags">Arbitrary bits to search for.
		/// If zero, this will return false.</param>
		/// <returns>True if ALL <The name="flags"/> are present on the
		/// <paramref name="source"/> (and at least one flags bit is set).</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasAllBits(byte source, byte flags)
			=> (flags != 0) && ((source & flags) == flags);

		/// <summary>
		/// Returns true if the <paramref name="source"/> has ONLY bits that are set
		/// on the <paramref name="flags"/> --- false if any bit is set on the source
		/// that is not defined on the flags. Notice that if the
		/// <paramref name="flags"/> are zero, this will return false.
		/// </summary>
		/// <param name="source">The source flags.
		/// If zero, this will return false.</param>
		/// <param name="flags">Arbitrary bits to search for.
		/// If zero, this will return false.</param>
		/// <param name="requiresAll">If true, then <paramref name="source"/>
		/// MUST contain ALL <paramref name="flags"/> AND NO other bits.
		/// If false, the source may contain zero or more bits
		/// present on the flags --- and no bits that are not present on the flags
		/// (source need not contain all, but can only contain a bit on the flags).</param>
		/// <returns>True if only the flags are present on the source --- false if any bit is
		/// set on the source that is not defined on the flags.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasOnlyBits(byte source, byte flags, bool requiresAll)
			=> (flags != 0)
					&& (source != 0)
					&& ((source & ~flags) == 0)
					&& (!requiresAll
					|| ((source & flags) == flags));

		/// <summary>
		/// Returns true if the <paramref name="source"/> has NONE of the
		/// bits that are set on <paramref name="flags"/>. Notice that if the
		/// <paramref name="flags"/> are zero, this will return TRUE.
		/// </summary>
		/// <param name="source">The source flags.
		/// If zero, this will return true.</param>
		/// <param name="flags">Arbitrary flags to search for.
		/// If zero, this will return true.</param>
		/// <returns>True if no flags bits are set here.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasNoBits(byte source, byte flags)
			=> (source & flags) == 0;


		/// <summary>
		/// Checks the range.
		/// </summary>
		/// <param name="position">[1, 8].</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void rangeCheckPosition(int position)
		{
			if ((position <= 0)
					|| (position > 8)) {
				throw new ArgumentOutOfRangeException(nameof(position), position, @"[1, 8]");
			}
		}


		/// <summary>
		/// Default constructor creates an empty instance.
		/// </summary>
		public ByteFlags() { }

		/// <summary>
		/// Creates a new instance with each flag position in the argument array set.
		/// </summary>
		/// <param name="flags">The flags to set.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public ByteFlags(params int[] flags)
			=> Set(flags);

		/// <summary>
		/// Creates a new instance with the given bits set.
		/// </summary>
		/// <param name="flags">The bits to copy. This directly
		/// sets <see cref="Flags"/>.</param>
		public ByteFlags(byte flags)
			=> Flags = flags;

		/// <summary>
		/// Creates a deep clone of the argument.
		/// </summary>
		/// <param name="clone">The value to copy.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ByteFlags(ByteFlags clone)
			=> Flags = clone.Flags;


		XmlSchema IXmlSerializable.GetSchema()
			=> null;

		void IXmlSerializable.WriteXml(XmlWriter writer)
			=> writer.WriteString(Flags.ToString(CultureInfo.InvariantCulture));

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			if (reader.IsEmptyElement) {
				Flags = 0;
			} else {
				reader.Read();
				switch (reader.NodeType) {
					case XmlNodeType.EndElement :
						Flags = 0; // empty after all...
						break;
					case XmlNodeType.Text :
					case XmlNodeType.CDATA :
						Flags = byte.Parse(reader.ReadContentAsString(), CultureInfo.InvariantCulture);
						break;
					default :
						throw new InvalidOperationException("Expected text/cdata");
				}
			}
		}


		/// <summary>
		/// The current bit flags. Flags are numbered from one to eight: where
		/// the right-most bit is one, and the left-most is eight.
		/// Methods do not require knowledge of the flag positions; and flags
		/// are simply numbered [1, 8].
		/// </summary>
		public byte Flags
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}


		/// <summary>
		/// An indexer that gets or sets a boolean indicating if the flag at the
		/// given <paramref name="position"/> is set.
		/// </summary>
		/// <param name="position">[1, 8].</param>
		/// <returns>True if the flag is set.</returns>
		public bool this[int position]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => IsSet(position);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				if (value)
					Set(position);
				else
					Clear(position);
			}
		}

		/// <summary>
		/// Returns true if the flag at the given position is set.
		/// </summary>
		/// <param name="position">The position to test: [1, 8].</param>
		/// <returns>True if the flag is set.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsSet(int position)
		{
			ByteFlags.rangeCheckPosition(position);
			return ByteFlags.HasAnyBits(Flags, (byte)(1 << (position - 1)));
		}

		/// <summary>
		/// Returns true if each flag in the argument array is set.
		/// This will return FALSE if none are provided.
		/// </summary>
		/// <param name="positions">[1, 8].</param>
		/// <returns>True if all provided flags are set. NOTICE: this will
		/// return FALSE if none are provided.</returns>
		public bool IsAllSet(params int[] positions)
		{
			if (positions.Length == 0)
				return false;
			foreach (int position in positions) {
				if (!IsSet(position))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns true if ANY flag in the argument array is set.
		/// This will return FALSE if none are provided.
		/// </summary>
		/// <param name="positions">[1, 8].</param>
		/// <returns>True if ANY provided flag is set; AND if AT LEAST ONE
		/// is provided.</returns>
		public bool IsAnySet(params int[] positions)
		{
			foreach (int position in positions) {
				if (IsSet(position))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns true if all flags are set.
		/// </summary>
		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Flags == byte.MaxValue;
		}

		/// <summary>
		/// Returns true if no flags are set.
		/// </summary>
		public bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Flags == 0;
		}


		/// <summary>
		/// Counts the flags that are set.
		/// </summary>
		/// <returns>A count of <see cref="Flags"/> bits that are set.</returns>
		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ByteFlags.CountBits(Flags);
		}

		/// <summary>
		/// Returns the position of highest flag that is set.
		/// </summary>
		/// <returns>The position of the highest bit that is set on <see cref="Flags"/>.</returns>
		public int HighestFlag
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ByteFlags.GetHighestBitPosition(Flags);
		}

		/// <summary>
		/// Returns the position of lowest flag that is set.
		/// </summary>
		/// <returns>The position of the lowest bit that is set on <see cref="Flags"/>.</returns>
		public int LowestFlag
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ByteFlags.GetLowestBitPosition(Flags);
		}

		/// <summary>
		/// Returns the <see langword="byte"/> VALUE of the SINGLE
		/// highest bit that is set.
		/// </summary>
		/// <returns>The value of the single highest bit that is set on <see cref="Flags"/>.</returns>
		public byte HighestFlagValue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ByteFlags.GetHighestBitValue(Flags);
		}

		/// <summary>
		/// Returns the <see langword="byte"/> VALUE of the SINGLE
		/// lowest bit that is set.
		/// </summary>
		/// <returns>The value of the single lowest bit that is set on <see cref="Flags"/>.</returns>
		public byte LowestFlagValue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ByteFlags.GetLowestBitValue(Flags);
		}


		/// <summary>
		/// Sets the flag at the position specified by the argument.
		/// </summary>
		/// <param name="position">[1, 8].</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags Set(int position)
		{
			ByteFlags.rangeCheckPosition(position);
			Flags |= (byte)(1 << (position - 1));
			return this;
		}

		/// <summary>
		/// Sets the flag at each specified position.
		/// </summary>
		/// <param name="positions">[1, 8].</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags Set(params int[] positions)
		{
			byte flags = Flags;
			try {
				foreach (int position in positions) {
					Set(position);
				}
				return this;
			} catch {
				Flags = flags;
				throw;
			}
		}

		/// <summary>
		/// Sets all flags to one.
		/// </summary>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags SetAll()
		{
			Flags = byte.MaxValue;
			return this;
		}


		/// <summary>
		/// Clears the flag at the position specified by the argument.
		/// </summary>
		/// <param name="position">[1, 8].</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags Clear(int position)
		{
			ByteFlags.rangeCheckPosition(position);
			Flags &= (byte)~(1 << (position - 1));
			return this;
		}

		/// <summary>
		/// Clears the flag at each position specified in the argument array.
		/// </summary>
		/// <param name="positions">[1, 8].</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags Clear(params int[] positions)
		{
			byte flags = Flags;
			try {
				foreach (int position in positions) {
					Clear(position);
				}
				return this;
			} catch {
				Flags = flags;
				throw;
			}
		}

		/// <summary>
		/// Resets all <see cref="Flags"/> to zero.
		/// </summary>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags ClearAll()
		{
			Flags = 0;
			return this;
		}


		/// <summary>
		/// Inverts the flag at the position specified by the argument.
		/// </summary>
		/// <param name="position">[1, 8].</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags Invert(int position)
		{
			this[position] = !this[position];
			return this;
		}

		/// <summary>
		/// Inverts the flag at each specified position.
		/// </summary>
		/// <param name="positions">[1, 8].</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags Invert(params int[] positions)
		{
			byte flags = Flags;
			try {
				foreach (int position in positions) {
					Invert(position);
				}
				return this;
			} catch {
				Flags = flags;
				throw;
			}
		}

		/// <summary>
		/// Inverts all flags at all positions.
		/// </summary>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags InvertAll()
		{
			Flags = (byte)~Flags;
			return this;
		}


		/// <summary>
		/// Sets <see cref="Flags"/> to the argument's value.
		/// </summary>
		/// <param name="clone">The value to copy.</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags SetFrom(ByteFlags clone)
		{
			Flags = clone.Flags;
			return this;
		}

		/// <summary>
		/// Sets <see cref="Flags"/> to the argument's value.
		/// </summary>
		/// <param name="clone">The value to copy.</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags SetFrom(byte clone)
		{
			Flags = clone;
			return this;
		}

		/// <summary>
		/// Adds all of the flags in the argument to this instance.
		/// </summary>
		/// <param name="flags">Arbitrary flags to add.</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags AddAllFlags(ByteFlags flags)
		{
			Flags |= flags.Flags;
			return this;
		}

		/// <summary>
		/// Removes all flags defined on the argument from this instance.
		/// </summary>
		/// <param name="flags">Arbitrary flags to remove.</param>
		/// <returns>This object for chaining.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags RemoveAllFlags(ByteFlags flags)
		{
			Flags &= (byte)~flags.Flags;
			return this;
		}


		/// <summary>
		/// Returns true if this instance has ANY of the <see cref="Flags"/>
		/// defined on the argument. Notice that if the
		/// <paramref name="flags"/> are zero, this will return false.
		/// </summary>
		/// <param name="flags">Arbitrary flags to search for.</param>
		/// <returns>True if any flags are present here
		/// (and at least one flag bit is set).</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasAny(ByteFlags flags)
			=> ByteFlags.HasAnyBits(Flags, flags.Flags);

		/// <summary>
		/// Returns true if this instance has ALL of the <see cref="Flags"/>
		/// defined on the argument. Notice that if the
		/// <paramref name="flags"/> are zero, this will return false.
		/// </summary>
		/// <param name="flags">Arbitrary flags to search for.</param>
		/// <returns>True if ALL flags are present here
		/// (and at least one flag bit is set).</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasAll(ByteFlags flags)
			=> ByteFlags.HasAllBits(Flags, flags.Flags);

		/// <summary>
		/// Returns true if this instance has ONLY flags that are set
		/// on the <paramref name="flags"/> --- false if any flag is set here
		/// that is not defined on the flags. Notice that if the
		/// <paramref name="flags"/> are zero, this will return false
		/// --- and if this <see cref="Flags"/> is zero this returns false.
		/// </summary>
		/// <param name="flags">Arbitrary flags to search for.
		/// If zero, this will return false.</param>
		/// <param name="requiresAll">If true, then this
		/// MUST contain ALL <paramref name="flags"/> AND NO other flags.
		/// If false, this may contain zero or more flags
		/// present on the flags --- and no flags that are not present on the flags
		/// (this need not contain all, but can only contain a flag on the flags).</param>
		/// <returns>True if only the flags are present here --- false if any flag is
		/// set here that is not defined on the flags.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasOnly(ByteFlags flags, bool requiresAll)
			=> ByteFlags.HasOnlyBits(Flags, flags.Flags, requiresAll);

		/// <summary>
		/// Returns true if this instance has NONE of the <see cref="Flags"/>
		/// defined on the argument.
		/// </summary>
		/// <param name="flags">Arbitrary.</param>
		/// <returns>True if no flags are present here.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasNone(ByteFlags flags)
			=> ByteFlags.HasNoBits(Flags, flags.Flags);


		/// <summary>
		/// Returns a deep clone of this object.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ByteFlags Clone()
			=> new ByteFlags(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		object ICloneable.Clone()
			=> Clone();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator byte(ByteFlags byteFlags)
			=> byteFlags.Flags;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ByteFlags(byte flags)
			=> new ByteFlags(flags);


		public override string ToString()
			=> $"{nameof(ByteFlags)}[{Convert.ToString(Flags, 2).PadLeft(8, '0')}]";
	}
}
