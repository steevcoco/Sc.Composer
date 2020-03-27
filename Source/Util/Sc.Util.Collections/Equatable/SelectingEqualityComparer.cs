using System;
using System.Collections.Generic;


namespace Sc.Util.Collections.Equatable
{
	/// <summary>
	/// Implements an <see cref="IEqualityComparer{T}"/> of <typeparamref name="T"/>,
	/// that takes a delegate comparer --- or uses the default --- and uses a provided
	/// selector to fetch the compared <typeparamref name="TSelection"/> value from
	/// each <see cref="T"/> object. Optionally also takes a hash code function.
	/// </summary>
	/// <typeparam name="T">This comparer's type.</typeparam>
	/// <typeparam name="TSelection">The selected value type.</typeparam>
	public sealed class SelectingEqualityComparer<T, TSelection>
			: IEqualityComparer<T>
	{
		private readonly Func<T, TSelection> selector;
		private readonly IEqualityComparer<TSelection> comparer;
		private readonly Func<TSelection, int> getHashCode;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="selector">Required value selector.</param>
		/// <param name="comparer">Optional <see cref="IEqualityComparer{T}"/> that is used
		/// to compare each selected value: if null, the default is used.</param>
		/// <param name="getHashCode">Optional function to return the hashcode for
		/// each <typeparamref name="TSelection"/> value. If null, <see cref="object.GetHashCode"/>
		/// is used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public SelectingEqualityComparer(
				Func<T, TSelection> selector,
				IEqualityComparer<TSelection> comparer = null,
				Func<TSelection, int> getHashCode = null)
		{
			this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
			this.comparer = comparer ?? EqualityComparer<TSelection>.Default;
			this.getHashCode = getHashCode ?? GetHashCode;
			static int GetHashCode(TSelection obj)
				=> obj?.GetHashCode() ?? 0;
		}


		public bool Equals(T x, T y)
			=> comparer.Equals(selector(x), selector(y));

		public int GetHashCode(T obj)
			=> getHashCode(selector(obj));
	}
}
