using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Comparable
{
	/// <summary>
	/// Implements a reversed-order <see cref="IComparer{T}"/> that invokes a provided
	/// <see cref="IComparer{T}"/>, or the default <see cref="Comparer{T}.Default"/>;
	/// and returns the inverted result of that delegate. Notice that your provided
	/// comparer compares in standard order; and this object will reverse the result.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed class ReverseComparer<T>
			: IComparer<T>
	{
		private readonly IComparer<T> comparer;


		/// <summary>
		/// Constructor uses the default <see cref="Comparer{T}.Default"/>
		/// as the underlying delegate.
		/// </summary>
		public ReverseComparer()
				: this(Comparer<T>.Default) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="comparer">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ReverseComparer(IComparer<T> comparer)
			=> this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(T x, T y)
			=> -comparer.Compare(x, y);
	}
}
