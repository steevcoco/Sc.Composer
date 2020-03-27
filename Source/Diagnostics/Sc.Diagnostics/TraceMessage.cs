using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Encapsulates an unformatted trace <see cref="Message"/>
	/// along with any <see cref="FormatArgs"/>; or an array
	/// of traced data objects. The ToString method returns the
	/// formatted string: it uses
	/// <see cref="TraceMessageHelper.FormatTraceMessage(string,bool,System.Exception,string,object[])"/>.
	/// </summary>
	[DataContract]
	[Serializable]
	public class TraceMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public TraceMessage() { }
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="exception">Not checked.</param>
		/// <param name="message">NOTICE: not tested.
		/// The traced message; OR the unformatted message format.</param>
		/// <param name="formatArgs">Not checked.</param>
		/// <param name="convertNamedFormatTokens">Optional: sets
		/// <see cref="ConvertNamedFormatTokens"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TraceMessage(
				Exception exception,
				string message,
				object[] formatArgs = null,
				bool convertNamedFormatTokens = false)
		{
			Exception = exception;
			Message = message;
			FormatArgs = formatArgs;
			ConvertNamedFormatTokens = convertNamedFormatTokens;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">NOTICE: not tested.
		/// The traced message; OR the unformatted message format.</param>
		/// <param name="formatArgs">Not checked.</param>
		/// <param name="convertNamedFormatTokens">Optional: sets
		/// <see cref="ConvertNamedFormatTokens"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TraceMessage(string message, object[] formatArgs = null, bool convertNamedFormatTokens = false)
				: this(null, message, formatArgs, convertNamedFormatTokens) { }

		/// <summary>
		/// Constructor for a single traced data object only:
		/// this <see cref="Message"/> will be null.
		/// </summary>
		/// <param name="data">Not checked.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TraceMessage(object data)
				: this(
						null,
						null,
						new[]
						{
							data
						}) { }

		/// <summary>
		/// Constructor for an array of traced data objects only:
		/// this <see cref="Message"/> will be null.
		/// </summary>
		/// <param name="data">Not checked.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TraceMessage(object[] data)
				: this(null, null, data) { }


		/// <summary>
		/// The optional Exception.
		/// </summary>
		[DataMember]
		public Exception Exception
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}

		/// <summary>
		/// The traced message; OR the unformatted message format.
		/// OR: this can be null if only an array of traces data
		/// objects is provided in <see cref="FormatArgs"/>.
		/// </summary>
		[DataMember]
		public string Message
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}

		/// <summary>
		/// Optional <see cref="Message"/> format args; OR an array of
		/// traced data objects only.
		/// </summary>
		[DataMember]
		public object[] FormatArgs
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}

		/// <summary>
		/// Optional parameter specifies that the <see cref="Message"/> contains
		/// named string format tokens --- instead of numbered tokens.
		/// </summary>
		[DataMember]
		public bool ConvertNamedFormatTokens
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}

		/// <summary>
		/// This is an optional string prefix, that if not null, will be
		/// prepended to the formatted message as-is.
		/// </summary>
		[DataMember]
		public string Prefix
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
			=> TraceMessageHelper.FormatTraceMessage(Prefix, ConvertNamedFormatTokens, Exception, Message, FormatArgs)
					.ToString();
	}
}
