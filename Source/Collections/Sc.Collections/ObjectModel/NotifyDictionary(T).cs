using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections.ObjectModel;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// Implementation of <see cref="INotifyDictionary{TKey,TValue}"/>.
	/// Implements an <see cref="IDictionary{TKey,TValue}"/> --- and
	/// <see cref="IReadOnlyDictionary{TKey,TValue}"/> --- that is observable: implements
	/// <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.
	/// Notice that collection changed events raise with the <see cref="TValue"/>
	/// values only --- not a <see cref="KeyValuePair{TKey,TValue}"/>.
	/// Provides support to suppress read and/or write exceptions.
	/// Serializable.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	[DataContract]
	public class NotifyDictionary<TKey, TValue>
			: NotifyCollectionBase<KeyValuePair<TKey, TValue>, Sequence<KeyValuePair<TKey, TValue>>, TValue>,
					INotifyDictionary<TKey, TValue>,
					IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Key and value collection.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		private sealed class ItemCollection<T>
				: ICollection<T>
		{
			private readonly Sequence<KeyValuePair<TKey, TValue>> sequence;
			private readonly Func<KeyValuePair<TKey, TValue>, T> selector;
			private IEqualityComparer<T> comparer;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="sequence">Required.</param>
			/// <param name="selector">Required.</param>
			/// <param name="comparer">Required.</param>
			public ItemCollection(
					Sequence<KeyValuePair<TKey, TValue>> sequence,
					Func<KeyValuePair<TKey, TValue>, T> selector,
					IEqualityComparer<T> comparer)
			{
				this.sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
				this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
				Comparer = comparer;
			}


			/// <summary>
			/// Required.
			/// </summary>
			public IEqualityComparer<T> Comparer
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => comparer;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				set => comparer = value ?? throw new ArgumentNullException(nameof(ItemCollection<T>.Comparer));
			}

			public int Count
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => sequence.Count;
			}

			public bool IsReadOnly
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => true;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Contains(T item)
			{
				foreach (KeyValuePair<TKey, TValue> kv in sequence) {
					if (comparer.Equals(item, selector(kv)))
						return true;
				}
				return false;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void CopyTo(T[] array, int arrayIndex)
			{
				foreach (KeyValuePair<TKey, TValue> kv in sequence) {
					if (arrayIndex >= array.Length)
						return;
					array[arrayIndex++] = selector(kv);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerator<T> GetEnumerator()
			{
				foreach (KeyValuePair<TKey, TValue> kv in sequence) {
					yield return selector(kv);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Add(T item)
				=> throw new NotSupportedException();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Clear()
				=> throw new NotSupportedException();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Remove(T item)
				=> throw new NotSupportedException();
		}


		private ItemCollection<TKey> keys;
		private ItemCollection<TValue> values;
		private IEqualityComparer<TKey> comparer;


		/// <summary>
		/// Constructor creates a default underlying Collection.
		/// </summary>
		/// <param name="equalityComparer">If null, the default comparer is set.</param>
		public NotifyDictionary(IEqualityComparer<TKey> equalityComparer = null)
			: this(null, equalityComparer) { }

		/// <summary>
		/// Constructor creates an underlying Collection with initial capacity.
		/// </summary>
		/// <param name="capacity">The initial capacity for the dictionary.</param>
		/// <param name="equalityComparer">If null, the default comparer is set.</param>
		public NotifyDictionary(int capacity, IEqualityComparer<TKey> equalityComparer = null)
			: this((int?)capacity, equalityComparer) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		private NotifyDictionary(int? capacity, IEqualityComparer<TKey> equalityComparer = null)
			: base(
					capacity.HasValue
							? new Sequence<KeyValuePair<TKey, TValue>>(false, capacity.Value)
							: new Sequence<KeyValuePair<TKey, TValue>>(false))
			=> setItemCollections(equalityComparer);


		[OnDeserialized]
		private void onDeserialized(StreamingContext c)
			=> setItemCollections();

		private void setItemCollections(IEqualityComparer<TKey> equalityComparer = null)
		{
			if (equalityComparer != null)
				comparer = equalityComparer;
			else if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;
			keys = new ItemCollection<TKey>(Collection, kv => kv.Key, comparer);
			values = new ItemCollection<TValue>(Collection, kv => kv.Value, EqualityComparer<TValue>.Default);
		}


		/// <summary>
		/// Provided to reset the <see cref="IEqualityComparer{T}"/> on the underlying collection.
		/// CAN be null: if null, the default comparer is set now.
		/// </summary>
		public IEqualityComparer<TKey> Comparer
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => comparer;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				comparer = value ?? EqualityComparer<TKey>.Default;
				keys.Comparer = comparer;
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


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int indexOfKey(TKey key, out KeyValuePair<TKey, TValue> element)
		{
			int index = 0;
			foreach (KeyValuePair<TKey, TValue> kv in Collection) {
				if (!Comparer.Equals(key, kv.Key)) {
					++index;
					continue;
				}
				element = kv;
				return index;
			}
			element = default;
			return -1;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override TValue GetCollectionChangedValue(KeyValuePair<TKey, TValue> element)
			=> element.Value;


		public ICollection<TKey> Keys
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => keys;
		}

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => keys;
		}

		public ICollection<TValue> Values
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => values;
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => values;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
		{
			if (key == null) {
				if (!SuppressReadExceptions)
					throw new ArgumentNullException(nameof(key));
				value = default;
				return false;
			}
			int index = indexOfKey(key, out KeyValuePair<TKey, TValue> element);
			if (index < 0) {
				value = default;
				return false;
			}
			value = element.Value;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(TKey key)
		{
			if (key != null)
				return indexOfKey(key, out _) >= 0;
			if (SuppressReadExceptions)
				return false;
			throw new ArgumentNullException(nameof(key));
		}

		public TValue this[TKey key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				if (key == null) {
					if (SuppressReadExceptions)
						return default;
					throw new ArgumentNullException(nameof(key));
				}
				int index = indexOfKey(key, out KeyValuePair<TKey, TValue> element);
				if (index >= 0)
					return element.Value;
				if (SuppressReadExceptions)
					return default;
				throw new KeyNotFoundException(key.ToString());
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				if (key == null) {
					if (SuppressWriteExceptions)
						return;
					throw new ArgumentNullException(nameof(key));
				}
				int index = indexOfKey(key, out KeyValuePair<TKey, TValue> oldElement);
				KeyValuePair<TKey, TValue> newElement = new KeyValuePair<TKey, TValue>(key, value);
				if (index >= 0) {
					ThrowIfReadOnly();
					Collection[index] = newElement;
					if (EventHandler.CheckNextEvent()) {
						EventHandler.RaiseSingleItemEvents(
								NotifyCollectionChangedAction.Replace,
								GetCollectionChangedValue(newElement),
								GetCollectionChangedValue(oldElement),
								index);
					}
				} else
					base.Add(newElement);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void Add(KeyValuePair<TKey, TValue> item)
		{
			if (item.Key == null) {
				if (SuppressWriteExceptions)
					return;
				throw new ArgumentNullException(nameof(KeyValuePair<TKey, TValue>.Key));
			}
			if (SuppressWriteExceptions) {
				if (indexOfKey(item.Key, out _) >= 0)
					return;
			}
			base.Add(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(TKey key, TValue value)
		{
			if (key == null) {
				if (SuppressWriteExceptions)
					return;
				throw new ArgumentNullException(nameof(key));
			}
			if (SuppressWriteExceptions) {
				if (indexOfKey(key, out _) >= 0)
					return;
			}
			base.Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(TKey key)
		{
			ThrowIfReadOnly();
			if (key == null) {
				if (SuppressWriteExceptions)
					return false;
				throw new ArgumentNullException(nameof(key));
			}
			int index = indexOfKey(key, out KeyValuePair<TKey, TValue> element);
			if (index < 0)
				return false;
			Collection.RemoveAt(index);
			if (EventHandler.CheckNextEvent()) {
				EventHandler.RaiseSingleItemEvents(
						NotifyCollectionChangedAction.Remove,
						default,
						GetCollectionChangedValue(element),
						index);
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Remove(KeyValuePair<TKey, TValue> item)
		{
			ThrowIfReadOnly();
			if (item.Key == null) {
				if (SuppressWriteExceptions)
					return false;
				throw new ArgumentNullException(nameof(KeyValuePair<TKey, TValue>.Key));
			}
			int index = 0;
			KeyValuePair<TKey, TValue> element = default;
			foreach (KeyValuePair<TKey, TValue> kv in Collection) {
				if (!Comparer.Equals(item.Key, kv.Key)
						|| !values.Comparer.Equals(item.Value, kv.Value)) {
					++index;
					continue;
				}
				element = kv;
				break;
			}
			if (index >= Collection.Count)
				return false;
			if (EventHandler.CheckNextEvent()) {
				EventHandler.RaiseSingleItemEvents(
						NotifyCollectionChangedAction.Remove,
						default,
						GetCollectionChangedValue(element),
						index);
			}
			return true;
		}
	}
}
