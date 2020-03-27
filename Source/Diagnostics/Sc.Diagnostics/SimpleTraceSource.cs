using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sc.Abstractions.Diagnostics;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Simple class that implements <see cref="ITrace"/> for a given
	/// <see cref="TraceSource"/>. All methods simply invoke the
	/// same-named method on the <see cref="TraceSource"/>.
	/// </summary>
	public sealed class SimpleTraceSource
			: ITrace,
					IEquatable<ITrace>,
					IEquatable<SimpleTraceSource>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="traceSource">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SimpleTraceSource(TraceSource traceSource)
			=> TraceSource = traceSource ?? throw new ArgumentNullException(nameof(traceSource));


		/// <summary>
		/// This underlying <see cref="System.Diagnostics.TraceSource"/>.
		/// </summary>
		public TraceSource TraceSource { get; }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TraceEvent(TraceEventType traceEventType, int eventId, string message)
			=> TraceSource.TraceEvent(traceEventType, eventId, message);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TraceData(TraceEventType traceEventType, int eventId, object data)
			=> TraceSource.TraceData(traceEventType, eventId, data);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TraceData(TraceEventType traceEventType, int eventId, params object[] data)
			=> TraceSource.TraceData(traceEventType, eventId, data);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Flush()
			=> TraceSource.Flush();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ShouldTrace(TraceEventType eventType)
			=> TraceSource.Switch.ShouldTrace(eventType);


		public override int GetHashCode()
			=> TraceSource.GetHashCode();

		public override bool Equals(object obj)
			=> Equals(obj as SimpleTraceSource);

		public bool Equals(ITrace other)
			=> Equals(other as SimpleTraceSource);

		public bool Equals(SimpleTraceSource other)
			=> (other != null)
					&& object.ReferenceEquals(TraceSource, other.TraceSource);

		public override string ToString()
			=> $"{GetType().Name}[{TraceSource}]";
	}
}
