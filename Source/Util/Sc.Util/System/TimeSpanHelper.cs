using System;


namespace Sc.Util.System
{
	/// <summary>
	/// Static helpers for <see cref="TimeSpan"/>.
	/// </summary>
	public static class TimeSpanHelper
	{
		/// <summary>
		/// Returns the shorter of this <paramref name="timeSpan"/> or the
		/// <paramref name="other"/>.
		/// </summary>
		/// <param name="timeSpan">This value to test.</param>
		/// <param name="other">The other value to test.</param>
		/// <returns>The shorter <see cref="TimeSpan"/>.</returns>
		public static TimeSpan Min(this TimeSpan timeSpan, TimeSpan other)
			=> other < timeSpan
					? other
					: timeSpan;

		/// <summary>
		/// Returns the longer of this <paramref name="timeSpan"/> or the
		/// <paramref name="other"/>.
		/// </summary>
		/// <param name="timeSpan">This value to test.</param>
		/// <param name="other">The other value to test.</param>
		/// <returns>The longer <see cref="TimeSpan"/>.</returns>
		public static TimeSpan Max(this TimeSpan timeSpan, TimeSpan other)
			=> other > timeSpan
					? other
					: timeSpan;
	}
}
