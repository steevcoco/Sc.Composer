using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sc.Util.Collections.Enumerable;


namespace Sc.Util.Collections
{
	/// <summary>
	/// Static helper methods for <see cref="IEnumerable{T}"/>
	/// and <see cref="IEnumerator{T}"/>.
	/// </summary>
	public static class EnumerableHelper
	{
		/// <summary>
		/// Implements an <see cref="IEnumerator{T}"/> that is empty.
		/// </summary>
		public static IEnumerator<T> EmptyEnumerator<T>()
		{
			yield break;
		}

		/// <summary>
		/// Implements an <see cref="IEnumerable{T}"/> that is empty.
		/// </summary>
		public static IEnumerable<T> EmptyEnumerable<T>()
		{
			yield break;
		}


		/// <summary>
		/// This extension method for <see cref="IEnumerable{T}"/> will return the
		/// enumerated Type, as specified by the generic <c>IEnumerable&lt;T&gt;</c> argument.
		/// </summary>
		/// <typeparam name="T">Captures the element type.</typeparam>
		/// <param name="_">Not read.</param>
		/// <returns><c>typeof(T)</c>.</returns>
		public static Type GetItemType<T>(this IEnumerable<T> _)
			=> typeof(T);

		/// <summary>
		/// This extension method for <see cref="IEnumerator{T}"/> will return the
		/// enumerated Type, as specified by the generic <c>IEnumerator&lt;T&gt;</c> argument.
		/// </summary>
		/// <typeparam name="T">Captures the element type.</typeparam>
		/// <param name="_">Not read.</param>
		/// <returns><c>typeof(T)</c>.</returns>
		public static Type GetItemType<T>(this IEnumerator<T> _)
			=> typeof(T);


		/// <summary>
		/// Returns a new <see cref="IEnumerator{T}"/> that returns only
		/// this single object.
		/// </summary>
		/// <typeparam name="T">The captured generic type.</typeparam>
		/// <param name="single">The single object to return --- may be null.</param>
		/// <param name="throwIfNull">Optional, and defaults to false: if set
		/// true, this will throw if the <paramref name="single"/> is null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<T> AsSingle<T>(this T single, bool throwIfNull = false)
		{
			if (throwIfNull
					&& (single == null)) {
				throw new ArgumentNullException(nameof(single));
			}
			yield return single;
		}


		/// <summary>
		/// Implements an <see cref="IEnumerable{T}"/> over a given Type
		/// <typeparamref name="TOut"/> that is assignable from this Type.
		/// </summary>
		/// <typeparam name="TIn">Source covariant type.</typeparam>
		/// <typeparam name="TOut">Target contravariant type.</typeparam>
		/// <param name="enumerable">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<TOut> As<TIn, TOut>(this IEnumerable<TIn> enumerable)
				where TIn : TOut
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			foreach (TIn element in enumerable) {
				yield return element;
			}
		}

		/// <summary>
		/// Implements an <see cref="IEnumerator{T}"/> over a given Type
		/// <typeparamref name="TOut"/> that is assignable from this Type.
		/// </summary>
		/// <typeparam name="TIn">Source covariant type.</typeparam>
		/// <typeparam name="TOut">Target contravariant type.</typeparam>
		/// <param name="enumerator">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerator<TOut> As<TIn, TOut>(this IEnumerator<TIn> enumerator)
				where TIn : TOut
		{
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));
			using (enumerator) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}
		}


		/// <summary>
		/// Implements an <see cref="IEnumerable{T}"/> that enumerates your enumerator.
		/// </summary>
		/// <param name="enumerator">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator)
		{
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));
			using (enumerator) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}
		}

		/// <summary>
		/// Returns an <see cref="IEnumerable{T}"/> that enumerates your
		/// <see langword="params"/> <paramref name="members"/>
		/// (returns the argument).
		/// </summary>
		/// <param name="members">NOTICE: CAN be null.</param>
		public static IEnumerable<T> AsEnumerable<T>(params T[] members)
			=> members ?? EnumerableHelper.EmptyEnumerable<T>();

		/// <summary>
		/// Implements an <see cref="IEnumerator{T}"/> that enumerates your enumerable.
		/// </summary>
		/// <param name="enumerable">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerator<T> AsEnumerator<T>(this IEnumerable<T> enumerable)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			foreach (T element in enumerable) {
				yield return element;
			}
		}


		/// <summary>
		/// Implements an <see cref="IEnumerator{T}"/> for the array.
		/// </summary>
		/// <typeparam name="T">The Array element type.</typeparam>
		/// <param name="array">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static IEnumerator<T> ArrayEnumerator<T>(this T[] array)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			foreach (T element in array) {
				yield return element;
			}
		}


		/// <summary>
		/// Implements an <see cref="IEnumerator{T}"/> over a given Type
		/// <typeparamref name="TOut"/> with a selector: you provide
		/// a Func that returns an object of Type <typeparamref name="TOut"/>
		/// for each Current, of Type <typeparamref name="TIn"/>.
		/// </summary>
		/// <typeparam name="TIn">Source type.</typeparam>
		/// <typeparam name="TOut">Target type.</typeparam>
		/// <param name="enumerator">Not null.</param>
		/// <param name="selector">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerator<TOut> Select<TIn, TOut>(this IEnumerator<TIn> enumerator, Func<TIn, TOut> selector)
		{
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));
			if (selector == null)
				throw new ArgumentNullException(nameof(selector));
			using (enumerator) {
				while (enumerator.MoveNext()) {
					yield return selector(enumerator.Current);
				}
			}
		}

		/// <summary>
		/// Implements an <see cref="IEnumerator{T}"/> over a given Type
		/// <typeparamref name="T"/> with a predicate your provide.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerator">Not null.</param>
		/// <param name="predicate">Required.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerator<T> Where<T>(this IEnumerator<T> enumerator, Func<T, bool> predicate)
		{
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			using (enumerator) {
				while (enumerator.MoveNext()) {
					if (predicate(enumerator.Current))
						yield return enumerator.Current;
				}
			}
		}

		/// <summary>
		/// Returns the first element from this <see cref="IEnumerator{T}"/>
		/// that is selected by your <paramref name="predicate"/>;
		/// or the given default <paramref name="defaultValue"/> value
		/// if the enumerator is empty or if your predicate does not select
		/// any item. Notice that if the <paramref name="predicate"/>
		/// is null, the default predicate will ONLY return the first
		/// NON-null element; and otherwise the <paramref name="defaultValue"/>
		/// is returned.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="enumerator">Not null.</param>
		/// <param name="predicate">OPTIONAL. Notice that if this
		/// is null, the default predicate will ONLY return the first
		/// NON-null element; and otherwise the <paramref name="defaultValue"/>
		/// is returned.</param>
		/// <returns>The first element that matches your optional
		/// <paramref name="predicate"/>; or else is not null; and
		/// otherwise the <paramref name="defaultValue"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static T FirstOrDefault<T>(
				this IEnumerator<T> enumerator,
				Func<T, bool> predicate = null,
				T defaultValue = default)
		{
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));
			using (enumerator) {
				if (predicate == null) {
					while (enumerator.MoveNext()) {
						if (enumerator.Current != null)
							return enumerator.Current;
					}
					return defaultValue;
				}
				while (enumerator.MoveNext()) {
					if (predicate(enumerator.Current))
						return enumerator.Current;
				}
				return defaultValue;
			}
		}


		/// <summary>
		/// Returns the first element from this <see cref="IEnumerable{T}"/>
		/// that is selected by your <paramref name="predicate"/>;
		/// or the given default <paramref name="defaultValue"/> value
		/// if the enumerator is empty or if your predicate does not select
		/// any item. Notice that if the <paramref name="predicate"/>
		/// is null, the default predicate will ONLY return the first
		/// NON-null element; and otherwise the <paramref name="defaultValue"/>
		/// is returned.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="enumerable">Not null.</param>
		/// <param name="predicate">OPTIONAL. Notice that if this
		/// is null, the default predicate will ONLY return the first
		/// NON-null element; and otherwise the <paramref name="defaultValue"/>
		/// is returned.</param>
		/// <returns>The first element that matches your optional
		/// <paramref name="predicate"/>; or else is not null; and
		/// otherwise the <paramref name="defaultValue"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static T FirstOrDefault<T>(
				this IEnumerable<T> enumerable,
				Func<T, bool> predicate = null,
				T defaultValue = default)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (predicate == null) {
				foreach (T item in enumerable) {
					if (item != null)
						return item;
				}
				return defaultValue;
			}
			foreach (T item in enumerable) {
				if (predicate(item))
					return item;
			}
			return defaultValue;
		}


		/// <summary>
		/// Enumerates this <paramref name="enumerable"/> and then the
		/// <paramref name="second"/>.
		/// </summary>
		/// <param name="enumerable">Not null.</param>
		/// <param name="second">Not null.</param>
		/// <returns>Not null.</returns>
		public static IEnumerable Concat(this IEnumerable enumerable, IEnumerable second)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (second == null)
				throw new ArgumentNullException(nameof(second));
			foreach (object o in enumerable) {
				yield return o;
			}
			foreach (object o in second) {
				yield return o;
			}
		}


		/// <summary>
		/// Enumerates this <see cref="IEnumerator"/> now, and returns the count.
		/// </summary>
		/// <param name="enumerator">Not null.</param>
		/// <returns>The count after iterating.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static int Count(this IEnumerator enumerator)
		{
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));
			int count = 0;
			while (enumerator.MoveNext()) {
				++count;
			}
			return count;
		}

		/// <summary>
		/// Enumerates this <see cref="IEnumerable"/> now, and returns the count.
		/// </summary>
		/// <param name="enumerable">Not null.</param>
		/// <returns>The count after iterating.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static int Count(this IEnumerable enumerable)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			int count = 0;
			foreach (object _ in enumerable) {
				++count;
			}
			return count;
		}


		/// <summary>
		/// Returns true if this <paramref name="enumerable"/> is null or empty.
		/// Note that the argument may be enumerated now.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerable">Can be null.</param>
		/// <returns>True if null or empty.</returns>
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
			=> (enumerable == null) || !enumerable.Any();

		/// <summary>
		/// Returns true if this <paramref name="enumerable"/> is null or empty.
		/// Note that the argument may be enumerated now.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerable">Can be null.</param>
		/// <returns>True if null or empty.</returns>
		public static bool IsNullOrEmpty(this IEnumerable enumerable)
			=> (enumerable == null) || (enumerable.Count() == 0);


		/// <summary>
		/// Enumerates the given <paramref name="predicate"/> <see cref="IEnumerable"/>,
		/// and returns true if this <see cref="IReadOnlyCollection{T}"/> contains any element.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="collection">This collection to search.</param>
		/// <param name="predicate">The items to search for.</param>
		/// <param name="comparer">Optional comparer: if null, the default is used.</param>
		/// <returns>True if any element is found here.</returns>
		public static bool ContainsAny<T>(
				this IReadOnlyCollection<T> collection,
				IEnumerable<T> predicate,
				IEqualityComparer<T> comparer = null)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (comparer == null)
				comparer = EqualityComparer<T>.Default;
			foreach (T element in predicate) {
				if (collection.Contains(element, comparer))
					return true;
			}
			return false;
		}


		/// <summary>
		/// Implements an <see cref="IEnumerable{T}"/> selecting only non-null elements
		/// from this instance.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerable">Not null.</param>
		/// <param name="throwIfArgumentNull">Defaults to true: this method throws
		/// if the <paramref name="enumerable"/> is null; and otherwise it
		/// may be null, and the method returns an empty collection.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<T> NotNull<T>(this IEnumerable<T> enumerable, bool throwIfArgumentNull = true)
				where T : class
		{
			if (enumerable == null) {
				if (throwIfArgumentNull)
					throw new ArgumentNullException(nameof(enumerable));
				yield break;
			}
			foreach (T element in enumerable) {
				if (element != null)
					yield return element;
			}
		}

		/// <summary>
		/// Implements an <see cref="IEnumerable{T}"/> selecting only non-null elements
		/// from this instance.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerator">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerator<T> NotNull<T>(this IEnumerator<T> enumerator)
				where T : class
		{
			if (enumerator == null)
				throw new ArgumentNullException(nameof(enumerator));
			using (enumerator) {
				while (enumerator.MoveNext()) {
					if (enumerator.Current != null)
						yield return enumerator.Current;
				}
			}
		}


		/// <summary>
		/// Returns an <see cref="IEnumerator{T}"/> that returns a tuple that
		/// holds the current element, and the next element in the enumeration.
		/// The returned tuple also holds a bool indicating if the <c>next</c>
		/// element returned there IS available --- so null checks should not
		/// be performed to test that condition.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerator">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerator<(T current, T next, bool hasNext)> LookAhead<T>(this IEnumerator<T> enumerator)
			=> new LookaheadEnumerator<T>(enumerator);

		/// <summary>
		/// Returns an <see cref="IEnumerable{T}"/> that returns a tuple that
		/// holds the current element, and the next element in the enumeration.
		/// The returned tuple also holds a bool indicating if the <c>next</c>
		/// element returned there IS available --- so null checks should not
		/// be performed to test that condition.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerable">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static IEnumerable<(T current, T next, bool hasNext)> LookAhead<T>(this IEnumerable<T> enumerable)
		{
			using (LookaheadEnumerator<T> enumerator = new LookaheadEnumerator<T>(enumerable.GetEnumerator())) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}
		}


		/// <summary>
		/// Enumerates this collection now and returns the zero-based
		/// index of the item. This method will test if this collection implements
		/// <see cref="IList{T}"/>; and if so will invoke
		/// <see cref="IList.IndexOf(object)"/>; and for an <see cref="Array"/>
		/// will invoke <see cref="Array.IndexOf(Array, object)"/>. Otherwise this
		/// iterates this collection now.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="enumerable">This <see cref="IEnumerable{T}"/> instance.</param>
		/// <param name="item">Not null.</param>
		/// <returns>-1 if your item is not found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static int FindIndex<T>(this IEnumerable<T> enumerable, T item)
		{
			switch (enumerable) {
				case null :
					throw new ArgumentNullException(nameof(enumerable));
				case IList<T> list :
					return list.IndexOf(item);
				case Array array :
					return Array.IndexOf(array, item);
			}
			int index = 0;
			foreach (T element in enumerable) {
				if (object.Equals(item, element))
					return index;
				++index;
			}
			return -1;
		}

		/// <summary>
		/// Enumerates this collection now and returns the zero-based
		/// index of the item, using reference equality.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="enumerable">This <see cref="IEnumerable{T}"/> instance.</param>
		/// <param name="item">Not null.</param>
		/// <returns>-1 if your item is not found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static int FindReferenceIndex<T>(this IEnumerable<T> enumerable, T item)
		{
			int index = 0;
			foreach (T element in enumerable) {
				if (object.ReferenceEquals(item, element))
					return index;
				++index;
			}
			return -1;
		}

		/// <summary>
		/// Enumerates this collection now and returns the zero-based
		/// index of the first element for which
		/// your predicate returns true.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="enumerable">This <see cref="IEnumerable{T}"/> instance.</param>
		/// <param name="predicate">Not null.</param>
		/// <param name="startIndex">The index at which to begin searching.</param>
		/// <returns>-1 if your predicate does not return true.</returns>
		/// <exception cref="ArgumentNullException">If either reference is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <c>startIndex</c> &lt; zero.</exception>
		public static int FindIndex<T>(this IEnumerable<T> enumerable, Predicate<T> predicate, int startIndex = 0)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex.ToString());
			switch (enumerable) {
				case null :
					throw new ArgumentNullException(nameof(enumerable));
				case IList<T> list :
					for (int i = startIndex; i < list.Count; ++i) {
						if (predicate(list[i]))
							return i;
					}
					return -1;
				case IReadOnlyList<T> readOnlyList :
					for (int i = startIndex; i < readOnlyList.Count; ++i) {
						if (predicate(readOnlyList[i]))
							return i;
					}
					return -1;
			}
			startIndex = 0 - startIndex;
			foreach (T element in enumerable) {
				if ((startIndex >= 0)
						&& predicate(element)) {
					return startIndex;
				}
				++startIndex;
			}
			return -1;
		}

		/// <summary>
		/// Enumerates this collection now and returns the zero-based
		/// index of the first element for which
		/// your predicate returns true.
		/// </summary>
		/// <param name="enumerable">This <see cref="IEnumerable"/> instance.</param>
		/// <param name="predicate">Not null.</param>
		/// <param name="startIndex">The index at which to begin searching.</param>
		/// <returns>-1 if your predicate does not return true.</returns>
		/// <exception cref="ArgumentNullException">If either reference is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If <c>startIndex</c> &lt; zero.</exception>
		public static int FindIndex(this IEnumerable enumerable, Predicate<object> predicate, int startIndex = 0)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex.ToString());
			switch (enumerable) {
				case null :
					throw new ArgumentNullException(nameof(enumerable));
				case IList list :
					for (int i = startIndex; i < list.Count; ++i) {
						if (predicate(list[i]))
							return i;
					}
					return -1;
			}
			startIndex = 0 - startIndex;
			foreach (object element in enumerable) {
				if ((startIndex >= 0)
						&& predicate(element)) {
					return startIndex;
				}
				++startIndex;
			}
			return -1;
		}


		/// <summary>
		/// Enumerates this collection now with <c>object.ReferenceEquals</c>,
		/// and returns true if the
		/// given reference is located.
		/// </summary>
		/// <typeparam name="T">This collection element type.</typeparam>
		/// <param name="enumerable">This collection to  enumerate: not null.</param>
		/// <param name="reference">Notice: may be null: the object reference to find.</param>
		/// <param name="throwIfReferenceNull">Defaults to true: this method will throw
		/// <see cref="ArgumentNullException"/> if the
		/// <paramref name="reference"/> is null.</param>
		/// <returns>True if found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool ContainsReference<T>(
				this IEnumerable<T> enumerable,
				T reference,
				bool throwIfReferenceNull = true)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (throwIfReferenceNull
					&& (reference == null)) {
				throw new ArgumentNullException(nameof(reference));
			}
			foreach (T element in enumerable) {
				if (object.ReferenceEquals(reference, element))
					return true;
			}
			return false;
		}


		/// <summary>
		/// Creates a string from the sequence by concatenating the result
		/// of the specified string selector function for each element.
		/// </summary>
		/// <param name="enumerable">Required.</param>
		/// <param name="stringSelector">Optional: if null, <see cref="object.ToString"/> is used.</param>
		/// <param name="separator">The string which separates each concatenated item. Optional:
		/// if null, <see cref="string.Empty"/> is used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static StringBuilder ToConcatenatedString<T>(
				this IEnumerable<T> enumerable,
				Func<T, string> stringSelector = null,
				string separator = null)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (stringSelector == null) {
				stringSelector = StringSelector;
				static string StringSelector(T element)
					=> element?.ToString() ?? string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (string.IsNullOrEmpty(separator)) {
				foreach (T item in enumerable) {
					stringBuilder.Append(stringSelector(item));
				}
			} else {
				bool isFirstElement = true;
				foreach (T item in enumerable) {
					if (isFirstElement)
						isFirstElement = false;
					else
						stringBuilder.Append(separator);
					stringBuilder.Append(stringSelector(item));
				}
			}
			return stringBuilder;
		}

		/// <summary>
		/// Notice that this method returns a <see cref="StringBuilder"/>.
		/// Creates a string for an <see cref="IEnumerable"/> that begins with the count,
		/// and stops at the <paramref name="characterLimit"/>; with the form:
		/// "<c>[Count]{ Element One, Element Two, [...] }</c>". If the <c>characterLimit</c> 
		/// is &lt;= zero, then ONLY the Count is returned: "<c>[Count]</c>".
		/// This collection argument CAN also be null: this returns "<c>[null]</c>".
		/// Notice that the <paramref name="characterLimit"/> will not not be strictly
		/// honored: it is checked after adding an element's string; and, this method
		/// adds braces and the count, which will be appended after the limit is discovered.
		/// </summary>
		/// <param name="enumerable">CAN be null.</param>
		/// <param name="characterLimit">A trigger threshold for the string length.
		/// If &lt;= zero, only the count is included. This will not not be strictly
		/// honored: it is checked after adding an element's string; and, this method
		/// adds braces and the count, which will be appended after the limit is discovered.</param>
		/// <param name="stringSelector">Optional Func to convert each element to a string.</param>
		/// <returns>Not null.</returns>
		public static StringBuilder ToStringCollection(
				this IEnumerable enumerable,
				int characterLimit = 256,
				Func<object, string> stringSelector = null)
		{
			return enumerable == null
				? new StringBuilder($"[{Convert.ToString(null)}]")
				: ObjectEnumerable(enumerable)
						.ToStringCollection(characterLimit, stringSelector);
			static IEnumerable<object> ObjectEnumerable(IEnumerable collection)
			{
				foreach (object o in collection) {
					yield return o;
				}
			}
		}

		/// <summary>
		/// Notice that this method returns a <see cref="StringBuilder"/>.
		/// Creates a string for an <see cref="IEnumerable"/> that begins with the count,
		/// and stops at the <paramref name="characterLimit"/>; with the form:
		/// "<c>[Count]{ Element One, Element Two, [...] }</c>". If the <c>characterLimit</c> 
		/// is &lt;= zero, then ONLY the Count is returned: "<c>[Count]</c>".
		/// This collection argument CAN also be null: this returns "<c>[null]</c>".
		/// Notice that the <paramref name="characterLimit"/> will not not be strictly
		/// honored: it is checked after adding an element's string; and, this method
		/// adds braces and the count, which will be appended after the limit is discovered.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <param name="enumerable">CAN be null.</param>
		/// <param name="characterLimit">A trigger threshold for the string length.
		/// If &lt;= zero, only the count is included. This will not not be strictly
		/// honored: it is checked after adding an element's string; and, this method
		/// adds braces and the count, which will be appended after the limit is discovered.</param>
		/// <param name="stringSelector">Optional Func to convert each element to a string.</param>
		/// <returns>Not null.</returns>
		public static StringBuilder ToStringCollection<T>(
				this IEnumerable<T> enumerable,
				int characterLimit = 256,
				Func<T, string> stringSelector = null)
		{
			if (enumerable == null)
				return new StringBuilder($"[{Convert.ToString(null)}]");
			if (characterLimit <= 0) {
				return enumerable switch
				{
					IReadOnlyCollection<T> re
						=> new StringBuilder($"[{re.Count}]"),
					ICollection<T> ce
						=> new StringBuilder($"[{ce.Count}]"),
					ICollection le
						=> new StringBuilder($"[{le.Count}]"),
					_
						=> new StringBuilder($"[{enumerable.Count()}]"),
				};
			}
			static string ToString(T o)
				=> o?.ToString();
			if (stringSelector == null)
				stringSelector = ToString;
			StringBuilder sb = new StringBuilder(characterLimit + 16).Append("[]{ ");
			int count = 0;
			int enumerate = 0;
			foreach (T element in enumerable) {
				++count;
				switch (enumerate) {
					case 0 :
						enumerate = 1;
						break;
					case 1 :
						if (sb.Length >= characterLimit) {
							sb.Append(", [...]");
							switch (enumerable) {
								case IReadOnlyCollection<T> r :
									count = r.Count;
									goto BreakEnumeration;
								case ICollection<T> c :
									count = c.Count;
									goto BreakEnumeration;
								case ICollection l :
									count = l.Count;
									goto BreakEnumeration;
							}
							enumerate = 2;
							continue;
						}
						sb.Append(", ");
						break;
					case 2 :
						continue;
				}
				sb.Append(stringSelector(element) ?? Convert.ToString(null));
			}
			BreakEnumeration:
			sb.Append(" }");
			sb.Insert(1, count);
			return sb;
		}
	}
}
