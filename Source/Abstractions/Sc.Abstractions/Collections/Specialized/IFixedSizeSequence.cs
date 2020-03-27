using System;
using System.Collections.Generic;


namespace Sc.Abstractions.Collections.Specialized
{
	/// <summary>
	/// An <see cref="ISequence{T}"/> that limits the size of the Collection to a given Count of the
	/// most recent elements. NOTICE: you MUST use the <see cref="FixedAdd"/> method, or the equivalent
	/// <see cref="IFixedSizeQueue{T}.FixedEnqueue"/> or <see cref="IFixedSizeStack{T}.FixedPush"/> methods
	/// to restrict the size. All other methods operate in the normally-implemented way; and WILL NOT
	/// 
	/// restrict the size. The collection takes a <see cref="MaximumSize"/>, and an <see cref="Overhead"/>:
	/// when the size will exceed the maximum, an element count equal to the overhead is removed.
	/// The Collection operates as either a Queue or a Stack: when elements are removed by a
	/// <see cref="IFixedSizeQueue{T}.FixedEnqueue"/> operation, they are dequeued by
	/// <see cref="IQueue{T}.DequeueRange"/>; and when removed by <see cref="IFixedSizeStack{T}.FixedPush"/>,
	/// they are dropped by <see cref="ISequence{T}.DropRange"/>. Notice that as with <see cref="ISequence{T}"/>,
	/// if you use the methods on this interface, each one always opertes based on the mode of the underlying
	/// implementation, which allows you to swap implementations.
	/// </summary>
	public interface IFixedSizeSequence<T>
			: ISequence<T>
	{
		/// <summary>
		/// Returns the current fixed maximum Count of this Collection. Note that this size is only honored
		/// by the <see cref="FixedAdd"/> method, or the equivalent <see cref="IFixedSizeQueue{T}.FixedEnqueue"/>
		/// or <see cref="IFixedSizeStack{T}.FixedPush"/> methods.
		/// </summary>
		/// <returns>The maximum Count.</returns>
		int MaximumSize { get; }

		/// <summary>
		/// Returns the count of elements that are removed from the Collection when the
		/// <see cref="IReadOnlyCollection{T}.Count"/> will exceed <see cref="MaximumSize"/>.
		/// </summary>
		/// <returns>The overhead Count.</returns>
		int Overhead { get; }

		/// <summary>
		/// Sets the <see cref="MaximumSize"/> of this Collection; and the <see cref="Overhead"/>.
		/// </summary>
		/// <param name="maximumSize">The enforced maximum Count. >= 1.</param>
		/// <param name="overhead">Elements removed when the Count must be brought back below the
		/// maximumSize. >= 0, &lt; maximumSize. Note that you may specify
		/// zero for this value --- the buffer will exhibit "strict" "circular" behavior, removing
		/// the next element each time one new element is added. When the value is positive,
		/// the count is reduced below the maximum size by this value when an add operation
		/// would exceed the maximum size.</param>
		/// <param name="setCapacity">Optional: if set true, ths will set the underlying array
		/// buffer to the <c>maximumSize</c>.</param>
		/// <returns>Any elements removed here to bring the Count below the maximum Size.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If the arguments do not meet the
		/// documented bounds.</exception>
		T[] SetFixedSize(int maximumSize, int overhead, bool setCapacity = false);

		/// <summary>
		/// Appends the element to the sequence while limiting the Collection's Count: NOTICE this method
		/// operates differently based on the type of the Collection. The value is always added as the
		/// "newest" value: for an <see cref="IFixedSizeQueue{T}"/> it is appended with
		/// <see cref="IFixedSizeQueue{T}.FixedEnqueue"/>; and for <see cref="IFixedSizeStack{T}"/> it is added
		/// with <see cref="IFixedSizeStack{T}.FixedPush"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		void FixedAdd(T element);

		/// <summary>
		/// As with <see cref="ISequence{T}.AddRange(ISequenceView{T},bool,int?,bool)"/>, this method adds all
		/// elements from the argument to this collection, and limits this collection's count. Elements
		/// are added as documented by the <see cref="ISequence{T}.AddRange(ISequenceView{T},bool,int?,bool)"/>
		/// method; and this method will emit any elements that exceed the <see cref="MaximumSize"/> from the
		/// <see cref="OnRemoveOverhead"/> delegate.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="enumerateInOrder">Defaults to false: if this collection is a Stack, the
		/// argument's elements will be enumerated in reverse. If true, the argument's elements are
		/// always enumerated in the order returned by that collection.</param>
		/// <param name="addCount">This optional argument can restrict the count of added elements.
		/// This defaults to null, and if null --- or if the value is greater than the count of the
		/// argument <c>collection</c> --- then all elements are added.</param>
		/// <param name="countInOrder">Only applies if <c>addCount</c> is specified; and defaults
		/// to false. If set true, and if the collection is enumerated in reverse, then the
		/// enumeration will begin from the eleent at index <c>addCount - 1</c>, and proceed
		/// to zero. Notice that when false -- the DEFAULT --- and the collection IS enumerated
		/// in reverse, then the last-enumerated elements in the WHOLE collection are the elements
		/// added.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void FixedAddRange(
				ISequenceView<T> collection,
				bool enumerateInOrder = false,
				int? addCount = null,
				bool countInOrder = false);

		/// <summary>
		/// As with <see cref="FixedAddRange(ISequenceView{T},bool,int?,bool)"/>, this method adds all elements
		/// from the argument to this collection, and limits this collection's count. Elements are added as
		/// documented by the <see cref="ISequence{T}.AddRange(IEnumerable{T},int?)"/> method; and this method
		/// will emit any elements that exceed the <see cref="MaximumSize"/> from the
		/// <see cref="OnRemoveOverhead"/> delegate.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="addCount">This optional argument can restrict the count of added elements.
		/// This defaults to null, and if null --- or if the value is greater than the count of the
		/// argument <c>collection</c> --- then all elements are added.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void FixedAddRange(IEnumerable<T> collection, int? addCount = null);

		/// <summary>
		/// This is an optional <see cref="Action{T}"/> that will be invoked from <see cref="FixedAdd"/>
		/// (and <see cref="IFixedSizeQueue{T}.FixedEnqueue"/> or <see cref="IFixedSizeStack{T}.FixedPush"/>),
		/// when elements are removed. You may handle the removed elements this way. The argument is the
		/// result of either <see cref="IQueue{T}.DequeueRange"/> or <see cref="ISequence{T}.DropRange"/>.
		/// </summary>
		Action<T[]> OnRemoveOverhead { get; set; }
	}
}
