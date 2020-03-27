using System;


namespace Sc.Abstractions.Collections
{
	/// <summary>
	/// An <see cref="ISequence{T}"/> with methods that implement a Queue interface.
	/// </summary>
	/// <typeparam name="T">Element type. Unbounded.</typeparam>
	public interface IQueue<T>
			: ISequence<T>
	{
		/// <summary>
		/// Adds the element to the Tail of the Queue. This method is equivalent to
		/// <see cref="ISequence{T}.Add"/>.
		/// </summary>
		/// <param name="element">May be null.</param>
		void Enqueue(T element);

		/// <summary>
		/// Removes the element at the Head of the Queue and returns it. This method is equivalent to
		/// <see cref="ISequence{T}.RemoveNext"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">The Queue is empty.</exception>
		/// <returns>Null if the element is null.</returns>
		T Dequeue();

		/// <summary>
		/// Removes the specified number of objects at the Head of the Queue and returns them.
		/// The elements in the returned array are in the order they were in the Queue. Does
		/// not trim the capacity.
		/// </summary>
		/// <param name="rangeCount">The number of elements to Dequeue and return.
		/// >= 0, &lt;= Count.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Not null, may be empty.</returns>
		T[] DequeueRange(int rangeCount);
	}
}
