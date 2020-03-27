using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.Specialized;
using Sc.Util.Collections;


namespace Sc.Collections.Specialized
{
	/// <summary>
	/// Implements an immutable <see cref="ISequenceView{T}"/> that is a simple wrapper
	/// around a given Array.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	[DataContract]
	public class ImmutableSequence<T>
			: ISequenceView<T>
	{
		[DataMember]
		private T[] array;


		/// <summary>
		/// Constructor: you may optionally specify to clone the given Array, or
		/// otherwise retain this actual reference here --- which IS the default.
		/// </summary>
		/// <param name="isStack">Specifies the reported mode of this collection. Notice that
		/// either mode will return elements from this Array in the Array's natural order
		/// --- element zero in the Array will be the Head of this Queue or the Top of this Stack.</param>
		/// <param name="array">The source Array; which is retained by reference here by default;
		/// or otherwise can be cloned now.</param>
		/// <param name="cloneArray">Defaults to false: the source Array is retained by reference
		/// here. If set true, the Array is Cloned now.</param>
		public ImmutableSequence(bool isStack, T[] array, bool cloneArray = false)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			this.array
					= cloneArray
							? (T[])array.Clone()
							: array;
			IsStack = isStack;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="isStack">Specifies the mode of this collection. Notice that
		/// either mode will return elements from this collection in it's mode
		/// --- element zero in the Sequence will be the Head of this Queue or the Top of
		/// this Stack.</param>
		/// <param name="collection">Not null.</param>
		public ImmutableSequence(bool isStack, ISequenceView<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			array = collection.ToArray();
			IsStack = isStack;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="isStack">Specifies the mode of this collection. Notice that
		/// either mode will return elements from this Enumerable in it's returned order
		/// --- element zero in the enumeration will be the Head of this Queue or the Top of
		/// this Stack.</param>
		/// <param name="collection">Not null.</param>
		public ImmutableSequence(bool isStack, IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			array = collection.ToArray();
			IsStack = isStack;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> CreateReadOnlyView(int startIndex, int rangeCount)
			=> new ReadOnlySequence<T>(this, startIndex, rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<TOut> CreateVariantView<TOut>(Func<T, TOut> variantFunc)
			=> new VariantSequenceView<T, TOut, ImmutableSequence<T>>(this, variantFunc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceChain<TChain> CreateChain<TChain>(bool asStack, params ISequenceView<TChain>[] chain)
			=> new SequenceChain<TChain>(asStack, chain);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> Clone()
			=> new ImmutableSequence<T>(IsStack, array, true);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray()
			=> (T[])array.Clone();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray(int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(Count, startIndex, rangeCount);
			T[] result = new T[rangeCount];
			Array.Copy(array, startIndex, result, 0, rangeCount);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Array destination, int destinationIndex = 0)
		{
			Sequence<T>.CheckDestinationRangeIndex(
					Count,
					0,
					destination.Length,
					destinationIndex,
					Count);
			Array.Copy(array, 0, destination, destinationIndex, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyRangeTo(int startIndex, Array destination, int destinationIndex, int rangeCount)
		{
			Sequence<T>.CheckDestinationRangeIndex(
					Count,
					startIndex,
					destination.Length,
					destinationIndex,
					rangeCount);
			Array.Copy(array, startIndex, destination, destinationIndex, rangeCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
			=> array.ArrayEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator(int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(Count, startIndex, rangeCount);
			startIndex = -startIndex;
			foreach (T element in array) {
				if (rangeCount == 0)
					yield break;
				if (startIndex < 0) {
					++startIndex;
					continue;
				}
				yield return element;
				--rangeCount;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator()
		{
			for (int i = array.Length - 1; i >= 0; --i) {
				yield return array[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator(int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(Count, startIndex, rangeCount, true);
			rangeCount = startIndex - rangeCount;
			for (; startIndex > rangeCount; --startIndex) {
				yield return array[startIndex];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek()
			=> array[0];

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => array[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T PeekAt(int index)
			=> array[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Poke()
			=> array[array.Length - 1];

		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => true;
		}

		[DataMember]
		public bool IsStack
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		}

		public int Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => 0;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => array.Length;
		}
	}
}
