using System;


namespace Sc.Composer
{
	/// <summary>
	/// Provides an <see cref="Exception"/> class for <see cref="Composer"/>.
	/// </summary>
	public class ComposerException
			: Exception
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Required.</param>
		/// <param name="innerException">Optional.</param>
		public ComposerException(string message, Exception innerException = null)
				: base(message, innerException) { }
	}
}
