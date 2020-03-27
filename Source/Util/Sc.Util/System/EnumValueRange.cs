using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Sc.Util.Collections;


namespace Sc.Util.System
{
	/// <summary>
	/// Implements a value class that is used to constrain an underlying
	/// <see cref="Enum"/> primitive value type value to actual values
	/// defined on the Enum. Each instance holds the single actual Enum
	/// <see cref="Value"/>, and holds a <see cref="Min"/> and <see cref="Max"/>
	/// primitive value that represent the minimum and maximumm primitive
	/// values that should be "rounded" to this value. I.E. given some
	/// <see langword="double"/> value, which is either cast form an actual Enum
	/// member value, OR, some arbitrary value that MAY NOT correspond
	/// to an actual Enum member value, then the instance is used to
	/// select the nearest actual Enum member value to that primitive
	/// value. The static <see cref="Get"/> method will return a full
	/// sorted list of ranges for all values on a given Enum.
	/// </summary>
	/// <typeparam name="T">The enum type.</typeparam>
	[DataContract]
	public sealed class EnumValueRange<T>
			: IComparable<EnumValueRange<T>>,
					IComparable<double>
			where T : struct, Enum
	{
		/// <summary>
		/// Implements a sorted list of range instances
		/// for all values in the <typeparamref name="T"/> enum type.
		/// The first member in the list has a <see cref="Min"/> value
		/// of the CEILING of <see cref="double"/> <see cref="double.MinValue"/>;
		/// and the last has a <see cref="Max"/> value of the FLOOR of
		/// <see cref="double.MaxValue"/>.
		/// Since the list is sorted, it will search for values in
		/// ranges with a binary search. The list is
		/// sorted lowest to highest; and covers all indicated
		/// <see cref="double"/> values; and all
		/// <typeparamref name="T"/> members.
		/// </summary>
		public sealed class Set
				: IReadOnlyList<EnumValueRange<T>>
		{
			private readonly EnumValueRange<T>[] ranges;


			/// <summary>
			/// Constructor; populates all values for
			/// <typeparamref name="T"/> now.
			/// </summary>
			public Set()
			{
				T[] values = Enum.GetValues(typeof(T))
						.Cast<T>()
						.ToArray();
				List<EnumValueRange<T>> list = new List<EnumValueRange<T>>(values.Length);
				foreach (T member in values) {
					double doubleValue = Convert.ToDouble(member);
					int equalIndex = list.BinarySearchIndexOf(doubleValue, DoubleSelector, null);
					if (equalIndex < 0)
						list.Insert(~equalIndex, new EnumValueRange<T>(member, doubleValue));
					else
						list[equalIndex].equalValues = list[equalIndex].equalValues.Append(member);
				}
				double? priorMax = null;
				foreach ((EnumValueRange<T> current, EnumValueRange<T> next, bool hasNext) in list.LookAhead()) {
					current.Min = priorMax.HasValue
							? priorMax.Value + 1D
							: EnumValueRange<T>.floor;
					current.Max = !hasNext
							? EnumValueRange<T>.ceiling
							: current.DoubleValue + Math.Floor((next.DoubleValue - current.DoubleValue) / 2D);
					priorMax = current.Max;
				}
				ranges = list.ToArray();
				static double DoubleSelector(EnumValueRange<T> item)
					=> item.DoubleValue;
			}


			/// <summary>
			/// Returns the (non-null) range that contains this
			/// <paramref name="value"/>.
			/// </summary>
			/// <param name="value">The value to locate.</param>
			/// <returns>Always returns a valid range.</returns>
			public EnumValueRange<T> Get(T value)
			{
				int index = ranges.BinarySearchIndexOf(Predicate);
				Debug.Assert(index >= 0, "index >= 0");
				return ranges[index];
				int Predicate(EnumValueRange<T> member)
					=> member.CompareTo(value);
			}

			/// <summary>
			/// Returns the (non-null) range that contains this
			/// <paramref name="value"/>.
			/// </summary>
			/// <param name="value">The value to locate.</param>
			/// <returns>Always returns a valid range.</returns>
			public EnumValueRange<T> Get(double value)
			{
				int index = ranges.BinarySearchIndexOf(Predicate);
				Debug.Assert(index >= 0, "index >= 0");
				return ranges[index];
				int Predicate(EnumValueRange<T> member)
					=> member.CompareTo(value);
			}

			/// <summary>
			/// Tries to get the range that contains the next-higher value
			/// from this <paramref name="value"/>.
			/// </summary>
			/// <param name="value">This value to locate.</param>
			/// <param name="nextHigherValue">The range containing the next-higher enum value.</param>
			/// <returns>True if found.</returns>
			public bool TryGetNextHigherValue(T value, out EnumValueRange<T> nextHigherValue)
			{
				int index = ranges.BinarySearchIndexOf(Predicate);
				Debug.Assert(index >= 0, "index >= 0");
				if (index < (ranges.Length - 1)) {
					nextHigherValue = ranges[index + 1];
					return true;
				}
				nextHigherValue = null;
				return false;
				int Predicate(EnumValueRange<T> member)
					=> member.CompareTo(value);

			}

			/// <summary>
			/// Tries to get the range that contains the next-lower value
			/// from this <paramref name="value"/>.
			/// </summary>
			/// <param name="value">This value to locate.</param>
			/// <param name="nextLowerValue">The range containing the next-lower enum value.</param>
			/// <returns>True if found.</returns>
			public bool TryGetNextLowerValue(T value, out EnumValueRange<T> nextLowerValue)
			{
				int index = ranges.BinarySearchIndexOf(Predicate);
				Debug.Assert(index >= 0, "index >= 0");
				if (index > 0) {
					nextLowerValue = ranges[index - 1];
					return true;
				}
				nextLowerValue = null;
				return false;
				int Predicate(EnumValueRange<T> member)
					=> member.CompareTo(value);

			}

			/// <summary>
			/// Returns the minimum underlying primitive value
			/// of this <typeparamref name="T"/> <see cref="Enum"/> Type.
			/// </summary>
			public long MinUnderlyingValue
			{
				get {
					EnumHelper.MinMaxValues<T>(out long min, out _);
					return min;
				}
			}

			/// <summary>
			/// Returns the maximum underlying primitive value
			/// of this <typeparamref name="T"/> <see cref="Enum"/> Type.
			/// </summary>
			public ulong MaxUnderlyingValue
			{
				get {
					EnumHelper.MinMaxValues<T>(out _, out ulong max);
					return max;
				}
			}


			public EnumValueRange<T> this[int index]
				=> ranges[index];

			public int Count
				=> ranges.Length;

			public IEnumerator<EnumValueRange<T>> GetEnumerator()
				=> ranges.ArrayEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();


			public override string ToString()
				=> $"{GetType().GetFriendlyName()}"
						+ $"{ranges.ToStringCollection(0)}";
		}


		private static readonly double floor
				= Math.Ceiling(double.MinValue);

		private static readonly double ceiling
				= Math.Floor(double.MaxValue);


		/// <summary>
		/// Static method creates a new sorted list of range instances
		/// for all values in the <typeparamref name="T"/> enum type.
		/// The first member in the list has a <see cref="Min"/> value
		/// of the CEILING of <see cref="double"/> <see cref="double.MinValue"/>;
		/// and the last has a <see cref="Max"/> value of the FLOOR of
		/// <see cref="double.MaxValue"/>.
		/// Since the list is sorted, it will search for values in
		/// ranges with a binary search.
		/// </summary>
		/// <returns>Not null. Sorted lowest to highest; and covers all
		/// indicated <see cref="double"/> values, and all
		/// <typeparamref name="T"/> <see cref="Enum"/> members.</returns>
		public static Set Get()
			=> new Set();


		[DataMember(Name = nameof(EnumValueRange<T>.EqualValues))]
		private T[] equalValues;


		/// <summary>
		/// Private constructor.
		/// </summary>
		/// <param name="value">The <see cref="Value"/>.</param>
		/// <param name="doubleValue">The <see cref="DoubleValue"/>.</param>
		/// <param name="equalValues">Optional: the <see cref="EqualValues"/>.</param>
		private EnumValueRange(T value, double doubleValue, T[] equalValues = null)
		{
			this.equalValues = equalValues ?? new T[0];
			Value = value;
			DoubleValue = getDouble(doubleValue);
			Min = DoubleValue;
			Max = DoubleValue;
		}


		private double getDouble(double value)
			=> double.IsNaN(value)
					? EnumValueRange<T>.floor
					: Math.Round(Math.Max(EnumValueRange<T>.floor, Math.Min(EnumValueRange<T>.ceiling, value)));


		/// <summary>
		/// The enum member value. If this Enum defines more than one
		/// member with the same value, then this is always the first value
		/// returned by <see cref="Enum.GetValues(Type)"/>; and in that
		/// case, <see cref="EqualValues"/> holds the remaining members
		/// with equal values; in the same returned order.
		/// </summary>
		[DataMember]
		public T Value { get; private set; }

		/// <summary>
		/// If this <typeparamref name="T"/> Enum type has defined
		/// more than one member with a value equal to <see cref="Value"/>,
		/// then this returns all OTHER members with equal values
		/// --- it will NOT include <see cref="Value"/>.
		/// Otherwise this is empty; but not null.
		/// The returned order is the order returned
		/// by <see cref="Enum.GetValues"/> (and
		/// <see cref="Value"/> is the first-returned member).
		/// </summary>
		public IReadOnlyList<T> EqualValues
			=> equalValues;

		/// <summary>
		/// Yields an enumeration of this <see cref="Value"/>, followed
		/// by any <see cref="EqualValues"/>.
		/// </summary>
		public IEnumerable<T> AllValues
			=> Value.AsSingle()
					.Concat(EqualValues);

		/// <summary>
		/// Holds this <see cref="Value"/> cast to <see cref="double"/>;
		/// anbd rounded.
		/// </summary>
		[DataMember]
		public double DoubleValue { get; private set; }

		/// <summary>
		/// Minimum value; which defines the selection range for an
		/// arbitrary primitive value to select this actual
		/// Enum <see cref="Value"/>. The minimum value for this
		/// property is the CEILING of <see cref="double.MinValue"/>.
		/// </summary>
		[DataMember]
		public double Min { get; private set; }

		/// <summary>
		/// Maximum value; which defines the selection range for an
		/// arbitrary primitive value to select this actual
		/// Enum <see cref="Value"/>. The maximum value for this
		/// property is the FLOOR of <see cref="double.MaxValue"/>.
		/// </summary>
		[DataMember]
		public double Max { get; private set; }


		/// <summary>
		/// Returns true if the given <paramref name="value"/>,
		/// cast to <see cref="double"/>, is within this <see cref="Min"/>
		/// and <see cref="Max"/>.
		/// </summary>
		/// <param name="value">An enum member value
		/// to compare with.</param>
		/// <returns>True if the value is in this range.</returns>
		public bool Contains(T value)
			=> Contains(Convert.ToDouble(value));

		/// <summary>
		/// Returns true if the given <see cref="double"/> <paramref name="value"/>
		/// is within this <see cref="Min"/> and <see cref="Max"/>.
		/// </summary>
		/// <param name="value">An enum member value
		/// to compare with.</param>
		/// <returns>True if the value is in this range.</returns>
		public bool Contains(double value)
		{
			value = getDouble(value);
			return (value >= Min)
					&& (value <= Max);
		}

		/// <summary>
		/// Returns true if this <see cref="DoubleValue"/> exactly
		/// equals this <paramref name="value"/>; cast to <see cref="double"/>.
		/// </summary>
		/// <param name="value">An enum member value
		/// to compare with.</param>
		/// <returns>True if the values are the same.</returns>
		public bool EqualsValue(T value)
			=> EqualsValue(Convert.ToDouble(value));

		/// <summary>
		/// Returns true if this <see cref="DoubleValue"/> exactly
		/// equals the given <paramref name="value"/>.
		/// </summary>
		/// <param name="value">An enum member underlying primitive value
		/// to compare with.</param>
		/// <returns>True if the values are the same.</returns>
		public bool EqualsValue(double value)
			=> DoubleValue == getDouble(value);


		public int CompareTo(EnumValueRange<T> other)
		{
			if (other == null)
				throw new ArgumentNullException(nameof(other));
			return DoubleValue != other.DoubleValue
					? DoubleValue.CompareTo(other.DoubleValue)
					: Min != other.Min
							? Min.CompareTo(other.Min)
							: Max.CompareTo(other.Max);
		}

		/// <summary>
		/// Implements an <see cref="IComparable"/> method
		/// for <typeparamref name="T"/>.
		/// </summary>
		/// <param name="other">The Enum value to compare with.</param>
		/// <returns>1 if the <paramref name="other"/> is less than
		/// this <see cref="Min"/>. -1 if greater than <see cref="Max"/>.
		/// Zero if within this range.</returns>
		public int CompareTo(T other)
			=> CompareTo(Convert.ToDouble(other));

		/// <summary>
		/// Implements an <see cref="IComparable"/> method
		/// for <see cref="double"/>.
		/// </summary>
		/// <param name="other">The value to compare with.</param>
		/// <returns>1 if the <paramref name="other"/> is less than
		/// this <see cref="Min"/>. -1 if greater than <see cref="Max"/>.
		/// Zero if within this range.</returns>
		public int CompareTo(double other)
		{
			other = getDouble(other);
			return other < Min
					? 1
					: other > Max
							? -1
							: 0;
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $"{Value} [{Min},{Max}] ({DoubleValue})"
					+ $", {nameof(EnumValueRange<T>.EqualValues)}{EqualValues.ToStringCollection(0)}"
					+ "]";
	}
}
