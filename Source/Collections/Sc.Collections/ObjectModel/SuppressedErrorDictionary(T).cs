using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// Implements an <see cref="IDictionary{TKey,TValue}"/> --- and
	/// <see cref="IReadOnlyDictionary{TKey,TValue}"/> that can suppress read and/or write
	/// exceptions.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	[DataContract]
	public class SuppressedErrorDictionary<TKey, TValue>
			: IReadOnlyDictionary<TKey, TValue>,
					IDictionary<TKey, TValue>
	{
		/// <summary>
		/// The values.
		/// </summary>
		[DataMember]
		protected Dictionary<TKey, TValue> Dictionary;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">Optional: if null, a new default instance is created.</param>
		public SuppressedErrorDictionary(Dictionary<TKey, TValue> collection = null)
			=> Dictionary = collection ?? new Dictionary<TKey, TValue>(EqualityComparer<TKey>.Default);


		/// <summary>
		/// Provided to reset the <see cref="IEqualityComparer{T}"/> on the underlying collection.
		/// CAN NOT be set null: can return null if the
		/// collection was constructed with a null instance. Notice that setting this property
		/// constructs a new collection and copies all elements.
		/// </summary>
		public IEqualityComparer<TKey> Comparer
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Dictionary.Comparer;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(SuppressedErrorDictionary<TKey, TValue>.Comparer));
				Dictionary = new Dictionary<TKey, TValue>(Dictionary, value);
			}
		}

		/// <summary>
		/// This property defaults to TRUE. When true, read operations will not raise exceptions,
		/// and will return null or default values where keys are not found.
		/// </summary>
		public bool SuppressReadExceptions
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}
				= true;

		/// <summary>
		/// This property defaults to FALSE. If set true, write operations will not raise exceptions.
		/// An Add operation will still not replace any existing value, but will not raise;
		/// and a null key is still not allowed but will not raise.
		/// </summary>
		public bool SuppressWriteExceptions
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set;
		}


		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).IsReadOnly;
		}

		public ICollection<TKey> Keys
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Dictionary.Keys;
		}

		public ICollection<TValue> Values
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Dictionary.Values;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Dictionary.Count;
		}

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Keys;
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Values;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			=> Dictionary.GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> Dictionary.GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			=> ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(KeyValuePair<TKey, TValue> item)
			=> ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(TKey key)
			=> Dictionary.ContainsKey(key);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
			=> Dictionary.TryGetValue(key, out value);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
			=> Dictionary.Clear();


		public TValue this[TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				if (SuppressReadExceptions) {
					if ((key != null)
							&& Dictionary.TryGetValue(key, out TValue value))
						return value;
					return default;
				}
				return Dictionary[key];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				if (SuppressWriteExceptions) {
					if (key == null)
						return;
				}
				Dictionary[key] = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey key, TValue value)
		{
			if (SuppressWriteExceptions) {
				if ((key == null)
						|| Dictionary.ContainsKey(key))
					return;
			}
			Dictionary.Add(key, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (SuppressWriteExceptions) {
				if ((item.Key == null)
						|| Dictionary.ContainsKey(item.Key))
					return;
			}
			((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Add(item);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key)
		{
			if (SuppressWriteExceptions) {
				if (key == null)
					return false;
			}
			return Dictionary.Remove(key);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (SuppressWriteExceptions) {
				if (item.Key == null)
					return false;
			}
			return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Remove(item);
		}
	}
}
