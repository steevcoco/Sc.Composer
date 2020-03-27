using System;
using System.Diagnostics;
using Sc.Util.System;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Since NetStandard 2.0 appears to lack the class, this is a very simple
	/// <see cref="TraceListener"/> that extends <see cref="TextWriterTraceListener"/>
	/// and writes to the <see cref="Console"/>.
	/// </summary>
	public class ConsoleTraceListener
			: TextWriterTraceListener
	{
		/// <summary>
		/// Returns the default <see cref="TraceListener.Name"/> that is used if the
		/// name passed to a constructor is null:
		/// <c>typeof(ConsoleTraceListener).GetFriendlyFullName()</c>.
		/// </summary>
		public static string DefaultName
			=> typeof(ConsoleTraceListener).GetFriendlyFullName();


		/// <summary>
		/// Constructs an instance that writes to <see cref="Console"/> <see cref="Console.Out"/>.
		/// Note that the <paramref name="name"/> is optional.
		/// </summary>
		/// <param name="name">Optional: if null, this uses the name
		/// <c>typeof(ConsoleTraceListener).FullName</c>: <see cref="DefaultName"/>.</param>
		public ConsoleTraceListener(string name = null)
				: this(false, name) { }

		/// <summary>
		/// Constructs an instance that writes to either <see cref="Console"/> <see cref="Console.Out"/>
		/// or <see cref="Console.Error"/>.
		/// Note that the <paramref name="name"/> is optional.
		/// </summary>
		/// <param name="useErrorStream">True for the error stream.</param>
		/// <param name="name">Optional: if null, this uses the name
		/// <c>typeof(ConsoleTraceListener).FullName</c>: <see cref="DefaultName"/>.</param>
		public ConsoleTraceListener(bool useErrorStream, string name = null)
				: base(
						useErrorStream
								? Console.Error
								: Console.Out,
						name ?? ConsoleTraceListener.DefaultName) { }


		public override void Close()
		{
			// No resources to clean up.
		}
	}
}
