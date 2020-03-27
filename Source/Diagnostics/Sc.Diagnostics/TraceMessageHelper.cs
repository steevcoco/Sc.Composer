using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Diagnostics helpers.
	/// </summary>
	public static class TraceMessageHelper
	{
		/// <summary>
		/// Converts a named format --- e.g. <c>"Text {NamedFormatItem} text."</c>
		/// --- to a numbered <see cref="M:string.Format"/> format; and invokes
		/// <see cref="string.Format(string,object[])"/> on the result.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string FormatNamedTokens(string format, params object[] values)
			=> string.Format(
					TraceMessageHelper.ConvertNamedTokens(format, out _)
							.ToString(),
					values);

		/// <summary>
		/// Converts a named format --- e.g. <c>"Message {NamedFormatItem}."</c>
		/// --- to a numbered <see cref="M:string.Format"/> format.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringBuilder ConvertNamedTokens(string format, out List<string> namedTokens)
		{
			if (string.IsNullOrWhiteSpace(format))
				throw new ArgumentNullException(nameof(format));

			StringBuilder sb = new StringBuilder();
			namedTokens = new List<string>(12);
			int index = 0;
			while (index < format.Length) {
				int openBraceIndex = FindBrace('{', index);
				int closeBraceIndex = FindBrace('}', openBraceIndex);
				int formatDelimiterIndex
						= format.IndexOfAny(
								new[] { ',', ':' },
								openBraceIndex,
								closeBraceIndex - openBraceIndex);
				if (formatDelimiterIndex == -1)
					formatDelimiterIndex = closeBraceIndex;
				if (closeBraceIndex == format.Length) {
					sb.Append(format, index, format.Length - index);
					index = format.Length;
				} else {
					sb.Append(format, index, (openBraceIndex - index) + 1);
					sb.Append(namedTokens.Count.ToString(CultureInfo.InvariantCulture));
					sb.Append(format, formatDelimiterIndex, (closeBraceIndex - formatDelimiterIndex) + 1);
					namedTokens.Add(format.Substring(openBraceIndex + 1, (formatDelimiterIndex - openBraceIndex) - 1));
					index = closeBraceIndex + 1;
				}
			}
			return sb;

			int FindBrace(char brace, int start)
			{
				int braceIndex = format.Length;
				int braceCount = 0;
				while (start < format.Length) {
					if ((braceCount > 0)
							&& (format[start] != brace)) {
						if ((braceCount % 2) == 0) {
							braceCount = 0;
							braceIndex = format.Length;
						} else
							break;
					} else if (format[start] == brace) {
						if (brace == '}') {
							if (braceCount == 0)
								braceIndex = start;
						} else
							braceIndex = start;
						++braceCount;
					}
					++start;
				}
				return braceIndex;
			}
		}


		/// <summary>
		/// Converts the message, Exception, and format args to a string now. Note that the
		/// arguments are checked, BUT the exception, message, and args can be null. If the
		/// message is null, the args will be output as ToString for each element. NOTICE
		/// that if the message AND args are not null, the the message MUST be a format
		/// that is compatible for the args.
		/// </summary>
		/// <param name="convertNamedFormatTokens">If true, the <c>message</c> can contain named format
		/// tokens --- e.g. <c>"Text {NamedFormatItem} text."</c> --- and they will be converted to
		/// numbers. Notice also that this conversion will succeed even if they are already numbered;
		/// BUT this will RE-ORDER all tokens incrementally from 0.</param>
		/// <param name="exception">Optional.</param>
		/// <param name="message">Can be null or empty.</param>
		/// <param name="args">Optional.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringBuilder FormatTraceMessage(
				bool convertNamedFormatTokens,
				Exception exception,
				string message,
				params object[] args)
			=> TraceMessageHelper.FormatTraceMessage(null, convertNamedFormatTokens, exception, message, args);

		/// <summary>
		/// As with <see cref="FormatTraceMessage(bool,System.Exception,string,object[])"/>:
		/// converts the message, Exception, and format args to a string now.
		/// This method takes a string that is prepended as-is to the result. Note that the
		/// arguments are checked, and the exception, message, and args can be null. If the
		/// message is null, the args will be output as ToString for each element. NOTICE
		/// that if the message AND args are not null, the the message MUST be a format
		/// that is compatible for the args.
		/// </summary>
		/// <param name="prefix">Prepended to the result. CAN be null.</param>
		/// <param name="convertNamedFormatTokens">If true, the <c>message</c> can contain named format
		/// tokens --- e.g. <c>"Text {NamedFormatItem} text."</c> --- and they will be converted to
		/// numbers. Notice also that this conversion will succeed even if they are already numbered;
		/// BUT this will RE-ORDER all tokens incrementally from 0.</param>
		/// <param name="exception">Optional.</param>
		/// <param name="message">Can be null or empty.</param>
		/// <param name="args">Optional.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StringBuilder FormatTraceMessage(
				string prefix,
				bool convertNamedFormatTokens,
				Exception exception,
				string message,
				params object[] args)
		{
			StringBuilder sb = new StringBuilder();
			if (prefix != null)
				sb.Append(prefix);
			if ((args != null)
					&& (args.Length != 0)) {
				if (!string.IsNullOrWhiteSpace(message)) {
					sb.AppendFormat(
							convertNamedFormatTokens
									? TraceMessageHelper.ConvertNamedTokens(message, out _)
											.ToString()
									: message,
							args);
				} else {
					foreach (object formatArg in args) {
						if (formatArg == null)
							continue;
						if (sb.Length != 0)
							sb.Append(", ");
						sb.Append(formatArg);
					}
				}
			} else {
				if (!string.IsNullOrWhiteSpace(message))
					sb.Append(message);
			}
			if (exception == null)
				return sb;
			if (sb.Length != 0)
				sb.AppendLine();
			return sb.Append(exception);
		}
	}
}
