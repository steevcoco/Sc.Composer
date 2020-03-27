using System.Diagnostics;
using Sc.Util.System;


namespace Sc.Diagnostics
{
	/// <summary>
	/// A <see cref="TraceListener"/> that simply writes to <see cref="Trace"/>.
	/// </summary>
	public sealed class TraceTraceListener
			: TraceListener
	{
		/// <summary>
		/// Returns the default <see cref="TraceListener.Name"/> that is used if the
		/// name passed to a constructor is null:
		/// <c>typeof(ConsoleTraceListener).GetFriendlyFullName()</c>.
		/// </summary>
		public static string DefaultName
			=> typeof(TraceTraceListener).GetFriendlyFullName();


		/// <summary>
		/// Constructor: this will have the default base <see cref="TraceListener.Filter"/>.
		/// Note that the <paramref name="name"/> is optional.
		/// </summary>
		/// <param name="name">Optional: if null, this uses the name
		/// <c>typeof(TraceTraceListener).FullName</c>: <see cref="DefaultName"/>.</param>
		public TraceTraceListener(string name = null)
				: base(name ?? TraceTraceListener.DefaultName) { }

		/// <summary>
		/// Constructor sets the <see cref="TraceListener.Filter"/> to
		/// <see cref="SourceLevels.Warning"/>by default.
		/// Note that the <paramref name="name"/> is optional.
		/// </summary>
		/// <param name="name">Optional: if null, this uses the name
		/// <c>typeof(TraceTraceListener).FullName</c>: <see cref="DefaultName"/>.</param>
		/// <param name="sourceLevels">Sets the <see cref="TraceListener.Filter"/>.</param>
		public TraceTraceListener(SourceLevels sourceLevels = SourceLevels.Warning, string name = null)
				: base(name ?? TraceTraceListener.DefaultName)
			=> Filter = new EventTypeFilter(sourceLevels);


		public override void Write(string message)
			=> Trace.Write(message);

		public override void WriteLine(string message)
			=> Trace.WriteLine(message);
	}
}
