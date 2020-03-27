using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;


namespace Sc.Util.System
{
	/// <summary>
	/// Implements a 96-bit hash code, that provides three <see cref="int"/>
	/// values as <see cref="Msb"/>, <see cref="MiddleSb"/>, and
	/// <see cref="Lsb"/>. Serializable, BUT mutable. Note that this
	/// is a mutable struct. Usage:
	/// <code>
	/// HashCode96 myHashCode96
	///     = new HashCode96()
	///             .Hash(aInt)
	///             .Hash(aString, StringComparison.Ordinal)
	///             .Hash(aObject);
	/// </code>
	/// The hash code is formed by contributing each provided value as
	/// an <see cref="int"/> hash code to the Lsb, MiddleSb, and Msb
	/// in a rotating order. Each int contribution is performed by
	/// <see cref="HashCodeHelper"/>.
	/// </summary>
	[DataContract]
	public struct HashCode96
			: IEquatable<HashCode96>
	{
		[DataMember(Name = nameof(HashCode96.Msb))]
		private int msb;

		[DataMember(Name = nameof(HashCode96.MiddleSb))]
		private int middleSb;

		[DataMember(Name = nameof(HashCode96.Lsb))]
		private int lsb;

		[DataMember]
		private bool? islsb;

		[DataMember]
		private bool hasHashed;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void checkHasHashed()
		{
			if (hasHashed)
				return;
			lsb = middleSb = msb = HashCodeHelper.Seed;
			islsb = true;
			hasHashed = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int getNextHash()
		{
			checkHasHashed();
			switch (islsb) {
				case true:
					return lsb;
				case null:
					return middleSb;
				default:
					return msb;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private HashCode96 setNextHash(int newValue)
		{
			switch (islsb) {
				case true:
					lsb = newValue;
					islsb = null;
					break;
				case null:
					middleSb = newValue;
					islsb = false;
					break;
				default:
					msb = newValue;
					islsb = true;
					break;
			}
			return this;
		}


		private StringBuilder toHexString(string separator, bool padTo8Chars, bool prependZeroX)
		{
			if (separator == null)
				separator = string.Empty;
			StringBuilder sb = new StringBuilder(26 + separator.Length + separator.Length);
			if (prependZeroX)
				sb.Append("0x");
			if (padTo8Chars) {
				sb.Append(Convert.ToString(Msb, 16).PadLeft(8, '0'))
						.Append(separator)
						.Append(Convert.ToString(MiddleSb, 16).PadLeft(8, '0'))
						.Append(separator)
						.Append(Convert.ToString(Lsb, 16).PadLeft(8, '0'));
			} else {
				sb.Append(Convert.ToString(Msb, 16))
						.Append(separator)
						.Append(Convert.ToString(MiddleSb, 16))
						.Append(separator)
						.Append(Convert.ToString(Lsb, 16));
			}
			return sb;
		}


		/// <summary>
		/// Holds the highest-order <see cref="int"/>.
		/// Updated last.
		/// </summary>
		public int Msb
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				checkHasHashed();
				return msb;
			}
		}

		/// <summary>
		/// Holds the middle <see cref="int"/>.
		/// updated after the <see cref="Lsb"/>.
		/// </summary>
		public int MiddleSb
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				checkHasHashed();
				return middleSb;
			}
		}

		/// <summary>
		/// Holds the lowest-order <see cref="int"/>.
		/// Updated first.
		/// </summary>
		public int Lsb
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				checkHasHashed();
				return lsb;
			}
		}


		/// <summary>
		/// Contributes a boolean to the developing HashCode.
		/// </summary>
		/// <param name="aBoolean">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(bool aBoolean)
			=> setNextHash(getNextHash().Hash(aBoolean));

		/// <summary>
		/// Contributes a char to the developing HashCode.
		/// </summary>
		/// <param name="aChar">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(char aChar)
			=> setNextHash(getNextHash().Hash(aChar));

		/// <summary>
		/// Contributes an int to the developing HashCode.
		/// Note that byte and short are handled by this method, through implicit conversion.
		/// </summary>
		/// <param name="aInt">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(int aInt)
			=> setNextHash(getNextHash().Hash(aInt));

		/// <summary>
		/// Contributes a long to the developing HashCode.
		/// </summary>
		/// <param name="aLong">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(long aLong)
			=> setNextHash(getNextHash().Hash(aLong));

		/// <summary>
		/// Contributes a float to the developing HashCode.
		/// </summary>
		/// <param name="aFloat">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(float aFloat)
			=> setNextHash(getNextHash().Hash(aFloat));

		/// <summary>
		/// Contributes a double to the developing HashCode.
		/// </summary>
		/// <param name="aDouble">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(double aDouble)
			=> setNextHash(getNextHash().Hash(aDouble));

		/// <summary>
		/// Contributes a string to the developing HashCode.
		/// </summary>
		/// <param name="aString">The value to contribute.</param>
		/// <param name="stringComparison">Optional comparison that creates the hash.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(
				string aString,
				StringComparison stringComparison = StringComparison.Ordinal)
			=> setNextHash(getNextHash().Hash(aString, stringComparison));

		/// <summary>
		/// Contributes a possibly-null array to the developing HashCode.
		/// Each element may be a primitive, a reference, or a possibly-null array.
		/// </summary>
		/// <param name="aArray">CAN be null.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(IEnumerable aArray)
			=> setNextHash(getNextHash().Hash(aArray));

		/// <summary>
		/// Contributes a possibly-null array to the developing HashCode.
		/// You must provide the hash function for each element.
		/// </summary>
		/// <param name="aArray">CAN be null.</param>
		/// <param name="hashElement">Required: yields the hash for each element
		/// in <paramref name="aArray"/>.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash<T>(IEnumerable<T> aArray, Func<T, int> hashElement)
			=> setNextHash(getNextHash().Hash(aArray, hashElement));

		/// <summary>
		/// Contributes a possibly-null object to the developing HashCode.
		/// </summary>
		/// <param name="aObject">CAN be null.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 Hash(object aObject)
			=> setNextHash(getNextHash().Hash(aObject));


		/// <summary>
		/// This utility method uses reflection to iterate all specified properties that are readable
		/// on the given object, excluding any property names given in the params arguments, and
		/// contributes to the hashcode.
		/// </summary>
		/// <param name="aObject">CAN be null.</param>
		/// <param name="propertySelector"><see cref="BindingFlags"/> to select the properties to hash.</param>
		/// <param name="ignorePropertyNames">Optional.</param>
		/// <returns>A hash from the properties contributed to <c>aSeed</c>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 HashAllProperties(
				object aObject,
				BindingFlags propertySelector
						= BindingFlags.Instance
						| BindingFlags.Public
						| BindingFlags.GetProperty,
				params string[] ignorePropertyNames)
			=> setNextHash(getNextHash().HashAllProperties(aObject, propertySelector, ignorePropertyNames));


		/// <summary>
		/// NOTICE: this method is provided to contribute a <see cref="KeyValuePair{TKey,TValue}"/> to
		/// the developing HashCode; by hashing the key and the value independently. HOWEVER,
		/// this method has a different name since it will not be automatically invoked by
		/// <see cref="Hash(int,object)"/>, <see cref="Hash(int,IEnumerable)"/>,
		/// or <see cref="HashAllProperties"/> --- you MUST NOT mix this method with those unless
		/// you are sure that no KeyValuePair instances will be passed to those methods; or otherwise
		/// the generated hash code will not be consistent. This method itself ALSO will not invoke
		/// this method on the Key or Value here if that itself is a KeyValuePair.
		/// </summary>
		/// <param name="keyValuePair">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 HashKeyAndValue<TKey, TValue>(KeyValuePair<TKey, TValue> keyValuePair)
			=> setNextHash(getNextHash().HashKeyAndValue(keyValuePair));

		/// <summary>
		/// NOTICE: this method is provided to contribute a collection of <see cref="KeyValuePair{TKey,TValue}"/>
		/// to the developing HashCode; by hashing the key and the value independently. HOWEVER,
		/// this method has a different name since it will not be automatically invoked by
		/// <see cref="Hash(int,object)"/>, <see cref="Hash(int,IEnumerable)"/>,
		/// or <see cref="HashAllProperties"/> --- you MUST NOT mix this method with those unless
		/// you are sure that no KeyValuePair instances will be passed to those methods; or otherwise
		/// the generated hash code will not be consistent. This method itself ALSO will not invoke
		/// this method on a Key or Value here if that itself is a KeyValuePair or an Enumerable of
		/// KeyValuePair.
		/// </summary>
		/// <param name="keyValuePairs">The values to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashCode96 HashKeysAndValues<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
			=> setNextHash(getNextHash().HashKeysAndValues(keyValuePairs));


		public override int GetHashCode()
		{
			checkHasHashed();
			return HashCodeHelper.Seed
					.Hash(Lsb)
					.Hash(MiddleSb)
					.Hash(Msb);
		}

		public override bool Equals(object obj)
			=> (obj is HashCode96 other)
					&& Equals(other);

		public bool Equals(HashCode96 other)
		{
			checkHasHashed();
			other.checkHasHashed();
			return (Lsb == other.Lsb)
					&& (MiddleSb == other.MiddleSb)
					&& (Msb == other.Msb);
		}


		/// <summary>
		/// Converts this instance to a Hex string, of the form:
		/// <c>Msb,MiddleSb,Lsb</c>.
		/// </summary>
		/// <param name="separator">Optional separator that will be inserted
		/// between each pair of ints. If null, no separator is used.</param>
		/// <param name="padTo8Chars">Defaults to true: each int is padded
		/// to eight Hex characters. If set false, each int outputs only
		/// the used Hex digits for that int.</param>
		/// <param name="prependZeroX">Defaults to false. If set true, the
		/// Hex output is prepended with "0x" (or "0X").</param>
		/// <param name="toUpperCase">Defaults to false. If set true, the
		/// result is converted to upper case.</param>
		/// <returns>Not null.</returns>
		public string ToHexString(
				string separator = null,
				bool padTo8Chars = true,
				bool prependZeroX = false,
				bool toUpperCase = false)
		{
			checkHasHashed();
			StringBuilder sb = toHexString(separator, padTo8Chars, prependZeroX);
			return toUpperCase
					? sb.ToString().ToUpperInvariant()
					: sb.ToString();
		}


		public override string ToString()
		{
			checkHasHashed();
			StringBuilder sb = toHexString("-", true, false);
			sb.Insert(0, '[');
			sb.Insert(0, nameof(HashCode96));
			return sb.Append(']').ToString();
		}
	}
}
