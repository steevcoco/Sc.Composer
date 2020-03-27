using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Collections
{
	/// <summary>
	/// Implements an <see cref="IReadOnlyDictionary{TKey,TValue}"/> view that
	/// returns contravariant types.
	/// </summary>
	/// <typeparam name="TKey">The exposed key type.</typeparam>
	/// <typeparam name="TValue">The exposed value type.</typeparam>
	/// <typeparam name="TKeyIn">The internal key type.</typeparam>
	/// <typeparam name="TValueIn">The internal value type.</typeparam>
	public sealed class VariantDictionary<TKey, TValue, TKeyIn, TValueIn>
			: IReadOnlyDictionary<TKey, TValue>
			where TKeyIn : TKey
			where TValueIn : TValue
	{
		private IReadOnlyDictionary<TKeyIn, TValueIn> dictionary;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="dictionary">Not null.</param>
		public VariantDictionary(IReadOnlyDictionary<TKeyIn, TValueIn> dictionary)
			=> Dictionary = dictionary;


		/// <summary>
		/// The actual backing dictionary. This can be changed; and cannot be null.
		/// </summary>
		public IReadOnlyDictionary<TKeyIn, TValueIn> Dictionary
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => dictionary;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => dictionary = value
					?? throw new ArgumentNullException(
							nameof(VariantDictionary<TKey, TValue, TKeyIn, TValueIn>.Dictionary));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			=> dictionary.GetEnumerator()
					.Select(kv => new KeyValuePair<TKey, TValue>(kv.Key, kv.Value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> dictionary.GetEnumerator();

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => dictionary.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			return key is TKeyIn keyIn && dictionary.ContainsKey(keyIn);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (!(key is TKeyIn keyIn)) {
				value = default;
				return false;
			}
			bool result = dictionary.TryGetValue(keyIn, out TValueIn valueIn);
			value = valueIn;
			return result;
		}

		public TValue this[TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				if (key == null)
					throw new ArgumentNullException(nameof(key));
				if (!(key is TKeyIn keyIn))
					throw new KeyNotFoundException(key.ToString());
				return dictionary[keyIn];
			}
		}

		public IEnumerable<TKey> Keys
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => dictionary.Keys.GetEnumerator()
					.As<TKeyIn, TKey>()
					.AsEnumerable();
		}

		public IEnumerable<TValue> Values
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => dictionary.Values.GetEnumerator()
					.As<TValueIn, TValue>()
					.AsEnumerable();
		}
	}
}
