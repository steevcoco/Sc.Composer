using System;
using System.Collections.Generic;
using Sc.Abstractions.Collections.Specialized;


namespace Sc.Abstractions.Collections
{
	/// <summary>
	/// An <see cref="ISequenceView{T}"/> with mutable methods. NOTICE that the Collection operates
	/// in a selected mode: either as a Queue or a Stack: methods may behave differently based on the
	/// type of the Collection. In fact, this is a "feature". For instance: the <see cref="Add"/>
	/// method will add the element as the "newest" value: for an <see cref="IQueue{T}"/> it is appended
	/// with <see cref="IQueue{T}.Enqueue"/>; and for <see cref="IStack{T}"/> it is inserted with
	/// <see cref="Push"/>. This allows your implementation to replace other collections, and
	/// change the behavior, by replacing the implementation: ALL methods on this interface will
	/// operate based on the underlying implementation. Specialized implementations also exist for
	/// <see cref="IFixedSizeSequence{T}"/> and <see cref="ISequenceChain{T}"/>.
	/// </summary>
	/// <typeparam name="T">Element type. Unbounded.</typeparam>
	public interface ISequence<T>
			: ISequenceView<T>
	{
		/// <summary>
		/// Returns a singleton <see cref="ISequenceView{T}"/> that implements a read-only live
		/// view of this collection. The returned object is associated with this collection,
		/// and will be serialized and restored with this collection as a view; yet notice that
		/// if the returned view is serialized but this collection is not, then the view will
		/// not deserialize this collection: it will raise exceptions. If this collection is
		/// serialized, then the view can be serialized and restored as a reference.
		/// </summary>
		/// <returns>Not null.</returns>
		ISequenceView<T> AsReadOnly();

		/// <summary>
		/// Adds the element to the sequence: NOTICE this method operates differently based on
		/// the type of the Collection. The element is always added as the "newest" element: for an
		/// <see cref="IQueue{T}"/> it is appended with <see cref="IQueue{T}.Enqueue"/>; and for
		/// <see cref="IStack{T}"/> it is inserted with <see cref="Push"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		void Add(T element);

		/// <summary>
		/// Inserts the element in the sequence in the "oldest" position: NOTICE this method operates
		/// differently based on the type of the Collection. The element is always added as the "oldest"
		/// element: for an <see cref="IQueue{T}"/> it is inserted with <see cref="Push"/>;
		/// and for <see cref="IStack{T}"/> it is appended with <see cref="IStack{T}.Lift"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		void InsertOldest(T element);

		/// <summary>
		/// Inserts the element in the sequence in the position that puts this element first in the
		/// collection's enumeration mode. NOTICE this method operates differently based on the type
		/// of the Collection. The element is always added as the "first enumerated" element: for an
		/// <see cref="IStack{T}"/> it is inserted at the top, as expected by a "Push" operation
		/// (and is equivalent to <see cref="Add"/>). But for a <see cref="IQueue{T}"/> it is inserted
		/// at the head of the Queue --- which is equivalent to <see cref="InsertOldest"/> for the Queue.
		/// </summary>
		/// <param name="element">May be null.</param>
		void Push(T element);

		/// <summary>
		/// Adds the element to the sequence in the position that puts this element last in the
		/// collection's enumeration mode. NOTICE this method operates differently based on the type
		/// of the Collection. The element is always added as the "last enumerated" element: for an
		/// <see cref="IQueue{T}"/> it is appended with <see cref="IQueue{T}.Enqueue"/>; and for
		/// <see cref="IStack{T}"/> it is appended with <see cref="IStack{T}.Lift"/>. Note that the
		/// "inverse" operation to this one is always <see cref="Push"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		void Append(T element);

		/// <summary>
		/// This method adds specified elements from the argument to this collection. Elements are always added
		/// here as if by <see cref="Add"/> --- the mode of this collection is used: elements are either
		/// Enqueued or Pushed, in order. The <c>enumerateInOrder</c> argument is provided to control the
		/// enumeration order of the argument's elements. If THIS collection is a Queue, then the value
		/// will not be used, and is always set TRUE. If true, then the elements are always added in
		/// the order returned by that collection's enumerator. If false, which is the DEFAULT, AND if
		/// THIS collection is a Stack, then the argument is enumerated in reverse --- no matter what
		/// the mode of the argument. When the argument is false, this creates a chain that preserves the
		/// enumeration orders --- if this is a Stack, the argument will be "Pushed" onto this Stack in
		/// a preserved order, and if this is a Queue, the argument will be "Enqueued" onto this Queue
		/// in a preserved order.
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
		/// enumeration will begin from the element at index <c>addCount - 1</c>, and proceed
		/// to zero. Notice that when false -- the DEFAULT --- and the collection IS enumerated
		/// in reverse, then the last-enumerated elements in the WHOLE collection are the elements
		/// added.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void AddRange(
				ISequenceView<T> collection,
				bool enumerateInOrder = false,
				int? addCount = null,
				bool countInOrder = false);

		/// <summary>
		/// As with <see cref="AddRange(ISequenceView{T},bool,int?,bool)"/>, this method adds all elements from
		/// the argument to this collection. Elements are always added here as if by <see cref="Add"/>
		/// --- the mode of this collection is used: elements are either Enqueued or Pushed, in order
		/// --- and the elements are always added in the order returned by that collection's enumerator.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="addCount">This optional argument can restrict the count of added elements.
		/// This defaults to null, and if null --- or if the value is greater than the count of the
		/// argument <c>collection</c> --- then all elements are added.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void AddRange(IEnumerable<T> collection, int? addCount = null);

		/// <summary>
		/// As with <see cref="AddRange(ISequenceView{T},bool,int?,bool)"/>, but this method inserts
		/// the elements into the "oldest" position: for an <see cref="IQueue{T}"/> elements are
		/// inserted with <see cref="Push"/>; and for <see cref="IStack{T}"/> elements are appended
		/// with <see cref="IStack{T}.Lift"/>. The <c>enumerateInOrder</c> argument is provided to
		/// control the enumeration order of the argument's elements; and is nullable. If null,
		/// the value is set equal to this <see cref="ISequenceView{T}.IsStack"/>. Then, If true,
		/// then the elements are added in the order returned by that collection's enumerator.
		/// If false, then the argument is enumerated in reverse. When the argument is NULL,
		/// this creates a chain that preserves the
		/// enumeration orders --- if this is a Stack, the argument will be "Lifted" under this Stack in
		/// a preserved order, and if this is a Queue, the argument will be "Pushed" into this Queue
		/// in a preserved order.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="enumerateInOrder">Defaults to null:  the value is set equal to this
		/// <see cref="ISequenceView{T}.IsStack"/> If true, the argument's elements are
		/// always enumerated in the order returned by that collection. if false, the
		/// argument's elements will be enumerated in reverse. </param>
		/// <param name="addCount">This optional argument can restrict the count of added elements.
		/// This defaults to null, and if null --- or if the value is greater than the count of the
		/// argument <c>collection</c> --- then all elements are added.</param>
		/// <param name="countInOrder">Only applies if <c>addCount</c> is specified; and defaults
		/// to false. If set true, and if the collection is enumerated in reverse, then the
		/// enumeration will begin from the element at index <c>addCount - 1</c>, and proceed
		/// to zero. Notice that when false -- the DEFAULT --- and the collection IS enumerated
		/// in reverse, then the last-enumerated elements in the WHOLE collection are the elements
		/// added.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void InsertRangeOldest(
				ISequenceView<T> collection,
				bool? enumerateInOrder = null,
				int? addCount = null,
				bool countInOrder = false);

		/// <summary>
		/// As with <see cref="AddRange(IEnumerable{T},int?)"/>, but this method inserts the elements
		/// into the "oldest" position: for an <see cref="IQueue{T}"/> elements are inserted with
		/// <see cref="Push"/>; and for <see cref="IStack{T}"/> elements are appended with
		/// <see cref="IStack{T}.Lift"/>.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="addCount">This optional argument can restrict the count of added elements.
		/// This defaults to null, and if null --- or if the value is greater than the count of the
		/// argument <c>collection</c> --- then all elements are added.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void InsertRangeOldest(IEnumerable<T> collection, int? addCount = null);

		/// <summary>
		/// Sets the element at the index within the Collection to the value.
		/// </summary>
		/// <param name="index">>= 0, &lt; Count.</param>
		/// <param name="element">May be null.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void Set(int index, T element);

		/// <summary>
		/// This method sets the element at the index to the result of your <see cref="Func{T,T}"/>.
		/// </summary>
		/// <param name="index">The zero-based index to mutate.</param>
		/// <param name="swap">Your func will be invoked with the existing element, and its return value
		/// will always be set as the new value at this index.</param>
		/// <param name="returnNewValue">Specifies what this method returns. Defaults to false, and will
		/// return the prior value at this index. If this is set true, then this method returns the new
		/// value as returned from your func.</param>
		/// <returns>As defined by <c>returnNewValue</c>: either the value that was returned by your Func,
		/// and has been set; or the prioer value --- either of which may be null.</returns>
		T Exchange(int index, Func<T, T> swap, bool returnNewValue = false);

		/// <summary>
		/// As with <see cref="Exchange(int,Func{T,T},bool)"/>: sets the element at the index.
		/// </summary>
		/// <param name="index">The zero-based index to mutate.</param>
		/// <param name="newValue">This value will always be set as the new value at this index.</param>
		/// <returns>The prioer value --- which may be null.</returns>
		T Exchange(int index, T newValue);

		/// <summary>
		/// This method swaps the position of the element at the given indexes.
		/// </summary>
		/// <param name="index1">The zero-based index the first element.</param>
		/// <param name="index2">The zero-based index the second element.</param>
		void Swap(int index1, int index2);

		/// <summary>
		/// Removes the element at the "next" position in he sequence. For an
		/// <see cref="IQueue{T}"/> it is dequeued with <see cref="IQueue{T}.Dequeue"/>; and for
		/// <see cref="IStack{T}"/> it is popped with <see cref="IStack{T}.Pop"/>.
		/// </summary>
		T RemoveNext();

		/// <summary>
		/// Removes the specified number of elements from "first" position in the sequence
		/// and returns them. For an <see cref="IQueue{T}"/> it is the Head of the Queue;
		/// and for <see cref="IStack{T}"/> it is the Top of the Stack. The elements in
		/// the returned array are in the order they were in the sequence. Does not trim
		/// the capacity.
		/// </summary>
		/// <param name="rangeCount">The number of elements to remove and return.
		/// >= 0, &lt;= Count.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Not null, may be empty.</returns>
		T[] RemoveNextRange(int rangeCount);

		/// <summary>
		/// Removes and returns the element at the "last" position in the sequence. For an
		/// <see cref="IQueue{T}"/> it is the Tail of the Queue; and for
		/// <see cref="IStack{T}"/> it is the Bottom of the Stack.
		/// </summary>
		/// <returns>Null if the element is null.</returns>
		/// <exception cref="InvalidOperationException">The Collection is empty.</exception>
		T Drop();

		/// <summary>
		/// Removes the specified number of elements from "last" position in the sequence
		/// and returns them. For an <see cref="IQueue{T}"/> it is the Tail of the Queue;
		/// and for <see cref="IStack{T}"/> it is the Bottom of the Stack. The elements in
		/// the returned array are in the order they were in the sequence. Does not trim
		/// the capacity.
		/// </summary>
		/// <param name="rangeCount">The number of elements to remove and return.
		/// >= 0, &lt;= Count.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Not null, may be empty.</returns>
		T[] DropRange(int rangeCount);

		/// <summary>
		/// Trims the internal array to the current Count (may be 0).
		/// </summary>
		void TrimToSize();

		/// <summary>
		/// Removes all elements from the Collection. Does not trim the capacity.
		/// </summary>
		void Clear();
	}
}
