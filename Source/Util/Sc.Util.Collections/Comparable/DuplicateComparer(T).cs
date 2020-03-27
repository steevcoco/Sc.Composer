using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Comparable
{
	/// <summary>
	/// Implements an <see cref="IComparer{T}"/> that sorts objects and retains duplicates.
	/// NOTICE that since duplicates are retained, duplicates can be lost in collections.
	/// The returned instance uses <see cref="Comparer{T}.Default"/> to sort by default;
	/// and can also take any delegate <see cref="IComparer{T}"/> --- and the result returned
	/// here is coerced when the delegate's result is zero: this comparer will instead
	/// return one. You may also specify another <see cref="IComparer{T}"/> to use only for
	/// objects that compare equal --- which allows double sorting. If provided, then that
	/// comparer is invoked when the default returns zero --- and again, that result
	/// will be coerced to one if it is zero. This also supports reversed sorting.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed class DuplicateComparer<T>
			: IComparer<T>
	{
		private readonly IComparer<T> comparer;
		private readonly IComparer<T> duplicateComparer;
		private readonly bool reverse;


		/// <summary>
		/// Constructor uses the default <see cref="Comparer{T}.Default"/> as the delegate comparer.
		/// </summary>
		/// <param name="reverse">Defaults to false: if set true, this comparer WILL BE REVERSED.
		/// If set true, the result of the default comparer is reversed. If this argument is
		/// false, this Comparer returns 1 for objects that compare equal. If this argument is
		/// true, the Comparer returns -1 for objects that compare equal.</param>
		public DuplicateComparer(bool reverse = false)
				: this(Comparer<T>.Default, Comparer<T>.Default, reverse) { }

		/// <summary>
		/// Constructor that allows passing either the default <see cref="IComparer{T}"/> that
		/// will be used to get the first result; and/or another <paramref name="duplicateComparer"/>
		/// that will be used to get the result only for objects that compare equal by the default
		/// comparer. Passing a custom object here allows double sorting. Note that the result of
		/// your custom <c>duplicateComparer</c> will still be coerced again if it returns zero.
		/// Note that all argument are optional.
		/// </summary>
		/// <param name="comparer">Optional: the delegate comparer invoked for the primary sort.
		/// If null, <see cref="Comparer{T}.Default"/> is used.</param>
		/// <param name="duplicateComparer">Optional. If provided, this is invoked only for elements that
		/// compare equal by the default comparer; AND, the result respects the <c>reverse</c> argument: if
		/// that is true, then this result is negated.</param>
		/// <param name="reverse">Defaults to false: if set true, this comparer WILL BE REVERSED.
		/// If set true, the result of the default comparer is reversed. If this argument is
		/// false, this Comparer returns 1 for objects that compare equal. If this argument is
		/// true, the Comparer returns -1 for objects that compare equal. NOTICE that the result
		/// from your <c>duplicateComparer</c> ALSO respects this argument: if this is false, this
		/// returns your comparer's result; and if true, it negates that result.</param>
		public DuplicateComparer(
				IComparer<T> comparer = null,
				IComparer<T> duplicateComparer = null,
				bool reverse = false)
		{
			this.comparer = comparer ?? Comparer<T>.Default;
			this.duplicateComparer = duplicateComparer;
			this.reverse = reverse;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(T x, T y)
		{
			if (reverse) {
				int comp = -comparer.Compare(x, y);
				if (comp != 0)
					return comp;
				if (duplicateComparer == null)
					return -1;
				comp = duplicateComparer.Compare(x, y);
				return comp == 0
						? -1
						: comp;
			} else {
				int comp = comparer.Compare(x, y);
				if (comp != 0)
					return comp;
				if (duplicateComparer == null)
					return 1;
				comp = duplicateComparer.Compare(x, y);
				return comp == 0
						? 1
						: comp;
			}
		}
	}
}
