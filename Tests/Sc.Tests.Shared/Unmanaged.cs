using System;
using System.Collections.Generic;
using Sc.Util.System;


namespace Sc.Tests.Shared
{
	/// <summary>
	/// Helper <see langword="struct"/> that implements a fully
	/// <see langword="unmanaged"/> type. The instance is generated from
	/// an <see cref="Index"/>. Overloads operators.
	/// </summary>
	public struct Unmanaged
			: IEquatable<Unmanaged>,
					IComparable<Unmanaged>
	{
		public static bool operator ==(Unmanaged a, Unmanaged b)
			=> a.Equals(b);

		public static bool operator !=(Unmanaged a, Unmanaged b)
			=> !a.Equals(b);

		public static bool operator >(Unmanaged a, Unmanaged b)
			=> a.CompareTo(b) > 0;

		public static bool operator >=(Unmanaged a, Unmanaged b)
			=> a.CompareTo(b) >= 0;

		public static bool operator <(Unmanaged a, Unmanaged b)
			=> a.CompareTo(b) < 0;

		public static bool operator <=(Unmanaged a, Unmanaged b)
			=> a.CompareTo(b) <= 0;


		/// <summary>
		/// Constant <see cref="Date"/> value set on all instances.
		/// </summary>
		public static readonly DateTime AnchorDate = new DateTime(1969, 8, 5, 12, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Implements a <see cref="Comparer{T}"/> that compares the <see cref="Index"/>;
		/// which is this <see cref="IComparable{T}"/> implementation.
		/// </summary>
		/// <param name="x">First value.</param>
		/// <param name="y">Second value.</param>
		/// <returns>The <see cref="Index"/> comparison.</returns>
		public static int IndexComparer(Unmanaged x, Unmanaged y)
			=> x.Index.CompareTo(y.Index);

		/// <summary>
		/// Validates a sequence of instances; assuming that the sequence was
		/// created with incrementing instances starting from <see cref="Index"/>
		/// <paramref name="startIndex"/> --- which defaults to ZERO.
		/// Will throw <see cref="IndexOutOfRangeException"/> if any
		/// instance's <see cref="Validate(int)"/> method return false.
		/// </summary>
		/// <param name="collection">Required.</param>
		/// <param name="startIndex">Defaults to zero: the <see cref="Index"/>
		/// of the first element: where all following elements increment.</param>
		/// <param name="reverse">Defaults to false: if set true, then the
		/// indexes are assumed to be reversed: note that the
		/// <paramref name="startIndex"/> is STILL used as the first
		/// returned index; and further elements are assumed to decrement.</param>
		/// <exception cref="IndexOutOfRangeException">Assert failure.</exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static void ValidateAll(IEnumerable<Unmanaged> collection, int startIndex = 0, bool reverse = false)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, @">= 0");
			if (reverse) {
				foreach (Unmanaged element in collection) {
					if (!element.Validate(startIndex--))
						throw new IndexOutOfRangeException($@"Expected Index={startIndex}; Index={element.Index}");
				}
			} else {
				foreach (Unmanaged element in collection) {
					if (!element.Validate(startIndex++))
						throw new IndexOutOfRangeException($@"Expected Index={startIndex}; Index={element.Index}");
				}
			}
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="index">Required non-negative.</param>
		public Unmanaged(int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), index, @">= 0");
			Date = Unmanaged.AnchorDate;
			Index = index;
			TimeSpan = Date.AddYears(Index) - Date;
		}


		/// <summary>
		/// Validates as if constructed with the given index.
		/// </summary>
		/// <param name="index">Required.</param>
		/// <returns>True if valid.</returns>
		public bool Validate(int index)
			=> (Date == Unmanaged.AnchorDate)
					&& (Index == index)
					&& (TimeSpan == (Date.AddYears(index) - Date));


		/// <summary>
		/// Always the value of <see cref="AnchorDate"/>
		/// </summary>
		public DateTime Date { get; }

		/// <summary>
		/// The unique index: >= 0.
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// Is the time span of <c>((<see cref="Date"/> plus <see cref="Index"/>
		/// Years) minus <see cref="Date"/>)</c>
		/// --- i.e. <see cref="Index"/> Years.
		/// </summary>
		public TimeSpan TimeSpan { get; }


		public override int GetHashCode()
			=> HashCodeHelper.Seed
					.Hash(Date)
					.Hash(Index)
					.Hash(TimeSpan);

		public override bool Equals(object obj)
			=> obj is Unmanaged other
					&& Equals(other);

		public bool Equals(Unmanaged other)
			=> (Date == other.Date)
					&& (Index == other.Index)
					&& (TimeSpan == other.TimeSpan);

		public int CompareTo(Unmanaged other)
			=> Unmanaged.IndexComparer(this, other);

		public override string ToString()
			=> $"{nameof(Unmanaged)}"
					+ "["
					+ $"{nameof(Unmanaged.Index)}={Index}"
					+ $", {nameof(Unmanaged.TimeSpan)}={TimeSpan}"
					+ $", {nameof(Unmanaged.Date)}={Date}"
					+ "]";
	}
}
