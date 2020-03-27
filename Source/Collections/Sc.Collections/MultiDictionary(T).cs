using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections.Specialized;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Collections
{
	/// <summary>
	/// Implements <see cref="IMultiDictionary{TKey,TValue}"/>. This collection
	/// uses an underlying <see cref="Dictionary{TKey,TValue}"/>; and is
	/// serializable. You may also specify where each element is added: by
	/// default, each added element is appended to the list for the key; and
	/// you may specify that the new element is inserted at the beginning
	/// of the list instead. Provides methods to inspect and mutate the dictionary.
	/// Not synchronized.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys.</typeparam>
	/// <typeparam name="TValue">The type of the list contents.</typeparam>
	[DataContract]
	public class MultiDictionary<TKey, TValue>
			: IMultiDictionary<TKey, TValue>,
					ICollection<KeyValuePair<TKey, TValue>>,
					ICollection
	{
		/// <summary>
		/// Implements a readonly wrapper.
		/// </summary>
		private sealed class KeysCollection
				: IReadOnlyCollection<TKey>
		{
			private readonly MultiDictionary<TKey, TValue> multiDictionary;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="multiDictionary">Required.</param>
			public KeysCollection(MultiDictionary<TKey, TValue> multiDictionary)
				=> this.multiDictionary = multiDictionary ?? throw new ArgumentNullException(nameof(multiDictionary));


			public int Count
				=> multiDictionary.Dictionary.Keys.Count;

			public IEnumerator<TKey> GetEnumerator()
				=> multiDictionary.Dictionary.Keys.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}


		/// <summary>
		/// Holds the values; and is serialized.
		/// </summary>
		[DataMember]
		protected Dictionary<TKey, List<TValue>> Dictionary;

		/// <summary>
		/// Holds the default initial capacity for each new list. Serialized.
		/// </summary>
		[DataMember]
		protected int DefaultListInitialCapacity;

		private KeysCollection keysCollection;


		/// <summary>
		/// Constructor. creates a new underlying Dictionary with the initial capacity.
		/// The given optional <see cref="IEqualityComparer{T}"/>
		/// is used for keys. NOTICE that the comparer
		/// must be serializable if this collection is serialized.
		/// </summary>
		/// <param name="initialCapacity">&gt;= 0.</param>
		/// <param name="comparer">Optional: if null, the default implementation is used.</param>
		/// <param name="isReadonly">If true, the collection will not allow mutations.</param>
		public MultiDictionary(
				IEqualityComparer<TKey> comparer = null,
				int initialCapacity = 8,
				bool isReadonly = false)
		{
			Dictionary = comparer == null
					? new Dictionary<TKey, List<TValue>>(initialCapacity)
					: new Dictionary<TKey, List<TValue>>(initialCapacity, comparer);
			DefaultListInitialCapacity = Math.Max(2, Math.Min(32, initialCapacity / 2));
			IsReadOnly = isReadonly;
			keysCollection = new KeysCollection(this);
		}

		/// <summary>
		/// Constructor. creates a new underlying Dictionary and copies the
		/// elements from the collection.
		/// The given optional <see cref="IEqualityComparer{T}"/> is used for keys.
		/// NOTICE that the comparer
		/// must be serializable of this collection is serialized.
		/// </summary>
		/// <param name="collection">Not null..</param>
		/// <param name="comparer">NOTICE: can be null: if null,
		/// the default implementation is used.</param>
		/// <param name="isReadonly">NOTICE: this is nullable.
		/// If true, the collection will not allow
		/// mutations. If false, it does. And if NULL,
		/// the value is COPIED from the argument collection.</param>
		public MultiDictionary(
				MultiDictionary<TKey, TValue> collection,
				IEqualityComparer<TKey> comparer = null,
				bool? isReadonly = false)
		{
			Dictionary = comparer == null
					? new Dictionary<TKey, List<TValue>>(collection.Count)
					: new Dictionary<TKey, List<TValue>>(collection.Count, comparer);
			DefaultListInitialCapacity = collection.DefaultListInitialCapacity;
			IsReadOnly = isReadonly ?? collection.IsReadOnly;
			foreach (KeyValuePair<TKey, List<TValue>> kv in collection.Dictionary) {
				Dictionary[kv.Key] = new List<TValue>(kv.Value);
			}
			keysCollection = new KeysCollection(this);
		}

		/// <summary>
		/// Constructor. creates a new underlying Dictionary
		/// and copies the elements from the enumeration.
		/// The given optional <see cref="IEqualityComparer{T}"/>
		/// is used for keys. NOTICE that the comparer
		/// must be serializable of this object is serialized.
		/// </summary>
		/// <param name="collection">Not null..</param>
		/// <param name="comparer">NOTICE: can be null: if null,
		/// the default implementation is used.</param>
		/// <param name="isReadonly">If true, the collection will
		/// not allow mutations.</param>
		public MultiDictionary(
				IEnumerable<KeyValuePair<TKey, TValue>> collection,
				IEqualityComparer<TKey> comparer = null,
				bool isReadonly = false)
		{
			Dictionary = comparer == null
					? new Dictionary<TKey, List<TValue>>()
					: new Dictionary<TKey, List<TValue>>(comparer);
			DefaultListInitialCapacity = 8;
			IsReadOnly = isReadonly;
			AddRange(collection);
			keysCollection = new KeysCollection(this);
		}


		[OnDeserialized]
		private void onDeserialized(StreamingContext c)
			=> keysCollection = new KeysCollection(this);

		private void throwIfReadOnly()
		{
			if (IsReadOnly) {
				throw new InvalidOperationException(
						$"Cannot modify {nameof(ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly)}"
						+ $" {nameof(MultiDictionary<TKey, TValue>)}.");
			}
		}


		/// <summary>
		/// Ensures the key is present and returns the list.
		/// Creates a new list if needed.
		/// THROWS if this is readonly.
		/// </summary>
		protected List<TValue> EnsureKey(TKey key)
		{
			throwIfReadOnly();
			if (Dictionary.TryGetValue(key, out List<TValue> result)
					&& (result != null))
				return result;
			result = new List<TValue>(DefaultListInitialCapacity);
			Dictionary[key] = result;
			return result;
		}

		/// <summary>
		/// Performs the add operation for a list: the new item is added
		/// to this list according to <see cref="InsertNewValuesAtZero"/>.
		/// </summary>
		/// <param name="list">Required.</param>
		/// <param name="newItem">NOTICE: not checked.</param>
		protected void AddElement(List<TValue> list, TValue newItem)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));
			if (InsertNewValuesAtZero)
				list.Insert(0, newItem);
			else
				list.Add(newItem);
		}


		/// <summary>
		/// This is the <see cref="IEqualityComparer{T}"/> used for keys. This is set on
		/// construction; and is ONLY serialized if the underlying <see cref="Dictionary{TKey,TValue}"/>
		/// supports serializing it. NOTICE: you may reset the value here; but this will
		/// create a NEW underlying Dictionary now.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public IEqualityComparer<TKey> Comparer
		{
			get => Dictionary.Comparer;
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(MultiDictionary<TKey, TValue>.Comparer));
				Dictionary = new Dictionary<TKey, List<TValue>>(Dictionary, value);
			}
		}

		/// <summary>
		/// Defaults to false. This controls where new elements are added in
		/// the list for each key. If false, the new value is appended to the
		/// list. If set true, the new value is inserted at index 0.
		/// </summary>
		public bool InsertNewValuesAtZero { get; set; }


		public int Count
			=> Dictionary.Count;

		[DataMember]
		public bool IsReadOnly { get; private set; }

		public object SyncRoot
			=> ((ICollection)Dictionary).SyncRoot;

		public bool IsSynchronized
			=> ((ICollection)Dictionary).IsSynchronized;


		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (KeyValuePair<TKey, List<TValue>> kv in Dictionary) {
				foreach (TValue value in kv.Value) {
					yield return new KeyValuePair<TKey, TValue>(kv.Key, value);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetListEnumerator()
		{
			foreach (KeyValuePair<TKey, List<TValue>> kv in Dictionary) {
				yield return new KeyValuePair<TKey, IReadOnlyList<TValue>>(kv.Key, kv.Value);
			}
		}

		public IEnumerator<TValue> GetValueEnumerator()
		{
			foreach (List<TValue> values in Dictionary.Values) {
				foreach (TValue value in values) {
					yield return value;
				}
			}
		}


		public bool Contains(KeyValuePair<TKey, TValue> item)
			=> Dictionary.TryGetValue(item.Key, out List<TValue> values)
					&& values.Contains(item.Value);

		/// <summary>
		/// Returns true if the key is present.
		/// </summary>
		/// <param name="key">The key to find.</param>
		/// <returns>True if found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool ContainsKey(TKey key)
			=> Dictionary.ContainsKey(key);

		/// <summary>
		/// Returns true if the value is present under ANY key.
		/// The <paramref name="comparer"/> is used if provided;
		/// and otherwise the default equality comparer.
		/// </summary>
		/// <param name="value">The value to find.</param>
		/// <param name="comparer">The optional search predicate.</param>
		/// <returns>True if found.</returns>
		public bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer = null)
			=> GetValueEnumerator()
					.AsEnumerable()
					.Contains(value, comparer ?? EqualityComparer<TValue>.Default);

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator()) {
				if (!enumerator.MoveNext()
						|| (arrayIndex >= array.Length))
					return;
				array[arrayIndex] = enumerator.Current;
				++arrayIndex;
			}
		}

		public void CopyTo(Array array, int index)
		{
			using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator()) {
				if (!enumerator.MoveNext()
						|| (index >= array.Length))
					return;
				array.SetValue(enumerator.Current, index);
				++index;
			}
		}


		public IReadOnlyCollection<TKey> Keys
			=> keysCollection;

		/// <summary>
		/// Returns the sum of the counts of all values under any key.
		/// </summary>
		public int CountAllValues
		{
			get {
				int count = 0;
				foreach (List<TValue> list in Dictionary.Values) {
					count += list.Count;
				}
				return count;
			}
		}

		/// <summary>
		/// Returns a new list containing all values for all keys.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		public List<TValue> GetAllValues()
		{
			List<TValue> result = new List<TValue>(Dictionary.Sum(kv => kv.Value.Count));
			foreach (List<TValue> value in Dictionary.Values) {
				result.AddRange(value);
			}
			return result;
		}

		/// <summary>
		/// Returns THE ACTUAL list of all values for the key. Returns a new empty
		/// list if the key is not found.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <returns>Not null; may be empty.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public IReadOnlyList<TValue> GetAllValues(TKey key)
			=> Dictionary.TryGetValue(key, out List<TValue> list)
					? list
					: new List<TValue>(0);

		/// <summary>
		/// Fetches THE ACTUAL list of all values for the key; and if not
		/// empty, invokes your Action with the actual mutable list: you may sort,
		/// or arbitrarily mutate this actual list. When returning, if the list is empty,
		/// this key is removed.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="mutate">Required. This will be invoked with a list that
		/// is not null or empty.</param>
		/// <returns>True if your Action was invoked (with a non-null and non-empty list).</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool WithAllValues(TKey key, Action<List<TValue>> mutate)
		{
			if (mutate == null)
				throw new ArgumentNullException(nameof(mutate));
			if (!Dictionary.TryGetValue(key, out List<TValue> list))
				return false;
			mutate(list);
			if (list.Count == 0)
				RemoveKey(key);
			return true;
		}

		/// <summary>
		/// Fetches THE ACTUAL list of all values for the key; and if not
		/// empty, invokes your Func with the actual mutable list: you may sort,
		/// or arbitrarily mutate this actual list. When returning, if the list is empty,
		/// this key is removed.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="mutate">Required. This will be invoked with a list that
		/// is not null or empty. This method will return the result of this func.</param>
		/// <param name="returnIfNotFound">Optional Func that will return this
		/// method's result if the key is not found.</param>
		/// <returns>Your Func's result if the key is found. The optional
		/// <c>returnIfNotFound</c> result if the key is not found. Otherwise default.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public TResult WithAllValues<TResult>(
				TKey key,
				Func<List<TValue>, TResult> mutate,
				Func<TResult> returnIfNotFound = null)
		{
			if (mutate == null)
				throw new ArgumentNullException(nameof(mutate));
			if (!Dictionary.TryGetValue(key, out List<TValue> list)) {
				return returnIfNotFound != null
						? returnIfNotFound()
						: default;
			}
			TResult result = mutate(list);
			if (list.Count == 0)
				RemoveKey(key);
			return result;
		}


		public bool TryGetValues(TKey key, out IReadOnlyList<TValue> values)
		{
			if (Dictionary.TryGetValue(key, out List<TValue> list)) {
				values = list;
				return true;
			}
			values = null;
			return false;
		}

		/// <summary>
		/// Returns a new <see cref="IDictionary{TKey,TValue}"/> with all
		/// keys and their values.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		public Dictionary<TKey, List<TValue>> GetAll()
		{
			Dictionary<TKey, List<TValue>> result = new Dictionary<TKey, List<TValue>>();
			foreach (KeyValuePair<TKey, List<TValue>> kv in Dictionary) {
				result[kv.Key] = new List<TValue>(kv.Value);
			}
			return result;
		}


		public void Add(KeyValuePair<TKey, TValue> item)
			=> AddValue(item.Key, item.Value);

		/// <summary>
		/// Adds a new value in the Values collection.
		/// </summary>
		/// <param name="key">The key where to place the item in the value list.</param>
		/// <param name="newItem">The new item to add.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddValue(TKey key, TValue newItem)
			=> AddElement(EnsureKey(key), newItem);

		/// <summary>
		/// Adds a new value in the Values collection only if the value does not already exist.
		/// The <paramref name="comparer"/> is used if provided;
		/// and otherwise the default equality comparer.
		/// </summary>
		/// <param name="key">The key where to place the item in the value list.</param>
		/// <param name="newItem">The new item to add.</param>
		/// <param name="comparer">The optional search predicate.</param>
		/// <returns>True if added.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryAddValue(TKey key, TValue newItem, IEqualityComparer<TValue> comparer = null)
		{
			List<TValue> list = EnsureKey(key);
			if (list.Contains(newItem, comparer ?? EqualityComparer<TValue>.Default))
				return false;
			AddElement(list, newItem);
			return true;
		}


		/// <summary>
		/// Adds a list of values to the value collection. Notice that this
		/// method respects the value of <see cref="InsertNewValuesAtZero"/>:
		/// if that is true, the given list of <paramref name="newItems"/>
		/// is inserted as if by enumerating in reverse order
		/// --- inserting the whole new list in its' order
		/// at the beginning of the current list.
		/// </summary>
		/// <param name="key">The key where to place the item in the values list.</param>
		/// <param name="newItems">The new items to add.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddRange(TKey key, IEnumerable<TValue> newItems)
		{
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			List<TValue> list = EnsureKey(key);
			if ((list.Count == 0)
					|| !InsertNewValuesAtZero)
				list.AddRange(newItems);
			else {
				TValue[] copy = list.ToArray();
				list.Clear();
				list.AddRange(newItems);
				list.AddRange(copy);
			}
			if (list.Count == 0)
				Dictionary.Remove(key);
		}

		/// <summary>
		/// Adds a list of keyed values to the value collection. Notice that this
		/// method respects the value of <see cref="InsertNewValuesAtZero"/>:
		/// if that is true, then for each key, the given list of <paramref name="newItems"/>
		/// is inserted as if by enumerating in reverse order
		/// --- inserting the whole new list in its' order
		/// at the beginning of the current list.
		/// </summary>
		/// <param name="newItems">The new items to add.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> newItems)
		{
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			foreach (IGrouping<TKey, KeyValuePair<TKey, TValue>> group in newItems.GroupBy(kv => kv.Key)) {
				AddRange(group.Key, group.Select(kv => kv.Value));
			}
		}

		/// <summary>
		/// Adds each new value in the Values collection
		/// only if the value does not already exist.
		/// Notice that this
		/// method respects the value of <see cref="InsertNewValuesAtZero"/>:
		/// if that is true, the given list of <paramref name="newItems"/>
		/// is inserted as if by enumerating in reverse order
		/// --- inserting the whole new list in its' order
		/// at the beginning of the current list.
		/// The <paramref name="comparer"/> is used if provided;
		/// and otherwise the default equality comparer.
		/// </summary>
		/// <param name="key">The key where to place the item in the value list.</param>
		/// <param name="newItems">The new items to add.</param>
		/// <param name="comparer">The optional search predicate.</param>
		/// <returns>A count of all items added.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public int TryAddRange(TKey key, IEnumerable<TValue> newItems, IEqualityComparer<TValue> comparer = null)
		{
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			if (comparer == null)
				comparer = EqualityComparer<TValue>.Default;
			List<TValue> list = EnsureKey(key);
			int originalCount = list.Count;
			if (list.Count == 0)
				list.AddRange(newItems.Distinct(comparer));
			else if (!InsertNewValuesAtZero) {
				list.AddRange(
						newItems.Distinct(comparer)
								.Where(item => !list.Contains(item, comparer)));
			}
			else {
				TValue[] copy = list.ToArray();
				list.Clear();
				list.AddRange(
						newItems.Distinct(comparer)
								.Where(item => !copy.Contains(item, comparer)));
				list.AddRange(copy);
			}
			if (list.Count == 0)
				Dictionary.Remove(key);
			return list.Count - originalCount;
		}

		/// <summary>
		/// Adds each new value in the keyed Values collection
		/// only if the value does not already
		/// exist. Notice that this
		/// method respects the value of <see cref="InsertNewValuesAtZero"/>:
		/// if that is true, then for each key, the given list of <paramref name="newItems"/>
		/// is inserted as if by enumerating in reverse order
		/// --- inserting the whole new list in its' order
		/// at the beginning of the current list.
		/// The <paramref name="comparer"/> is used if provided;
		/// and otherwise the default equality comparer.
		/// </summary>
		/// <param name="newItems">The new items to add.</param>
		/// <param name="comparer">The optional search predicate.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void TryAddRange(
				IEnumerable<KeyValuePair<TKey, TValue>> newItems,
				IEqualityComparer<TValue> comparer = null)
		{
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			if (comparer == null)
				comparer = EqualityComparer<TValue>.Default;
			foreach (IGrouping<TKey, KeyValuePair<TKey, TValue>> group in newItems.GroupBy(kv => kv.Key)) {
				TryAddRange(group.Key, group.Select(kv => kv.Value), comparer);
			}
		}


		/// <summary>
		/// ALWAYS removes any elements currently under the <paramref name="key"/>,
		/// and replaces them with the given <paramref name="values"/>.
		/// Returns any existing members.
		/// </summary>
		/// <param name="key">Required.</param>
		/// <param name="values">CAN be null or empty.</param>
		/// <returns>Not null; may be empty.</returns>
		/// <exception cref="ArgumentNullException">If the <paramref name="key"/>
		/// is null.</exception>
		public List<TValue> Replace(TKey key, IEnumerable<TValue> values)
		{
			Dictionary.TryGetValue(key, out List<TValue> removed);
			Dictionary.Remove(key);
			List<TValue> list = EnsureKey(key);
			if (values != null)
				list.AddRange(values);
			if (list.Count == 0)
				Dictionary.Remove(key);
			return removed ?? new List<TValue>(0);
		}


		public bool Remove(KeyValuePair<TKey, TValue> item)
			=> RemoveValue(item.Key, item.Value);

		/// <summary>
		/// Removes a key from the dict.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns>Returns false if the key was not found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool RemoveKey(TKey key)
		{
			throwIfReadOnly();
			return Dictionary.Remove(key);
		}

		/// <summary>
		/// Removes a specific element from the dict.
		/// If the value list is empty the key is removed from the dict.
		/// </summary>
		/// <param name="key">The key from where to remove the value.</param>
		/// <param name="value">The value to remove.</param>
		/// <returns>Returns false if the key or the value was not found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool RemoveValue(TKey key, TValue value)
		{
			throwIfReadOnly();
			if (!Dictionary.TryGetValue(key, out List<TValue> list))
				return false;
			bool result = list.Remove(value);
			if (list.Count == 0)
				Dictionary.Remove(key);
			return result;
		}

		/// <summary>
		/// Removes the first specific element from the dict.
		/// If the value list is empty the key is removed from the dict.
		/// </summary>
		/// <param name="key">The key from where to remove the value.</param>
		/// <param name="match">The predicate to match the item.</param>
		/// <returns>Returns false if the key or the value was not found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool RemoveValue(TKey key, Predicate<TValue> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			throwIfReadOnly();
			if (!Dictionary.TryGetValue(key, out List<TValue> list))
				return false;
			int index = list.FindIndex(match);
			if (index < 0)
				return false;
			list.RemoveAt(index);
			if (list.Count == 0)
				Dictionary.Remove(key);
			return true;
		}

		/// <summary>
		/// Removes all items that match the predicate under any key.
		/// If the value list is empty the key is removed from the dict.
		/// </summary>
		/// <param name="match">The predicate to match the items.</param>
		/// <returns>Returns 0 if the predicate had no matches.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public int RemoveAllValues(Predicate<TValue> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			throwIfReadOnly();
			int result = 0;
			foreach (TKey key in Keys.ToArray()) {
				List<TValue> list = Dictionary[key];
				result += list.RemoveAll(match);
				if (list.Count == 0)
					Dictionary.Remove(key);
			}
			return result;
		}

		/// <summary>
		/// Removes all items under the key that match the predicate.
		/// If the value list is empty the key is removed from the dict.
		/// </summary>
		/// <param name="key">The key from where to remove the value.</param>
		/// <param name="match">The predicate to match the items</param>
		/// <returns>Returns 0 if the key was not found or if the predicate
		/// had no matches.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public int RemoveAllValues(TKey key, Predicate<TValue> match)
		{
			if (match == null)
				throw new ArgumentNullException(nameof(match));
			throwIfReadOnly();
			if (!Dictionary.TryGetValue(key, out List<TValue> list))
				return 0;
			int count = list.RemoveAll(match);
			if (list.Count == 0)
				Dictionary.Remove(key);
			return count;
		}


		public void Clear()
		{
			throwIfReadOnly();
			Dictionary.Clear();
		}

		public override string ToString()
			=> $"{GetType().GetFriendlyName()}[{Dictionary}]";
	}
}
