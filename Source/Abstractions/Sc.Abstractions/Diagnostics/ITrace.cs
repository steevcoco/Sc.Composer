using System.Diagnostics;


namespace Sc.Abstractions.Diagnostics
{
	/// <summary>
	/// Defines a base interface for an object that handles diagnostic
	/// tracing.
	/// </summary>
    public interface ITrace
	{
		/// <summary>
		/// Checks and formats the message according to the arguments; and traces.
		/// Notice that the message may be null.
		/// </summary>
		/// <param name="traceEventType">Required.</param>
		/// <param name="eventId">Required.</param>
		/// <param name="message">Can be null or empty.</param>
		void TraceEvent(TraceEventType traceEventType, int eventId, string message);

		/// <summary>
		/// Checks and traces the data object according to the arguments.
		/// Notice that the data object may be null.
		/// </summary>
		/// <param name="traceEventType">Required.</param>
		/// <param name="eventId">Required.</param>
		/// <param name="data">Can be null.</param>
		void TraceData(TraceEventType traceEventType, int eventId, object data);

		/// <summary>
		/// Checks and traces the data object according to the arguments.
		/// Notice that the data may be null or empty.
		/// </summary>
		/// <param name="traceEventType">Required.</param>
		/// <param name="eventId">Required.</param>
		/// <param name="data">Can be null or empty.</param>
		void TraceData(TraceEventType traceEventType, int eventId, params object[] data);

		/// <summary>
		/// Flushes all listeners.
		/// </summary>
		void Flush();

		/// <summary>
		/// Determines if trace listeners should be called, based on the trace event type.
		/// </summary>
		/// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" />
		/// values.</param>
		/// <returns><see langword="True" /> if the trace listeners should be called;
		/// otherwise, <see langword="false" />.</returns>
		bool ShouldTrace(TraceEventType eventType);
	}
}
