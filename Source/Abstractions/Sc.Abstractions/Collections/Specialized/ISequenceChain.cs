using System.Runtime.CompilerServices;


namespace Sc.Abstractions.Collections.Specialized
{
	/// <summary>
	/// Defines a chained sequence of <see cref="ISequenceView{T}"/> collections.
	/// Collections are held here in an <see cref="ISequence{T}"/> of collections, and elements
	/// enumerate in each collection's order, in sequence from the <see cref="Chain"/>. Note
	/// that the implementation will always report itself as either an <see cref="IQueue{T}"/>
	/// or <see cref="IStack{T}"/> based on how the <see cref="Chain"/> is constructed, but this
	/// interface does not restrict chaining both <see cref="IQueue{T}"/> and <see cref="IStack{T}"/>
	/// collections together. Elements are enumerated from the <see cref="Chain"/> in
	/// each collection's own order.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public interface ISequenceChain<T>
			: ISequenceView<T>
	{
		/// <summary>
		/// Provides access to the sequence of collections. Not null.
		/// </summary>
		ISequenceView<ISequenceView<T>> Chain
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
		}
	}
}
