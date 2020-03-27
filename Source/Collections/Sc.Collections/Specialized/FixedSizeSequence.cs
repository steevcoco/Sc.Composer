using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.Specialized;
using Sc.Util.System;


namespace Sc.Collections.Specialized
{
	/// <summary>
	/// An <see cref="IFixedSizeSequence{T}"/> implementation. This class implements both
	/// <see cref="IFixedSizeQueue{T}"/> and <see cref="IFixedSizeStack{T}"/>; and as with
	/// <see cref="Sequence{T}"/>, you should expose the object as the intended interface; OR prefer
	/// to use <see cref="IFixedSizeSequence{T}"/> methods, which allow you to "swap" implementations.
	/// This class is serializable, but the Action is not serialized: you may restore the object
	/// on deserialization.
	/// </summary>
	[DataContract]
	public sealed class FixedSizeSequence<T>
			: Sequence<T>,
					IFixedSizeQueue<T>,
					IFixedSizeStack<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="maximumSize">The maximum allowed <see cref="ISequenceView{T}.Count"/>
		/// of the Collection; enforced only by <see cref="FixedEnqueue"/> and
		/// <see cref="FixedPush"/> methods (and <see cref="FixedAdd"/>). The underlying buffer will
		/// be created here with this size; and will not change unless the fixed size is changed;
		/// or if <see cref="ISequence{T}.TrimToSize"/> is invoked. This must be <c>>= 1</c>.</param>
		/// <param name="overhead">The number of elements removed when the count will exceed
		/// the maximumSize. This must be <c>>= 0, &lt;= maximumSize</c>. Note that you may specify
		/// zero for this value --- the buffer will exhibit "strict" "circular" behavior, removing
		/// the next element each time one new element is added. When the value is positive,
		/// the count is reduced below the maximum size by this value when an add operation
		/// would exceed the maximum size.</param>
		/// <param name="isStack">The mode for the Collection: if true, this is an
		/// <see cref="IStack{T}"/> else an <see cref="IQueue{T}"/>.</param>
		/// <param name="onRemoveOverhead">An optional <see cref="Action{T}"/> that will be invoked
		/// from <see cref="FixedEnqueue"/> and <see cref="FixedPush"/> (and <see cref="FixedAdd"/>)
		/// when elements are removed. You may handle the removed elements this way. The argument
		/// is the result of either <see cref="IQueue{T}.DequeueRange"/> or
		/// <see cref="IStack{T}.DropRange"/>.</param>
		/// <param name="growFactor">When the array must grow, it's current size is multiplied
		/// by this value to create a new array. &gt; 1.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FixedSizeSequence(
				int maximumSize,
				int overhead,
				bool isStack,
				Action<T[]> onRemoveOverhead = null,
				float growFactor = 2F)
				: base(isStack, maximumSize, growFactor)
		{
			trySetFixedSizeProperties(maximumSize, overhead);
			OnRemoveOverhead = onRemoveOverhead;
		}

		/// <summary>
		/// Creates a Collection by EITHER copying the elements from the argument array, OR retaining
		/// THE ACTUAL ARRAY as the current backing store. Note that the array will be released and
		/// recreated if the collection grows or is trimmed. The enumeration order will be retained:
		/// the current first item in the argument will be the first element in this collection.
		/// ALSO: if the given array is larger than the <paramref name="maximumSize"/>, then
		/// elements ARE retained now; and will be dropped on the first mutation.
		/// </summary>
		/// <param name="array">Not null.</param>
		/// <param name="keepArray">If true, the the actual array is used now; and if false, elements
		/// are copied now to a new array.</param>
		/// <param name="maximumSize">The maximum allowed <see cref="ISequenceView{T}.Count"/>
		/// of the Collection; enforced only by <see cref="FixedEnqueue"/> and
		/// <see cref="FixedPush"/> methods (and <see cref="FixedAdd"/>). The underlying buffer will
		/// be created here with this size; and will not change unless the fixed size is changed;
		/// or if <see cref="ISequence{T}.TrimToSize"/> is invoked. This must be <c>>= 1</c>.</param>
		/// <param name="overhead">The number of elements removed when the count will exceed
		/// the maximumSize. This must be <c>>= 0, &lt;= maximumSize</c>. Note that you may specify
		/// zero for this value --- the buffer will exhibit "strict" "circular" behavior, removing
		/// the next element each time one new element is added. When the value is positive,
		/// the count is reduced below the maximum size by this value when an add operation
		/// would exceed the maximum size.</param>
		/// <param name="isStack">Sets the mode of this Collection, which will be honored by
		/// interface implementations: if true, this will be a Stack, otherwise a Queue.</param>
		/// <param name="onRemoveOverhead">An optional <see cref="Action{T}"/> that will be invoked
		/// from <see cref="FixedEnqueue"/> and <see cref="FixedPush"/> (and <see cref="FixedAdd"/>)
		/// when elements are removed. You may handle the removed elements this way. The argument
		/// is the result of either <see cref="IQueue{T}.DequeueRange"/> or
		/// <see cref="IStack{T}.DropRange"/>.</param>
		/// <param name="growFactor">When the array must grow, it's current size is multiplied
		/// by this value to create a new array. &gt; 1.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException">If the array is incompatible.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FixedSizeSequence(
				T[] array,
				bool keepArray,
				int maximumSize,
				int overhead,
				bool isStack,
				Action<T[]> onRemoveOverhead = null,
				float growFactor = 2F)
				: base(array, keepArray, isStack, growFactor)
		{
			trySetFixedSizeProperties(maximumSize, overhead);
			OnRemoveOverhead = onRemoveOverhead;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void trySetFixedSizeProperties(int maximumSize, int overhead)
		{
			if (maximumSize < 1)
				throw new ArgumentOutOfRangeException(nameof(maximumSize), maximumSize, "Must be >= 1.");
			if ((overhead > maximumSize)
					|| (overhead < 0))
				throw new ArgumentOutOfRangeException(nameof(overhead), overhead, "Must be >= 0, <= maximumSize.");
			MaximumSize = maximumSize;
			Overhead = overhead;
		}


		/// <summary>
		/// This property defaults to false. If set true, then this will invoke
		/// an internal SetCapacity method when any
		/// <see cref="FixedAddRange(ISequenceView{T},bool,int?,bool)"/> method is invoked.
		/// This explicitly sets the capacity of the underlying buffer to the
		/// <see cref="MaximumSize"/> --- which may incur an array copy operation.
		/// </summary>
		public bool SetCapacityOnFixedAddRange { get; set; }


		[DataMember]
		public int MaximumSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		[DataMember]
		public int Overhead
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private set;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] SetFixedSize(int maximumSize, int overhead, bool setCapacity = false)
		{
			trySetFixedSizeProperties(maximumSize, overhead);
			T[] removed = Count <= maximumSize
					? new T[0]
					: IsStack
							? DropRange(Count - maximumSize)
							: DequeueRange(Count - maximumSize);
			if (setCapacity)
				SetCapacity(maximumSize);
			return removed;
		}

		/// <summary>
		/// This method performs a <see cref="Debug.Assert(bool)"/>, and fails if the current
		/// <see cref="ISequenceView{T}.Count"/> is greater than the <see cref="MaximumSize"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void FixedEnqueue(T element)
		{
			if (Count >= MaximumSize) {
				Debug.Assert(
						Count == MaximumSize,
						$"{GetType().GetFriendlyName()}: the fixed-size Queue contains more elements ({Count}) "
						+ $"than its MaximumSize ({MaximumSize}).");
				T[] removed = DequeueRange(Math.Min(Count, (Count - MaximumSize) + Overhead + 1));
				if (SetCapacityOnFixedAddRange)
					SetCapacity(MaximumSize);
				OnRemoveOverhead?.Invoke(removed);
			}
			Enqueue(element);
		}

		/// <summary>
		/// This method performs a <see cref="Debug.Assert(bool)"/>, and fails if the current
		/// <see cref="ISequenceView{T}.Count"/> is > <see cref="MaximumSize"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void FixedPush(T element)
		{
			if (Count >= MaximumSize) {
				Debug.Assert(
						Count == MaximumSize,
						$"{GetType().GetFriendlyName()}: the fixed-size Stack contains more elements ({Count}) "
						+ $"than its MaximumSize ({MaximumSize}).");
				T[] removed = DropRange(Math.Min(Count, (Count - MaximumSize) + Overhead + 1));
				if (SetCapacityOnFixedAddRange)
					SetCapacity(MaximumSize);
				OnRemoveOverhead?.Invoke(removed);
			}
			Push(element);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void FixedAdd(T element)
		{
			if (IsStack)
				FixedPush(element);
			else
				FixedEnqueue(element);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void FixedAddRange(
				ISequenceView<T> collection,
				bool enumerateInOrder = false,
				int? addCount = null,
				bool countInOrder = false)
		{
			AddRange(collection, enumerateInOrder, addCount, countInOrder);
			if (Count <= MaximumSize)
				return;
			T[] removed
					= IsStack
							? DropRange((Count - MaximumSize) + Overhead)
							: DequeueRange((Count - MaximumSize) + Overhead);
			if (SetCapacityOnFixedAddRange)
				SetCapacity(MaximumSize);
			OnRemoveOverhead?.Invoke(removed);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void FixedAddRange(IEnumerable<T> collection, int? addCount = null)
		{
			AddRange(collection, addCount);
			if (Count <= MaximumSize)
				return;
			T[] removed
					= IsStack
							? DropRange((Count - MaximumSize) + Overhead)
							: DequeueRange((Count - MaximumSize) + Overhead);
			if (SetCapacityOnFixedAddRange)
				SetCapacity(MaximumSize);
			OnRemoveOverhead?.Invoke(removed);
		}

		public Action<T[]> OnRemoveOverhead
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}
	}
}
