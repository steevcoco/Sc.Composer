using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Enumerable
{
	/// <summary>
	/// A simple adapter that takes an <see cref="IEnumerator{T}"/> and
	/// implements <see cref="IDictionaryEnumerator"/>.
	/// </summary>
	/// <typeparam name="TKey">Type of the supplied <see cref="IEnumerator{T}"/>.</typeparam>
	/// <typeparam name="TValue">Type of the supplied <see cref="IEnumerator{T}"/>.</typeparam>
	public sealed class DictionaryEnumeratorAdapter<TKey, TValue>
			: IDictionaryEnumerator
	{
		private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="enumerator">Not null</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DictionaryEnumeratorAdapter(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
			=> this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));


		public DictionaryEntry Entry
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new DictionaryEntry(enumerator.Current.Key, enumerator.Current.Value);
		}

		public object Key
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => enumerator.Current.Key;
		}

		public object Value
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => enumerator.Current.Value;
		}

		public object Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Entry;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
			=> enumerator.MoveNext();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
			=> enumerator.Reset();
	}
}
