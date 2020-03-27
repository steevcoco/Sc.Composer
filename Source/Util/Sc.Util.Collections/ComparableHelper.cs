using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sc.Util.Collections.Comparable;


namespace Sc.Util.Collections
{
	/// <summary>
	/// Static utilities for <see cref="IComparable{T}"/>,
	/// <see cref="IComparer{T}"/>, and <see cref="Comparison{T}"/>.
	/// </summary>
	public static class ComparableHelper
	{
		/// <summary>
		/// Returns a simple <see cref="IComparer{T}"/> object that invokes your
		/// <see cref="Comparison{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IComparer<T> ToIComparer<T>(this Comparison<T> comparison)
			=> new ComparisonAdapter<T>(comparison);

		/// <summary>
		/// Returns an <see cref="IComparer{T}"/> that uses your <c>selector</c> to get the
		/// actual value used by the comparer. You may provide an <see cref="IComparer{T}"/>,
		/// or else the default is used.
		/// </summary>
		/// <typeparam name="T">The returned <see cref="IComparer{T}"/> type.</typeparam>
		/// <typeparam name="TIn">The type that your selector selects.</typeparam>
		/// <param name="selector">Not null. For each object of Type <see cref="T"/>,
		/// your selector returns the <see cref="TIn"/> object to compare.</param>
		/// <param name="comparer">Optional. If null, the default comparer of <see cref="TIn"/>
		/// is used..</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IComparer<T> ToIComparer<TIn, T>(Func<T, TIn> selector, IComparer<TIn> comparer = null)
			=> new SelectingComparer<TIn, T>(selector, comparer);

		/// <summary>
		/// Returns a generic <see cref="IComparer{T}"/> that implements reverse sorting; and uses
		/// <see cref="Comparer{T}.Default"/> to sort. The result of your comparer
		/// is negated by the comparer returned here.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IComparer<T> ReverseComparer<T>()
			=> new ReverseComparer<T>();

		/// <summary>
		/// Returns a generic <see cref="IComparer{T}"/> that implements reverse sorting; and uses
		/// your <see cref="Comparer{T}"/> to sort by default. The result of your comparer
		/// is negated by the comparer returned here.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IComparer<T> Reverse<T>(this IComparer<T> comparer)
			=> new ReverseComparer<T>(comparer);

		/// <summary>
		/// Returns an <see cref="IComparer{T}"/> that sorts objects and retains duplicates.
		/// Notice that since duplicates are retained, duplicates can be lost in collections.
		/// The returned instance uses <see cref="Comparer{T}.Default"/> to sort by default;
		/// and you may specify an <see cref="IComparer{T}"/> to use only
		/// for objects that compare equal --- which allows double sorting
		/// </summary>
		/// <param name="reverse">Defaults to false: if set true, this comparer WILL BE REVERSED.
		/// If set true, the result of the default comparer is reversed. If this argument is
		/// false, this Comparer returns 1 for objects that compare equal. If this argument is
		/// true, the Comparer returns -1 for objects that compare equal. NOTICE that the result
		/// from your <c>duplicateComparer</c> ALSO respects this argument: if this is false, this
		/// returns your comparer's result; and if true, it negates that result.</param>
		/// <param name="duplicateComparer">Optional. If provided, this is invoked only for elements that
		/// compare equal by the default comparer; AND, the result respects the <c>reverse</c> argument: if
		/// that is true, then this result is negated.</param>
		/// <returns>Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IComparer<T> DuplicateComparer<T>(bool reverse = false, IComparer<T> duplicateComparer = null)
			=> new DuplicateComparer<T>(null, duplicateComparer, reverse);

		/// <summary>
		/// Returns an <see cref="IComparer{T}"/> that sorts objects and retains duplicates.
		/// Notice that since duplicates are retained, duplicates can be lost in collections.
		/// The returned instance uses this <see cref="Comparer{T}"/> to sort;
		/// and you may specify an <see cref="IComparer{T}"/> to use only
		/// for objects that compare equal --- which allows double sorting
		/// </summary>
		/// <param name="comparer">Required: performs the primary comparison.</param>
		/// <param name="reverse">Defaults to false: if set true, this comparer WILL BE REVERSED.
		/// If set true, the result of the default comparer is reversed. If this argument is
		/// false, this Comparer returns 1 for objects that compare equal. If this argument is
		/// true, the Comparer returns -1 for objects that compare equal. NOTICE that the result
		/// from your <c>duplicateComparer</c> ALSO respects this argument: if this is false, this
		/// returns your comparer's result; and if true, it negates that result.</param>
		/// <param name="duplicateComparer">Optional. If provided, this is invoked only for elements that
		/// compare equal by the default comparer; AND, the result respects the <c>reverse</c> argument: if
		/// that is true, then this result is negated.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IComparer<T> AllowDuplicates<T>(
				this IComparer<T> comparer,
				bool reverse = false,
				IComparer<T> duplicateComparer = null)
		{
			if (comparer == null)
				throw new ArgumentNullException(nameof(comparer));
			return new DuplicateComparer<T>(comparer, duplicateComparer, reverse);
		}
	}
}
