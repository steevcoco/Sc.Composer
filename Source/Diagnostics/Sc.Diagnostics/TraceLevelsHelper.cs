using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Static helpers for trace levels.
	/// </summary>
	public static class TraceLevelsHelper
	{
		/// <summary>
		/// Returns true if this <see cref="TraceEventType"/> is an Activity type.
		/// </summary>
		/// <param name="traceEventType">Required.</param>
		/// <returns>True for Activity types.</returns>
		public static bool IsActivity(this TraceEventType traceEventType)
			=> (int)traceEventType > (int)TraceEventType.Verbose;

		/// <summary>
		/// Returns true if this <see cref="TraceEventType"/> should log based
		/// on the given allowed filter level.
		/// </summary>
		/// <param name="traceEventType">This level to test.</param>
		/// <param name="filterLevel">The allowed filter level.</param>
		/// <returns>True if this level should trace.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ShouldTrace(this TraceEventType traceEventType, TraceEventType filterLevel)
			=> (int)traceEventType <= (int)filterLevel;


		/// <summary>
		/// Selects the most verbose of this <paramref name="sourceLevels"/> or
		/// the <paramref name="other"/>.
		/// </summary>
		/// <param name="sourceLevels">This level to check.</param>
		/// <param name="other">The other level to check.</param>
		/// <returns>The most verbose level</returns>
		public static SourceLevels MostVerbose(this SourceLevels sourceLevels, SourceLevels other)
			=> (sourceLevels == SourceLevels.All)
					|| (other == SourceLevels.All)
							? SourceLevels.All
							: (SourceLevels)Math.Max((int)sourceLevels, (int)other);
	}
}
