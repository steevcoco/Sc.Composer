using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Sc.Util.System;


namespace Sc.Util.Text
{
	/// <summary>
	/// Static utilities for working with test.
	/// </summary>
	public static class TextHelper
	{
		/// <summary>
		/// Is an encoding with no BOM, and riases exceptions on bad formats.
		/// </summary>
		public static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);


		/// <summary>
		/// Returns a hex string of the data: simply converts each byte as <c>"x2"</c> --- two
		/// hex characters each --- and appends each one. You may specify a separator string,
		/// and groupings.
		/// </summary>
		/// <param name="data">Not null.</param>
		/// <param name="upperCase">Defaults to false: the string format is "<c>x2</c>". If set true,
		/// the format is "<c>X2</c>".</param>
		/// <param name="separator">Optional: defaults to empty. If given, each group, oreach byte,
		/// will be separated this character.</param>
		/// <param name="grouping">Can be used only if the <c>separator</c> is also provided. If
		/// given, the bytes are grouped by the counts given. E.g.
		/// <c>ToHexString(bytes, false, "-", 4, 2, 2, 2, 6</c> produces output like a <see cref="Guid"/>:
		/// "<c>936da01f-9abd-4d9d-80c7-02af85c822a8</c>".</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string ToHexString(
				this byte[] data,
				bool upperCase = false,
				string separator = "",
				params int[] grouping)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			StringBuilder sb;
			if (string.IsNullOrWhiteSpace(separator)) {
				separator = upperCase
						? "X2"
						: "x2";
				sb = new StringBuilder(data.Length * 2);
				foreach (byte b in data) {
					sb.Append(b.ToString(separator));
				}
			} else {
				string format = upperCase
						? "X2"
						: "x2";
				sb = new StringBuilder(data.Length * (separator.Length + 2));
				if ((grouping == null)
						|| (grouping.Length == 0)) {
					sb.Append(
							data[0]
									.ToString(format));
					for (int i = 1; i < data.Length; ++i) {
						sb.Append(separator)
								.Append(
										data[i]
												.ToString(format));
					}
				} else {
					sb.Append(
							data[0]
									.ToString(format));
					int i = 0;
					foreach (int group in grouping) {
						if (i >= data.Length)
							break;
						if (sb.Length != 0)
							sb.Append(separator);
						for (int j = 0; j < @group; ++j) {
							sb.Append(
									data[i++]
											.ToString(format));
							if (i >= data.Length)
								break;
						}
					}
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Intended to provide the opposite function for <see cref="ToHexString"/>, this method
		/// simply iterates the given characters, and skips any that are not Hex characters, and
		/// skips any that are not a pair of Hex characters. For all paris of Hex characters found,
		/// a byte is parsed and added to the result.
		/// </summary>
		/// <param name="hexString">Not null.</param>
		/// <returns>Not null.</returns>
		public static byte[] FromHexString(string hexString)
		{
			if (hexString == null)
				throw new ArgumentNullException(nameof(hexString));
			char[] hex
					=
					{
						'0',
						'1',
						'2',
						'3',
						'4',
						'5',
						'6',
						'7',
						'8',
						'9',
						'a',
						'b',
						'c',
						'd',
						'e',
						'f',
						'A',
						'B',
						'C',
						'D',
						'E',
						'F'
					};
			List<byte> bytes = new List<byte>(hexString.Length / 2);
			for (int i = 0; i < (hexString.Length - 1); ++i) {
				if (Array.IndexOf(hex, hexString[i]) < 0)
					continue;
				if (Array.IndexOf(hex, hexString[i + 1]) < 0) {
					++i;
					continue;
				}
				bytes.Add((byte)int.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber));
			}
			return bytes.ToArray();
		}


		/// <summary>
		/// Creates a json-like string, that will print the properties
		/// returned from a new TypeDescriptor for the argument;
		/// and will begin with the object's Type Name.
		/// Each property goes on its own line.
		/// The first and last brace are each on their own line.
		/// Each line, including the first and last, will begin at least with the
		/// number of indents, as four spaces per indent.
		/// Each property goes on a new line, is indented
		/// one further indent, and ends with a comma.
		/// The property names, and values are surrounded with quotes.
		/// The property names are followed with a colon.
		/// The whole string does not end with a new line.
		/// </summary>
		/// <param name="obj">Not null. The Type Name will be output first.</param>
		/// <param name="target">Optional. All text begins to append immediately here.</param>
		/// <param name="indent">Number of indents for the braces: property pairs are indented
		/// another indent from this. Will be coerced to a non-negative value.</param>
		/// <param name="getPropertyKeyValuePair">Optional func that generates each
		/// <see cref="KeyValuePair{TKey,TValue}"/> that will be printed for each property.
		/// The first argument is this <paramref name="obj"/>. The current property's
		/// <see cref="PropertyDescriptor"/> is given. Must return the printed property
		/// name and the value. The default implementation returns
		/// <see cref="MemberDescriptor.Name"/>,
		/// <see cref="PropertyDescriptor.GetValue(object)"/>.<see cref="object.ToString"/>.</param>
		/// <returns>The argument after appending.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static StringBuilder Stringify(
				object obj,
				StringBuilder target = null,
				int indent = 0,
				Func<object, PropertyDescriptor, KeyValuePair<string, string>> getPropertyKeyValuePair = null)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			KeyValuePair<string, string> DefaultKeyValuePair(object o, PropertyDescriptor descriptor)
				=> new KeyValuePair<string, string>(
						descriptor.Name,
						descriptor.GetValue(obj)
								?.ToString()
						?? Convert.ToString(null));
			if (getPropertyKeyValuePair == null)
				getPropertyKeyValuePair = DefaultKeyValuePair;
			return TextHelper.Stringify(
					obj.GetType().GetFriendlyName(),
					TypeDescriptor.GetProperties(obj)
							.OfType<PropertyDescriptor>()
							.Select(propertyDescriptor => getPropertyKeyValuePair(obj, propertyDescriptor))
							.ToArray(),
					target,
					indent);
		}

		/// <summary>
		/// Creates a json-like string, where each given property on its own line.
		/// The first and last brace are each on their own line.
		/// Each line, including the first and last, will begin at least with the
		/// number of indents, as four spaces per indent.
		/// Each property goes on a new line, is indented
		/// one further indent, and ends with a comma.
		/// The property names, and values are surrounded with quotes.
		/// The property names are followed with a colon.
		/// The whole string does not end with a new line.
		/// </summary>
		/// <param name="properties">Not null; may be empty.</param>
		/// <param name="target">Optional. All text begins to append immediately here.</param>
		/// <param name="indent">Number of indents for the braces: property pairs are indented
		/// another indent from this. Will be coerced to a non-negative value.</param>
		/// <returns>The argument after appending.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static StringBuilder Stringify(
				IEnumerable<KeyValuePair<string, string>> properties,
				StringBuilder target = null,
				int indent = 0)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));
			if (target == null)
				target = new StringBuilder();
			indent = Math.Max(0, indent);
			target.AppendIndent(indent);
			target.AppendLine("{");
			bool hasProperty = false;
			foreach (KeyValuePair<string, string> kv in properties) {
				hasProperty = true;
				target.AppendIndent(indent + 1)
						.Append('"')
						.Append(kv.Key)
						.Append("\": \"")
						.Append(kv.Value)
						.AppendLine("\",");
			}
			if (hasProperty)
				target.AppendIndent(indent);
			else
				target.Append(' ');
			return target.Append('}');
		}

		/// <summary>
		/// Creates a json-like string, that can begin with an "object name",
		/// and then each given property on its own line. The object name,
		/// first, and last brace are each on their own line.
		/// Each line, including the first and last, will begin at least with the
		/// number of indents, as four spaces per indent.
		/// Each property goes on a new line, is indented
		/// one further indent, and ends with a comma.
		/// The object name, property names, and values are surrounded with quotes.
		/// The object name and property names are followed with a colon.
		/// The whole string does not end with a new line.
		/// </summary>
		/// <param name="objectOrClassName">Is an object name or class name that is appended first to
		/// the target; and if the properties are empty, the braces will be placed on one line with
		/// this name. NOTICE: CAN be null or empty.</param>
		/// <param name="properties">Not null; may be empty.</param>
		/// <param name="target">Optional. All text begins to append immediately here.</param>
		/// <param name="indent">Number of indents for the braces: property pairs are indented
		/// another indent from this. Will be coerced to a non-negative value.</param>
		/// <returns>The argument after appending.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static StringBuilder Stringify(
				string objectOrClassName,
				IReadOnlyCollection<KeyValuePair<string, string>> properties,
				StringBuilder target = null,
				int indent = 0)
		{
			if (properties == null)
				throw new ArgumentNullException(nameof(properties));
			if (target == null)
				target = new StringBuilder();
			indent = Math.Max(0, indent);
			target.AppendIndent(indent);
			if (properties.Count == 0) {
				return string.IsNullOrEmpty(objectOrClassName)
						? target.Append("{ }")
						: target.Append($"\"{objectOrClassName}\": {{ }}");
			}
			target.AppendLine($"\"{objectOrClassName}\":");
			return TextHelper.Stringify(properties, target, indent);
		}


		/// <summary>
		/// Appends one <paramref name="indentString"/> per <paramref name="indent"/>
		/// to the argument.
		/// </summary>
		/// <param name="target">Not null.</param>
		/// <param name="indent">Will be coerced to a non-negative value.</param>
		/// <param name="indentString">Defaults to four spaces.</param>
		/// <returns>The argument after appending.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static StringBuilder AppendIndent(this StringBuilder target, int indent, string indentString = "    ")
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (indentString == null)
				throw new ArgumentNullException(nameof(indentString));
			indent = Math.Max(0, indent);
			for (int i = 0; i < indent; ++i) {
				target.Append(indentString);
			}
			return target;
		}

		/// <summary>
		/// Inserts one <paramref name="indentString"/> per <paramref name="indent"/>
		/// at the beginning the argument.
		/// </summary>
		/// <param name="target">Not null.</param>
		/// <param name="indent">Will be coerced to a non-negative value.</param>
		/// <param name="indentString">Defaults to four spaces.</param>
		/// <returns>The argument after appending.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static StringBuilder InsertIndent(this StringBuilder target, int indent, string indentString = "    ")
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			indent = Math.Max(0, indent);
			for (int i = 0; i < indent; ++i) {
				target.Insert(0, indentString);
			}
			return target;
		}

		/// <summary>
		/// Inserts one <paramref name="indentString"/> per <paramref name="indent"/>
		/// at the beginning of each line in the argument. Always
		/// inserts at least one indent.
		/// </summary>
		/// <param name="target">Not null.</param>
		/// <param name="indent">Will be coerced to a non-negative value.</param>
		/// <param name="indentString">Defaults to four spaces.</param>
		/// <returns>The argument after inserting.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static StringBuilder IndentAllLines(this StringBuilder target, int indent, string indentString = "    ")
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			indent = Math.Max(0, indent);
			StringBuilder temp = new StringBuilder().AppendIndent(indent, indentString);
			target.Insert(0, temp.ToString());
			temp.Insert(0, Environment.NewLine);
			target.Replace(Environment.NewLine, temp.ToString());
			return target;
		}

		/// <summary>
		/// Iterates the argument backwards and removes ny character that is
		/// <see cref="char.IsWhiteSpace(char)"/>.
		/// </summary>
		/// <param name="target">Not null.</param>
		/// <returns>The argument after removing.</returns>
		public static StringBuilder RemoveWhitespace(this StringBuilder target)
		{
			for (int i = target.Length - 1; i >= 0; --i) {
				if (char.IsWhiteSpace(target[i]))
					target.Remove(i, 1);
			}
			return target;
		}


		/// <summary>
		/// Returns a new object that will return this delegate's string result
		/// from <see cref="object"/> <see cref="object.ToString"/>.
		/// </summary>
		/// <param name="toString">Will only be invoked in the returned object's
		/// <see cref="object.ToString"/> method.</param>
		/// <returns>Not null.</returns>
		public static DelegateToString ToStringDelegate(this Func<string> toString)
			=> new DelegateToString(toString);


		/// <summary>
		/// Returns a new object that implements <see cref="IReadOnlyList{T}"/>
		/// (and <see cref="IEnumerable{T}"/>) of <see langword="char"/>,
		/// wrapping this <see cref="StringBuilder"/>.
		/// Notice that this object ALSO returns the <see cref="StringBuilder"/>
		/// <see cref="object.ToString"/> result from its <see cref="object.ToString"/>
		/// method. Also implements <see cref="IEquatable{T}"/> and
		/// <see cref="IComparable{T}"/>.
		/// </summary>
		/// <param name="stringBuilder">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IReadOnlyList<char> AsIEnumerable(this StringBuilder stringBuilder)
			=> new EnumerableStringBuilder(stringBuilder);

		/// <summary>
		/// Returns a new object that implements <see cref="IReadOnlyList{T}"/>
		/// of <see langword="char"/> from this <see langword="string"/>.
		/// This object also returns the <see langword="string"/>
		/// from its <see cref="object.ToString"/>
		/// method. Also implements <see cref="IEquatable{T}"/> and
		/// <see cref="IComparable{T}"/>.
		/// </summary>
		/// <param name="text">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IReadOnlyList<char> AsReadOnlyList(this string text)
			=> new StringReadOnlyList(text);


		/// <summary>
		/// This method replaces all newlines in the <paramref name="text"/>;
		/// by first replacing all <see cref="Environment.NewLine"/>
		/// strings with the <paramref name="replacement"/>, and then,
		/// replacing all <c>CR/LF</c> pairs, and then replacing all
		/// single <c>CR</c> and single <c>LF</c> characters.
		/// Ensuring that all <c>CR</c> and <c>LF</c> characters
		/// are replaced. Note that the <paramref name="text"/> argument
		/// CAN be null: the method will then return null. Note also that
		/// the <paramref name="replacement"/> argument defaults to
		/// a single space.
		/// </summary>
		/// <param name="text">CAN be null --- this method will return null.</param>
		/// <param name="replacement">The string that replaces EACH
		/// <see cref="Environment.NewLine"/>, then <c>CR/LF</c> pair,
		/// then each single <c>CR</c> and single <c>LF</c> character.
		/// Note that this defaults to a single space character.</param>
		/// <returns>Null if the argument is null; and otherwise not null.</returns>
		/// <exception cref="ArgumentNullException">For <paramref name="replacement"/>.</exception>
		public static string ReplaceAllNewLines(this string text, string replacement = " ")
		{
			if (replacement == null)
				throw new ArgumentNullException(nameof(replacement));
			return text?.Replace(Environment.NewLine, replacement)
					.Replace("\r\n", replacement)
					.Replace("\r", replacement)
					.Replace("\n", replacement);
		}
	}
}
