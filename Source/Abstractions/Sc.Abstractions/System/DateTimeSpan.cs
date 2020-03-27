using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace Sc.Abstractions.System
{
	/// <summary>
	/// Defines a <see cref="TimeSpan"/> with a <see cref="StartTime"/>.
	/// </summary>
	[DataContract]
	public struct DateTimeSpan
			: IEquatable<DateTimeSpan>
	{
		/// <summary>
		/// Static helper method will construct a new <see cref="DateTImeSpan"/>
		/// instance that is used to include only the exact given ticks inclusively.
		/// This creates a <see cref="TimeSpan"/> that is EXCLUSIVE
		/// of your provided INCLUSIVE <paramref name="lastTick"/>.
		/// E.G. for a start time of 12:00, and lastTick of 12:09:59.9,
		/// the <see cref="TimeSpan"/> would be an even tne minutes
		/// --- which then is "exclusive" of this provided last tick.
		/// If the provided lastTick were 12:10, then the
		/// constructed <see cref="TimeSpan"/> will be
		/// 10:00.1 --- ten minutes plus one tick; such that
		/// the constructed instance is inclusive only of the
		/// provided ticks exactly.
		/// </summary>
		/// <param name="startTime">Required; and must be valid.</param>
		/// <param name="lastTick">Required; and must be valid.
		/// Note that negative time spans are supported.</param>
		/// <returns></returns>
		public static DateTimeSpan FromInclusiveTicks(DateTime startTime, DateTime lastTick)
			=> new DateTimeSpan(
					startTime,
					lastTick < startTime
							? startTime.AddTicks(1L) - lastTick
							: lastTick.AddTicks(1L) - startTime);


		/// <summary>
		/// Constructor creates an instance with the given <paramref name="startTime"/>,
		/// and <paramref name="timeSpan"/>. Notice that the constructed
		/// <see cref="TimeSpan"/> is inclusive of the last tick in your given
		/// <paramref name="timeSpan"/>; which is relevant especially for
		/// consecutive adjacent time spans. E.G. a ten minute time span
		/// starting at midnight will return the ten minute time
		/// span (this <see cref="TimeSpan"/> IS ten minutes),
		/// and the exclusive <see cref="GetEndTime"/> value returns 12:10
		/// --- it is "exclusive" since the actual included time span by ticks is
		/// one tick longer than ten elapsed minutes, from the start tick.
		/// The inclusive <see cref="GetEndTime"/> value will include your
		/// specified ticks ONLY: it returns the time span
		/// minus one tick, to include only inclusive ticks.
		/// A  following adjacent ten-minute time span --- beginning AT
		/// 12:10 --- returns the same time span; and, the "exclusive"
		/// <see cref="GetEndTime"/> value starting with the prior time span
		/// would point instead to that following time span for a tick
		/// AT 12:10 exactly (though the prior inclusive time
		/// span WOULD include that tick instead). I.E. <see cref="StartTime"/>
		/// values are always considered inclusive exactly, and the
		/// <see cref="GetEndTime"/> method allows finding ticks
		/// exactly.
		/// </summary>
		/// <param name="startTime">Required; must be valid.</param>
		/// <param name="timeSpan">Required; must be valid.
		/// Note that negative time spans are supported.</param>
		/// <exception cref="ArgumentOutOfRangeException">If the resulting
		/// <see cref="GetEndTime"/> is invalid.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTimeSpan(DateTime startTime, TimeSpan timeSpan)
		{
			StartTime = startTime;
			TimeSpan = timeSpan;
			GetEndTime();
		}


		/// <summary>
		/// Returns true if the given point in time is contained by this range.
		/// </summary>
		/// <param name="time">Required.</param>
		/// <param name="exclusive">Determines the exact end time used: defaults to FALSE.
		/// See <see cref="GetEndTime"/>.</param>
		/// <returns>True if the time is at or after the start and before or at the end of this range.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(DateTime time, bool exclusive = false)
			=> (time >= StartTime)
					&& (time <= GetEndTime(exclusive));

		/// <summary>
		/// Returns 0 if the given point in time is contained by this range; 1 if it is later, and -1 if earlier.
		/// </summary>
		/// <param name="time">Required.</param>
		/// <param name="exclusive">Determines the exact end time used: defaults to FALSE.
		/// See <see cref="GetEndTime"/>.</param>
		/// <returns>The relative time offset of the argument compared to this range.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(DateTime time, bool exclusive = false)
			=> time > GetEndTime(exclusive)
					? 1
					: time < StartTime
							? -1
							: 0;

		/// <summary>
		/// Returns true only if the given range is fully contained by this range.
		/// </summary>
		/// <param name="dateTimeSpan">Required.</param>
		/// <returns>True if the range is fully within this range.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(DateTimeSpan dateTimeSpan)
			=> (dateTimeSpan.StartTime >= StartTime)
					&& (dateTimeSpan.GetEndTime() <= GetEndTime());

		/// <summary>
		/// Returns true if the given range is partly contained by this range.
		/// </summary>
		/// <param name="dateTimeSpan">Required.</param>
		/// <returns>True if either the start or end of the range is within this range.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Overlaps(DateTimeSpan dateTimeSpan)
		{
			switch (dateTimeSpan.StartTime.CompareTo(StartTime)) {
				case 1 :
					return dateTimeSpan.StartTime < GetEndTime();
				case 0 :
					return true;
				default :
					DateTime endTime = dateTimeSpan.GetEndTime(false);
					return (endTime >= StartTime)
							&& (endTime < GetEndTime());
			}
		}


		/// <summary>
		/// The start time.
		/// </summary>
		[DataMember]
		public DateTime StartTime
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		}

		/// <summary>
		/// The time span.
		/// </summary>
		[DataMember]
		public TimeSpan TimeSpan
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		}

		/// <summary>
		/// Creates a new <see cref="DateTime"/> by adding the <see cref="TimeSpan"/> to the
		/// <see cref="StartTime"/>.
		/// </summary>
		/// <param name="exclusive">Determines the exact time returned: if TRUE --- the DEFAULT --- then the
		/// <see cref="TimeSpan"/> is simply added to the <see cref="StartTime"/>, which yields an exclusive
		/// time --- at the next tick. Otherwise a tick is subtracted from the result.
		/// Pass false when you must determine the inclusiveness of a tick
		/// exactly --- where the following time span may start exactly at this
		/// exclusive end time, and the tick at that moment would be
		/// included in that following time span and not this one
		/// (e.g. for consecutive equal time spans, each exact tick is
		/// included at the exact start).</param>
		/// <returns>The time AT the end of the <see cref="TimeSpan"/>,
		/// if <paramref name="exclusive"/> is true. Otherwise the time
		/// at the last inclusive exact tick in this time span.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DateTime GetEndTime(bool exclusive = true)
			=> exclusive
					? StartTime.Add(TimeSpan)
					: TimeSpan.Ticks == 0L
							? StartTime
							: TimeSpan.Ticks < 0L
									? StartTime.Add(TimeSpan)
											.AddTicks(1L)
									: StartTime.Add(TimeSpan)
											.AddTicks(-1L);


		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
		{
			unchecked {
				return (StartTime.GetHashCode() * 397) ^ TimeSpan.GetHashCode();
			}
		}

		public override bool Equals(object obj)
			=> obj is DateTimeSpan other
					&& Equals(other);

		public bool Equals(DateTimeSpan other)
			=> StartTime.Equals(other.StartTime)
					&& TimeSpan.Equals(other.TimeSpan);

		public static bool operator ==(DateTimeSpan left, DateTimeSpan right)
			=> left.Equals(right);

		public static bool operator !=(DateTimeSpan left, DateTimeSpan right)
			=> !left.Equals(right);


		public override string ToString()
			=> $"{GetType().Name}[{StartTime.ToShortDateString()} {StartTime.ToShortTimeString()}][{TimeSpan}]";
	}
}
