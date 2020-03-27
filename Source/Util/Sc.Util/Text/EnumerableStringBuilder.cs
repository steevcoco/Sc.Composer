using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;


namespace Sc.Util.Text
{
	/// <summary>
	/// Implements <see cref="IReadOnlyList{T}"/> (and <see cref="IEnumerable{T}"/>)
	/// of <see langword="char"/>, wrapping a <see cref="global::System.Text.StringBuilder"/>.
	/// Notice that this class ALSO returns the <see cref="global::System.Text.StringBuilder"/>
	/// <see cref="object.ToString"/> result from this <see cref="object.ToString"/>
	/// method. Also implements <see cref="IEquatable{T}"/> and
	/// <see cref="IComparable{T}"/>.
	/// </summary>
	public sealed class EnumerableStringBuilder
			: IReadOnlyList<char>,
					IEquatable<IReadOnlyList<char>>,
					IEquatable<StringBuilder>,
					IEquatable<EnumerableStringBuilder>,
					IEquatable<string>,
					IComparable<string>
	{
		/// <summary>
		/// Default constructor creates a new empty <see cref="StringBuilder"/>.
		/// </summary>
		public EnumerableStringBuilder()
			=> StringBuilder = new StringBuilder();

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stringBuilder">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public EnumerableStringBuilder(StringBuilder stringBuilder)
			=> StringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));

		/// <summary>
		/// Constructor creates a new <see cref="StringBuilder"/>
		/// with the given <paramref name="text"/>.
		/// </summary>
		/// <param name="text">CAN be null.</param>
		public EnumerableStringBuilder(string text)
			=> StringBuilder = new StringBuilder(text);


		/// <summary>
		/// This actual <see cref="System.Text.StringBuilder"/>
		/// </summary>
		public StringBuilder StringBuilder { get; }


		public IEnumerator<char> GetEnumerator()
		{
			for (int i = 0; i < StringBuilder.Length; ++i) {
				yield return StringBuilder[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public int Count
			=> StringBuilder.Length;

		public char this[int index]
			=> StringBuilder[index];


		public override int GetHashCode()
			=> StringBuilder.GetHashCode();

		public override bool Equals(object obj)
			=> (obj is IReadOnlyList<char> list && Equals(list))
					|| (obj is StringBuilder sb && Equals(sb))
					|| (obj is EnumerableStringBuilder esb && Equals(esb))
					|| (obj is string s && Equals(s));

		public bool Equals(IReadOnlyList<char> other)
			=> (other != null) && this.SequenceEqual(other);

		public bool Equals(StringBuilder other)
			=> (other != null) && StringBuilder.Equals(other);

		public bool Equals(EnumerableStringBuilder other)
			=> (other != null) && StringBuilder.Equals(other.StringBuilder);

		public bool Equals(string other)
			=> (other != null)
					&& StringBuilder.ToString()
							.Equals(other);

		[SuppressMessage("ReSharper", "StringCompareToIsCultureSpecific")]
		public int CompareTo(string other)
			=> StringBuilder.ToString()
					.CompareTo(other);

		public override string ToString()
			=> StringBuilder.ToString();
	}
}
