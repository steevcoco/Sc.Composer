using System.Runtime.CompilerServices;


namespace Sc.Collections
{
	public partial class Sequence<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Pop()
			=> Dequeue();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] PopRange(int rangeCount)
			=> DequeueRange(rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Lift(T element)
			=> Enqueue(element);
	}
}
