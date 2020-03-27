using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Sc.Util.Collections.Enumerable;


namespace Sc.Util.Collections
{
	/// <summary>
	/// An <see cref="IDictionary{TKey,TValue}"/> that holds weak values.
	/// There are differences from a standard Dictionary.
	/// The <c>WeakReferenceDictionary</c> cannot hold null values:
	/// exceptions will be raised when trying to add a null value.
	/// Methods will only return live instances; and enumerators or other
	/// operations will skip collected values.
	/// When an element is removed from the collection because it has been
	/// collection, an optional <see cref="HandleValueCollected"/>
	/// action can be invoked. Since the collection is not synchronized,
	/// idioms like <c>if (!TryGetValue) { Add }</c> MAY fail
	/// from non-atomic collection.
	/// The <see cref="PruneOnEveryAccess"/> property is provided to
	/// prune all collected keys on each read or write; and that defaults to true.
	/// In order to support generic interface types for the Value
	/// Type, this implementation uses <see cref="WeakReference"/>
	/// and not <see cref="WeakReference{T}"/>:
	/// therefore most methods that return <c>TValue</c> instances perform a cast.
	/// </summary>
	/// <typeparam name="TKey">The Key Type.</typeparam>
	/// <typeparam name="TValue">The Value Type.</typeparam>
	[DataContract]
	public class WeakReferenceDictionary<TKey, TValue>
			: IDictionary<TKey, TValue>,
					IDictionary,
					IReadOnlyDictionary<TKey, TValue>
	{
		private bool trackResurrection;


		/// <summary>
		/// Default constructor. Creates an underlying <see cref="Dictionary{TKey,TValue}"/>.
		/// </summary>
		public WeakReferenceDictionary()
			=> Dictionary = new Dictionary<TKey, WeakReference>();

		/// <summary>
		/// Constructor. Creates an underlying <see cref="Dictionary{TKey,TValue}"/>
		/// with the argument.
		/// </summary>
		public WeakReferenceDictionary(IEqualityComparer<TKey> comparer)
			=> Dictionary = new Dictionary<TKey, WeakReference>(comparer);

		/// <summary>
		/// Constructor. Creates an underlying <see cref="Dictionary{TKey,TValue}"/>
		/// with the arguments.
		/// </summary>
		public WeakReferenceDictionary(int capacity, IEqualityComparer<TKey> comparer)
			=> Dictionary = new Dictionary<TKey, WeakReference>(capacity, comparer);

		/// <summary>
		/// Constructor. Creates an underlying <see cref="Dictionary{TKey,TValue}"/>
		/// with the argument.
		/// </summary>
		public WeakReferenceDictionary(int capacity)
			=> Dictionary = new Dictionary<TKey, WeakReference>(capacity);


		[OnDeserializing]
		private void onDeserializing(StreamingContext _)
			=> Dictionary = new Dictionary<TKey, WeakReference>();

		private bool tryPrune()
		{
			if (PruneOnEveryAccess)
				Prune();
			return PruneOnEveryAccess;
		}

		private WeakReference newWeakReference(TValue value)
			=> new WeakReference(value, TrackResurrection);


		/// <summary>
		/// This is the actual underlying dictionary.
		/// </summary>
		protected IDictionary<TKey, WeakReference> Dictionary { get; set; }


		/// <summary>
		/// Defaults to true: each read or write will first invoke <see cref="Prune"/>.
		/// <see cref="HandleValueCollected"/> will be invoked for any removed elements.
		/// </summary>
		public bool PruneOnEveryAccess { get; set; } = true;

		/// <summary>
		/// Defaults to false: specifies the value for each new <see cref="WeakReference"/>.
		/// </summary>
		public bool TrackResurrection
		{
			get => trackResurrection;
			set {
				if (trackResurrection == value)
					return;
				trackResurrection = value;
				KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Dictionary.Count];
				CopyTo(array, 0);
				foreach (KeyValuePair<TKey, TValue> kv in array) {
					if (kv.Value == null)
						continue;
					Dictionary[kv.Key] = newWeakReference(kv.Value);
				}
			}
		}

		/// <summary>
		/// Provides an optional action that will be invoked whenever this collection
		/// removes a key because the value has been collected.
		/// </summary>
		public Action<TKey, WeakReference> HandleValueCollected { get; set; }

		/// <summary>
		/// Iterates the <see cref="Dictionary"/> and removes all collected references.
		/// <see cref="HandleValueCollected"/> will be invoked for any removed elements.
		/// </summary>
		/// <returns>A count of removed elements.</returns>
		public int Prune()
		{
			int result = 0;
			foreach (KeyValuePair<TKey, WeakReference> kv in Dictionary.ToArray()) {
				if (kv.Value.IsAlive)
					continue;
				Dictionary.Remove(kv.Key);
				HandleValueCollected?.Invoke(kv.Key, kv.Value);
				++result;
			}
			return result;
		}


		/// <summary>
		/// Fetches the value for the key, and if the key is not present or if the value
		/// has been collected, the result of the Func is added and returned.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="addFunc">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If the func creates a null value.</exception>
		public TValue GetOrAdd(TKey key, Func<TValue> addFunc)
		{
			if (addFunc == null)
				throw new ArgumentNullException(nameof(addFunc));
			tryPrune();
			if (Dictionary.TryGetValue(key, out WeakReference weakValue)) {
				if (weakValue.IsAlive)
					return (TValue)weakValue.Target;
				Dictionary.Remove(key);
				HandleValueCollected?.Invoke(key, weakValue);
			}
			TValue value = addFunc();
			if (value == null)
				throw new ArgumentException("Added value cannot be null.", nameof(addFunc));
			Dictionary[key] = newWeakReference(value);
			return value;
		}

		/// <summary>
		/// Fetches the value for the key, and if the key is not present or if the value
		/// has been collected, the result of the Func is added and returned.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="returnExisting">This func will be invoked if there is an existing
		/// value, as a predicate: if this returns true, then this existing value is returned,
		/// and otherwise the <c>addFunc</c> will be invoked to replace it.</param>
		/// <param name="addFunc">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If the func creates a null value.</exception>
		public TValue GetOrReplace(TKey key, Func<TValue, bool> returnExisting, Func<TValue> addFunc)
		{
			if (returnExisting == null)
				throw new ArgumentNullException(nameof(returnExisting));
			if (addFunc == null)
				throw new ArgumentNullException(nameof(addFunc));
			tryPrune();
			TValue value;
			if (Dictionary.TryGetValue(key, out WeakReference weakValue)) {
				if (!weakValue.IsAlive) {
					Dictionary.Remove(key);
					HandleValueCollected?.Invoke(key, weakValue);
				} else {
					value = (TValue)weakValue.Target;
					if (returnExisting(value))
						return value;
				}
			}
			value = addFunc();
			if (value == null)
				throw new ArgumentException("Added value cannot be null.", nameof(addFunc));
			Dictionary[key] = newWeakReference(value);
			return value;
		}


		/// <summary>
		/// Will return the value if the key is present AND the reference is alive.
		/// The setter will throw if the value is null.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>May be null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="KeyNotFoundException"></exception>
		public TValue this[TKey key]
		{
			get {
				tryPrune();
				WeakReference weakValue = Dictionary[key];
				if (weakValue.IsAlive)
					return (TValue)weakValue.Target;
				throw new KeyNotFoundException($"{nameof(key)}: '{key}'");
			}
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (!tryPrune()
						&& Dictionary.TryGetValue(key, out WeakReference weakValue)
						&& !weakValue.IsAlive) {
					Dictionary.Remove(key);
					HandleValueCollected?.Invoke(key, weakValue);
				}
				Dictionary[key] = newWeakReference(value);
			}
		}


		/// <summary>
		/// Returns the count of ONLY live values.
		/// </summary>
		public int Count
		{
			get {
				tryPrune();
				return Dictionary.Values
						.Count(value => value.IsAlive);
			}
		}

		/// <summary>
		/// Returns the value specified on construction.
		/// </summary>
		public bool IsReadOnly
			=> Dictionary.IsReadOnly;

		/// <summary>
		/// Returns a new collection of ONLY live instances.
		/// </summary>
		public ICollection<TKey> Keys
			=> keys;

		private List<TKey> keys
		{
			get {
				tryPrune();
				return new List<TKey>(
						Dictionary.Where(kv => kv.Value.IsAlive)
								.Select(kv => kv.Key));
			}
		}

		/// <summary>
		/// Returns a new collection of ONLY live values.
		/// </summary>
		public ICollection<TValue> Values
			=> values;

		private List<TValue> values
			=> new List<TValue>(
					Dictionary.Values.Where(value => value.IsAlive)
							.Select(value => (TValue)value.Target));

		/// <summary>
		/// Adds the value under the key.
		/// </summary>
		/// <param name="item">Neither Key nor Value may be null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void Add(KeyValuePair<TKey, TValue> item)
			=> Add(item.Key, item.Value);


		/// <summary>
		/// Adds the value under the key.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="value">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void Add(TKey key, TValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (!tryPrune()
					&& Dictionary.TryGetValue(key, out WeakReference weakValue)
					&& !weakValue.IsAlive) {
				Dictionary.Remove(key);
				HandleValueCollected?.Invoke(key, weakValue);
			}
			Dictionary.Add(key, newWeakReference(value));
		}

		/// <summary>
		/// Clears the collection.
		/// </summary>
		public void Clear()
		{
			tryPrune();
			Dictionary.Clear();
		}

		/// <summary>
		/// Tries to find the Key, and the value must alive and be Equal.
		/// </summary>
		/// <param name="item">Neither Key nor Value may be null.</param>
		/// <returns>True if the value is found and alive.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			if (item.Value == null)
				throw new ArgumentNullException(nameof(item));
			tryPrune();
			return Dictionary.TryGetValue(item.Key, out WeakReference weakValue)
					&& weakValue.IsAlive
					&& (weakValue.Target?.Equals(item.Value) ?? false);
		}

		/// <summary>
		/// Finds the key in the collection only if the value is also alive.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>True if the key is found and the value is alive.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool ContainsKey(TKey key)
		{
			tryPrune();
			return Dictionary.TryGetValue(key, out WeakReference weakValue)
					&& weakValue.IsAlive;
		}

		/// <summary>
		/// Copies only live values.
		/// </summary>
		/// <param name="array">Not null.</param>
		/// <param name="arrayIndex">Checked.</param>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			=> CopyTo(array as Array, arrayIndex);

		/// <summary>
		/// Returns only live values.
		/// </summary>
		/// <returns>Not null.</returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			tryPrune();
			foreach (KeyValuePair<TKey, WeakReference> kv in Dictionary) {
				TValue value;
				if (kv.Value.IsAlive
						&& ((value = (TValue)kv.Value.Target) != null)) {
					yield return new KeyValuePair<TKey, TValue>(kv.Key, value);
				}
			}
		}

		/// <summary>
		/// Removes the entry if the key is found; and either the value Equals the
		/// argument, or the value is not alive.
		/// </summary>
		/// <param name="item">Not null.</param>
		/// <returns>False if the Key is not found, or the found value is alive
		/// but not Equal to the argument Value.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			tryPrune();
			if (!Dictionary.TryGetValue(item.Key, out WeakReference weakValue))
				return false;
			if (!weakValue.IsAlive) {
				Dictionary.Remove(item.Key);
				HandleValueCollected?.Invoke(item.Key, weakValue);
				return true;
			}
			if (!(weakValue.Target?.Equals(item.Value) ?? false))
				return false;
			Dictionary.Remove(item.Key);
			return true;
		}

		/// <summary>
		/// Removes the entry from the collection.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>True if removed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool Remove(TKey key)
		{
			tryPrune();
			if (!Dictionary.TryGetValue(key, out WeakReference weakValue))
				return false;
			Dictionary.Remove(key);
			if (!weakValue.IsAlive)
				HandleValueCollected?.Invoke(key, weakValue);
			return true;
		}

		/// <summary>
		/// Tries to find the Key, and returns true only if found and the value
		/// is alive.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="value">Not null.</param>
		/// <returns>False if the Key is not found or the found value is not alive.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryGetValue(TKey key, out TValue value)
		{
			tryPrune();
			if (Dictionary.TryGetValue(key, out WeakReference weakValue))
				return (value = (TValue)weakValue.Target) != null;
			value = default;
			return false;
		}

		/// <summary>
		/// Returns only live values.
		/// </summary>
		/// <returns>Not null.</returns>
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		/// <summary>
		/// Copies only live values.
		/// </summary>
		/// <param name="array">Not null.</param>
		/// <param name="index">Checked.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (array.Rank != 1)
				throw new ArgumentException("Multidimensional array not supported.");
			if (array.GetLowerBound(0) != 0)
				throw new ArgumentException("Array has non-zero lower bound.");
			if ((index < 0)
					|| (index > array.Length)) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if (index == array.Length)
				return;
			tryPrune();
			int i = index;
			TValue value;
			if (array.GetType()
							.GetElementType()
							?.IsAssignableFrom(typeof(KeyValuePair<TKey, TValue>))
					?? false) {
				foreach (KeyValuePair<TKey, WeakReference> kv in Dictionary) {
					if (!kv.Value.IsAlive
							|| ((value = (TValue)kv.Value.Target) == null)) {
						continue;
					}
					if (i == array.Length) {
						throw new ArgumentException(
								$"Array Length ({array.Length}) or start index ({index}) "
								+ $"is too small for collection Count ({Count}).",
								nameof(array));
					}
					array.SetValue(new KeyValuePair<TKey, TValue>(kv.Key, value), i);
					++i;
				}
			} else if (array.GetType()
							.GetElementType()
							?.IsAssignableFrom(typeof(DictionaryEntry))
					?? false) {
				foreach (KeyValuePair<TKey, WeakReference> kv in Dictionary) {
					if (!kv.Value.IsAlive
							|| ((value = (TValue)kv.Value.Target) == null)) {
						continue;
					}
					if (i == array.Length) {
						throw new ArgumentException(
								$"Array Length ({array.Length}) or start index ({index}) "
								+ $"is too small for collection Count ({Count}).",
								nameof(array));
					}
					array.SetValue(new DictionaryEntry(kv.Key, value), i);
					++i;
				}
			} else {
				throw new ArgumentException(
						$"Array element type is not supported: {array.GetType().GetElementType()}.",
						nameof(array));
			}
		}

		/// <summary>
		/// Note: this implementation is not synchronized.
		/// Returns the value from the underlying collection.
		/// </summary>
		public object SyncRoot
			=> ((ICollection)Dictionary).SyncRoot;

		/// <summary>
		/// Note: this implementation is not synchronized.
		/// Returns the value from the underlying collection.
		/// </summary>
		public bool IsSynchronized
			=> ((ICollection)Dictionary).IsSynchronized;

		/// <summary>
		/// Finds the key in the collection only if the value is also alive.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public bool Contains(object key)
			=> key is TKey tKey && ContainsKey(tKey);

		/// <summary>
		/// Adds the value under the key.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="value">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void Add(object key, object value)
			=> Add((TKey)key, (TValue)value);

		/// <summary>
		/// Returns only live values.
		/// </summary>
		/// <returns>Not null.</returns>
		IDictionaryEnumerator IDictionary.GetEnumerator()
			=> new DictionaryEnumeratorAdapter<TKey, TValue>(GetEnumerator());

		/// <summary>
		/// Removes the entry from the collection.
		/// </summary>
		/// <param name="key"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void Remove(object key)
		{
			if (key is TKey tKey)
				Remove(tKey);
		}

		/// <summary>
		/// Will return null if the key is present but the reference has been collected.
		/// The setter will throw if the value is null.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>May be null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		object IDictionary.this[object key]
		{
			get => this[(TKey)key];
			set => this[(TKey)key] = (TValue)value;
		}

		/// <summary>
		/// Returns a new collection of ONLY live instances.
		/// </summary>
		ICollection IDictionary.Keys
			=> keys;

		/// <summary>
		/// Returns a new collection of ONLY live values.
		/// </summary>
		ICollection IDictionary.Values
			=> values;

		/// <summary>
		/// Returns the value from the underlying collection.
		/// </summary>
		public bool IsFixedSize
			=> ((IDictionary)Dictionary).IsFixedSize;

		/// <summary>
		/// Returns a new collection of ONLY live instances.
		/// </summary>
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
			=> Keys;

		/// <summary>
		/// Returns a new collection of ONLY live values.
		/// </summary>
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
			=> values;
	}
}
