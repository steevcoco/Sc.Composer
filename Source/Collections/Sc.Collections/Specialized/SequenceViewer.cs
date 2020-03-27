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
	/// Implements a read only wrapper around an <see cref="ISequenceView{T}"/>,
	/// and provides a view that can be specified as a range index and count from
	/// the viewed collection; and also supports a reversed view of the
	/// collection or any given range in the collection. Note that this
	/// collection is serializable, BUT the wrapped collection is NOT serialized:
	/// you MUST restore the collection with <see cref="SetCollection"/>.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	[DataContract]
	public class SequenceViewer<T>
			: ISequenceView<T>
	{
		/// <summary>
		/// Constructor creates an "unlimited" view that always returns the full collection.
		/// The <see cref="RangeStartIndex"/> and <see cref="RangeCount"/> properties
		/// are set to <c>-1</c>, which is what specifies an "unlimited" view here.
		/// You may change the range at any time. Note that in this mode, this view
		/// always mirrors the whole viewed collection: if it's Count changes,
		/// this view still always returns all elements at all times. Those
		/// properties will also hold true if this is set to a <see cref="Reverse"/>
		/// view ---- the view returns all elements, but enumerates in reverse;
		/// and the "unlimited" range always returns all elements.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="reverse">Optionally sets the value of <see cref="Reverse"/>;
		/// defaults to <see langword="false"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceViewer(ISequenceView<T> collection, bool reverse = false)
		{
			ViewedCollection = collection ?? throw new ArgumentNullException(nameof(collection));
			Reverse = reverse;
			RangeStartIndex = -1;
			RangeCount = -1;
		}

		/// <summary>
		/// Constructor creates a range-limited view by index and count. Note that the given
		/// arguments are fixed: if the collection changes, the start index remains the same
		/// index value; AND the count remains the same: methods MAY raise exceptions if the
		/// viewed collection's count shrinks. You can change the range at any time; and you
		/// can validate and optionally adjust the range with <see cref="CheckRange"/>.
		/// This class never adjusts the range values here (<see cref="CheckRange"/>
		/// WILL make changes if you specify [and it does by default when invoked],
		/// and the same is true for <see cref="SetCollection"/> when invoked; but no other
		/// methods ever make changes to your explicit values) The accepted argument values
		/// are as defined by <see cref="SetRange"/>: if either is negative, then an
		/// "unlimited" range view is created: see <see cref="SetRange(int, int)"/> for more.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="startIndex">The start index for the view: see
		/// <see cref="SetRange(int, int)"/> for more.</param>
		/// <param name="rangeCount">The rangeCount for the view: see
		/// <see cref="SetRange(int, int)"/> for more.</param>
		/// <param name="reverse">Optionally sets the value of <see cref="Reverse"/>;
		/// defaults to <see langword="false"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceViewer(ISequenceView<T> collection, int startIndex, int rangeCount, bool reverse = false)
		{
			ViewedCollection = collection ?? throw new ArgumentNullException(nameof(collection));
			Reverse = reverse;
			RangeStartIndex = -1;
			RangeCount = -1;
			SetRange(startIndex, rangeCount);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void checkThisRangeCount(int rangeCount, int? maxValue = null)
		{
			if ((rangeCount < 0)
					|| (rangeCount > (maxValue ?? RangeCount))) {
				throw new ArgumentOutOfRangeException(
						nameof(rangeCount),
						rangeCount,
						$"Must be >= 0, <= {maxValue ?? RangeCount}");
			}
		}


		/// <summary>
		/// This is the actual viewed collection.
		/// </summary>
		public ISequenceView<T> ViewedCollection
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		/// <summary>
		/// If set true, this view provides a reversed view of the <see cref="ViewedCollection"/>.
		/// All accessors return values in the reverse order. Note that if this view's
		/// range is set, then the range from the underlying collection does not change:
		/// that specified range is provided as a reversed view here.
		/// </summary>
		[DataMember]
		public bool Reverse
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}

		/// <summary>
		/// If this view has been set to a range view of the <see cref="ViewedCollection"/>,
		/// this is the start index within the underlying collection. If this is an unlimited
		/// view, then this value is <c>-1</c>.
		/// </summary>
		[DataMember]
		public int RangeStartIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		/// <summary>
		/// If this view has been set to a range view of the <see cref="ViewedCollection"/>,
		/// this is the count within the underlying collection. If this is an unlimited
		/// view, then this value is <c>-1</c>.
		/// </summary>
		[DataMember]
		public int RangeCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		/// <summary>
		/// This method can be used to set or change the viewed range in the
		/// <see cref="ViewedCollection"/> that this view returns. Notice that the arguments
		/// will be checked; AND, to set this view to an unlimited live view of the collection,
		/// pass a negative value for either argument. Notice also that the values are
		/// always in terms of the underlying collection's actual count: this view's value of
		/// <see cref="Reverse"/> does not apply to these values, but affects the resulting
		/// enumeration of this range from the underlying collection. Note also that any range
		/// enumeration returned from THIS view is a range in terms of THIS view's Count,
		/// within this range of elements in the underlying collection.
		/// </summary>
		/// <param name="startIndex">The new start index within the <see cref="ViewedCollection"/>,
		/// OR, a negative value to create an unlimited live view.</param>
		/// <param name="rangeCount">The new range count within the <see cref="ViewedCollection"/>,
		/// OR, a negative value to create an unlimited live view.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void SetRange(int startIndex, int rangeCount)
		{
			if ((startIndex < 0)
					|| (rangeCount < 0)) {
				RangeStartIndex = -1;
				RangeCount = -1;
				return;
			}
			Sequence<T>.CheckRangeIndex(ViewedCollection.Count, startIndex, rangeCount);
			RangeStartIndex = startIndex;
			RangeCount = rangeCount;
		}

		/// <summary>
		/// This method is provided to ADJUST the specified <see cref="RangeStartIndex"/>
		/// and <see cref="RangeCount"/> values now. If the range is unlimited, then
		/// nothing happens. Otherwise the <see cref="RangeStartIndex"/> is checked
		/// first: if it falls outside the collection Count, then this start index is set
		/// to the last element, AND this Count is set to ZERO. If the start index is
		/// valid, then the <see cref="RangeCount"/> is checked, and will be reduced,
		/// possibly to zero to fall within the collection's Count.
		/// </summary>
		/// <param name="adjustNow">Defaults to TRUE: the range values WILL be CHANGED
		/// now as documented. If set false, the method reports the state, and
		/// makes no changes.</param>
		/// <returns>True if both current range values are valid, and NO adjustments
		/// are made; and otherwise false.</returns>
		public bool CheckRange(bool adjustNow = true)
		{
			if (RangeStartIndex < 0)
				return true;
			if (RangeStartIndex >= ViewedCollection.Count) {
				if (!adjustNow)
					return false;
				if (ViewedCollection.Count == 0) {
					RangeStartIndex = 0;
					RangeCount = 0;
					return false;
				}
				RangeStartIndex = ViewedCollection.Count - 1;
				RangeCount = 0;
				return false;
			}
			if (RangeCount <= (ViewedCollection.Count - RangeStartIndex))
				return true;
			if (adjustNow)
				RangeCount = ViewedCollection.Count - RangeStartIndex;
			return false;
		}

		/// <summary>
		/// Can be used to change the underlying collection. NOTICE that this WILL
		/// ADJUST the range by default if it is not unlimited and the values are not
		/// valid for the new collection --- but no other methods here ever make
		/// adjustments to the range (except <see cref="CheckRange"/>). This method
		/// always invokes <see cref="CheckRange"/> here, and returns that result:
		/// this <paramref name="adjustRange"/> argument is passed to that method;
		/// and if true, the range WILL be changed if not valid --- if you set this
		/// false, the range will not be changed, but may become invalid now.
		/// This method then then always returns the result of <see cref="CheckRange"/>.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="adjustRange">Defaults to TRUE: the range values WILL be CHANGED
		/// now if not valid, as documented by <see cref="CheckRange"/>. If set false,
		/// the method reports the state, and makes no changes.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public bool SetCollection(ISequenceView<T> collection, bool adjustRange = true)
		{
			ViewedCollection = collection ?? throw new ArgumentNullException(nameof(collection));
			return CheckRange(adjustRange);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
			=> RangeStartIndex < 0
					? Reverse
							? ViewedCollection.GetReverseEnumerator()
							: ViewedCollection.GetEnumerator()
					: Reverse
							? ViewedCollection.GetReverseEnumerator(
									RangeStartIndex + Math.Max(0, RangeCount - 1),
									RangeCount)
							: ViewedCollection.GetEnumerator(RangeStartIndex, RangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator(int startIndex, int rangeCount)
		{
			if (RangeStartIndex >= 0)
				checkThisRangeCount(rangeCount, RangeCount - startIndex);
			return RangeStartIndex < 0
					? Reverse
							? ViewedCollection.GetReverseEnumerator(
									Math.Max(0, ViewedCollection.Count - 1) - startIndex,
									rangeCount)
							: ViewedCollection.GetEnumerator(startIndex, rangeCount)
					: Reverse
							? ViewedCollection.GetReverseEnumerator(
									(RangeStartIndex + Math.Max(0, RangeCount - 1)) - startIndex,
									rangeCount)
							: ViewedCollection.GetEnumerator(RangeStartIndex + startIndex, rangeCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator()
			=> RangeStartIndex < 0
					? Reverse
							? ViewedCollection.GetEnumerator()
							: ViewedCollection.GetReverseEnumerator()
					: Reverse
							? ViewedCollection.GetEnumerator(
									RangeStartIndex,
									RangeCount)
							: ViewedCollection.GetReverseEnumerator(
									RangeStartIndex + Math.Max(0, RangeCount - 1),
									RangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator(int startIndex, int rangeCount)
		{
			if (RangeStartIndex >= 0)
				checkThisRangeCount(rangeCount, startIndex + 1);
			return RangeStartIndex < 0
					? Reverse
							? ViewedCollection.GetEnumerator(
									Math.Max(0, ViewedCollection.Count - 1) - startIndex,
									rangeCount)
							: ViewedCollection.GetReverseEnumerator(startIndex, rangeCount)
					: Reverse
							? ViewedCollection.GetEnumerator(
									(RangeStartIndex + Math.Max(0, RangeCount - 1)) - startIndex,
									rangeCount)
							: ViewedCollection.GetReverseEnumerator(RangeStartIndex + startIndex, rangeCount);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> CreateReadOnlyView(int startIndex, int rangeCount)
			=> new ReadOnlySequence<T>(this, startIndex, rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<TOut> CreateVariantView<TOut>(Func<T, TOut> variantFunc)
			=> new VariantSequenceView<T, TOut, SequenceViewer<T>>(this, variantFunc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceChain<TChain> CreateChain<TChain>(bool asStack, params ISequenceView<TChain>[] chain)
			=> new SequenceChain<TChain>(asStack, chain);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> Clone()
			=> new ImmutableSequence<T>(IsStack, this);

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RangeStartIndex < 0
					? ViewedCollection.Count
					: RangeCount;
		}

		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => true;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ViewedCollection[RangeStartIndex < 0
					? Reverse
							? Math.Max(0, ViewedCollection.Count - 1) - index
							: index
					: Reverse
							? (RangeStartIndex + Math.Max(0, RangeCount - 1)) - index
							: RangeStartIndex + index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Array destination, int destinationIndex)
			=> CopyRangeTo(0, destination, destinationIndex, Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyRangeTo(int startIndex, Array destination, int destinationIndex, int rangeCount)
		{
			if (RangeStartIndex >= 0)
				checkThisRangeCount(rangeCount, RangeCount - startIndex);
			if (RangeStartIndex < 0) {
				if (Reverse) {
					ViewedCollection.CopyRangeTo(
							Math.Max(0, ViewedCollection.Count - 1) - startIndex - Math.Max(0, rangeCount - 1),
							destination,
							destinationIndex,
							rangeCount);
					Array.Reverse(destination, destinationIndex, rangeCount);
				} else
					ViewedCollection.CopyRangeTo(startIndex, destination, destinationIndex, rangeCount);
			} else {
				if (Reverse) {
					ViewedCollection.CopyRangeTo(
							(RangeStartIndex + Math.Max(0, RangeCount - 1)) - startIndex - Math.Max(0, rangeCount - 1),
							destination,
							destinationIndex,
							rangeCount);
					Array.Reverse(destination, destinationIndex, rangeCount);
				} else {
					ViewedCollection.CopyRangeTo(
							RangeStartIndex + startIndex,
							destination,
							destinationIndex,
							rangeCount);
				}
			}
		}


		public bool IsStack
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ViewedCollection.IsStack;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek()
			=> RangeStartIndex < 0
					? Reverse
							? ViewedCollection.Poke()
							: ViewedCollection.Peek()
					: Reverse
							? ViewedCollection.PeekAt(RangeStartIndex + Math.Max(0, RangeCount - 1))
							: ViewedCollection.PeekAt(RangeStartIndex);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T PeekAt(int index)
			=> this[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Poke()
			=> RangeStartIndex < 0
					? Reverse
							? ViewedCollection.Peek()
							: ViewedCollection.Poke()
					: Reverse
							? ViewedCollection.PeekAt(RangeStartIndex)
							: ViewedCollection.PeekAt(RangeStartIndex + Math.Max(0, RangeCount - 1));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray()
		{
			T[] result = RangeStartIndex < 0
					? ViewedCollection.ToArray()
					: ViewedCollection.ToArray(RangeStartIndex, RangeCount);
			if (Reverse)
				Array.Reverse(result);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray(int startIndex, int rangeCount)
		{
			T[] result;
			if (RangeStartIndex < 0)
				result = ViewedCollection.ToArray(startIndex, rangeCount);
			else {
				checkThisRangeCount(rangeCount, RangeCount - startIndex);
				if (Reverse) {
					result = ViewedCollection.ToArray(
							(RangeStartIndex + Math.Max(0, RangeCount - 1)) - startIndex
							- Math.Max(0, rangeCount - 1),
							rangeCount);
				} else
					result = ViewedCollection.ToArray(RangeStartIndex + startIndex, rangeCount);
			}
			if (Reverse)
				Array.Reverse(result);
			return result;
		}

		public int Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ViewedCollection.Version;
		}
	}
}
