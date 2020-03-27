using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.Specialized;


namespace Sc.Collections.Specialized
{
	/// <summary>
	/// An <see cref="ISequenceView{T}"/> view implementation that delegates to a given
	/// <see cref="ISequenceView{T}"/> instance. Notice that this is merely a concrete implementation
	/// of an <see cref="ISequenceView{T}"/> that does not have mutable methods: a <see cref="Sequence{T}"/>
	/// instance also implements this interface. NOTICE ALSO: instances that are created explicitly
	/// with the constructors here WILL NOT SERIALIZE. The <see cref="ISequence{T}.AsReadOnly"/> method
	/// creates a delegate view instance that will serialize if the parent collection is serialized, but
	/// no other instances will serialize any data; and will raise exceptions if deserialized. (Note
	/// also that a view created by <see cref="ISequence{T}.AsReadOnly"/> will only serialize if the
	/// parent is serialized.)
	/// </summary>
	[DataContract]
	public sealed class ReadOnlySequence<T>
			: ISequenceView<T>
	{
		private ISequenceView<T> collection;

		/// <summary>
		/// Mutable. Is -1 if bound to the collection.
		/// </summary>
		[DataMember]
		private int thisStartIndex;

		/// <summary>
		/// Mutable. Is -1 if bound to the collection.
		/// </summary>
		[DataMember]
		private int thisRangeCount;


		/// <summary>
		/// Constructor creates a "live" view that always returns the full collection.
		/// </summary>
		/// <param name="collection">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySequence(ISequenceView<T> collection)
		{
			this.collection = collection ?? throw new ArgumentNullException(nameof(collection));
			thisStartIndex = -1;
			thisRangeCount = -1;
		}

		/// <summary>
		/// Constructor creates a range-limited view by index and count. Note that the given arguments
		/// are fixed: if the collection changes, the start index remains the smae index value;
		/// AND the count remaims the same: methods MAY raise exceptions if the delegate
		/// collection's count shrinks.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="startIndex">The start index for the view.</param>
		/// <param name="rangeCount">The rangeCount for the view.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySequence(ISequenceView<T> collection, int startIndex, int rangeCount)
		{
			this.collection = collection ?? throw new ArgumentNullException(nameof(collection));
			SetRange(startIndex, rangeCount);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void checkThisRangeCount(int rangeCount, int? maxValue = null)
		{
			if ((rangeCount < 0)
					|| (rangeCount > (maxValue ?? thisRangeCount))) {
				throw new ArgumentOutOfRangeException(
						nameof(rangeCount),
						rangeCount,
						$"Must be >= 0, <= {maxValue ?? thisRangeCount}");
			}
		}


		/// <summary>
		/// This method can be used to change the range on a range-limited view only. Notice
		/// that the arguments will be checked; AND this may NOT be used to change a
		/// range-limited view to a full live view, nor change a full live view to a range.
		/// </summary>
		/// <param name="startIndex">The new start index.</param>
		/// <param name="rangeCount">The new range count.</param>
		public void SetRange(int startIndex, int rangeCount)
		{
			if (thisStartIndex < 0)
				throw new InvalidOperationException("View cannot be changed to a range.");
			Sequence<T>.CheckRangeIndex(collection.Count, startIndex, rangeCount);
			thisStartIndex = startIndex;
			thisRangeCount = rangeCount;
		}

		/// <summary>
		/// Provided for deserialization for the parent.
		/// Sets this delegate collection ONLY IF it is currently null.
		/// </summary>
		/// <param name="parent">Not null.</param>
		internal void ResetCollection(ISequenceView<T> parent)
		{
			if (collection != null)
				throw new ArgumentException("Collection cannot be changed.", nameof(parent));
			collection = parent ?? throw new ArgumentNullException(nameof(parent));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
			=> thisStartIndex < 0
					? collection.GetEnumerator()
					: collection.GetEnumerator(thisStartIndex, thisRangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator(int startIndex, int rangeCount)
		{
			if (thisStartIndex >= 0) {
				checkThisRangeCount(rangeCount, thisRangeCount - startIndex);
				startIndex += thisStartIndex;
			}
			return collection.GetEnumerator(startIndex, rangeCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator()
		{
			int count = Count;
			return GetReverseEnumerator(
					count == 0
							? 0
							: count - 1,
					count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator(int startIndex, int rangeCount)
		{
			if (thisStartIndex >= 0) {
				checkThisRangeCount(rangeCount, startIndex + 1);
				startIndex += thisStartIndex;
			}
			return collection.GetReverseEnumerator(startIndex, rangeCount);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> CreateReadOnlyView(int startIndex, int rangeCount)
		{
			if (thisStartIndex > 0)
				startIndex += thisStartIndex;
			return new ReadOnlySequence<T>(collection, startIndex, rangeCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<TOut> CreateVariantView<TOut>(Func<T, TOut> variantFunc)
			=> new VariantSequenceView<T, TOut, ReadOnlySequence<T>>(this, variantFunc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceChain<TChain> CreateChain<TChain>(bool asStack, params ISequenceView<TChain>[] chain)
			=> new SequenceChain<TChain>(asStack, chain);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> Clone()
			=> new ImmutableSequence<T>(IsStack, this);

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => thisStartIndex < 0
					? collection.Count
					: thisRangeCount;
		}

		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => true;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => collection[thisStartIndex < 0
					? index
					: thisStartIndex + index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Array destination, int destinationIndex)
			=> CopyRangeTo(0, destination, destinationIndex, Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyRangeTo(int startIndex, Array destination, int destinationIndex, int rangeCount)
		{
			if (thisStartIndex >= 0) {
				checkThisRangeCount(rangeCount, thisRangeCount - startIndex);
				startIndex += thisStartIndex;
			}
			collection.CopyRangeTo(startIndex, destination, destinationIndex, rangeCount);
		}


		public bool IsStack
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => collection.IsStack;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek()
			=> thisStartIndex < 0
					? collection.Peek()
					: collection.PeekAt(thisStartIndex);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T PeekAt(int index)
			=> thisStartIndex < 0
					? collection.PeekAt(index)
					: collection.PeekAt(thisStartIndex + index);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Poke()
			=> thisStartIndex < 0
					? collection.Poke()
					: collection.PeekAt((thisStartIndex + thisRangeCount) - 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray()
			=> thisStartIndex < 0
					? collection.ToArray()
					: collection.ToArray(thisStartIndex, thisRangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray(int startIndex, int rangeCount)
		{
			if (thisStartIndex < 0)
				return collection.ToArray(startIndex, rangeCount);
			checkThisRangeCount(rangeCount, thisRangeCount - startIndex);
			return collection.ToArray(thisStartIndex + startIndex, rangeCount);
		}

		public int Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => collection.Version;
		}
	}
}
