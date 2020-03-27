using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Abstractions.Collections
{
	/// <summary>
	/// Extension methods for <see cref="ISequenceView{T}"/>.
	/// </summary>
	public static class SequenceHelper
	{
		/// <summary>
		/// Returns <see cref="IQueue{T}.Dequeue"/> if the collection is not empty.
		/// Equivalent to <see cref="TryRemoveNext{T}"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="queue">This <see cref="IQueue{T}"/> instance.</param>
		/// <param name="head">The removed value if the collection is not empty.</param>
		/// <returns>True if the out argument is set.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryDequeue<T>(this IQueue<T> queue, out T head)
		{
			if (queue == null)
				throw new ArgumentNullException(nameof(queue));
			if (queue.Count == 0) {
				head = default;
				return false;
			}
			head = queue.Dequeue();
			return true;
		}

		/// <summary>
		/// Returns <see cref="IStack{T}.Pop"/> if the collection is not empty.
		/// Equivalent to <see cref="TryRemoveNext{T}"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="stack">This <see cref="IStack{T}"/> instance.</param>
		/// <param name="top">The removed value if the collection is not empty.</param>
		/// <returns>True if the out argument is set.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryPop<T>(this IStack<T> stack, out T top)
		{
			if (stack == null)
				throw new ArgumentNullException(nameof(stack));
			if (stack.Count == 0) {
				top = default;
				return false;
			}
			top = stack.Pop();
			return true;
		}

		/// <summary>
		/// Returns <see cref="ISequence{T}.RemoveNext"/> if the collection is not empty.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">This <see cref="ISequence{T}"/> instance.</param>
		/// <param name="next">The removed value if the collection is not empty.</param>
		/// <returns>True if the out argument is set.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRemoveNext<T>(this ISequence<T> sequence, out T next)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			if (sequence.Count == 0) {
				next = default;
				return false;
			}
			next = sequence.RemoveNext();
			return true;
		}

		/// <summary>
		/// Returns <see cref="ISequence{T}.Drop"/> if the collection is not empty.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">This <see cref="ISequence{T}"/> instance.</param>
		/// <param name="last">The removed value if the collection is not empty.</param>
		/// <returns>True if the out argument is set.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryDrop<T>(this ISequence<T> sequence, out T last)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			if (sequence.Count == 0) {
				last = default;
				return false;
			}
			last = sequence.Drop();
			return true;
		}


		/// <summary>
		/// Returns <see cref="ISequenceView{T}.Peek"/> if the collection is not empty; and
		/// otherwise returns the argument.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">This <see cref="ISequenceView{T}"/> instance.</param>
		/// <param name="returnIfEmpty">This value is returned if the collection is empty.</param>
		/// <returns><see cref="ISequenceView{T}.Peek"/> if the collection is not empty; and
		/// otherwise returns the <c>returnIfEmpty</c> argument.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryPeek<T>(this ISequenceView<T> sequence, T returnIfEmpty)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			return sequence.Count == 0
					? returnIfEmpty
					: sequence.Peek();
		}

		/// <summary>
		/// Returns <see cref="ISequenceView{T}.Peek"/> in the out argument if the collection is not empty.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">This <see cref="ISequenceView{T}"/> instance.</param>
		/// <param name="next">The next element if the collection is not empty.</param>
		/// <returns>True if the out argument is set.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryPeek<T>(this ISequenceView<T> sequence, out T next)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			if (sequence.Count == 0) {
				next = default;
				return false;
			}
			next = sequence.Peek();
			return true;
		}

		/// <summary>
		/// Returns <see cref="ISequenceView{T}.PeekAt"/> if the index is in range.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">This <see cref="ISequenceView{T}"/> instance.</param>
		/// <param name="index">The index to peek at.</param>
		/// <param name="element">The result if the method returns true.</param>
		/// <returns><see cref="ISequenceView{T}.PeekAt"/> if the index is in range.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryPeekAt<T>(this ISequenceView<T> sequence, int index, out T element)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			if ((index < 0)
					|| (index >= sequence.Count)) {
				element = default;
				return false;
			}
			element = sequence.PeekAt(index);
			return true;
		}


		/// <summary>
		/// Returns <see cref="ISequenceView{T}.Poke"/> if the collection is not empty; and
		/// otherwise returns the argument.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">This <see cref="ISequenceView{T}"/> instance.</param>
		/// <param name="returnIfEmpty">This value is returned if the collection is empty.</param>
		/// <returns><see cref="ISequenceView{T}.Poke"/> if the collection is not empty; and
		/// otherwise returns the <c>returnIfEmpty</c> argument.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryPoke<T>(this ISequenceView<T> sequence, T returnIfEmpty)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			return sequence.Count == 0
					? returnIfEmpty
					: sequence.Poke();
		}

		/// <summary>
		/// Returns <see cref="ISequenceView{T}.Poke"/> in the out argument if the collection is not empty.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">This <see cref="ISequenceView{T}"/> instance.</param>
		/// <param name="last">The Tail if the collection is not empty.</param>
		/// <returns>True if the out argument is set.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryPoke<T>(this ISequenceView<T> sequence, out T last)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			if (sequence.Count == 0) {
				last = default;
				return false;
			}
			last = sequence.Poke();
			return true;
		}


		/// <summary>
		/// Returns the newest element added to the collection: for an <see cref="IQueue{T}"/>
		/// it is <see cref="ISequenceView{T}.Poke"/>, and for sn <see cref="IStack{T}"/>
		/// it is <see cref="ISequenceView{T}.Peek"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">Not null; and not empty.</param>
		/// <returns>The newest element added to the collection.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Newest<T>(this ISequenceView<T> sequence)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			return sequence.IsStack
					? sequence.Peek()
					: sequence.Poke();
		}

		/// <summary>
		/// Returns the newest element added to the collection: for an <see cref="IQueue{T}"/>
		/// it is <see cref="ISequenceView{T}.Poke"/>, and for sn <see cref="IStack{T}"/>
		/// it is <see cref="ISequenceView{T}.Peek"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">Not null.</param>
		/// <param name="returnIfEmpty">This value will be returned if the
		/// <paramref name="sequence"/> is empty.</param>
		/// <returns>The newest element added to the collection; or
		/// <paramref name="returnIfEmpty"/> if the collection is empty.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryNewest<T>(this ISequenceView<T> sequence, T returnIfEmpty)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			return sequence.Count == 0
				? returnIfEmpty
				: sequence.IsStack
					? sequence.Peek()
					: sequence.Poke();
		}

		/// <summary>
		/// Returns the newest element added to the collection: for an <see cref="IQueue{T}"/>
		/// it is <see cref="ISequenceView{T}.Poke"/>, and for sn <see cref="IStack{T}"/>
		/// it is <see cref="ISequenceView{T}.Peek"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">Not null.</param>
		/// <param name="newest">The newest element in the collection..</param>
		/// <returns>False if the collection is empty.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryNewest<T>(this ISequenceView<T> sequence, out T newest)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			if (sequence.Count == 0) {
				newest = default;
				return false;
			}
			newest = sequence.IsStack
					? sequence.Peek()
					: sequence.Poke();
			return true;
		}


		/// <summary>
		/// Returns the oldest element added to the collection: for an <see cref="IQueue{T}"/>
		/// it is <see cref="ISequenceView{T}.Peek"/>, and for sn <see cref="IStack{T}"/>
		/// it is <see cref="ISequenceView{T}.Poke"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">Not null; and not empty.</param>
		/// <returns>The oldest element added to the collection.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Oldest<T>(this ISequenceView<T> sequence)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			return sequence.IsStack
					? sequence.Poke()
					: sequence.Peek();
		}

		/// <summary>
		/// Returns the oldest element added to the collection: for an <see cref="IQueue{T}"/>
		/// it is <see cref="ISequenceView{T}.Peek"/>, and for sn <see cref="IStack{T}"/>
		/// it is <see cref="ISequenceView{T}.Poke"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">Not null.</param>
		/// <param name="returnIfEmpty">This value will be returned if the
		/// <paramref name="sequence"/> is empty.</param>
		/// <returns>The oldest element added to the collection; or
		/// <paramref name="returnIfEmpty"/> if the collection is empty.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryOldest<T>(this ISequenceView<T> sequence, T returnIfEmpty)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			return sequence.Count == 0
				? returnIfEmpty
				: sequence.IsStack
					? sequence.Poke()
					: sequence.Peek();
		}

		/// <summary>
		/// Returns the oldest element added to the collection: for an <see cref="IQueue{T}"/>
		/// it is <see cref="ISequenceView{T}.Peek"/>, and for sn <see cref="IStack{T}"/>
		/// it is <see cref="ISequenceView{T}.Poke"/>.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		/// <param name="sequence">Not null.</param>
		/// <param name="oldest">The oldest element in the collection..</param>
		/// <returns>False if the collection is empty.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryOldest<T>(this ISequenceView<T> sequence, out T oldest)
		{
			if (sequence == null)
				throw new ArgumentNullException(nameof(sequence));
			if (sequence.Count == 0) {
				oldest = default;
				return false;
			}
			oldest = sequence.IsStack
					? sequence.Poke()
					: sequence.Peek();
			return true;
		}


		/// <summary>
		/// Convenience method gets the <see cref="ISequenceView{T}.GetEnumerator(int,int)"/>,
		/// and enumerates the elements.
		/// </summary>
		/// <typeparam name="T">Collection element type.</typeparam>
		/// <param name="sequenceView">Not null.</param>
		/// <param name="startIndex">The first index to return.</param>
		/// <param name="rangeCount">The total count to return.</param>
		/// <returns>The reverse enumeration.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> EnumerateRange<T>(
				this ISequenceView<T> sequenceView,
				int startIndex,
				int rangeCount)
		{
			if (sequenceView == null)
				throw new ArgumentNullException(nameof(sequenceView));
			using (IEnumerator<T> enumerator = sequenceView.GetEnumerator(startIndex, rangeCount)) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}
		}

		/// <summary>
		/// Convenience method gets the <see cref="ISequenceView{T}.GetReverseEnumerator()"/>,
		/// and enumerates the elements.
		/// </summary>
		/// <typeparam name="T">Collection element type.</typeparam>
		/// <param name="sequenceView">Not null.</param>
		/// <returns>The reverse enumeration.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> EnumerateInReverse<T>(this ISequenceView<T> sequenceView)
		{
			if (sequenceView == null)
				throw new ArgumentNullException(nameof(sequenceView));
			using (IEnumerator<T> enumerator = sequenceView.GetReverseEnumerator()) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}
		}

		/// <summary>
		/// Convenience method gets the <see cref="ISequenceView{T}.GetReverseEnumerator(int,int)"/>,
		/// and enumerates the elements.
		/// </summary>
		/// <typeparam name="T">Collection element type.</typeparam>
		/// <param name="sequenceView">Not null.</param>
		/// <param name="startIndex">Argument for
		/// <see cref="ISequenceView{T}.GetReverseEnumerator(int,int)"/>.</param>
		/// <param name="rangeCount">Argument for
		/// <see cref="ISequenceView{T}.GetReverseEnumerator(int,int)"/>.</param>
		/// <returns>The reverse enumeration.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> EnumerateInReverse<T>(
				this ISequenceView<T> sequenceView,
				int startIndex,
				int rangeCount)
		{
			if (sequenceView == null)
				throw new ArgumentNullException(nameof(sequenceView));
			using (IEnumerator<T> enumerator = sequenceView.GetReverseEnumerator(startIndex, rangeCount)) {
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			}
		}
	}
}
