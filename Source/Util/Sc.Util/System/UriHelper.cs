using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace Sc.Util.System
{
	/// <summary>
	/// Static utilities for <see cref="Uri"/>s.
	/// </summary>
	public static class UriHelper
	{
		/// <summary>
		/// Similar to <see cref="Path.GetDirectoryName"/>, this will construct a new <see cref="Uri"/>
		/// that is rooted at the given Uri, and ends without the final file or query parts. The form is
		/// <c>new Uri(uri, ".")</c> for an absolute Uri, and
		/// <c>new Uri(Path.GetDirectoryName(uri.ToString()))</c> for a relative Uri.
		/// </summary>
		/// <param name="uri">Not null.</param>
		/// <returns>Not null.</returns>
		public static Uri GetDirectoryName(this Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));
			if (uri.IsAbsoluteUri)
				return new Uri(uri, ".");
			string directoryName = Path.GetDirectoryName(uri.ToString());
			return directoryName == null
					? new Uri(".", UriKind.Relative)
					: new Uri(directoryName, UriKind.Relative);
		}


		/// <summary>
		/// This method does similar work to <see cref="Uri(Uri,Uri)"/>, but this method checks the
		/// last character in <c>baseUri</c> and the first character in the <c>relativeUri</c>, and
		/// ensures that the base ends with <c>"/"</c> and the relative does not begin with <c>"/"</c>.
		/// --- For use when <c>new Uri(Uri,Uri)</c> does not return the expected absolute path.
		/// </summary>
		/// <param name="baseUri">The first path to combine: not null.</param>
		/// <param name="relativeUri">The second path to combine: not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <seealso cref="ToCombinePath"/>
		public static Uri UriCombineAbs(this Uri baseUri, Uri relativeUri)
			=> new Uri(
					baseUri?.ToCombinePath(false)
					?? throw new ArgumentNullException(nameof(baseUri)),
					relativeUri.ToCombinePath(true)
					?? throw new ArgumentNullException(nameof(relativeUri)));

		/// <summary>
		/// This method prepares a <see cref="Uri"/> for <see cref="Uri(Uri,Uri)"/>: this checks the
		/// last character in a <c>baseUri</c> or the first character in a <c>relativeUri</c>, and
		/// ensures that a base ends with <c>"/"</c> and a relative does not begin with <c>"/"</c>.
		/// --- For use when <c>new Uri(Uri,Uri)</c> does not return the expected absolute path.
		/// </summary>
		/// <param name="uri">An absolute or relative <see cref="Uri"/>.</param>
		/// <param name="isRelative">Defaults to <c>null</c>: specifies the type of the given <c>uri</c>.
		/// If <c>null</c>, then the method uses the return value of <see cref="Uri.IsAbsoluteUri"/> to
		/// determine whether to check the last character or the first. Otherwise the argument specifies
		/// the intended type of the <c>uri</c>.</param>
		/// <returns>Not null.</returns>
		/// <seealso cref="UriCombineAbs"/>
		public static Uri ToCombinePath(this Uri uri, bool? isRelative = null)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));
			string s = uri.ToString();
			if (string.Empty.Equals(s))
				return uri;
			return (!isRelative ?? uri.IsAbsoluteUri)
					? ('/'.Equals(s[s.Length - 1])
							? uri
							: new Uri(s + "/", UriKind.Absolute))
					: ('/'.Equals(s[0])
							? new Uri(s.Substring(1), UriKind.Relative)
							: uri);
		}

		/// <summary>
		/// Builds a URI query string from the key/value pairs:
		/// "<c>?k=v&amp;k2=v2&amp;k3=</c>". Notice that any
		/// null or whitespace Key is skipped. Values are first converted with
		/// <see cref="Convert.ToString(object)"/>. An empty list will return "?".
		/// </summary>
		/// <param name="parameters">Not null.</param>
		/// <param name="urlEncodeKeys">If true --- the default --- then each key is passed through
		/// <see cref="WebUtility.UrlEncode"/>.</param>
		/// <param name="urlEncodeValues">If true --- the default --- then each Value is passed through
		/// <see cref="WebUtility.UrlEncode"/>.</param>
		/// <param name="emitQuestionMark">Can be set false to omit the question mark.
		/// If false the query will not begin wih "&".</param>
		/// <param name="valueSelector">Optional: if null, <see cref="Convert.ToString(object)"/>
		/// is used.</param>
		/// <returns>Not null, whitespace, or empty.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="parameters"/>
		/// is null.</exception>
		public static string AsQueryString(
				IEnumerable<(string key, object value)> parameters,
				bool urlEncodeKeys = true,
				bool urlEncodeValues = true,
				bool emitQuestionMark = true,
				Func<object, string> valueSelector = null)
		{
			if (parameters == null)
				throw new ArgumentNullException(nameof(parameters));
			if (valueSelector == null)
				valueSelector = Convert.ToString;
			StringBuilder sb = new StringBuilder(
					emitQuestionMark
							? "?"
							: string.Empty);
			bool isFirstArg = true;
			foreach ((string key, object value) in parameters.Where(kv => !string.IsNullOrWhiteSpace(kv.key))) {
				if (isFirstArg)
					isFirstArg = false;
				else
					sb.Append("&");
				sb.Append(
						urlEncodeKeys
								? WebUtility.UrlEncode(key)
								: key);
				sb.Append("=");
				sb.Append(
						value == null
								? ""
								: urlEncodeValues
										? WebUtility.UrlEncode(Convert.ToString(value))
										: valueSelector(value));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Appends a URI with query string from the key/value pairs: "?k=v&amp;k2=v2&amp;k3=".
		/// Notice that any null or whitespace Key is skipped. Values are first converted with
		/// <see cref="Convert.ToString(object)"/>. An empty list will return "?".
		/// If the <paramref name="root"/> contains "?" then it will not be added;
		/// and otherwise it is appended first.
		/// If the <paramref name="root"/> ends with "&" then that is not appended twice.
		/// </summary>
		/// <param name="root">CAN be null.</param>
		/// <param name="parameters">Not null.</param>
		/// <param name="urlEncodeKeys">If true --- the default --- then each key is passed through
		/// <see cref="WebUtility.UrlEncode"/>.</param>
		/// <param name="urlEncodeValues">If true --- the default --- then each Value is passed through
		/// <see cref="WebUtility.UrlEncode"/>.</param>
		/// <param name="valueSelector">Optional: if null, <see cref="Convert.ToString(object)"/>
		/// is used.</param>
		/// <returns>Not null, whitespace, or empty.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="parameters"/>
		/// is null.</exception>
		public static string AppendQueryString(
				string root,
				IEnumerable<(string key, object value)> parameters,
				bool urlEncodeKeys = true,
				bool urlEncodeValues = true,
				Func<object, string> valueSelector = null)
		{
			if (root == null)
				root = string.Empty;
			return !root.Contains("?")
					? root
					+ UriHelper.AsQueryString(
							parameters,
							urlEncodeKeys,
							urlEncodeValues,
							true,
							valueSelector)
					: root.EndsWith("&")
							? root
							+ UriHelper.AsQueryString(
									parameters,
									urlEncodeKeys,
									urlEncodeValues,
									false,
									valueSelector)
							: root
							+ "&"
							+ UriHelper.AsQueryString(
									parameters,
									urlEncodeKeys,
									urlEncodeValues,
									false,
									valueSelector);
		}
	}
}
