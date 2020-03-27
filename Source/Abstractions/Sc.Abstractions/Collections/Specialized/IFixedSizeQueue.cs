using System.Collections.Generic;


namespace Sc.Abstractions.Collections.Specialized
{
	/// <summary>
	/// <see cref="IFixedSizeSequence{T}"/> interface for an <see cref="IQueue{T}"/>. Provides the
	/// <see cref="FixedEnqueue"/> method; which is equivalent to <see cref="IFixedSizeSequence{T}.FixedAdd"/>.
	/// </summary>
	public interface IFixedSizeQueue<T>
			: IFixedSizeSequence<T>,
					IQueue<T>
	{
		/// <summary>
		/// Used to Enqueue a new element while limiting the Queue's Count. If the current
		/// <see cref="IReadOnlyCollection{T}.Count"/> is >= <see cref="IFixedSizeSequence{T}.MaximumSize"/>,
		/// then <see cref="IQueue{T}.DequeueRange"/> is invoked, removing at least the
		/// specified <see cref="IFixedSizeSequence{T}.Overhead"/>. This method is equivalent to
		/// <see cref="IFixedSizeSequence{T}.FixedAdd"/>.
		/// </summary>
		/// <param name="element">Can be null.</param>
		void FixedEnqueue(T element);
	}
}
