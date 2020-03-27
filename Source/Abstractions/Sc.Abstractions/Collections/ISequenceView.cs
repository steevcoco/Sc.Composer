using System;
using System.Collections.Generic;
using Sc.Abstractions.Collections.Specialized;


namespace Sc.Abstractions.Collections
{
	/// <summary>
	/// A linear Queue or Stack of objects, with indexed access to elements. The Collection is implemented
	/// as a circular buffer. Enqueue and Dequeue are O(1). Notice that the Collection operates as either
	/// a Queue or a Stack: the semantic is simply that the last-added element is the newest. For
	/// a Queue, the <see cref="ISequenceView{T}.Peek"/> method returns the Head, which is the oldest
	/// element; and for a Stack, the Top is returned, which is the newest element. Enumerations proceed
	/// from the Head to the Tail; which for a Stack is the Top to the Bottom. Instance can also easily
	/// be chained arbitrarily: the <see cref="CreateChain{TChain}"/> method returns a new collection
	/// that enumerates each chained collection in its own order --- <see cref="IsStack"/> will report
	/// the type on any given instance. This interface extends <see cref="IReadOnlyList{T}"/>.
	/// </summary>
	/// <typeparam name="T">Element type. Unbounded.</typeparam>
	public interface ISequenceView<out T>
			: IReadOnlyList<T>
	{
		/// <summary>
		/// Creates a new <see cref="ISequenceView{T}"/> that implements a read-only live view of this
		/// collection that begins at the specified startIndex, and has the specified rangeCount. Unlike
		/// the singleton instance returned by <see cref="ISequence{t}.AsReadOnly"/>, this returned object
		/// is not serialized and restored with this collection, AND this view will NOT deserialize this
		/// collection: the returned collection will raise exceptions if serialized.
		/// </summary>
		/// <param name="startIndex">The first index to return.</param>
		/// <param name="rangeCount">The Count of the returned view.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		ISequenceView<T> CreateReadOnlyView(int startIndex, int rangeCount);

		/// <summary>
		/// Creates a new <see cref="ISequenceView{T}"/> that implements a read-only live view of this
		/// collection and returns elements through your Func. Unlike the singleton instance returned by
		/// <see cref="ISequence{t}.AsReadOnly"/>, this returned object is not serialized and restored
		/// with this collection, AND this view will NOT deserialize this collection: the returned collection
		/// will raise exceptions if serialized.
		/// </summary>
		/// <typeparam name="TOut">Output element type. Unbounded.</typeparam>
		/// <param name="variantFunc">This func converts all <see cref="T"/> values to the target
		/// <c>TOut</c> type.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		ISequenceView<TOut> CreateVariantView<TOut>(Func<T, TOut> variantFunc);

		/// <summary>
		/// Notice: this method functions as a "factory": the returned collection NEED NOT contain THIS
		/// instance. Creates a new <see cref="ISequenceChain{T}"/>, which implements a chained series of
		/// <see cref="ISequenceView{T}"/> collections. The chain contains ONLY the instances specified in the
		/// arguments, and elements enumerate in each individual collection's order, in the sequence given.
		/// Note that the returned instance will always always report itself as either an <see cref="IQueue{T}"/>
		/// or <see cref="IStack{T}"/> based on the <c>asStack</c> argument, but this interface does not
		/// restrict chaining both <see cref="IQueue{T}"/> and <see cref="IStack{T}"/> collections together.
		/// Also notice: unlike the singleton instance returned by <see cref="ISequence{t}.AsReadOnly"/>,
		/// this returned object is not serialized and restored with this collection: the returned object
		/// is a new independent collection; AND, it will serialize and restore only if all chained elements
		/// are serializable. The returned chain of sequences is mutable.
		/// </summary>
		/// <param name="asStack">Specifies the mode of the returned collection.</param>
		/// <param name="chain">The sequences to chain, which will be enumerated in the order given here.
		/// MAY be null or empty.</param>
		/// <returns>Not null.</returns>
		ISequenceChain<TChain> CreateChain<TChain>(bool asStack, params ISequenceView<TChain>[] chain);

		/// <summary>
		/// This method creates a new collection that contains the elements copied from this collection. The
		/// returned instance is a new, independent collection, and can be serialized. The mode of the
		/// collection matches this instance.
		/// </summary>
		/// <returns>Not null.</returns>
		ISequenceView<T> Clone();

		/// <summary>
		/// Returns true if this instance is a readonly instance.
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		/// This is the mode of this Collection. If true, this instance is used as a Stack;
		/// and if false, a Queue. NOTICE that this will be honored by interface implementations
		/// that perform operations dependent on the mode; and this determines the enumeration
		/// order: for a Stack, the first element returned is the newest (the Top of the Stack),
		/// and for a Queue, the first element is the oldest (the Head of the Queue).
		/// </summary>
		bool IsStack { get; }

		/// <summary>
		/// Returns an <see cref="IEnumerator{T}"/> over this collection that begins at the
		/// specified start Index, and has the specified Count.
		/// </summary>
		/// <param name="startIndex">The first index to return.</param>
		/// <param name="rangeCount">The total count to return.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		IEnumerator<T> GetEnumerator(int startIndex, int rangeCount);

		/// <summary>
		/// Returns <see cref="GetReverseEnumerator(int,int)"/> (Count - 1, Count).
		/// </summary>
		/// <returns>Not null.</returns>
		IEnumerator<T> GetReverseEnumerator();

		/// <summary>
		/// Returns an <see cref="IEnumerator{T}"/> over this collection that begins at the specified
		/// zero-based startIndex, has the specified rangeCount, and enumerates in reverse. Note that
		/// the startIndex is just as with the default enumerator --- zero-based. The rangeCount must
		/// be &lt;= startIndex + 1; and the enumeration runs from the startIndex towards 0.
		/// </summary>
		/// <param name="startIndex">The first zero-based index to return.</param>
		/// <param name="rangeCount">The total count to return.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		IEnumerator<T> GetReverseEnumerator(int startIndex, int rangeCount);

		/// <summary>
		/// Returns the first element in the Collection; which for a Queue is the oldest element
		/// (the Head of the Queue), and for a Stack, is the newest element (the Top of the Stack).
		/// The object remains in the Collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">The Collection is empty.</exception>
		/// <returns>Null if the element is null.</returns>
		T Peek();

		/// <summary>
		/// Returns the element at the index within the Collection. For an <see cref="IQueue{T}"/>,
		/// index <c>0</c> is the Head; and for an <see cref="IStack{T}"/>, index <c>0</c> is the Top.
		/// The object remains in the Collection. PeekAt(0) is equivalent to Peek().
		/// </summary>
		/// <param name="index">>= 0, &lt; Count.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Null if the element is null.</returns>
		T PeekAt(int index);

		/// <summary>
		/// Returns the last element in the Collection; which for a Queue is the newest element
		/// (the Tail of the Queue), and for a Stack, is the oldest element (the Bottom of the Stack).
		/// The object remains in the Collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">The Collection is empty.</exception>
		/// <returns>Null if the element is null.</returns>
		T Poke();

		/// <summary>
		/// Copies the elements in the Collection to a new Array.
		/// </summary>
		/// <returns>Not null; may be Length 0.</returns>
		T[] ToArray();

		/// <summary>
		/// Copies a specified range of elements in the collection to a new Array.
		/// </summary>
		/// <param name="startIndex">>= 0, &lt; Count.</param>
		/// <param name="rangeCount">>= 0, &lt;= Count - startIndex.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Not null; may be Length 0.</returns>
		T[] ToArray(int startIndex, int rangeCount);

		/// <summary>
		/// Copies the elements in this collection to the given <see cref="Array"/>, starting at the
		/// specified <c>arrayIndex</c>.
		/// </summary>
		/// <param name="destination">Must be a one-dimensional Array; and the element type must be assignable
		/// from this one.</param>
		/// <param name="destinationIndex">The zero-based index in destination at which copying begins.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void CopyTo(Array destination, int destinationIndex = 0);

		/// <summary>
		/// The method is like <see cref="ISequenceView{T}.CopyTo"/>, but allows you to specify
		/// further Array Copy arguments.
		/// </summary>
		/// <param name="startIndex">An index in this collection.</param>
		/// <param name="destination">The destination Array.</param>
		/// <param name="destinationIndex">The start index in <c>destination</c> at which to begin copying.</param>
		/// <param name="rangeCount">Must be valid for this <c>Count</c> and <c>destination.Length</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		void CopyRangeTo(int startIndex, Array destination, int destinationIndex, int rangeCount);

		/// <summary>
		/// This property is provided to track mutations: this value will be incremented with any
		/// mutation to the collection
		/// </summary>
		int Version { get; }
	}
}
