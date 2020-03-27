using System;


namespace Sc.Util.System
{
	/// <summary>
	/// Static helpers for Arrays.
	/// </summary>
	public static class ArrayHelper
	{
		/// <summary>
		/// Convenience method returns a new array from the <see langword="params"/> array.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="elements">Params array.</param>
		/// <returns>The <paramref name="elements"/> argument copied to a new array.</returns>
		public static T[] ToArray<T>(params T[] elements)
		{
			T[] result = new T[elements.Length];
			Array.Copy(elements, result, result.Length);
			return result;
		}

		/// <summary>
		/// Convenience method creates a new array with the single given
		/// <paramref name="element"/>.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="element">CAN be null.</param>
		/// <returns>Not null.</returns>
		public static T[] ToArray<T>(T element)
			=> new[] { element };


		/// <summary>
		/// Returns true if this <paramref name="array"/> is null or zero length.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="array">Can be null.</param>
		/// <returns>True if this <paramref name="array"/> is null or
		/// zero length.</returns>
		public static bool IsNullOrEmpty<T>(this T[] array)
			=> (array == null) || (array.Length == 0);

		/// <summary>
		/// Returns true if this <paramref name="array"/> is null or zero length.
		/// </summary>
		/// <param name="array">Can be null.</param>
		/// <returns>True if this <paramref name="array"/> is null or
		/// zero length.</returns>
		public static bool IsNullOrEmpty(this Array array)
			=> (array == null) || (array.Length == 0);


		/// <summary>
		/// Creates a new array of length <paramref name="length"/>, and
		/// copies the elements from this <paramref name="array"/> to
		/// the new array, starting at the <paramref name="offset"/>
		/// within this <paramref name="array"/>.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="array">Not null.</param>
		/// <param name="offset">Offset within <paramref name="array"/>
		/// to begin the slice.</param>
		/// <param name="length">Length of the slice to create.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static T[] GetSlice<T>(this T[] array, int offset, int length)
		{
			if (array is null)
				throw new ArgumentNullException(nameof(array));
			if ((offset < 0)
					|| (offset >= array.Length)) {
				throw new ArgumentOutOfRangeException(nameof(offset), offset, $@"[0, {array.Length}).");
			}
			if ((length < 0)
					|| (length > (array.Length - offset))) {
				throw new ArgumentOutOfRangeException(nameof(length), length, $@"[0, {array.Length - offset}).");
			}
			T[] slice = new T[length];
			Array.Copy(array, offset, slice, 0, length);
			return slice;
		}


		/// <summary>
		/// Swaps the values at the two given indexes in this Array.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="array">Not null; and not empty.</param>
		/// <param name="indexX">The index to swap with the second.</param>
		/// <param name="indexY">The index to swap with the first.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">The array is empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static void Swap<T>(this T[] array, int indexX, int indexY)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (array.Length == 0)
				throw new ArgumentException($@"{nameof(Array.Length)}=0", nameof(array));
			if ((indexX < 0)
					|| (indexX >= array.Length)) {
				throw new ArgumentOutOfRangeException(nameof(array), indexX, $@"[0,{array.Length - 1}]");
			}
			if ((indexY < 0)
					|| (indexY >= array.Length)) {
				throw new ArgumentOutOfRangeException(nameof(array), indexY, $@"[0,{array.Length - 1}]");
			}
			if (indexX == indexY)
				return;
			T y = array[indexY];
			array[indexY] = array[indexX];
			array[indexX] = y;
		}


		/// <summary>
		/// Moves the value at the <paramref name="index"/> in this Array
		/// to the <paramref name="destination"/> index; and shifts all elements
		/// between the indexes forward or back, in order.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="array">Not null; and not empty.</param>
		/// <param name="index">The index to move to the <paramref name="destination"/>.</param>
		/// <param name="destination">The index where the <see cref="index"/> is moved.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">The array is empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static void Move<T>(this T[] array, int index, int destination)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (array.Length == 0)
				throw new ArgumentException($@"{nameof(Array.Length)}=0", nameof(array));
			if ((index < 0)
					|| (index >= array.Length)) {
				throw new ArgumentOutOfRangeException(nameof(array), index, $@"[0,{array.Length - 1}]");
			}
			if ((destination < 0)
					|| (destination >= array.Length)) {
				throw new ArgumentOutOfRangeException(nameof(array), destination, $@"[0,{array.Length - 1}]");
			}
			if (index == destination)
				return;
			T move = array[index];
			if (destination > index) {
				for (int i = index + 1; i <= destination; ++i) {
					array[i - 1] = array[i];
				}
			} else {
				for (int i = index - 1; i >= destination; --i) {
					array[i + 1] = array[i];
				}
			}
			array[destination] = move;
		}


		/// <summary>
		/// Creates a new array, and inserts the <paramref name="element"/> in
		/// the new array at the <paramref name="index"/>; copying all prior
		/// elements, and moving all from the <paramref name="index"/> forward.
		/// Note that if your argument <paramref name="array"/> is null, this
		/// method will create a new array by default.
		/// </summary>
		/// <typeparam name="T">Array element type.</typeparam>
		/// <param name="array">May be null, according to <paramref name="createNewIfNull"/>.</param>
		/// <param name="index">Index at which to insert the <paramref name="element"/>:
		/// this can range from <c>[0,array.Length]</c>.</param>
		/// <param name="element">Note: not checked here: the element to insert.</param>
		/// <param name="createNewIfNull">DEFAULTS to TRUE: if the current
		/// <paramref name="array"/> is null, this method will create a new array.</param>
		/// <returns>Not null</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static T[] Insert<T>(this T[] array, int index, T element, bool createNewIfNull = true)
		{
			if ((array == null)
					&& !createNewIfNull) {
				throw new ArgumentNullException(nameof(array));
			}
			if ((index < 0)
					|| (index > (array?.Length ?? 0))) {
				throw new ArgumentOutOfRangeException(
						nameof(index),
						index,
						$@"{nameof(Array.Length)}={array?.Length.ToString() ?? Convert.ToString(null)}");
			}
			if ((array == null)
					|| (array.Length == 0)) {
				return new[] { element };
			}
			T[] newArray = new T[array.Length + 1];
			if (index > 0)
				Array.Copy(array, 0, newArray, 0, index);
			if (index < array.Length)
				Array.Copy(array, index, newArray, index + 1, array.Length - index);
			newArray[index] = element;
			return newArray;
		}

		/// <summary>
		/// Creates a new array, and appends the <paramref name="element"/> at the end
		/// of the new array; copying all prior elements.
		/// Note that if your argument <paramref name="array"/> is null, this
		/// method will create a new array by default.
		/// </summary>
		/// <typeparam name="T">Array element type.</typeparam>
		/// <param name="array">May be null, according to <paramref name="createNewIfNull"/>.</param>
		/// <param name="element">Note: not checked here: the element to append.</param>
		/// <param name="createNewIfNull">DEFAULTS to TRUE: if the current
		/// <paramref name="array"/> is null, this method will create a new array.</param>
		/// <returns>Not null</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static T[] Append<T>(this T[] array, T element, bool createNewIfNull = true)
			=> array.Insert(array?.Length ?? 0, element, createNewIfNull);
	}
}
