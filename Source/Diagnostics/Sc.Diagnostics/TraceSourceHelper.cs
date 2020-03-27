using System;
using System.Diagnostics;
using System.Linq;
using Sc.Util.Collections;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Static helpers for <see cref="TraceSource"/>.
	/// </summary>
	public static class TraceSourceHelper
	{
		/// <summary>
		/// Checks this <paramref name="traceSource"/> <see cref="TraceSource.Listeners"/>
		/// collection for a <see cref="TraceListener"/> that equals the given
		/// <paramref name="traceListener"/> OR HAs the same <see cref="TraceListener.Name"/>.
		/// </summary>
		/// <param name="traceSource">Not null.</param>
		/// <param name="traceListener">Not null.</param>
		/// <returns>True if added.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool TryAdd(this TraceSource traceSource, TraceListener traceListener)
		{
			if (traceSource == null)
				throw new ArgumentNullException(nameof(traceSource));
			if (traceListener == null)
				throw new ArgumentNullException(nameof(traceListener));
			int index = traceSource.Listeners
					.OfType<TraceListener>()
					.FindIndex(
							listener => traceListener.Equals(listener)
									|| string.Equals(traceListener.Name, listener.Name));
			if (index >= 0)
				return false;
			traceSource.Listeners.Add(traceListener);
			return true;
		}
	}
}
