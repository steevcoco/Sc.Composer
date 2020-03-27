using System.Collections.Generic;


namespace Sc.Abstractions.Collections.Specialized
{
	/// <summary>
	/// <see cref="IFixedSizeSequence{T}"/> interface for an <see cref="IStack{T}"/>. Provides the
	/// <see cref="FixedPush"/> method; which is equivalent to <see cref="IFixedSizeSequence{T}.FixedAdd"/>.
	/// </summary>
	public interface IFixedSizeStack<T>
			: IFixedSizeSequence<T>,
					IStack<T>
	{
		/// <summary>
		/// Used to Push a new element while limiting the Stack's Count. If the current
		/// <see cref="IReadOnlyCollection{T}.Count"/> is >= <see cref="IFixedSizeSequence{T}.MaximumSize"/>,
		/// then <see cref="ISequence{T}.DropRange"/> is invoked, removing at least the specified
		/// <see cref="IFixedSizeSequence{T}.Overhead"/>. This method is equivalent to
		/// <see cref="IFixedSizeSequence{T}.FixedAdd"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		void FixedPush(T element);
	}
}
