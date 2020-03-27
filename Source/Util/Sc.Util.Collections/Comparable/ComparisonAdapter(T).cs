using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Comparable
{
	/// <summary>
	/// Simple <see cref="IComparer{T}"/> implementation that invokes a delegate
	/// <see cref="Comparison{T}"/>.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed class ComparisonAdapter<T>
			: IComparer<T>
	{
		private readonly Comparison<T> comparison;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="comparison">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ComparisonAdapter(Comparison<T> comparison)
			=> this.comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(T x, T y)
			=> comparison(x, y);
	}
}
