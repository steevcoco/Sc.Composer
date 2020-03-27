using System;
using System.Diagnostics;
using Sc.Composer;
using Sc.Diagnostics;


namespace SimpleExample
{
	internal static class Program
	{
		/// <summary>
		/// Static helper method is used to configure optional verbose tracing.
		/// </summary>
		private static void configureTracing()
		{
			Console.WriteLine("Verbose Tracing? (Y for Yes)");
			ConsoleKey consoleKey = Console.ReadKey().Key;
			Console.WriteLine();
			if (consoleKey != ConsoleKey.Y)
				return;
			void Configure(SimpleTraceSource traceSource)
			{
				traceSource.TraceSource.TryAdd(
						new Sc.Diagnostics.ConsoleTraceListener
						{
							Filter = new EventTypeFilter(SourceLevels.All),
						});
				traceSource.TraceSource.Switch.Level = SourceLevels.All;
			}
			TraceSources.AddSelector(new DelegateTraceSourceSelector(Configure));
			TraceSources.For(typeof(Program))
					.Verbose("TraceSources are verbose.");
		}


		public static void Main(string[] args)
		{
			Console.WriteLine("Begin ...");
			Program.configureTracing();
			MyTarget target = new MyTarget(); // Our composition target
			Console.WriteLine($"New MyTarget: {target}");
			using (Composer<MyTarget> composer = new Composer<MyTarget>(() => target)) { // Composer
				composer.Participate(new MyParticipant()); // Manual addition of Participant
				Console.WriteLine($"Composer: {composer}");
				Console.WriteLine("Compose ...");
				composer.Compose(); // Compose
			}
			Console.WriteLine($"MyTarget: {target}");
			Console.WriteLine("Done");
			Console.ReadKey();
		}
	}
}
