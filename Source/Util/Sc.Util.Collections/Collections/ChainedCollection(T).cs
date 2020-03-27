using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Collections
{
	/// <summary>
	/// Implements a simple <see cref="IReadOnlyCollection{T}"/> that enumerates the
	/// elements of a first and then second delegate collection.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed class ChainedCollection<T>
			: IReadOnlyCollection<T>
	{
		private readonly IReadOnlyCollection<T> first;
		private readonly IReadOnlyCollection<T> second;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="first">Not null; may be empty. The first returned elements.</param>
		/// <param name="second">Not null; may be empty. The remaining returned elements.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ChainedCollection(IReadOnlyCollection<T> first, IReadOnlyCollection<T> second)
		{
			this.first = first ?? throw new ArgumentNullException(nameof(first));
			this.second = second ?? throw new ArgumentNullException(nameof(second));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
		{
			foreach (T value in first) {
				yield return value;
			}
			foreach (T value in second) {
				yield return value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => first.Count + second.Count;
		}
	}
}
