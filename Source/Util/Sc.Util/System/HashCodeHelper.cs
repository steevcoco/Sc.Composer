using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace Sc.Util.System
{
	/// <summary>
	/// Static methods that allow easy implementation of hashCode. Example usage:
	/// <code>
	/// public override int GetHashCode()
	///     => HashCodeHelper.Seed
	///         .Hash(primitiveField)
	///         .Hsh(objectField)
	///         .Hash(iEnumerableField);
	/// </code>
	/// </summary>
	public static class HashCodeHelper
	{
		/// <summary>
		/// An initial value for a hashCode, to which is added contributions from fields.
		/// Using a non-zero value decreases collisions of hashCode values.
		/// </summary>
		public const int Seed = 23;

		private const int oddPrimeNumber = 37;


		/// <summary>
		/// Rotates the seed against a prime number.
		/// </summary>
		/// <param name="aSeed">The hash's first term.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int rotateFirstTerm(int aSeed)
		{
			unchecked {
				return HashCodeHelper.oddPrimeNumber * aSeed;
			}
		}


		/// <summary>
		/// Contributes a boolean to the developing HashCode seed.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aBoolean">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, bool aBoolean)
		{
			unchecked {
				return HashCodeHelper.rotateFirstTerm(aSeed)
						+ (aBoolean
								? 1
								: 0);
			}
		}

		/// <summary>
		/// Contributes a char to the developing HashCode seed.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aChar">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, char aChar)
		{
			unchecked {
				return HashCodeHelper.rotateFirstTerm(aSeed)
						+ aChar;
			}
		}

		/// <summary>
		/// Contributes an int to the developing HashCode seed.
		/// Note that byte and short are handled by this method, through implicit conversion.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aInt">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, int aInt)
		{
			unchecked {
				return HashCodeHelper.rotateFirstTerm(aSeed)
						+ aInt;
			}
		}

		/// <summary>
		/// Contributes a long to the developing HashCode seed.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aLong">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, long aLong)
		{
			unchecked {
				return HashCodeHelper.rotateFirstTerm(aSeed)
						+ (int)(aLong ^ (aLong >> 32));
			}
		}

		/// <summary>
		/// Contributes a float to the developing HashCode seed.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aFloat">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, float aFloat)
		{
			unchecked {
				return HashCodeHelper.rotateFirstTerm(aSeed)
						+ Convert.ToInt32(aFloat);
			}
		}

		/// <summary>
		/// Contributes a double to the developing HashCode seed.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aDouble">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, double aDouble)
			=> aSeed.Hash(Convert.ToInt64(aDouble));

		/// <summary>
		/// Contributes a string to the developing HashCode seed.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aString">The value to contribute.</param>
		/// <param name="stringComparison">Optional comparison that creates the hash.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(
				this int aSeed,
				string aString,
				StringComparison stringComparison = StringComparison.Ordinal)
		{
			if (aString == null)
				return aSeed.Hash(0);
			switch (stringComparison) {
				case StringComparison.CurrentCulture :
					return StringComparer.CurrentCulture.GetHashCode(aString);
				case StringComparison.CurrentCultureIgnoreCase :
					return StringComparer.CurrentCultureIgnoreCase.GetHashCode(aString);
				case StringComparison.InvariantCulture :
					return StringComparer.InvariantCulture.GetHashCode(aString);
				case StringComparison.InvariantCultureIgnoreCase :
					return StringComparer.InvariantCultureIgnoreCase.GetHashCode(aString);
				case StringComparison.OrdinalIgnoreCase :
					return StringComparer.OrdinalIgnoreCase.GetHashCode(aString);
				default :
					return StringComparer.Ordinal.GetHashCode(aString);
			}
		}

		/// <summary>
		/// Contributes a possibly-null array to the developing HashCode seed.
		/// Each element may be a primitive, a reference, or a possibly-null array.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aArray">CAN be null.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, IEnumerable aArray)
		{
			if (aArray == null)
				return aSeed.Hash(0);
			int countPlusOne = 1; // So it differs from null
			foreach (object item in aArray) {
				++countPlusOne;
				if (item is IEnumerable arrayItem) {
					if (!object.ReferenceEquals(aArray, arrayItem))
						aSeed = aSeed.Hash(arrayItem); // recursive call!
				} else
					aSeed = aSeed.Hash(item);
			}
			return aSeed.Hash(countPlusOne);
		}

		/// <summary>
		/// Contributes a possibly-null array to the developing HashCode seed.
		/// You must provide the hash function for each element.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aArray">CAN be null.</param>
		/// <param name="hashElement">Required: yields the hash for each element
		/// in <paramref name="aArray"/>.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash<T>(this int aSeed, IEnumerable<T> aArray, Func<T, int> hashElement)
		{
			if (aArray == null)
				return aSeed.Hash(0);
			int countPlusOne = 1; // So it differs from null
			foreach (T item in aArray) {
				++countPlusOne;
				aSeed = aSeed.Hash(hashElement(item));
			}
			return aSeed.Hash(countPlusOne);
		}

		/// <summary>
		/// Contributes a possibly-null object to the developing HashCode seed.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="aObject">CAN be null.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Hash(this int aSeed, object aObject)
		{
			switch (aObject) {
				case null :
					return aSeed.Hash(0);
				case bool b :
					return aSeed.Hash(b);
				case char c :
					return aSeed.Hash(c);
				case int i :
					return aSeed.Hash(i);
				case long l :
					return aSeed.Hash(l);
				case float f :
					return aSeed.Hash(f);
				case double d :
					return aSeed.Hash(d);
				case string s :
					return aSeed.Hash(s);
				case IEnumerable iEnumerable :
					return aSeed.Hash(iEnumerable);
			}
			return aSeed.Hash(aObject.GetHashCode());
		}


		/// <summary>
		/// This utility method uses reflection to iterate all specified properties that are readable
		/// on the given object, excluding any property names given in the params arguments, and
		/// generates a hashcode.
		/// </summary>
		/// <param name="aSeed">The developing hash code, or the seed: if you have no seed, use
		/// the <see cref="Seed"/>.</param>
		/// <param name="aObject">CAN be null.</param>
		/// <param name="propertySelector"><see cref="BindingFlags"/> to select the properties to hash.</param>
		/// <param name="ignorePropertyNames">Optional.</param>
		/// <returns>A hash from the properties contributed to <c>aSeed</c>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int HashAllProperties(
				this int aSeed,
				object aObject,
				BindingFlags propertySelector
						= BindingFlags.Instance
						| BindingFlags.Public,
				params string[] ignorePropertyNames)
		{
			if (aObject == null)
				return aSeed.Hash(0);
			if ((ignorePropertyNames != null)
					&& (ignorePropertyNames.Length != 0)) {
				foreach (PropertyInfo propertyInfo in aObject.GetType()
						.GetProperties(propertySelector)) {
					if (!propertyInfo.CanRead
							|| (Array.IndexOf(ignorePropertyNames, propertyInfo.Name) >= 0))
						continue;
					aSeed = aSeed.Hash(propertyInfo.GetValue(aObject));
				}
			} else {
				foreach (PropertyInfo propertyInfo in aObject.GetType()
						.GetProperties(propertySelector)) {
					if (propertyInfo.CanRead)
						aSeed = aSeed.Hash(propertyInfo.GetValue(aObject));
				}
			}
			return aSeed;
		}


		/// <summary>
		/// NOTICE: this method is provided to contribute a <see cref="KeyValuePair{TKey,TValue}"/> to
		/// the developing HashCode seed; by hashing the key and the value independently. HOWEVER,
		/// this method has a different name since it will not be automatically invoked by
		/// <see cref="Hash(int,object)"/>, <see cref="Hash(int,IEnumerable)"/>,
		/// or <see cref="HashAllProperties"/> --- you MUST NOT mix this method with those unless
		/// you are sure that no KeyValuePair instances will be passed to those methods; or otherwise
		/// the generated hash code will not be consistent. This method itself ALSO will not invoke
		/// this method on the Key or Value here if that itself is a KeyValuePair.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="keyValuePair">The value to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int HashKeyAndValue<TKey, TValue>(this int aSeed, KeyValuePair<TKey, TValue> keyValuePair)
			=> aSeed.Hash(keyValuePair.Key)
					.Hash(keyValuePair.Value);

		/// <summary>
		/// NOTICE: this method is provided to contribute a collection of <see cref="KeyValuePair{TKey,TValue}"/>
		/// to the developing HashCode seed; by hashing the key and the value independently. HOWEVER,
		/// this method has a different name since it will not be automatically invoked by
		/// <see cref="Hash(int,object)"/>, <see cref="Hash(int,IEnumerable)"/>,
		/// or <see cref="HashAllProperties"/> --- you MUST NOT mix this method with those unless
		/// you are sure that no KeyValuePair instances will be passed to those methods; or otherwise
		/// the generated hash code will not be consistent. This method itself ALSO will not invoke
		/// this method on a Key or Value here if that itself is a KeyValuePair or an Enumerable of
		/// KeyValuePair.
		/// </summary>
		/// <param name="aSeed">The developing HashCode value or seed.</param>
		/// <param name="keyValuePairs">The values to contribute.</param>
		/// <returns>The new hash code.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int HashKeysAndValues<TKey, TValue>(
				this int aSeed,
				IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
		{
			if (keyValuePairs == null)
				return aSeed.Hash(null);
			foreach (KeyValuePair<TKey, TValue> keyValuePair in keyValuePairs) {
				aSeed = aSeed.HashKeyAndValue(keyValuePair);
			}
			return aSeed;
		}
	}
}
