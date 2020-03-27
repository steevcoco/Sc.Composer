using System;


namespace Sc.Abstractions.Collections
{
	/// <summary>
	/// An <see cref="ISequence{T}"/> with methods that implement a Stack interface.
	/// </summary>
	/// <typeparam name="T">Element type. Unbounded.</typeparam>
	public interface IStack<T>
			: ISequence<T>
	{
		/// <summary>
		/// Removes and returns the element on the Top of the Stack. This method is equivalent to
		/// <see cref="ISequence{T}.RemoveNext"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">The Stack is empty.</exception>
		/// <returns>Null if the element is null.</returns>
		T Pop();

		/// <summary>
		/// Removes the specified number of elements from the Top of the Stack and returns them.
		/// The elements in the returned array are in the order they were on the Stack. Does
		/// not trim the capacity.
		/// </summary>
		/// <param name="rangeCount">The number of elements to Pop and return.
		/// >= 0, &lt;= Count.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Not null, may be empty.</returns>
		T[] PopRange(int rangeCount);

		/// <summary>
		/// Inserts an element at the Bottom of the Stack. This method is equivalent to
		/// <see cref="ISequence{T}.InsertOldest"/>.
		/// </summary>
		/// <param name="element">May be null.</param>
		void Lift(T element);
	}
}
