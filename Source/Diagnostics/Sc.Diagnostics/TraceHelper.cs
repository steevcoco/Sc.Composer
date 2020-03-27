using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Sc.Abstractions.Diagnostics;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Extension methods for <see cref="ITrace"/>. Note that this class uses
	/// <see cref="TraceMessageHelper"/> methods to format messages.
	/// </summary>
	public static class TraceHelper
	{
		// ##########    Switch    ##########


		/// <summary>
		/// Probes whether this <see cref="Switch"/> should trace
		/// <see cref="TraceEventType.Critical"/>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCritical(this ITrace trace)
			=> trace?.ShouldTrace(TraceEventType.Critical) ?? throw new ArgumentNullException(nameof(trace));

		/// <summary>
		/// Probes whether this <see cref="Switch"/> should trace
		/// <see cref="TraceEventType.Error"/>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsError(this ITrace trace)
			=> trace?.ShouldTrace(TraceEventType.Error) ?? throw new ArgumentNullException(nameof(trace));

		/// <summary>
		/// Probes whether this <see cref="Switch"/> should trace
		/// <see cref="TraceEventType.Warning"/>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsWarning(this ITrace trace)
			=> trace?.ShouldTrace(TraceEventType.Warning) ?? throw new ArgumentNullException(nameof(trace));

		/// <summary>
		/// Probes whether this <see cref="Switch"/> should trace
		/// <see cref="TraceEventType.Information"/>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInfo(this ITrace trace)
			=> trace?.ShouldTrace(TraceEventType.Information) ?? throw new ArgumentNullException(nameof(trace));

		/// <summary>
		/// Probes whether this <see cref="Switch"/> should trace
		/// <see cref="TraceEventType.Verbose"/>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsVerbose(this ITrace trace)
			=> trace?.ShouldTrace(TraceEventType.Verbose) ?? throw new ArgumentNullException(nameof(trace));

		/// <summary>
		/// Probes whether this <see cref="Switch"/> should trace
		/// any activity events.
		/// </summary>
		/// <param name="trace">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsActivity(this ITrace trace)
		{
			if (trace == null)
				throw new ArgumentNullException(nameof(trace));
			return trace.ShouldTrace(TraceEventType.Start)
					|| trace.ShouldTrace(TraceEventType.Stop)
					|| trace.ShouldTrace(TraceEventType.Suspend)
					|| trace.ShouldTrace(TraceEventType.Resume)
					|| trace.ShouldTrace(TraceEventType.Transfer);
		}


		// ##########    Scope    ##########


		/// <summary>
		/// Creates a logical operation
		/// scope based on System.Diagnostics LogicalOperationStack.
		/// </summary>
		/// <param name="trace">Can be null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <returns>A disposable scope object. Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
				"Style",
				"IDE0060:Remove unused parameter",
				Justification = "Extension method syntax only.")]
		public static IDisposable BeginScope(this ITrace trace, string message, params object[] args)
			=> new TraceScope(message, args);

		/// <summary>
		/// Creates a logical operation
		/// scope based on System.Diagnostics LogicalOperationStack.
		/// </summary>
		/// <param name="trace">Can be null.</param>
		/// <param name="data">Optional data to trace.</param>
		/// <returns>A disposable scope object. Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
				"Style",
				"IDE0060:Remove unused parameter",
				Justification = "Extension method syntax only.")]
		public static IDisposable BeginScope(this ITrace trace, object data)
			=> new TraceScope(data);

		/// <summary>
		/// Creates a logical operation
		/// scope based on System.Diagnostics LogicalOperationStack.
		/// </summary>
		/// <param name="trace">Can be null.</param>
		/// <param name="data">Optional data to trace.</param>
		/// <returns>A disposable scope object. Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
				"Style",
				"IDE0060:Remove unused parameter",
				Justification = "Extension method syntax only.")]
		public static IDisposable BeginScope(this ITrace trace, object[] data)
			=> new TraceScope(data);


		// ##########    Trace    ##########


		/// <summary>
		/// Invokes
		/// <see cref="TraceSource"/> <see cref="TraceSource.TraceEvent(TraceEventType,int,string)"/>,
		/// and first formats the massage. This method first checks if this <paramref name="trace"/>
		/// <see cref="ITrace.ShouldTrace"/> for the given <paramref name="traceEventType"/>. If
		/// so, this converts the message, Exception, and format args to a string now. Note that the
		/// arguments are checked, BUT the exception, message, and args can be null. If the
		/// message is null, the args will be output as ToString for each element. NOTICE
		/// that if the message AND args are not null, the the message MUST be a format
		/// that is compatible for the args. This method delegates to
		/// <see cref="TraceMessageHelper.FormatTraceMessage(bool,Exception,string,object[])"/>
		/// to construct this formatted message.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="convertNamedFormatTokens">If true, the <c>message</c> can contain named format
		/// tokens --- e.g. <c>"Text {NamedFormatItem} text."</c> --- and they will be converted to
		/// numbers. Notice also that this conversion will succeed even if they are already numbered;
		/// BUT this will RE-ORDER all tokens incrementally from 0.</param>
		/// <param name="traceEventType">Required.</param>
		/// <param name="eventId">Required.</param>
		/// <param name="exception">Optional.</param>
		/// <param name="message">Can be null or empty.</param>
		/// <param name="args">Optional.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TraceEvent(
				this ITrace trace,
				bool convertNamedFormatTokens,
				TraceEventType traceEventType,
				int eventId,
				Exception exception,
				string message,
				params object[] args)
		{
			if (trace == null)
				throw new ArgumentNullException(nameof(trace));
			if (!trace.ShouldTrace(traceEventType))
				return;
			StringBuilder sb
					= TraceMessageHelper.FormatTraceMessage(
							convertNamedFormatTokens,
							exception,
							message,
							args);
			if (sb.Length != 0)
				trace.TraceEvent(traceEventType, eventId, sb.ToString());
		}


		// ##########    With EventId    ##########


		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Critical"/> with <c>eventId</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Critical(this ITrace trace, int eventId, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Critical, eventId, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Error"/> with <c>eventId</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Error(this ITrace trace, int eventId, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Error, eventId, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Warning"/> with <c>eventId</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Warning(this ITrace trace, int eventId, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Warning, eventId, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Information"/> with <c>eventId</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Info(this ITrace trace, int eventId, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Information, eventId, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Verbose"/> with <c>eventId</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Verbose(this ITrace trace, int eventId, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Verbose, eventId, null, message, args);


		// ##########    No EventId    ##########


		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Critical"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Critical(this ITrace trace, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Critical, 0, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Error"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Error(this ITrace trace, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Error, 0, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Warning"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Warning(this ITrace trace, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Warning, 0, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Information"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Info(this ITrace trace, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Information, 0, null, message, args);

		/// <summary>
		/// Traces the <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Verbose"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Verbose(this ITrace trace, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Verbose, 0, null, message, args);


		// ##########    With Exception, and EventId    ##########


		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Critical"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Critical(
				this ITrace trace,
				int eventId,
				Exception exception,
				string message,
				params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Critical, eventId, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Error"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Error(
				this ITrace trace,
				int eventId,
				Exception exception,
				string message,
				params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Error, eventId, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Warning"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Warning(
				this ITrace trace,
				int eventId,
				Exception exception,
				string message,
				params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Warning, eventId, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Information"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Info(
				this ITrace trace,
				int eventId,
				Exception exception,
				string message,
				params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Information, eventId, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Verbose"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="eventId">Optional event correlation Id.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Verbose(
				this ITrace trace,
				int eventId,
				Exception exception,
				string message,
				params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Verbose, eventId, exception, message, args);


		// ##########    With Exception, No EventId    ##########


		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Critical"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Critical(this ITrace trace, Exception exception, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Critical, 0, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Error"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Error(this ITrace trace, Exception exception, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Error, 0, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Warning"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Warning(this ITrace trace, Exception exception, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Warning, 0, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Information"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Info(this ITrace trace, Exception exception, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Information, 0, exception, message, args);

		/// <summary>
		/// Traces the <c>exception</c>, <c>message</c> and <c>args</c> as
		/// <see cref="TraceEventType.Verbose"/> with <c>eventId 0</c>.
		/// </summary>
		/// <param name="trace">Not null.</param>
		/// <param name="exception">Exception to trace.</param>
		/// <param name="message">If the <c>args</c> are provided, then this MUST be a string format for
		/// <see cref="TraceSource.TraceEvent(TraceEventType,int,string,object[])"/>; and otherwise
		/// is logged as-is.</param>
		/// <param name="args">Optional format args.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Verbose(this ITrace trace, Exception exception, string message, params object[] args)
			=> trace.TraceEvent(false, TraceEventType.Verbose, 0, exception, message, args);
	}
}
