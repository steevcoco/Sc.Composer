using System;
using System.Runtime.CompilerServices;


namespace Sc.Collections
{
	public partial class Sequence<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(T element)
		{
			unchecked {
				if (!TryGrowCapacity())
					++Version;
				array[tail] = element;
				if (tail == (array.Length - 1))
					tail = 0;
				else
					++tail;
				++count;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Dequeue()
		{
			if (count == 0)
				throw new InvalidOperationException("Collection is empty.");
			unchecked {
				++Version;
				T removed = array[head];
				if (!isElementTypeValueType)
					array[head] = default;
				if (head == (array.Length - 1))
					head = 0;
				else
					++head;
				--count;
				return removed;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] DequeueRange(int rangeCount)
		{
			if ((rangeCount < 0)
					|| (rangeCount > count))
				throw new ArgumentOutOfRangeException(nameof(rangeCount), rangeCount, $"Must be <= {count}");
			T[] removed = new T[rangeCount];
			doUncheckedCopyRange(0, removed, 0, rangeCount, true);
			return removed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] DropRange(int rangeCount)
		{
			if ((rangeCount < 0)
					|| (rangeCount > count))
				throw new ArgumentOutOfRangeException(nameof(rangeCount), rangeCount, $"Must be <= {count}");
			T[] removed = new T[rangeCount];
			doUncheckedCopyRange(count - rangeCount, removed, 0, rangeCount, false, true);
			return removed;
		}
	}
}
