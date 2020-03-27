using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Collections
{
	/// <summary>
	/// Implements <see cref="IOwnerCache{TKey, TOwner, TValue}"/>.
	/// Provides the <see cref="EntryFactory(TKey)"/> method and the
	/// <typeparamref name="TEntry"/> Type parameter to allow you to specify
	/// a specific <see cref="OwnerCacheEntry{TKey, TOwner, TValue}"/>
	/// implementation Type. Protected methods are provide for added
	/// and removed Keys. Please notice also that ALL operations on the
	/// cache CAN result in that invoker removing an entry by side
	/// effect if the entry's Owners and/or Value have become
	/// weakly collected --- that invoker will process the Key
	/// removal before its' invoked method returns (which IS
	/// only possible if an entry HAS specified weak references).
	/// Note that by default, Owners are compared by REFERENCE
	/// equality; and Keys by the default value equality for the type.
	/// </summary>
	/// <typeparam name="TKey">The Key Type.</typeparam>
	/// <typeparam name="TOwner">The Owner Type</typeparam>
	/// <typeparam name="TValue">The Value Type.</typeparam>
	/// <typeparam name="TEntry">Specifies your specific
	/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}"/> implementation Type.</typeparam>
	public class OwnerCache<TKey, TOwner, TValue, TEntry>
			: IOwnerCache<TKey, TOwner, TValue>,
					OwnerCacheEntry<TKey, TOwner, TValue>.ICache
			where TOwner : class
			where TValue : class
			where TEntry : OwnerCacheEntry<TKey, TOwner, TValue>
	{
		/// <summary>
		/// Protects all asccess. This is the actual object returned
		/// from <see cref="OwnerCacheEntry{TKey, TOwner, TValue}.ICache.SyncLock"/>;
		/// AND is shared by all entries.
		/// </summary>
		protected readonly object GlobalSyncLock = new object();

		/// <summary>
		/// This is the actual entry collection: must be protected with the
		/// <see cref="GlobalSyncLock"/>.
		/// </summary>
		protected readonly Dictionary<TKey, TEntry> Entries;

		private readonly Func<TKey, TEntry> entryFactory;
		private bool isHoldWeakOwners;
		private bool isHoldWeakValues;
		private bool isPruneOnEveryMutation;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ownerComparer">Optional: if null, then reference equality is used.</param>
		/// <param name="keyComparer">Optional: if null then the default
		/// <see cref="EqualityComparer{T}"/> for the type <typeparamref name="TKey"/>
		/// is used.</param>
		/// <param name="initialKeyCapacity">Specifies the initial capatity for the entry cache.</param>
		/// <param name="entryFactory">Optional delegate can provide the implementation for the
		/// protected virtual <see cref="EntryFactory(TKey)"/> method; which constructs
		/// all new entries. Can be null if that method is overridden; and can also
		/// be null if this <typeparamref name="TEntry"/> Type IS
		/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}"/> (the default).
		/// For any custom tyoe you must eother provide this delegate or override
		/// the factory method.</param>
		public OwnerCache(
				IEqualityComparer<TOwner> ownerComparer = null,
				IEqualityComparer<TKey> keyComparer = null,
				int initialKeyCapacity = 8,
				Func<TKey, TEntry> entryFactory = null)
		{
			OwnerComparer = ownerComparer ?? EquatableHelper.ReferenceEqualityComparer<TOwner>();
			KeyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
			Entries = new Dictionary<TKey, TEntry>(initialKeyCapacity, KeyComparer);
			this.entryFactory = entryFactory;
			if ((this.entryFactory == null)
					&& (typeof(TEntry) == typeof(OwnerCacheEntry<TKey, TOwner, TValue>)))
				this.entryFactory = DefaultEntryFactory;
			static TEntry DefaultEntryFactory(TKey key)
				=> (TEntry)new OwnerCacheEntry<TKey, TOwner, TValue>(key);
		}


		private TValue tryGetNewValue(
				TValue priorValue,
				Func<TValue, TValue> addOrReplaceValue,
				TKey key,
				string delegateArgumentName)
		{
			TValue value = addOrReplaceValue(priorValue);
			if (value == null)
				throw new ArgumentException($"Value cannot be null for: {key}.", delegateArgumentName);
			return value;
		}

		private (TEntry entry, TValue currentValue) handleAddOwner(
				TKey key,
				TOwner owner,
				Func<TValue, TValue> addOrReplaceValue,
				bool isTryAdd,
				bool forceReplace,
				Func<TEntry, bool> tryAddPredicate = null)
		{
			TEntry entry;
			TValue currentValue;
			bool wasEntryAdded;
			Action afterValueReplaced;
			lock (GlobalSyncLock) {
				if (Entries.TryGetValue(key, out entry)) {
					wasEntryAdded = false;
					TValue priorValue;
					if (isTryAdd) {
						return entry.TryAddOwner(owner, out _, out priorValue, false, TryAdd) == false
								? (default(TEntry), default(TValue))
								: (entry, priorValue);
					} else if (!forceReplace) {
						if (entry.TryAddOwner(owner, out _, out priorValue) != false)
							return (entry, priorValue);
					} else
						priorValue = entry.Value;
					currentValue = tryGetNewValue(priorValue, addOrReplaceValue, key, nameof(addOrReplaceValue));
					entry.TryReplaceValue(currentValue, out priorValue, out afterValueReplaced, true);
					bool? entryResult = entry.TryAddOwner(owner, out _, out currentValue);
					Debug.Assert(entryResult != false, "entryResult != false");
				} else {
					afterValueReplaced = null;
					if (isTryAdd) {
						wasEntryAdded = false;
						currentValue = default;
					} else {
						wasEntryAdded = true;
						currentValue = tryGetNewValue(null, addOrReplaceValue, key, nameof(addOrReplaceValue));
						entry = EntryFactory(key);
						entry.IsHoldWeakOwners = isHoldWeakOwners;
						entry.IsHoldWeakValue = isHoldWeakValues;
						entry.IsPruneOnEveryMutation = isPruneOnEveryMutation;
						Entries[key] = entry;
						entry.NotifyAdded(this, owner, currentValue);
					}
				}
			}
			if (wasEntryAdded)
				AfterAdded(entry);
			else
				afterValueReplaced?.Invoke();
			return (entry, currentValue);
			bool TryAdd()
				=> tryAddPredicate(entry);
		}


		/// <summary>
		/// Invoked outside the Monitor lock with each new entry.
		/// </summary>
		/// <param name="entry">Not null.</param>
		protected virtual void AfterAdded(TEntry entry) { }

		/// <summary>
		/// Invoked outside the Monitor lock with each removed entry.
		/// </summary>
		/// <param name="entry">Not null.</param>
		/// <param name="removedValue">Note: this can be null: privides the
		/// Value at the time this entry is removed.</param>
		protected virtual void AfterRemoved(TEntry entry, TValue removedValue) { }


		/// <summary>
		/// This protected virtual method must construct each new entry to be added for the
		/// given <paramref name="key"/>. This implementation will invoke the delegate
		/// provided at construction; or otherwise throws <see cref="NotImplementedException"/>.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>Not null.</returns>
		protected virtual TEntry EntryFactory(TKey key)
		{
			if (entryFactory == null)
				throw new NotImplementedException($"{GetType().GetFriendlyName(true)}.{nameof(EntryFactory)}");
			TEntry newEntry = entryFactory(key);
			Debug.Assert(newEntry != null, "newEntry != null");
			Debug.Assert(KeyComparer.Equals(key, newEntry.Key), "KeyComparer.Equals(key, newEntry.Key)");
			return newEntry;
		}


		/// <summary>
		/// The non-null comparer used to identify unique Keys.
		/// </summary>
		public IEqualityComparer<TKey> KeyComparer { get; }

		/// <summary>
		/// The non-null comparer used to identify unique Owners.
		/// </summary>
		public IEqualityComparer<TOwner> OwnerComparer { get; }


		/// <summary>
		/// This property provides a default value for
		/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}.IsHoldWeakOwners"/>
		/// for all NEW entries. Note that each entry can then
		/// set its' own property individually. Defaults to false.
		/// </summary>
		public bool IsHoldWeakOwners
		{
			get {
				lock (GlobalSyncLock) {
					return isHoldWeakOwners;
				}
			}
			set {
				lock (GlobalSyncLock) {
					isHoldWeakOwners = value;
				}
			}
		}

		/// <summary>
		/// This property provides a default value for
		/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}.IsHoldWeakValues"/>
		/// for all NEW entries. Note that each entry can then
		/// set its' own property individually. Defaults to false.
		/// </summary>
		public bool IsHoldWeakValues
		{
			get {
				lock (GlobalSyncLock) {
					return isHoldWeakValues;
				}
			}
			set {
				lock (GlobalSyncLock) {
					isHoldWeakValues = value;
				}
			}
		}

		/// <summary>
		/// This property provides a default value for
		/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}.IsPruneOnEveryMutation"/>
		/// for all NEW entries. Note that each entry can then
		/// set its' own property individually. Defaults to false.
		/// </summary>
		public bool IsPruneOnEveryMutation
		{
			get {
				lock (GlobalSyncLock) {
					return isPruneOnEveryMutation;
				}
			}
			set {
				lock (GlobalSyncLock) {
					isPruneOnEveryMutation = value;
				}
			}
		}


		object OwnerCacheEntry<TKey, TOwner, TValue>.ICache.SyncLock
			=> GlobalSyncLock;

		void OwnerCacheEntry<TKey, TOwner, TValue>.ICache.NotifyRemoveWeak(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (Monitor.IsEntered(GlobalSyncLock)) {
				throw new InvalidOperationException(
						$"{nameof(OwnerCacheEntry<TKey, TOwner, TValue>.ICache.NotifyRemoveWeak)}"
						+ $" cannot be invoked while holding the SyncLock.");
			}
			TEntry entry;
			TValue removedValue;
			lock (GlobalSyncLock) {
				if (!Entries.TryGetValue(key, out entry)
						|| entry.IsAlive)
					return;
				Entries.Remove(key);
				removedValue = entry.NotifyRemoved();
			}
			AfterRemoved(entry, removedValue);
		}


		public bool TryGetValue(TKey key, out IReadOnlyCollection<TOwner> owners, out TValue value)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			TEntry entry;
			lock (GlobalSyncLock) {
				if (!Entries.TryGetValue(key, out entry)) {
					owners = null;
					value = null;
					return false;
				}
				owners = entry.Owners;
				value = entry.Value;
				if (!owners.IsNullOrEmpty()
						&& (value != null))
					return true;
				Entries.Remove(key);
				value = entry.NotifyRemoved();
			}
			AfterRemoved(entry, value);
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
			=> TryGetValue(key, out _, out value);

		public bool TryGetEntry(TKey key, out OwnerCacheEntry<TKey, TOwner, TValue> entry)
		{
			if (TryGetEntry(key, out TEntry tEntry)) {
				entry = tEntry;
				return true;
			}
			entry = null;
			return false;
		}

		/// <summary>
		/// Overloads this <see cref="TryGetEntry(TKey, out OwnerCacheEntry{TKey, TOwner, TValue})"/>
		/// method to return the <typeparamref name="TEntry"/> Type.
		/// Notice that this method does not
		/// operate atomically if the entry is holding weak Owners and/or a
		/// weak Value: this will return the entry, which is current and
		/// alive at this moment, but then your next access to the
		/// returned <paramref name="entry"/> does not guarantee to return
		/// live Owners and/or a Value at that time.
		/// </summary>
		/// <typeparam name="TEntry">Your actual entry type.</typeparam>
		/// <param name="key">Not null.</param>
		/// <param name="entry">The result.</param>
		/// <returns>True if there is a cached value and at least one owner
		/// at this time.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryGetEntry(TKey key, out TEntry entry)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			TValue removedValue;
			lock (GlobalSyncLock) {
				if (!Entries.TryGetValue(key, out entry))
					return false;
				if (entry.IsAlive)
					return true;
				Entries.Remove(key);
				removedValue = entry.NotifyRemoved();
			}
			AfterRemoved(entry, removedValue);
			return false;
		}


		public OwnerCacheEntry<TKey, TOwner, TValue> GetOrAdd(
				TKey key,
				TOwner owner,
				Func<TValue, TValue> addOrReplaceValue,
				bool forceReplace = false)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));
			if (addOrReplaceValue == null)
				throw new ArgumentNullException(nameof(addOrReplaceValue));
			return handleAddOwner(key, owner, addOrReplaceValue, false, forceReplace).entry;
		}

		/// <summary>
		/// Overloads this <see cref="GetOrAdd(TKey, TOwner, Func{TValue, TValue}, bool)"/>
		/// method to return  the <typeparamref name="TEntry"/> Type.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="owner">Not null.</param>
		/// <param name="addOrReplaceValue">Not null. This is invoked if the
		/// value must be added now: the delegate always receives the current
		/// value; and in this case, it is always null --- a value must be added
		/// now for this first owner. Otherwise, if <paramref name="forceReplace"/>
		/// is set true, then this will be invoked with the current value, and
		/// must return the new value (or you may return a non-null existing value
		/// after inspecting it here if it is not to be replaced. Cannot return null.</param>
		/// <param name="forceReplace">Defaults to false. If set true, then the
		/// value is always replaced now if there is any existing value.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If the
		/// <paramref name="addOrReplaceValue"/> delegate returns null.</exception>
		public TEntry GetOrAddEntry(
				TKey key,
				TOwner owner,
				Func<TValue, TValue> addOrReplaceValue,
				bool forceReplace = false)
			=> handleAddOwner(key, owner, addOrReplaceValue, false, forceReplace).entry;

		public TValue AddOwner(
				TKey key,
				TOwner owner,
				Func<TValue, TValue> addOrReplaceValue,
				bool forceReplace = false)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));
			if (addOrReplaceValue == null)
				throw new ArgumentNullException(nameof(addOrReplaceValue));
			return handleAddOwner(key, owner, addOrReplaceValue, false, forceReplace).currentValue;
		}

		/// <summary>
		/// This method will add the given <paramref name="owner"/> only if
		/// there is a current entry for this <paramref name="key"/>.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="owner">Not null. Will be added or ensured if already added.</param>
		/// <param name="entry">Returns the entry if the owner is added.</param>
		/// <param name="predicate">This is optional: if provided, and if the
		/// <paramref name="entry"/> is alive and does not already hold this
		/// <paramref name="owner"/>, then this will be invoked before adding
		/// this owner. If this returns false, the owner is not added.</param>
		/// <returns>True if the <paramref name="owner"/> is added or already
		/// present; and the <paramref name="entry"/> is returned.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryAddOwner(TKey key, TOwner owner, out TEntry entry, Func<TEntry, bool> predicate = null)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));
			entry = handleAddOwner(key, owner, currentValue => currentValue, true, false, predicate).entry;
			return (entry != null) && entry.IsAlive;
		}

		public bool RemoveOwner(TKey key, TOwner owner)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));
			TEntry entry;
			bool result;
			TValue removedValue;
			lock (GlobalSyncLock) {
				if (!Entries.TryGetValue(key, out entry))
					return false;
				result = entry.TryRemoveOwner(owner, out bool isEntryAlive);
				if (isEntryAlive)
					return result;
				Entries.Remove(key);
				removedValue = entry.NotifyRemoved();
			}
			AfterRemoved(entry, removedValue);
			return result;
		}


		public bool TryReplace(TKey key, Func<TValue, TValue> replaceValue)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (replaceValue == null)
				throw new ArgumentNullException(nameof(replaceValue));
			TEntry entry;
			bool result;
			TValue removedValue;
			Action callbackOutsideLock;
			lock (GlobalSyncLock) {
				if (!Entries.TryGetValue(key, out entry))
					return false;
				result = entry.TryReplaceValue(
						tryGetNewValue(entry.Value, replaceValue, key, nameof(replaceValue)),
						out removedValue,
						out callbackOutsideLock);
				if (!result) {
					Entries.Remove(key);
					removedValue = entry.NotifyRemoved();
				}
			}
			if (result)
				callbackOutsideLock?.Invoke();
			else
				AfterRemoved(entry, removedValue);
			return result;
		}


		public bool Remove(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			TEntry entry;
			TValue removedValue;
			lock (GlobalSyncLock) {
				if (!Entries.TryGetValue(key, out entry))
					return false;
				Entries.Remove(key);
				removedValue = entry.NotifyRemoved();
			}
			AfterRemoved(entry, removedValue);
			return true;
		}

		/// <summary>
		/// Provides a method to atomically inslect an entry and remove it.
		/// </summary>
		/// <param name="key">Required.</param>
		/// <param name="removePredicate">Required. This is invoked if there is
		/// an entry for this <paramref name="key"/>. if this returns TRUE,
		/// then the entry is removed.</param>
		/// <param name="entry">Will return the entry, only if there is an entry
		/// for this <paramref name="key"/> --- and this may now be removed.</param>
		/// <returns>Trus if there is an entry for this <paramref name="key"/>,
		/// and the <paramref name="removePredicate"/> returns true, and the
		/// <paramref name="entry"/> is now removed.</returns>
		/// <exception cref="ArgumentNullException"/>
		public bool TryRemove(TKey key, Func<TEntry, bool> removePredicate, out TEntry entry)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (removePredicate is null)
				throw new ArgumentNullException(nameof(removePredicate));
			TValue removedValue;
			lock (GlobalSyncLock) {
				if (!Entries.TryGetValue(key, out entry)
						|| !removePredicate(entry))
					return false;
				Entries.Remove(key);
				removedValue = entry.NotifyRemoved();
			}
			AfterRemoved(entry, removedValue);
			return true;
		}


		public void Clear()
		{
			foreach (TKey key in GetKeys()) {
				Remove(key);
			}
		}


		public int Count
		{
			get {
				lock (GlobalSyncLock) {
					return Entries.Count;
				}
			}
		}

		public IEnumerable<TKey> GetKeys()
		{
			lock (GlobalSyncLock) {
				return Entries.Keys.ToArray();
			}
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}[{Count}]";
	}
}
