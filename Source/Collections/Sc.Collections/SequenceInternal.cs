using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;


namespace Sc.Collections
{
	public partial class Sequence<T>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
				"Code Quality",
				"IDE0051:Remove unused private members",
				Justification = "DEBUG")]
		private bool checkIsValid()
		{
			if ((head >= array.Length)
					|| (tail >= array.Length)
					|| (head < 0)
					|| (tail < 0)) {
				return false;
			}
			if ((count == 0)
					|| (count == array.Length)) {
				return head == tail;
			}
			if (head == tail)
				return false;
			if (head < tail)
				return count == (tail - head);
			return count == ((array.Length - head) + tail);
		}


		/// <summary>
		/// Returns the valid pointer to the array index at the specified collection index. This MUST
		/// be invoked with the current pointer values --- before the count or pointers are modified.
		/// </summary>
		/// <param name="index">Must be valid, including comparing to count --- is not checked.</param>
		/// <returns>An <c>array</c> index.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int getPointerAt(int index)
		{
			int result = tail - (Count - index);
			return result < 0
					? head + index
					: result;
		}

		/// <summary>
		/// The method implements <see cref="SetCapacity"/> and any other range-copy methods. NOTICE: the
		/// arguments are not checked. This method also implements <see cref="DequeueRange"/> and
		/// <see cref="DropRange"/>: if one is set true, then the copied elements are cleared, and the
		/// count is changed; and this will increment the version.
		/// </summary>
		/// <param name="startIndex">A logical INDEX in this Collection.</param>
		/// <param name="target">The target array.</param>
		/// <param name="targetIndex">An Array index in the <c>target</c> Array.</param>
		/// <param name="rangeCount">MUST be &lt;= Count AND &lt;= <c>(target.Length - targetIndex)</c>.</param>
		/// <param name="isDequeueRange">Used by <see cref="DequeueRange"/>: if true, then the elements that
		/// are copied are CLEARED from this collection; AND, if true, <c>startIndex</c> MUST be 0, and
		/// <c>isDropRange</c> must be false.</param>
		/// <param name="isDropRange">Used by <see cref="DropRange"/>: if true, then the elements that
		/// are copied are CLEARED from this collection; AND, if true, <c>startIndex</c> MUST be
		/// (count - rangeCount), and <c>isDequeueRange</c> must be false.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void doUncheckedCopyRange(
				int startIndex,
				Array target,
				int targetIndex,
				int rangeCount,
				bool isDequeueRange = false,
				bool isDropRange = false)
		{
			// TODO: if Buffer.BlockCopy is guaranteed to succeed when isElementTypeValueType is true, then use that
			Debug.Assert(rangeCount <= count);
			Debug.Assert(rangeCount <= (target.Length - targetIndex));
			Debug.Assert(!isDequeueRange || ((startIndex == 0) && !isDropRange));
			Debug.Assert(!isDropRange || ((startIndex == (count - rangeCount)) && !isDequeueRange));
			if (rangeCount <= 0)
				return;
			unchecked {
				int endPointer = getPointerAt(startIndex + (rangeCount - 1));
				startIndex = getPointerAt(startIndex);
				if (startIndex <= endPointer) {
					Array.Copy(array, startIndex, target, targetIndex, rangeCount);
					if (!isDequeueRange
							&& !isDropRange)
						return;
					if (!isElementTypeValueType)
						Array.Clear(array, startIndex, rangeCount);
				} else {
					int headCount = array.Length - startIndex;
					Array.Copy(array, startIndex, target, targetIndex, headCount);
					Array.Copy(array, 0, target, targetIndex + headCount, rangeCount - headCount);
					if (!isDequeueRange
							&& !isDropRange)
						return;
					if (!isElementTypeValueType) {
						Array.Clear(array, startIndex, headCount);
						Array.Clear(array, 0, rangeCount - headCount);
					}
				}
				++Version;
				if (rangeCount == count) {
					head = tail = count = 0;
					return;
				}
				if (isDequeueRange)
					head = getPointerAt(rangeCount); // Before count changes
				else {
					tail = getPointerAt(count - (rangeCount + 1)); // Before count changes
					if (tail == (array.Length - 1))
						tail = 0;
					else
						++tail;
				}
				count -= rangeCount;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void insertNullUnchecked(int nullElementCount, bool moveHead)
		{
			Debug.Assert(array.Length >= (Count + nullElementCount));
			Debug.Assert(nullElementCount <= (array.Length - Count));
			unchecked {
				++Version;
				if (moveHead) {
					head -= nullElementCount;
					if (head < 0)
						head += array.Length;
				} else {
					tail += nullElementCount;
					if (tail >= array.Length)
						tail -= array.Length;
				}
				count += nullElementCount;
			}
		}


		/// <summary>
		/// Grows or shrinks the buffer to exactly <c>capacity</c>.
		/// </summary>
		/// <param name="capacity">Must be >= Count.</param>
		/// <param name="forcePackArray">If false, and if the array Length is currently
		/// equal to <c>capacity</c>, then nothing happens. If true, the array will be copied: in
		/// other cases, the array size needs to change, and so it is copied; and whenever copied,
		/// elements are arranged in sequence at the beginning or end of the new Array (based on
		/// <see cref="ISequenceView{T}.IsStack"/>: at the end of the array if a Stack, and else the
		/// beginning).</param>
		/// <returns>True if the Array has been modified; and the Version has been incremented; and
		/// when true, the array has always been re-packed.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool SetCapacity(int capacity, bool forcePackArray = false)
		{
			if (capacity < count) {
				throw new ArgumentOutOfRangeException(
						nameof(capacity),
						$"{nameof(capacity)} must be >= Count ({Count})");
			}
			if (capacity > Sequence<T>.ArrayMaxLength)
				throw new InvalidOperationException("The Collection's capacity would exceed the maximum length.");
			if ((capacity == array.Length)
					&& !forcePackArray)
				return false;
			unchecked {
				++Version;
				T[] newarray = new T[capacity];
				if (count == 0)
					head = tail = 0;
				else if (IsStack) {
					doUncheckedCopyRange(0, newarray, capacity - count, count);
					head = capacity - count;
					tail = 0;
				} else {
					doUncheckedCopyRange(0, newarray, 0, count);
					head = 0;
					tail = count == capacity
							? 0
							: count;
				}
				array = newarray;
				return true;
			}
		}


		/// <summary>
		/// Ensures that the underlying array is at least as large as the argument. If this method
		/// must grow the underlying Array, it is set to exactly the given capacity --- the
		/// <see cref="GrowFactor"/> is not used. Note that this will throw if the capacity exceeds
		/// <see cref="ArrayMaxLength"/>.
		/// </summary>
		/// <param name="capacity">Non-negative.</param>
		/// <returns>True if the Array has been modified; and the Version has been incremented.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="InvalidOperationException">If the <c>capacity</c> is larger than
		/// <see cref="ArrayMaxLength"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool EnsureCapacity(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), capacity.ToString());
			if (capacity > Sequence<T>.ArrayMaxLength) {
				throw new InvalidOperationException(
						"The Collection's capacity would exceed the maximum length.");
			}
			return (capacity > array.Length) && SetCapacity(capacity);
		}

		/// <summary>
		/// As with <see cref="EnsureCapacity"/>, but this will grow the Array using the grow factor.
		/// Note that this will throw if the new minimum capacity would exceed <see cref="ArrayMaxLength"/>.
		/// </summary>
		/// <param name="elementsToBeAdded">Elements beyond the current count to ensure.</param>
		/// <returns>True if the Array has been modified; and the Version has been incremented.</returns>
		/// <exception cref="InvalidOperationException">If the new minimum capacity would be larger than
		/// <see cref="ArrayMaxLength"/>.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGrowCapacity(int elementsToBeAdded = 1)
		{
			int newCount = count + Math.Max(0, elementsToBeAdded);
			if ((newCount > Sequence<T>.ArrayMaxLength)
					|| (newCount < 0)) {
				throw new InvalidOperationException(
						"The Collection's Count has reached the maximum length.");
			}
			if (newCount <= array.Length)
				return false;
			do {
				elementsToBeAdded = (int)Math.Ceiling(Math.Max(1L, array.Length) * GrowFactor);
				if ((elementsToBeAdded > Sequence<T>.ArrayMaxLength)
						|| (elementsToBeAdded < 0))
					elementsToBeAdded = Sequence<T>.ArrayMaxLength;
			} while (elementsToBeAdded < newCount);
			return SetCapacity(elementsToBeAdded);
		}


		/// <summary>
		/// This method grows the Count of the collection by the specified value; and simply adds default
		/// elements to the collection, as if by <see cref="ISequence{T}.Add"/>. This method operates
		/// efficiently because it simply grows the capacity of the buffer if needed, and then moves
		/// a pointer and increments the count by the argument.
		/// </summary>
		/// <param name="nullElementCount">The count by which to grow the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddNull(int nullElementCount)
		{
			if (nullElementCount < 0)
				throw new ArgumentOutOfRangeException(nameof(nullElementCount));
			TryGrowCapacity(nullElementCount);
			insertNullUnchecked(nullElementCount, IsStack);
		}

		/// <summary>
		/// This method grows the Count of the collection by the specified value; and simply adds default
		/// elements to the collection, as if by <see cref="ISequence{T}.InsertOldest"/>. This method operates
		/// efficiently because it simply grows the capacity of the buffer if needed, and then moves
		/// a pointer and increments the count by the argument.
		/// </summary>
		/// <param name="nullElementCount">The count by which to grow the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InsertNullOldest(int nullElementCount)
		{
			if (nullElementCount < 0)
				throw new ArgumentOutOfRangeException(nameof(nullElementCount));
			TryGrowCapacity(nullElementCount);
			insertNullUnchecked(nullElementCount, !IsStack);
		}


		/// <summary>
		/// This method copies a count of elements from the head of this collection into the given
		/// <c>target</c>, OVERWRITING elements there, and SETTING the <c>Count</c> of the
		/// <c>target</c> to the <c>count</c> transferred ONLY. The mode of the target is respected
		/// (Queue or Stack).
		/// </summary>
		/// <param name="target">Not null.</param>
		/// <param name="rangeCount">>= 0, &lt;= <see cref="Count"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PeekRangeInto(Sequence<T> target, int rangeCount)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if ((rangeCount < 0)
					|| (rangeCount > count))
				throw new ArgumentOutOfRangeException(nameof(rangeCount), rangeCount, $"Must be >= 0, <= {count}");
			if (rangeCount == 0) {
				target.Clear();
				return;
			}
			unchecked {
				++target.Version;
				if (target.array.Length <= rangeCount) {
					if (target.array.Length < rangeCount)
						target.array = new T[rangeCount];
					target.head = 0;
					target.tail = 0;
					target.count = rangeCount;
					doUncheckedCopyRange(0, target.array, 0, rangeCount);
					return;
				}
				if (target.IsStack) {
					target.head = target.array.Length - rangeCount;
					target.tail = 0;
					target.count = rangeCount;
					doUncheckedCopyRange(0, target.array, target.head, rangeCount);
					if (!target.isElementTypeValueType)
						Array.Clear(target.array, 0, target.array.Length - rangeCount);
					return;
				}
				target.head = 0;
				target.tail = rangeCount;
				target.count = rangeCount;
				doUncheckedCopyRange(0, target.array, 0, rangeCount);
				if (!target.isElementTypeValueType)
					Array.Clear(target.array, rangeCount, target.array.Length - rangeCount);
			}
		}


		/// <summary>
		/// Provides access to the <c>growFactor</c> that was specified on construction.
		/// </summary>
		[DataMember]
		public float GrowFactor
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		/// <summary>
		/// Provides access to the current underlying array capacity.
		/// </summary>
		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => array.Length;
		}
	}
}
