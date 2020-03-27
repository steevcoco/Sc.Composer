using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Equatable
{
	/// <summary>
	/// Implements a simple <see cref="IEqualityComparer{T}"/> that invokes a provided
	/// delegate to perform the equality comparison; and can also take a delegate
	/// to return the hash code.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed class DelegateEqualityComparer<T>
			: IEqualityComparer<T>
	{
		private readonly Func<T, T, bool> equalsFunc;
		private readonly Func<T, int> hashCodeFunc;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="equalsFunc">Required.</param>
		/// <param name="hashCodeFunc">Optional</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DelegateEqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int> hashCodeFunc)
		{
			this.equalsFunc = equalsFunc ?? throw new ArgumentNullException(nameof(equalsFunc));
			this.hashCodeFunc = hashCodeFunc ?? (obj => obj?.GetHashCode() ?? 0);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetHashCode(T obj)
			=> hashCodeFunc(obj);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(T x, T y)
			=> equalsFunc(x, y);
	}
}
