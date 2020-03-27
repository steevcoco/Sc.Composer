using System;
using System.Diagnostics;


namespace Sc.Diagnostics.TraceFactory
{
	/// <summary>
	/// Static helpers for <see cref="LogFileFactory"/>.
	/// </summary>
	public static class LogFileFactoryHelper
	{
		/// <summary>
		/// This is a convenience method that will
		/// construct a new delegate <see cref="ILogFileFactorySelector"/>
		/// implementation that invokes your delegate; and adds it to
		/// this <paramref name="logFileFactory"/>
		/// </summary>
		/// <param name="logFileFactory">Not null.</param>
		/// <param name="selector">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static ILogFileFactorySelector AddSelector(
				this LogFileFactory logFileFactory,
				Func<TraceSource, LogFileFactorySelection, SourceLevels, LogFileFactorySelection> selector)
		{
			if (logFileFactory == null)
				throw new ArgumentNullException(nameof(logFileFactory));
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));
			DelegateLogFileFactorySelector result = new DelegateLogFileFactorySelector(selector);
			logFileFactory.AddSelector(result);
			return result;
		}

		/// <summary>
		/// Removes a delegate added by
		/// <see cref="AddSelector"/>.
		/// </summary>
		/// <param name="logFileFactory">Not null.</param>
		/// <param name="selector">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void RemoveSelector(
				this LogFileFactory logFileFactory,
				Func<TraceSource, LogFileFactorySelection, SourceLevels, LogFileFactorySelection> selector)
		{
			if (logFileFactory == null)
				throw new ArgumentNullException(nameof(logFileFactory));
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));
			logFileFactory.RemoveSelector(new DelegateLogFileFactorySelector(selector));
		}
	}
}
