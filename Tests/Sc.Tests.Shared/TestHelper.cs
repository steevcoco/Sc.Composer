using System;
using System.Diagnostics;
using System.IO;
using Sc.Diagnostics;
using Sc.Util.System;


namespace Sc.Tests.Shared
{
	/// <summary>
	/// Static test helpers.
	/// </summary>
	public static class TestHelper
	{
		/// <summary>
		/// Invokes the <see cref="TraceSources"/> factory method with a delegate
		/// that adds a verbose console trace listener to all sources.
		/// </summary>
		/// <returns>Dispose to reset the default factory.</returns>
		public static IDisposable TraceAllVerbose()
		{
			void Configure(SimpleTraceSource traceSource)
			{
				traceSource.TraceSource.TryAdd(
						new Diagnostics.ConsoleTraceListener
						{
								Filter = new EventTypeFilter(SourceLevels.All),
						});
				traceSource.TraceSource.Switch.Level = SourceLevels.All;
			}
			void Remove(SimpleTraceSource traceSource)
				=> traceSource.TraceSource.Listeners.Remove(Diagnostics.ConsoleTraceListener.DefaultName);
			DelegateTraceSourceSelector selector = new DelegateTraceSourceSelector(Configure, Remove);
			TraceSources.AddSelector(selector);
			TraceSources.For(typeof(TestHelper))
					.Verbose("TraceSources are verbose.");
			void Dispose()
				=> TraceSources.RemoveSelector(selector);
			return DelegateDisposable.With(Dispose);
		}

		/// <summary>
		/// Ensures that the directory exists, and returns a disposable
		/// that will delete the directory and all files.
		/// </summary>
		/// <param name="folderPath">Required.</param>
		/// <returns>Not null.</returns>
		public static IDisposable UsingTempFolder(string folderPath)
		{
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);
			void DeleteTempFolder()
			{
				if (!Directory.Exists(folderPath))
					return;
				try {
					Directory.Delete(folderPath, true);
				} catch {
					// The folder may be open in Explorer
				}
			}
			return DelegateDisposable.With(DeleteTempFolder);
		}
	}
}
