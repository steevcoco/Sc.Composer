using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.Specialized;
using Sc.Collections.Specialized;


namespace Sc.Collections
{
	public partial class Sequence<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> CreateReadOnlyView(int startIndex, int rangeCount)
			=> new ReadOnlySequence<T>(this, startIndex, rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<TOut> CreateVariantView<TOut>(Func<T, TOut> variantFunc)
			=> new VariantSequenceView<T, TOut, Sequence<T>>(this, variantFunc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceChain<TChain> CreateChain<TChain>(bool asStack, params ISequenceView<TChain>[] chain)
			=> new SequenceChain<TChain>(asStack, chain);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> Clone()
			=> new ImmutableSequence<T>(IsStack, this);


		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => count;
		}


		[DataMember]
		public bool IsStack
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
			=> GetEnumerator(0, Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator(int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(Count, startIndex, rangeCount);
			unchecked {
				if ((Count == 0)
						|| (rangeCount == 0))
					yield break;
				int ver = Version;
				int h = getPointerAt(startIndex);
				int t = getPointerAt(startIndex + (rangeCount - 1));
				if (t < h) {
					do {
						if (Version != ver) {
							throw new InvalidOperationException(
									"Collection has been modified. Enumeration cannot continue.");
						}
						yield return array[h];
					} while (++h < array.Length);
					h = 0;
				}
				do {
					if (Version != ver) {
						throw new InvalidOperationException(
								"Collection has been modified. Enumeration cannot continue.");
					}
					yield return array[h];
				} while (++h <= t);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator()
			=> GetReverseEnumerator(
					Count == 0
							? 0
							: Count - 1,
					Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator(int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(Count, startIndex, rangeCount, true);
			unchecked {
				if ((Count == 0)
						|| (rangeCount == 0))
					yield break;
				int ver = Version;
				int t = getPointerAt(startIndex);
				int h = getPointerAt(startIndex - (rangeCount - 1));
				if (t < h) {
					do {
						if (Version != ver) {
							throw new InvalidOperationException(
									"Collection has been modified. Enumeration cannot continue.");
						}
						yield return array[t];
					} while (--t >= 0);
					t = array.Length - 1;
				}
				do {
					if (Version != ver) {
						throw new InvalidOperationException(
								"Collection has been modified. Enumeration cannot continue.");
					}
					yield return array[t];
				} while (--t >= h);
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek()
		{
			if (Count == 0)
				throw new InvalidOperationException("Collection is empty.");
			return array[head];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T PeekAt(int index)
		{
			if ((index < 0)
					|| (index >= Count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index),
						index,
						$"Must be >= 0, < {Count}.");
			}
			return array[getPointerAt(index)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Poke()
		{
			if (Count == 0)
				throw new InvalidOperationException("Collection is empty.");
			unchecked {
				return array[tail == 0
						? array.Length - 1
						: tail - 1];
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray()
		{
			T[] result = new T[Count];
			doUncheckedCopyRange(0, result, 0, Count);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray(int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(Count, startIndex, rangeCount);
			T[] result = new T[rangeCount];
			if (rangeCount != 0)
				doUncheckedCopyRange(startIndex, result, 0, rangeCount);
			return result;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Array destination, int destinationIndex = 0)
			=> CopyRangeTo(0, destination, destinationIndex, Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyRangeTo(int startIndex, Array destination, int destinationIndex, int rangeCount)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			Sequence<T>.CheckDestinationRangeIndex(
					Count,
					startIndex,
					destination.Length,
					destinationIndex,
					rangeCount);
			doUncheckedCopyRange(startIndex, destination, destinationIndex, rangeCount);
		}


		[DataMember]
		public int Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			protected set;
		}
	}
}
