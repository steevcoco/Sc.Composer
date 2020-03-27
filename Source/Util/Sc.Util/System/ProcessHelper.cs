using System;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace Sc.Util.System
{
	/// <summary>
	/// Process helpers.
	/// </summary>
	public static class ProcessHelper
	{
		/// <summary>
		/// This helper method starts a new <see cref="Process"/> with
		/// the arguments, and optionally redirects standard output and/or error.
		/// The method does not dispose the process, but returns an
		/// <see cref="IDisposable"/> that will dispose it. Therefore you
		/// can inspect the process before it is disposed, within a using block
		/// on the returned object. The returned object holds a
		/// <see cref="DelegateDisposable{TState}.State"/> tuple,
		/// which holds the process, and also will hold an
		/// <see cref="Exception"/> if this method catches any unhandled
		/// exception while starting the process or redirecting output.
		/// This method always sets <see cref="ProcessStartInfo.UseShellExecute"/>
		/// false, and if either output delegate handler is not null,
		/// will redirect one or both output streams, and invoke your
		/// handler(s) on events.
		/// </summary>
		/// <param name="processStartInfo">Required.</param>
		/// <param name="onOutputDataReceived">Optional: if this is not null,
		/// this method will redirect the process standard output, and invoke
		/// this when output events are raised.</param>
		/// <param name="onErrorDataReceived">Optional: if this is not null,
		/// this method will redirect the process standard error, and invoke
		/// this when error events are raised.</param>
		/// <returns>The process; and any <see cref="Exception"/>,
		/// caught while starting the process, or while redirecting output.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static DelegateDisposable<(Process process, Exception unhandledException)> StartProcess(
				ProcessStartInfo processStartInfo,
				Action<string> onOutputDataReceived = null,
				Action<string> onErrorDataReceived = null)
		{
			if (processStartInfo == null)
				throw new ArgumentNullException(nameof(processStartInfo));
			(Process process, Exception unhandledException) state;
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardOutput = onOutputDataReceived != null;
			processStartInfo.RedirectStandardError = onErrorDataReceived != null;
			Process process = new Process
			{
				StartInfo = processStartInfo,
				EnableRaisingEvents = true,
			};
			if (onOutputDataReceived != null)
				process.OutputDataReceived += HandleOutputDataReceived;
			if (onErrorDataReceived != null)
				process.ErrorDataReceived += HandleErrorDataReceived;
			try {
				process.Start();
				if (onOutputDataReceived != null)
					process.BeginOutputReadLine();
				if (onErrorDataReceived != null)
					process.BeginErrorReadLine();
				state = (process, null);
			} catch (Exception exception) {
				Trace.TraceWarning(
						"Catching error starting process '{0}' - '{1}'.",
						processStartInfo.FileName,
						exception.Message);
				Trace.WriteLine(exception);
				state = (process, exception);
			}
			return DelegateDisposable.With(state, Dispose);
			void HandleOutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
				=> onOutputDataReceived?.Invoke(eventArgs.Data);
			void HandleErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
				=> onErrorDataReceived?.Invoke(eventArgs.Data);
			static void Dispose((Process process, Exception unhandledException) tuple)
				=> tuple.process?.Dispose();
		}


		/// <summary>
		/// Fetches the PATH environment variable, and tries to locate the given file name in each
		/// path. Returns the first match that exists.
		/// </summary>
		/// <param name="fileName">The file name to find.</param>
		/// <param name="filePath">The full path to the file if found.</param>
		/// <returns>True if found.</returns>
		public static bool TryFindOnPath(string fileName, out string filePath)
		{
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentNullException(nameof(fileName));
			string environmentPath = Environment.GetEnvironmentVariable("PATH");
			if (!string.IsNullOrWhiteSpace(environmentPath)) {
				foreach (string path
						in environmentPath.Split(Path.PathSeparator)
								.Where(element => !string.IsNullOrWhiteSpace(element))) {
					filePath = Path.Combine(path, fileName);
					if (File.Exists(filePath))
						return true;
				}
			}
			filePath = null;
			return false;
		}
	}
}
