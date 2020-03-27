using System;
using System.Collections.Generic;
using System.Linq;


namespace Sc.Composer
{
	/// <summary>
	/// Provides a <see cref="Composer"/> <see cref="AggregateException"/> class.
	/// </summary>
	public class AggregateComposerException
			: AggregateException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Required.</param>
		/// <param name="innerExceptions">Optional</param>
		public AggregateComposerException(string message, IEnumerable<ComposerException> innerExceptions)
				: base(
						message,
						innerExceptions?.OfType<Exception>()
						?? new Exception[0]) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Required.</param>
		/// <param name="innerExceptions">Optional</param>
		public AggregateComposerException(string message, params ComposerException[] innerExceptions)
				: this(message, (IEnumerable<ComposerException>)innerExceptions) { }
	}
}
