using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Sc.Abstractions.ServiceLocator;
using Sc.BasicContainer;
using Sc.Composer;
using Sc.Composer.Composers;
using Sc.Composer.Mef;
using Sc.Diagnostics;


namespace ContainerExample
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
			BasicContainer target = new BasicContainer(); // Our composition target
			Console.WriteLine($"New BasicContainer: {target}");
			using (ContainerComposer<IContainerBase> composer = new ContainerComposer<IContainerBase>(() => target)) { // Composer
				// Discover Assemblies in our Program CodeBase,
				// and search for any Exported IComposerParticipant<IContainerBase> participants:
				// (crude implementation will simply load all Assemblies here and look for Exports in every one)
				string codeBase = new Uri(typeof(Program).Assembly.GetName().CodeBase)
						.LocalPath;
				codeBase = Path.GetDirectoryName(codeBase);
				foreach (string filePath in Directory.EnumerateFiles(
						codeBase,
						"*.dll",
						SearchOption.TopDirectoryOnly)) {
					// MefComposerHelper loads any Exports from the assembly:
					composer.ParticipateRange(
							MefComposerHelper.GetInstances<IComposerParticipant<IContainerBase>>(
									Assembly.LoadFrom(filePath)));
				}
				Console.WriteLine($"Composer: {composer}");
				Console.WriteLine("Compose ...");
				// Tracing is not set verbose until here, because the above
				// MEF helper methods also create Composer instances while
				// fetching Exports ... The console will be cluttered
				// with verbose traces from each Assembly above ...
				Program.configureTracing();
				composer.Compose(); // Compose
			}
			Console.WriteLine($"BasicContainer: {target}");
			Console.WriteLine("  ... All Registered Service Types:");
			foreach (Type serviceType in target.GetRegisteredServiceTypes()) {
				Console.WriteLine($"    {serviceType.FullName}");
			}
			Console.WriteLine("Done");
			Console.ReadKey();
		}
	}
}
