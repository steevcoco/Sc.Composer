using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Enumerable
{
	/// <summary>
	/// Implements an <see cref="IEnumerator{T}"/> that returns a tuple that
	/// holds the current element, and the next element in the enumeration.
	/// The returned tuple also holds a bool indicating if the <c>next</c>
	/// element returned there IS available --- so that null checks do not
	/// need to be performed, and the element type can be a value type
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed class LookaheadEnumerator<T>
			: IEnumerator<(T current, T next, bool hasNext)>
	{
		private readonly IEnumerator<T> enumerator;
		private bool hasMoved;
		private bool hasNext;
		private T current;
		private T next;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="enumerator">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public LookaheadEnumerator(IEnumerator<T> enumerator)
			=> this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (!hasMoved) {
				hasMoved = true;
				if (!enumerator.MoveNext())
					return false;
				current = enumerator.Current;
				if (enumerator.MoveNext()) {
					next = enumerator.Current;
					hasNext = true;
				}
			} else {
				if (!hasNext)
					return false;
				current = next;
				if (enumerator.MoveNext()) {
					next = enumerator.Current;
					hasNext = true;
				} else {
					next = default;
					hasNext = false;
				}
			}
			return true;
		}

		public (T current, T next, bool hasNext) Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (current, next, hasNext);
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			enumerator.Reset();
			hasMoved = false;
			hasNext = false;
			current = default;
			next = default;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			hasMoved = true;
			hasNext = false;
			current = default;
			next = default;
			enumerator.Dispose();
		}
	}
}
