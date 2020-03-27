using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;


namespace Sc.Util.Text
{
	/// <summary>
	/// A simple class that implements <see cref="IReadOnlyList{T}"/>
	/// of <see langword="char"/> from a <see langword="string"/>.
	/// This class also returns the <see langword="string"/>
	/// from this <see cref="object.ToString"/>
	/// method. Also implements <see cref="IEquatable{T}"/> and
	/// <see cref="IComparable{T}"/>.
	/// </summary>
	public sealed class StringReadOnlyList
			: IReadOnlyList<char>,
					IEquatable<IReadOnlyList<char>>,
					IEquatable<string>,
					IEquatable<StringReadOnlyList>,
					IComparable<string>
	{
		private readonly string text;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="text"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public StringReadOnlyList(string text)
			=> this.text = text ?? throw new ArgumentNullException(nameof(text));


		public IEnumerator<char> GetEnumerator()
			=> text.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> text.GetEnumerator();

		public int Count
			=> text.Length;

		public char this[int index]
			=> text[index];


		public override int GetHashCode()
			=> text.GetHashCode();

		public override bool Equals(object obj)
			=> (obj is IReadOnlyList<char> list && Equals(list))
					|| (obj is StringBuilder sb && Equals(sb))
					|| (obj is EnumerableStringBuilder esb && Equals(esb))
					|| (obj is string s && Equals(s));

		public bool Equals(IReadOnlyList<char> other)
			=> (other != null) && this.SequenceEqual(other);

		public bool Equals(string other)
			=> (other != null) && text.Equals(other);

		public bool Equals(StringReadOnlyList other)
			=> (other != null) && text.Equals(other.text);

		[SuppressMessage("ReSharper", "StringCompareToIsCultureSpecific")]
		public int CompareTo(string other)
			=> text.CompareTo(other);

		public override string ToString()
			=> text;
	}
}
