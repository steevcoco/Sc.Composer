using System;


namespace Sc.Util.System
{
	/// <summary>
	/// Helpers for <see cref="DateTime"/> and <see cref="TimeSpan"/>.
	/// </summary>
	public static class DateTimeHelper
	{
		/// <summary>
		/// Compares this <see cref="DateTime"/> with the <paramref name="other"/>
		/// and returns the earlier value. Notice that the arguments are not checked.
		/// This method compares the dates with <c>DateTime &lt; other</c>.
		/// </summary>
		/// <param name="dateTime">This value to compare.</param>
		/// <param name="other">The value to compare with.</param>
		/// <returns>The earlier value in time.</returns>
		public static DateTime Min(this DateTime dateTime, DateTime other)
			=> other < dateTime
					? other
					: dateTime;

		/// <summary>
		/// Compares this <see cref="DateTime"/> with the <paramref name="other"/>
		/// and returns the later value. Notice that the arguments are not checked.
		/// This method compares the dates with <c>DateTime > other</c>.
		/// </summary>
		/// <param name="dateTime">This value to compare.</param>
		/// <param name="other">The value to compare with.</param>
		/// <returns>The later value in time.</returns>
		public static DateTime Max(this DateTime dateTime, DateTime other)
			=> other > dateTime
					? other
					: dateTime;


		/// <summary>
		/// Creates a new <see cref="DateTime"/> from this <paramref name="dateTime"/>
		/// that is truncated to the minute.
		/// </summary>
		/// <param name="dateTime">This value.</param>
		/// <returns>A new instance.</returns>
		public static DateTime TruncateToMinutes(this DateTime dateTime)
			=> new DateTime(
					dateTime.Year,
					dateTime.Month,
					dateTime.Day,
					dateTime.Hour,
					dateTime.Minute,
					0,
					0,
					dateTime.Kind);

		/// <summary>
		/// Creates a new <see cref="DateTime"/> from this <paramref name="dateTime"/>
		/// that is truncated to the hour.
		/// </summary>
		/// <param name="dateTime">This value.</param>
		/// <returns>A new instance.</returns>
		public static DateTime TruncateToHours(this DateTime dateTime)
			=> new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0, dateTime.Kind);

		/// <summary>
		/// Creates a new <see cref="DateTime"/> from this <paramref name="dateTime"/>
		/// that is truncated to the day.
		/// </summary>
		/// <param name="dateTime">This value.</param>
		/// <returns>A new instance.</returns>
		public static DateTime TruncateToDays(this DateTime dateTime)
			=> new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, dateTime.Kind);


		/// <summary>
		/// Returns the <see cref="DateTime.Ticks"/> from the argument by converting with
		/// <see cref="BitConverter"/>, and first converting the argument to Universal TIme.
		/// Then will reverse the bytes if this platform is little endian.
		/// </summary>
		/// <param name="dateTime">Not checked.</param>
		/// <returns>Not null or empty.</returns>
		public static byte[] GetUtcTicksNetworkBytes(this DateTime dateTime)
		{
			byte[] timeStampBytes
					= BitConverter.GetBytes(
							dateTime.ToUniversalTime()
									.Ticks);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(timeStampBytes);
			return timeStampBytes;
		}

		/// <summary>
		/// Converts the bytes returned by <see cref="GetUtcTicksNetworkBytes"/> --- which is the
		/// <see cref="DateTime.Ticks"/> --- and will first reverse the bytes if this platform is
		/// little endian.
		/// </summary>
		/// <param name="dateTimeUtcTicksNetworkBytes">Not checked.</param>
		/// <returns>Will be <see cref="DateTimeKind.Utc"/>.</returns>
		public static DateTime FromUtcTicksNetworkBytes(this byte[] dateTimeUtcTicksNetworkBytes)
		{
			if (dateTimeUtcTicksNetworkBytes == null)
				throw new ArgumentNullException(nameof(dateTimeUtcTicksNetworkBytes));
			if (dateTimeUtcTicksNetworkBytes.Length != sizeof(long))
				throw new ArgumentException(nameof(dateTimeUtcTicksNetworkBytes));
			if (BitConverter.IsLittleEndian)
				Array.Reverse(dateTimeUtcTicksNetworkBytes);
			return new DateTime(BitConverter.ToInt64(dateTimeUtcTicksNetworkBytes, 0), DateTimeKind.Utc);
		}

		/// <summary>
		/// Returns the <see cref="TimeSpan.Ticks"/> from the argument by converting with
		/// <see cref="BitConverter"/>. Then will reverse the bytes if this platform is little endian.
		/// </summary>
		/// <param name="timeSpan">Not checked.</param>
		/// <returns>Not null or empty.</returns>
		public static byte[] GetTicksNetworkBytes(this TimeSpan timeSpan)
		{
			byte[] timeStampBytes = BitConverter.GetBytes(timeSpan.Ticks);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(timeStampBytes);
			return timeStampBytes;
		}

		/// <summary>
		/// Converts the bytes returned by <see cref="GetTicksNetworkBytes"/> --- which is the
		/// <see cref="TimeSpan.Ticks"/> --- and will first reverse the bytes if this platform is
		/// little endian.
		/// </summary>
		/// <param name="timeSpanTicksNetworkBytes">Not checked.</param>
		/// <returns>The <see cref="TimeSpan"/>.</returns>
		public static TimeSpan FromTimeSpanTicksNetworkBytes(this byte[] timeSpanTicksNetworkBytes)
		{
			if (timeSpanTicksNetworkBytes == null)
				throw new ArgumentNullException(nameof(timeSpanTicksNetworkBytes));
			if (timeSpanTicksNetworkBytes.Length != sizeof(long))
				throw new ArgumentException(nameof(timeSpanTicksNetworkBytes));
			if (BitConverter.IsLittleEndian)
				Array.Reverse(timeSpanTicksNetworkBytes);
			return new TimeSpan(BitConverter.ToInt64(timeSpanTicksNetworkBytes, 0));
		}
	}
}
