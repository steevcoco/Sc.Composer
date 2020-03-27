using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sc.Util.Collections.Collections;


namespace Sc.Util.Collections
{
	/// <summary>
	/// Static utility methods for working with Collections.
	/// </summary>
	public static class CollectionsHelper
	{
		/// <summary>
		/// Returns the string used to raise <see cref="PropertyChangedEventArgs"/> when the
		/// indexer property has changed: <c>"Item[]"</c>.
		/// </summary>
		public static string CollectionIndexerItemPropertyName
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => "Item[]";
		}


		/// <summary>
		/// This convenience method enumerates this <see cref="IReadOnlyList{T}"/> in reverse order;
		/// by enumerating by index in reverse
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<T> EnumerateInReverse<T>(this IReadOnlyList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			for (int i = list.Count - 1; i >= 0; --i) {
				yield return list[i];
			}
		}

		/// <summary>
		/// This convenience method enumerates this <see cref="IList{T}"/> in reverse order;
		/// by enumerating by index in reverse
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<T> EnumerateInReverse<T>(this IList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			for (int i = list.Count - 1; i >= 0; --i) {
				yield return list[i];
			}
		}

		/// <summary>
		/// This convenience method enumerates this <see cref="List{T}"/> in reverse order;
		/// by enumerating by index in reverse
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<T> EnumerateInReverse<T>(this List<T> list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			for (int i = list.Count - 1; i >= 0; --i) {
				yield return list[i];
			}
		}


		/// <summary>
		/// Returns an <see cref="IEnumerable{T}"/> of elements in this
		/// <see cref="IReadOnlyList{T}"/>, that begins at the <paramref name="startIndex"/>,
		/// and returns the given <paramref name="count"/> of elements.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="list">Required: note that this CAN be empty; and in that case,
		/// the <paramref name="startIndex"/> is allowed to be zero.</param>
		/// <param name="startIndex">Must be a valid index; OR ONLY if the
		/// <paramref name="list"/> is empty, can be zero.</param>
		/// <param name="count">Defaults to null: the enumeration begins at the
		/// <paramref name="startIndex"/> and proceeds to the end of the list.
		/// Otherwise can specify the returned count.</param>
		/// <returns>Not null; can be empty.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static IEnumerable<T> Range<T>(this IReadOnlyList<T> list, int startIndex, int? count = null)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (list.Count == 0) {
				if ((startIndex == 0)
						&& (!count.HasValue
								|| (count.Value == 0))) {
					return EnumerableHelper.EmptyEnumerable<T>();
				}
			}
			if ((startIndex < 0)
					|| (startIndex >= list.Count)) {
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, $@"[0,{list.Count - 1}");
			}
			if (!count.HasValue) {
				count = list.Count - startIndex;
			} else if ((count.Value < 0)
					|| (count.Value > (list.Count - startIndex))) {
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, $@"[0,{list.Count - startIndex}");
			}
			return (startIndex == 0)
					&& (count.Value == list.Count)
				? list
				: Enumerate(list, startIndex, startIndex + count.Value);
			static IEnumerable<T> Enumerate(IReadOnlyList<T> collection, int start, int endExclusive)
			{
				for (; start < endExclusive; ++start) {
					yield return collection[start];
				}
			}
		}

		/// <summary>
		/// Returns an <see cref="IReadOnlyCollection{T}"/> that first returns the elements from this collection;
		/// and then the elements from a readonly <c>tail</c> collection.
		/// </summary>
		/// <param name="first">Not null; may be empty. The first returned elements.</param>
		/// <param name="second">Not null; may be empty. The remaining returned elements.</param>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyCollection<T> Chain<T>(this IReadOnlyCollection<T> first, IReadOnlyCollection<T> second)
			=> new ChainedCollection<T>(first, second);

		/// <summary>
		/// Returns a new an <see cref="IReadOnlyCollection{T}"/> that crops a given count
		/// of elements from the head of this collection; and optionally crops the tail also.
		/// Note that both arguments may exceed the count of the source collection: if the
		/// underlying collection changes, the cropped counts remain the same --- and so
		/// the resulting view may change. The counts are ALSO mutable on the returned
		/// collection.
		/// </summary>
		/// <param name="sourceCollection">Not null.</param>
		/// <param name="cropFromHead">Count of elements omitted from the head of the
		/// <paramref name="sourceCollection"/>. Not negative. Notice that this MAY be zero;
		/// AND may exceed the current count of the source collection.</param>
		/// <param name="thisCount">The count to return in this collection. If this is null,
		/// then all elements are returned from the source, less the
		/// <paramref name="cropFromHead"/> elements. Note also that this MAY be zero;
		/// AND may exceed the current count of the source collection.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException">Only if an int argument is negative:
		/// either may exceed the count of the source collection.</exception>
		/// <seealso cref="CroppedCollection{T}"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CroppedCollection<T> Crop<T>(
				this IReadOnlyCollection<T> sourceCollection,
				int cropFromHead,
				int? thisCount = null)
			=> new CroppedCollection<T>(sourceCollection, cropFromHead, thisCount);


		/// <summary>
		/// Compares that the two Collections' <c>Counts</c> are equal, and that for each element in a,
		/// the number of Equal elements in a is equal to the number in b; and vice-versa. NOTICE
		/// that this method allows null arguments.
		/// </summary>
		/// <typeparam name="T">Captures the element type.</typeparam>
		/// <param name="a">Can be null.</param>
		/// <param name="b">Can be null.</param>
		/// <param name="comparer">Optional: if given this is used to compare elements for equality;
		/// otherwise <see cref="EqualityComparer{T}.Default"/> is used.</param>
		/// <returns>True if both Collections have the same number of equal elements.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SetEqual<T>(
				this IReadOnlyCollection<T> a,
				IReadOnlyCollection<T> b,
				IEqualityComparer<T> comparer = null)
		{
			if (a == null)
				return b == null;
			if (a.Count != (b?.Count ?? -1))
				return false;
			Debug.Assert(b != null);
			if (comparer == null)
				comparer = EqualityComparer<T>.Default;
			static int Count(IEnumerable<T> collection, IEqualityComparer<T> comparison, T predicate)
			{
				int count = 0;
				foreach (T element in collection) {
					if (comparison.Equals(predicate, element))
						++count;
				}
				return count;
			}
			foreach (T aElement in a) {
				if (Count(a, comparer, aElement) != Count(b, comparer, aElement))
					return false;
			}
			foreach (T bElement in b) {
				if (Count(b, comparer, bElement) != Count(a, comparer, bElement))
					return false;
			}
			return true;
		}


		/// <summary>
		/// Generic utility method implements a binary search. The <paramref name="list"/>
		/// emulates an indexed list: it must return each member at the given index.
		/// The <paramref name="comparer"/> must retrun the sorted comparison for each
		/// existing member compared to your desired predicate item. I.E. this is
		/// passed each member in the list and must return the comparison value as
		/// would an <see cref="IComparer{T}"/>, as if the provided argument
		/// member is compared to your search predicate item:
		/// <c>IComparer.Compare(memberArgument, yourPedicateItem)</c>.
		/// If the predicate value is found --  the <paramref name="comparer"/> returns
		/// zero --- this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// If the predicate value is less than one or more elements,
		/// the complemented index is the index of the first element that is larger
		/// than the predicate value. If the predicate value is greater
		/// than all elements, the complemented index is the
		/// <paramref name="count"/>. You can always use this
		/// complemted value as an insertion indecx for this the predicate value.
		/// If the list is empty, this method returns -1 --- which will also
		/// complement to an insertion index (zero) [and
		/// is again the complement of the size of the list].
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="list">Returns the elements by zero-based index.</param>
		/// <param name="count">The total count of the <paramref name="list"/>.</param>
		/// <param name="comparer">Compares each member in the <paramref name="list"/>.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearch<T>(Func<int, T> list, int count, Func<T, int> comparer)
		{
			int low = 0;
			int high = count - 1;
			while (low <= high) {
				int i = low + ((high - low) >> 1);
				int order = comparer(list(i));
				if (order == 0) {
					while (i > low) {
						if (comparer(list(i - 1)) == 0)
							--i;
						else
							return i;
					}
					return i;
				}
				if (order < 0)
					low = i + 1;
				else
					high = i - 1;
			}
			return ~low;
		}


		/// <summary>
		/// As with <see cref="BinarySearch"/>.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="value">The value to find: can be null.</param>
		/// <param name="comparer">Optional comparer to use. The first value passed
		/// to the comparer is always the existing element in the list; and
		/// the second is the <paramref name="value"/>.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<T>(this IReadOnlyList<T> list, T value, IComparer<T> comparer)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (comparer == null)
				comparer = Comparer<T>.Default;
			return BinarySearch(List, list.Count, Comparer);
			T List(int index)
				=> list[index];
			int Comparer(T member)
				=> comparer.Compare(member, value);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="value">THe value to find: can be null.</param>
		/// <param name="comparer">Optional comparer to use. The first value passed
		/// to the comparer is always the existing element in the list; and
		/// the second is the <paramref name="value"/>.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<T>(this IList<T> list, T value, IComparer<T> comparer)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (comparer == null)
				comparer = Comparer<T>.Default;
			return BinarySearch(List, list.Count, Comparer);
			T List(int index)
				=> list[index];
			int Comparer(T member)
				=> comparer.Compare(member, value);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="value">THe value to find: can be null.</param>
		/// <param name="comparer">Optional comparer to use. The first value passed
		/// to the comparer is always the existing element in the list; and
		/// the second is the <paramref name="value"/>.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<T>(this List<T> list, T value, IComparer<T> comparer)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (comparer == null)
				comparer = Comparer<T>.Default;
			return BinarySearch(List, list.Count, Comparer);
			T List(int index)
				=> list[index];
			int Comparer(T member)
				=> comparer.Compare(member, value);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <param name="list">Not null.</param>
		/// <param name="value">THe value to find: can be null.</param>
		/// <param name="comparer">Optional comparer to use. The first value passed
		/// to the comparer is always the existing element in the list; and
		/// the second is the <paramref name="value"/>.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf(this IList list, object value, IComparer comparer)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (comparer == null)
				comparer = Comparer<object>.Default;
			return BinarySearch(List, list.Count, Comparer);
			object List(int index)
				=> list[index];
			int Comparer(object member)
				=> comparer.Compare(member, value);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>:
		/// this method allows you to pass a predicate, and search for an object with
		/// an arbitrary equality comparison.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The actual list element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="predicate">Must compare each
		/// value from each <typeparamref name="T"/> list element.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<T>(this IReadOnlyList<T> list, Func<T, int> predicate)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			return BinarySearch(List, list.Count, Comparer);
			T List(int index)
				=> list[index];
			int Comparer(T member)
				=> predicate(member);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>:
		/// this method allows you to pass a predicate, and search for an object with
		/// an arbitrary equality comparison.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The actual list element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="predicate">Must compare each
		/// value from each <typeparamref name="T"/> list element.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<T>(this IList<T> list, Func<T, int> predicate)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			return BinarySearch(List, list.Count, Comparer);
			T List(int index)
				=> list[index];
			int Comparer(T member)
				=> predicate(member);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>:
		/// this method allows you to pass a predicate, and search for an object with
		/// an arbitrary equality comparison.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The actual list element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="predicate">Must compare each
		/// value from each <typeparamref name="T"/> list element.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<T>(this List<T> list, Func<T, int> predicate)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			return BinarySearch(List, list.Count, Comparer);
			T List(int index)
				=> list[index];
			int Comparer(T member)
				=> predicate(member);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>:
		/// this method allows you to pass a predicate, and search for an object with
		/// an arbitrary equality comparison.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The actual list element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="predicate">Must compare each
		/// value from each <typeparamref name="T"/> list element.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<T>(this T[] list, Func<T, int> predicate)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			return BinarySearch(List, list.Length, Comparer);
			T List(int index)
				=> list[index];
			int Comparer(T member)
				=> predicate(member);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>:
		/// this method allows you to pass a predicate, and search for an object with
		/// an arbitrary equality comparison.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="T">The actual list element type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="predicate">Must compare each
		/// value from each <typeparamref name="T"/> list element.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf(this IList list, Func<object, int> predicate)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			return BinarySearch(List, list.Count, Comparer);
			object List(int index)
				=> list[index];
			int Comparer(object member)
				=> predicate(member);
		}

		/// <summary>
		/// As with <see cref="BinarySearch"/>:
		/// this method allows you to pass a selector, and search for an object with
		/// an arbitrary equality comparison type.
		/// If the <paramref name="value"/> is found,
		/// this returns the index of the value. Otherwise this returns a negative
		/// number. The bitwise complement of this negative number always yields
		/// an insertion index. This complemented value is in <c>[0,list.Count]</c>.
		/// </summary>
		/// <typeparam name="TList">The actual list element type.</typeparam>
		/// <typeparam name="TValue">The selected comparison type.</typeparam>
		/// <param name="list">Not null.</param>
		/// <param name="value">The value to find: can be null.</param>
		/// <param name="listSelector">Must select the <typeparamref name="TValue"/>
		/// value to compare from each <typeparamref name="TList"/> list element.</param>
		/// <param name="comparer">Optional comparer to use. The first value passed
		/// to the comparer is always the existing element in the list; and
		/// the second is the <paramref name="value"/>.</param>
		/// <returns>The zero-based index as documented.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int BinarySearchIndexOf<TList, TValue>(
				this IReadOnlyList<TList> list,
				TValue value,
				Func<TList, TValue> listSelector,
				IComparer<TValue> comparer)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (listSelector == null)
				throw new ArgumentNullException(nameof(listSelector));
			if (comparer == null)
				comparer = Comparer<TValue>.Default;
			return BinarySearch(List, list.Count, Comparer);
			TValue List(int index)
				=> listSelector(list[index]);
			int Comparer(TValue member)
				=> comparer.Compare(member, value);
		}


		/// <summary>
		/// This method takes a <paramref name="target"/> <see cref="IList{T}"/>,
		/// and a <paramref name="predicate"/> <see cref="IEnumerable{T}"/> and moves,
		/// adds, and removes elements in the <paramref name="target"/> so that the
		/// it becomes sequence-equal to the <paramref name="predicate"/>.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="target">The list to mutate.</param>
		/// <param name="predicate">The enumerable to match.</param>
		/// <param name="equalityComparer">Used to compare element instances. If not provided, the
		/// default EqualityComparer is used. The first argument provided to the comparer is
		/// always an existing item in this <paramref name="target"/>; and the second is an
		/// item from the <paramref name="predicate"/>.</param>
		/// <param name="onAdded">An optional action that will be invoked with each element added from
		/// the <paramref name="predicate"/> to the <paramref name="target"/>.
		/// The second argument will be the index.</param>
		/// <param name="onRemoved">An optional action that will be invoked with each element
		/// removed from the <paramref name="target"/>.</param>
		/// <param name="onExisting">An optional action that will be invoked with each existing element
		/// in the <paramref name="target"/> --- possibly moved. The first argument is the existing element,
		/// and the second argument is the matched element from the <paramref name="predicate"/>.
		/// The third argument will be the new index.</param>
		/// <returns>True if any changes are made.</returns>
		/// <exception cref="ArgumentNullException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SetTo<T>(
				this IList<T> target,
				IEnumerable<T> predicate,
				IEqualityComparer<T> equalityComparer = null,
				Action<T, int> onAdded = null,
				Action<T> onRemoved = null,
				Action<T, T, int> onExisting = null)
		{
			return target.SetTo(
					predicate,
					(equalityComparer ?? EqualityComparer<T>.Default).Equals,
					NewTarget,
					onAdded,
					onRemoved,
					onExisting);
			static T NewTarget(T item)
				=> item;
		}

		/// <summary>
		/// This method takes a <paramref name="target"/> <see cref="IList{T}"/>,
		/// and a <paramref name="predicate"/> <see cref="IEnumerable{T}"/> and moves,
		/// adds, and removes elements in the <paramref name="target"/> so that the
		/// it becomes sequence-equal to the <paramref name="predicate"/>.
		/// This method requires the <paramref name="newTarget"/> Func, which will be
		/// invoked when an element exists in the <paramref name="predicate"/>
		/// and must be added to the <paramref name="target"/>: this allows you to
		/// pass different types for the <paramref name="target"/> and
		/// <paramref name="predicate"/>; though also, the types can be the same
		/// and you can pass any selector here.
		/// </summary>
		/// <typeparam name="TTarget">The <paramref name="target"/> element type.</typeparam>
		/// <typeparam name="TSource">The <paramref name="predicate"/> element type.</typeparam>
		/// <param name="target">The list to mutate.</param>
		/// <param name="predicate">The enumerable to match.</param>
		/// <param name="equalityComparer">Required: used to compare element instances.
		/// The first argument provided to the comparer is
		/// always an existing item in this <paramref name="target"/>; and the second is an
		/// item from the <paramref name="predicate"/>.</param>
		/// <param name="newTarget">Required: must return a new item for the
		/// <paramref name="target"/> from an item in the <paramref name="predicate"/>.</param>
		/// <param name="onAdded">An optional action that will be invoked with each element added from
		/// the <paramref name="predicate"/> to the <paramref name="target"/>.
		/// The second argument will be the index.</param>
		/// <param name="onRemoved">An optional action that will be invoked with each element
		/// removed from the <paramref name="target"/>.</param>
		/// <param name="onExisting">An optional action that will be invoked with each existing element
		/// in the <paramref name="target"/> --- possibly moved. The first argument is the existing element,
		/// and the second argument is the matched element from the <paramref name="predicate"/>.
		/// The third argument will be the new index.</param>
		/// <returns>True if any changes are made.</returns>
		/// <exception cref="ArgumentNullException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SetTo<TTarget, TSource>(
				this IList<TTarget> target,
				IEnumerable<TSource> predicate,
				Func<TTarget, TSource, bool> equalityComparer,
				Func<TSource, TTarget> newTarget,
				Action<TTarget, int> onAdded = null,
				Action<TTarget> onRemoved = null,
				Action<TTarget, TSource, int> onExisting = null)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (equalityComparer == null)
				throw new ArgumentNullException(nameof(equalityComparer));
			if (newTarget == null)
				throw new ArgumentNullException(nameof(newTarget));
			bool result = false;
			int predicateIndex = -1;
			foreach (TSource predicateItem in predicate) {
				++predicateIndex;
				if (predicateIndex >= target.Count) {
					TTarget newTargetItem = newTarget(predicateItem);
					target.Add(newTargetItem);
					onAdded?.Invoke(newTargetItem, predicateIndex);
					result = true;
					continue;
				}
				if (equalityComparer(target[predicateIndex], predicateItem)) {
					onExisting?.Invoke(target[predicateIndex], predicateItem, predicateIndex);
					continue;
				}
				int targetIndex
						= target.FindIndex(element => equalityComparer(element, predicateItem), predicateIndex + 1);
				if (targetIndex >= 0) {
					TTarget existingTargetItem = target[targetIndex];
					target.RemoveAt(targetIndex);
					target.Insert(predicateIndex, existingTargetItem);
					onExisting?.Invoke(target[predicateIndex], predicateItem, predicateIndex);
				} else {
					TTarget newTargetItem = newTarget(predicateItem);
					target.Insert(predicateIndex, newTargetItem);
					onAdded?.Invoke(newTargetItem, predicateIndex);
				}
				result = true;
			}
			if (target.Count <= (predicateIndex + 1))
				return result;
			for (int j = target.Count - 1; j > predicateIndex; --j) {
				TTarget removed = target[j];
				target.RemoveAt(j);
				onRemoved?.Invoke(removed);
			}
			return true;
		}

		/// <summary>
		/// This non-generic method takes a <paramref name="target"/> <see cref="IList"/>,
		/// and a <paramref name="predicate"/> <see cref="IEnumerable"/> and moves,
		/// adds, and removes elements in the <paramref name="target"/> so that the
		/// it becomes sequence-equal to the <paramref name="predicate"/>.
		/// This method also allows you to pass the <paramref name="newTarget"/> Func,
		/// which will be invoked when an element exists in the <paramref name="predicate"/>
		/// and must be added to the <paramref name="target"/>: you may return
		/// any new or selected object for the <paramref name="target"/> (if null,
		/// the default Func simply compies the element directly from the
		/// <paramref name="predicate"/>.
		/// </summary>
		/// <param name="target">The list to mutate.</param>
		/// <param name="predicate">The enumerable to match.</param>
		/// <param name="equalityComparer">Optional: used to compare element instances.
		/// If not provided, the default EqualityComparer is used.
		/// The first argument provided to the comparer is
		/// always an existing item in this <paramref name="target"/>; and the second is an
		/// item from the <paramref name="predicate"/>.</param>
		/// <param name="newTarget">Optional: if provided, this must return a new item for the
		/// <paramref name="target"/> from an item in the <paramref name="predicate"/>.
		/// If null, the method simply copies the element from the <paramref name="predicate"/>.</param>
		/// <param name="onAdded">An optional action that will be invoked with each element added from
		/// the <paramref name="predicate"/> to the <paramref name="target"/>.
		/// The second argument will be the index.</param>
		/// <param name="onRemoved">An optional action that will be invoked with each element
		/// removed from the <paramref name="target"/>.</param>
		/// <param name="onExisting">An optional action that will be invoked with each existing element
		/// in the <paramref name="target"/> --- possibly moved. The first argument is the existing element,
		/// and the second argument is the matched element from the <paramref name="predicate"/>.
		/// The third argument will be the new index.</param>
		/// <returns>True if any changes are made.</returns>
		/// <exception cref="ArgumentNullException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool SetTo(
				this IList target,
				IEnumerable predicate,
				Func<object, object, bool> equalityComparer = null,
				Func<object, object> newTarget = null,
				Action<object, int> onAdded = null,
				Action<object> onRemoved = null,
				Action<object, object, int> onExisting = null)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (equalityComparer == null)
				equalityComparer = EqualityComparer<object>.Default.Equals;
			if (newTarget == null) {
				newTarget = NewTarget;
				static object NewTarget(object item)
					=> item;
			}
			bool result = false;
			int predicateIndex = -1;
			foreach (object predicateItem in predicate) {
				++predicateIndex;
				if (predicateIndex >= target.Count) {
					object newTargetItem = newTarget(predicateItem);
					target.Add(newTargetItem);
					onAdded?.Invoke(newTargetItem, predicateIndex);
					result = true;
					continue;
				}
				if (equalityComparer(target[predicateIndex], predicateItem)) {
					onExisting?.Invoke(target[predicateIndex], predicateItem, predicateIndex);
					continue;
				}
				int targetIndex
						= target.FindIndex(element => equalityComparer(element, predicateItem), predicateIndex + 1);
				if (targetIndex >= 0) {
					object existingTargetItem = target[targetIndex];
					target.RemoveAt(targetIndex);
					target.Insert(predicateIndex, existingTargetItem);
					onExisting?.Invoke(target[predicateIndex], predicateItem, predicateIndex);
				} else {
					object newTargetItem = newTarget(predicateItem);
					target.Insert(predicateIndex, newTargetItem);
					onAdded?.Invoke(newTargetItem, predicateIndex);
				}
				result = true;
			}
			if (target.Count <= (predicateIndex + 1))
				return result;
			for (int j = target.Count - 1; j > predicateIndex; --j) {
				object removed = target[j];
				target.RemoveAt(j);
				onRemoved?.Invoke(removed);
			}
			return true;
		}


		/// <summary>
		/// Swaps the values at the two given indexes in this List.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="list">Not null; and not empty.</param>
		/// <param name="indexX">The index to swap with the second.</param>
		/// <param name="indexY">The index to swap with the first.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">The list is empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static void Swap<T>(this IList<T> list, int indexX, int indexY)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (list.Count == 0)
				throw new ArgumentException($@"{nameof(Array.Length)}=0", nameof(list));
			if ((indexX < 0)
					|| (indexX >= list.Count)) {
				throw new ArgumentOutOfRangeException(nameof(list), indexX, $@"[0,{list.Count - 1}]");
			}
			if ((indexY < 0)
					|| (indexY >= list.Count)) {
				throw new ArgumentOutOfRangeException(nameof(list), indexY, $@"[0,{list.Count - 1}]");
			}
			if (indexX == indexY)
				return;
			T y = list[indexY];
			list[indexY] = list[indexX];
			list[indexX] = y;
		}

		/// <summary>
		/// Moves the value at the <paramref name="index"/> in this List
		/// to the <paramref name="destination"/> index; and shifts all elements
		/// between the indexes forward or back, in order.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="list">Not null; and not empty.</param>
		/// <param name="index">The index to move to the <paramref name="destination"/>.</param>
		/// <param name="destination">The index where the <see cref="index"/> is moved.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">The list is empty.</exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static void Move<T>(this IList<T> list, int index, int destination)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (list.Count == 0)
				throw new ArgumentException($@"{nameof(Array.Length)}=0", nameof(list));
			if ((index < 0)
					|| (index >= list.Count)) {
				throw new ArgumentOutOfRangeException(nameof(list), index, $@"[0,{list.Count - 1}]");
			}
			if ((destination < 0)
					|| (destination >= list.Count)) {
				throw new ArgumentOutOfRangeException(nameof(list), destination, $@"[0,{list.Count - 1}]");
			}
			if (index == destination)
				return;
			T move = list[index];
			if (destination > index) {
				for (int i = index + 1; i <= destination; ++i) {
					list[i - 1] = list[i];
				}
			} else {
				for (int i = index - 1; i >= destination; --i) {
					list[i + 1] = list[i];
				}
			}
			list[destination] = move;
		}

		/// <summary>
		/// Removes the first element that match the conditions defined by the
		/// specified predicate.
		/// </summary>
		/// <param name="collection">Required.</param>
		/// <param name="match">The <see cref="Predicate{T}"/> delegate that
		/// defines the conditions of the elements to remove.</param>
		/// <returns>True if the element is found and removed from the collection.</returns>
		/// <exception cref="ArgumentNullException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool RemoveFirst<T>(this ICollection<T> collection, Predicate<T> match)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			if (collection is IList<T> list) {
				int i = 0;
				foreach (T element in list) {
					if (!match(element)) {
						++i;
						continue;
					}
					list.RemoveAt(i);
					return true;
				}
			}
			foreach (T element in collection) {
				if (!match(element))
					continue;
				collection.Remove(element);
				return true;
			}
			return false;
		}


		/// <summary>
		/// Static helper method enumertes this <paramref name="linkedList"/> and
		/// removes all nodes for which your <paramref name="predicate"/>
		/// returns TRUE. Yields an enumeration of each REMOVED value,
		/// AS the list is being enumerated.
		/// </summary>
		/// <typeparam name="T">The list type.</typeparam>
		/// <param name="linkedList">Not null.</param>
		/// <param name="predicate">Not null: return TRUE to REMOVE a value.</param>
		/// <returns>An enumeration of each removed value.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<T> RemoveWhere<T>(this LinkedList<T> linkedList, Func<T, bool> predicate)
		{
			if (linkedList == null)
				throw new ArgumentNullException(nameof(linkedList));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			LinkedListNode<T> prior = null;
			LinkedListNode<T> current = linkedList.First;
			while (current != null) {
				if (predicate(current.Value)) {
					T value = current.Value;
					yield return value;
					linkedList.Remove(current);
					current = prior == null
							? linkedList.First
							: prior.Next;
				} else {
					prior = current;
					current = current.Next;
				}
			}
		}
	}
}
