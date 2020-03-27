using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Collections
{
	/// <summary>
	/// Implements a variant <see cref="IReadOnlyList{T}"/> that returns a
	/// selection from the backing list either from a provided delegate,
	/// or a protected virtual implementation method here. Note that
	/// this class also allows the backing list and/or the selector to be changed.
	/// </summary>
	/// <typeparam name="TIn">Your backing list type. Unrestricted.</typeparam>
	/// <typeparam name="TOut">This list type. Unrestricted.</typeparam>
	public class DelegateReadOnlyList<TIn, TOut>
			: IReadOnlyList<TOut>
	{
		private IReadOnlyList<TIn> list;
		private Func<TIn, TOut> selector;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="list">Required.</param>
		/// <param name="selector">Required implementation for
		/// <see cref="Select"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DelegateReadOnlyList(IReadOnlyList<TIn> list, Func<TIn, TOut> selector)
		{
			List = list;
			Selector = selector;
		}

		/// <summary>
		/// Protected constructor for a subclass, which MUST override and implement
		/// <see cref="Select"/>.
		/// </summary>
		/// <param name="list">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		protected DelegateReadOnlyList(IReadOnlyList<TIn> list)
			=> List = list;


		/// <summary>
		/// This method selects each <see cref="TOut"/> element from each
		/// <see cref="TIn"/> from the <see cref="List"/>. This implementation
		/// will invoke the delegate given in the public constructor.
		/// </summary>
		/// <param name="element">Not checked: the element from the <see cref="List"/>.</param>
		/// <returns>Your selection.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual TOut Select(TIn element)
			=> selector(element);


		/// <summary>
		/// This backing list: this can be changed; and may not be null.
		/// </summary>
		public IReadOnlyList<TIn> List
		{
			get => list;
			set => list = value ?? throw new ArgumentNullException(nameof(DelegateReadOnlyList<TIn, TOut>.List));
		}

		/// <summary>
		/// This selector: this can be changed; and may not be set null.
		/// </summary>
		public Func<TIn, TOut> Selector
		{
			get => selector;
			set => selector
					= value
					?? throw new ArgumentNullException(nameof(DelegateReadOnlyList<TIn, TOut>.Selector));
		}


		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => list.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<TOut> GetEnumerator()
		{
			foreach (TIn element in list) {
				yield return Select(element);
			}
		}

		public TOut this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Select(list[index]);
		}
	}
}
