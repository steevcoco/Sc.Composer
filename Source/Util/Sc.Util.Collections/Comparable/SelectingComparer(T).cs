using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Comparable
{
	/// <summary>
	/// Implements an <see cref="IComparer{T}"/> over the type <typeparamref name="TOut"/>,
	/// from a delegate <see cref="IComparer{T}"/> over <typeparamref name="TIn"/>,
	/// using a provided delegate selector that selects the <c>TIn</c> value to
	/// compare from each <c>TOut</c> object passed to this comparer.
	/// The result of this comparer is the value implemented by the provided
	/// <c>TIn</c> comparer; and your delegate selects the pair of compared
	/// objects from the input <c>TOut</c> objects passed to this comparer.
	/// </summary>
	/// <typeparam name="TIn">The delegate object type, implemented by the delegate
	/// <see cref="IComparer{T}"/>.</typeparam>
	/// <typeparam name="TOut">The type implemented by this comparer.</typeparam>
	public sealed class SelectingComparer<TIn, TOut>
			: IComparer<TOut>
	{
		private readonly Func<TOut, TIn> delegateFunc;
		private readonly IComparer<TIn> comparer;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="delegateFunc">Required.</param>
		/// <param name="comparer">Optional: if null the default comparer is used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public SelectingComparer(Func<TOut, TIn> delegateFunc, IComparer<TIn> comparer = null)
		{
			this.delegateFunc = delegateFunc ?? throw new ArgumentNullException(nameof(delegateFunc));
			this.comparer = comparer ?? Comparer<TIn>.Default;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(TOut x, TOut y)
			=> comparer.Compare(delegateFunc(x), delegateFunc(y));
	}
}
