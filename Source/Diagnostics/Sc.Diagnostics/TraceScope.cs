using System;
using System.Diagnostics;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Provides an IDisposable that represents a logical operation
	/// scope based on System.Diagnostics LogicalOperationStack.
	/// </summary>
	public sealed class TraceScope
			: IDisposable
	{
		private volatile bool isDisposed;


		/// <summary>
		/// Pushes state onto the LogicalOperationStack by calling
		/// <see cref="CorrelationManager.StartLogicalOperation(object)"/>
		/// </summary>
		/// <param name="message">The traced message;
		/// OR the unformatted message format.</param>
		/// <param name="formatArgs">Not checked.</param>
		public TraceScope(string message, object[] formatArgs = null)
			=> Trace.CorrelationManager.StartLogicalOperation(new TraceMessage(message, formatArgs));

		/// <summary>
		/// Pushes state onto the LogicalOperationStack by calling
		/// <see cref="CorrelationManager.StartLogicalOperation(object)"/>
		/// </summary>
		/// <param name="data">The state: may not be null.</param>
		public TraceScope(object data)
			=> Trace.CorrelationManager.StartLogicalOperation(new TraceMessage(data));

		/// <summary>
		/// Pushes state onto the LogicalOperationStack by calling
		/// <see cref="CorrelationManager.StartLogicalOperation(object)"/>
		/// </summary>
		/// <param name="data">The traced data.</param>
		public TraceScope(object[] data = null)
			=> Trace.CorrelationManager.StartLogicalOperation(new TraceMessage(data));


		/// <summary>
		/// Pops a state off the LogicalOperationStack by calling
		/// <see cref="CorrelationManager.StopLogicalOperation()"/>
		/// </summary>
		public void Dispose()
		{
			if (isDisposed)
				return;
			isDisposed = true;
			Trace.CorrelationManager.StopLogicalOperation();
		}
	}
}
