using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Sc.Util.System;


namespace Sc.Collections
{
	/// <summary>
	/// Defines the entry that holds the <see cref="Key"/>,
	/// <see cref="Owners"/> and <see cref="Value"/>, for each
	/// entry in an <see cref="IOwnerCache{TKey, TOwner, TValue}"/>.
	/// </summary>
	/// <typeparam name="TKey">The cache <see cref="Key"/> Type.</typeparam>
	/// <typeparam name="TOwner">The cache <see cref="Owners"/> Type.</typeparam>
	/// <typeparam name="TValue">The cache <see cref="Value"/> Type.</typeparam>
	public class OwnerCacheEntry<TKey, TOwner, TValue>
			where TOwner : class
			where TValue : class
	{
		/// <summary>
		/// Internal cache implementation interface.
		/// </summary>
		internal interface ICache
				: IOwnerCacheBase<TKey, TOwner, TValue>
		{
			/// <summary>
			/// Protects all asccess.
			/// </summary>
			object SyncLock { get; }

			/// <summary>
			/// Provides an implementation method for an entry to
			/// double-check and possibly remove itself when weak
			/// references have been collected. The cache will
			/// double-check, and possibly remove this entry now
			/// if it is not alive. Notice that this method MUST be
			/// invoked OUTSIDE of the Monitor lock.
			/// </summary>
			/// <param name="removeEntry">Not null.</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <exception cref="InvalidOperationException">If invoked
			/// while under the lock.</exception>
			void NotifyRemoveWeak(TKey key);
		}


		/// <summary>
		/// Implements the weak owner.
		/// </summary>
		private class Owner
				: ConvertibleWeakReference<TOwner>,
						IEquatable<Owner>
		{
			internal sealed class Search
					: Owner
			{
				/// <summary>
				/// Constructor.
				/// </summary>
				/// <param name="owner">Not null.</param>
				/// <param name="ownerComparer">Not null.</param>
				/// <exception cref="ArgumentNullException"></exception>
				/// <param name="holdStrongReferenceNow"></param>
				public Search(TOwner owner, IEqualityComparer<TOwner> ownerComparer)
						: base(owner, true, ownerComparer) { }


				/// <summary>
				/// Will be set in this Equals method if another instance
				/// has a live reference with any value that does not Equal this one.
				/// </summary>
				public bool DidFindOtherLiveReference { get; set; }


				public override bool Equals(Owner other)
				{
					if (object.ReferenceEquals(this, other))
						return true;
					if (other == null)
						return false;
					if (!TryGetTarget(out TOwner thisOwner)
							|| !other.TryGetTarget(out TOwner otherOwner))
						return false;
					if (OwnerComparer.Equals(thisOwner, otherOwner))
						return true;
					DidFindOtherLiveReference = true;
					return false;
				}
			}


			private readonly int hashCode;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="owner">Not null.</param>
			/// <param name="holdStrongReferenceNow">Base argument.</param>
			/// <param name="ownerComparer">Not null.</param>
			/// <exception cref="ArgumentNullException"></exception>
			/// <param name="holdStrongReferenceNow"></param>
			public Owner(TOwner owner, bool holdStrongReferenceNow, IEqualityComparer<TOwner> ownerComparer)
					: base(owner, holdStrongReferenceNow)
			{
				hashCode = owner?.GetHashCode() ?? throw new ArgumentNullException(nameof(owner));
				OwnerComparer = ownerComparer ?? throw new ArgumentNullException(nameof(ownerComparer));
			}


			/// <summary>
			/// Not null.
			/// </summary>
			public IEqualityComparer<TOwner> OwnerComparer { get; }


			public override int GetHashCode()
				=> hashCode;

			public override bool Equals(object obj)
				=> Equals(obj as Owner);

			public virtual bool Equals(Owner other)
				=> object.ReferenceEquals(this, other)
						|| ((other != null)
								&& TryGetTarget(out TOwner thisOwner)
								&& other.TryGetTarget(out TOwner otherOwner)
								&& OwnerComparer.Equals(thisOwner, otherOwner));
		}


		/// <summary>
		/// Protects all asccess.
		/// </summary>
		protected object SyncLock
			=> _syncLock;

		private readonly HashSet<Owner> owners;
		private readonly ConvertibleWeakReference<TValue> value;
		private ICache cache;
		private object _syncLock = new object();
		private bool isHoldWeakOwners;
		private bool isHoldWeakValue;
		private bool isPruneOnEveryMutation;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public OwnerCacheEntry(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			Key = key;
			owners = new HashSet<Owner>(EqualityComparer<Owner>.Default);
			value = new ConvertibleWeakReference<TValue>(null, !isHoldWeakValue);
		}


		private void throwIfNotAddedUnsafe()
		{
			if (cache == null)
				throw new InvalidOperationException($"Entry is removed from its' cache: {this}.");
			Debug.Assert(
					object.ReferenceEquals(cache.SyncLock, _syncLock),
					"object.ReferenceEquals(cache.SyncLock, _syncLock)");
			Debug.Assert(Monitor.IsEntered(SyncLock), "Monitor.IsEntered(SyncLock)");
		}


		private bool checkPrune(out bool hasLiveOwners, out TValue currentValue, bool forcePrune = false)
		{
			if (forcePrune
					|| (isHoldWeakOwners
							&& isPruneOnEveryMutation)) {
				owners.RemoveWhere(Prune);
				hasLiveOwners = owners.Count != 0;
			} else {
				hasLiveOwners = isHoldWeakOwners
					   ? isAnyOwnerAlive()
					   : (owners.Count != 0);
			}
			value.TryGetTarget(out currentValue);
			return hasLiveOwners && (currentValue != null);
			static bool Prune(Owner owner)
				=> !owner.IsAlive;
		}

		private bool tryFindAndCheckPrune(
				TOwner predicate,
				bool removeIfFound,
				out bool hasLiveOwners,
				out TValue currentValue)
		{
			bool foundPredicate;
			if (isHoldWeakOwners
					&& isPruneOnEveryMutation) {
				foundPredicate = false;
				owners.RemoveWhere(PruneAndFind);
				hasLiveOwners = owners.Count != 0;
			} else {
				Owner.Search findOwner = new Owner.Search(predicate, cache.OwnerComparer);
				foundPredicate = removeIfFound
						? owners.Remove(findOwner)
						: owners.Contains(findOwner);
				hasLiveOwners = findOwner.DidFindOtherLiveReference
						? true
						: (isHoldWeakOwners
								? isAnyOwnerAlive()
								: (owners.Count != 0));
			}
			value.TryGetTarget(out currentValue);
			return foundPredicate;
			bool PruneAndFind(Owner owner)
			{
				if (!owner.TryGetTarget(out TOwner member))
					return true;
				if (!owner.OwnerComparer.Equals(predicate, member))
					return false;
				foundPredicate = true;
				return removeIfFound;
			}
		}

		private bool isAnyOwnerAlive()
		{
			return owners.Any(IsOwnerAlive);
			static bool IsOwnerAlive(Owner owner)
				=> owner.IsAlive;
		}


#if DEBUG
		/// <summary>
		/// Compiled only in DEBUG: this will set the target on the
		/// weak reference for this Value null now --- only if
		/// <see cref="IsHoldWeakValue"/> is true.
		/// </summary>
		protected void DebugReleaseValueNow()
		{
			if (IsHoldWeakValue)
				value.SetTarget(null);
		}
#endif


		/// <summary>
		/// Protected virtual method is invoked when this instance is added
		/// to the owner cache. This <see cref="Value"/> property will return the
		/// non-null added Value now. After this event, there is guaranteed to
		/// be an <see cref="OnOwnerAdded(TOwner)"/> event for the first-added
		/// owner. This method is invoked under the <see cref="SyncLock"/>.
		/// </summary>
		/// <param name="addedValue">Provides the added <see cref="Value"/>.</param>
		protected virtual void OnAdded(TValue addedValue) { }

		/// <summary>
		/// Protected virtual method is invoked when this instance is removed
		/// from the owner cache. Notice that the <see cref="Owners"/> and
		/// <see cref="Value"/> properties WILL return empty/null at this time;
		/// and, each Owner has already been removed and notified with
		/// <see cref="OnOwnerRemoved(TOwner)"/>. This method
		/// is invoked under the <see cref="SyncLock"/>.
		/// </summary>
		/// <param name="removedValue">Notice: this CAN be null. This is the
		/// <see cref="Value"/> at the time that this entry is removed;
		/// and if the cache is holding weak references to Values, then
		/// this MAY be null at this time.</param>
		protected virtual void OnRemoved(TValue removedValue) { }

		/// <summary>
		/// Protected virtual method is invoked when an owner is added
		/// under this entry. This method is invoked
		/// under the <see cref="SyncLock"/>.
		/// </summary>
		/// <param name="owner">Not null.</param>
		protected virtual void OnOwnerAdded(TOwner owner) { }

		/// <summary>
		/// Protected virtual method is invoked when an owner is removed
		/// under this entry. This method is invoked
		/// under the <see cref="SyncLock"/>. Notice also that this
		/// <see cref="Value"/> may be null at this time.
		/// </summary>
		/// <param name="owner">Not null.</param>
		protected virtual void OnOwnerRemoved(TOwner owner) { }

		/// <summary>
		/// Protected virtual method is invoked when the <see cref="Value"/>
		/// under this entry is replaced. This method is invoked
		/// under the <see cref="SyncLock"/>. This method also allows you
		/// to return an <see cref="Action"/> that will run outisde of the
		/// Monitor lock when this completes.
		/// </summary>
		/// <param name="newValue">Not null: provides the NEW Value.</param>
		/// <param name="priorValue">Note: can be null if this instance holds
		/// weak values: provides the PRIOR Value.</param>
		/// <returns>Can optionally return an Action to be invoked outside
		/// of the Monitor lock when this event completes. Can be null.</returns>
		protected virtual Action OnValueReplaced(TValue newValue, TValue priorValue)
			=> null;


		/// <summary>
		/// This method will try to return the
		/// <see cref="IOwnerCacheBase{TKey, TOwner, TValue}"/> that is holding
		/// this entry. Returns false if <see cref="IsRemoved"/>.
		/// </summary>
		/// <param name="cache">The owner if this is current.</param>
		/// <returns>True if the result is set.</returns>
		protected bool TryGetCache(out IOwnerCacheBase<TKey, TOwner, TValue> cache)
		{
			lock (SyncLock) {
				cache = this.cache;
				return cache != null;
			}
		}


		/// <summary>
		/// Internal implementation method must be invoked when this
		/// entry is added to the cache. MUST be invoked under the
		/// <see cref="ICache.SyncLock"/>.
		/// </summary>
		/// <param name="cache">Not null.</param>
		/// <param name="addedOwner">Not null.</param>
		/// <param name="value">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		internal void NotifyAdded(ICache cache, TOwner addedOwner, TValue addedValue)
		{
			if (addedOwner == null)
				throw new ArgumentNullException(nameof(addedOwner));
			if (addedValue == null)
				throw new ArgumentNullException(nameof(addedValue));
			if (this.cache != null)
				throw new InvalidOperationException($"Entry is already added to a cache: {this}.");
			this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
			Debug.Assert(Monitor.IsEntered(cache.SyncLock), "Monitor.IsEntered(cache.SyncLock)");
			Interlocked.Exchange(ref _syncLock, cache.SyncLock);
			value.SetTarget(addedValue, !isHoldWeakValue);
			OnAdded(addedValue);
			owners.Add(new Owner(addedOwner, !isHoldWeakOwners, cache.OwnerComparer));
			OnOwnerAdded(addedOwner);
		}

		/// <summary>
		/// Internal implementation method must be invoked when this
		/// entry is removed from the cache. MUST be invoked under the
		/// <see cref="ICache.SyncLock"/>.
		/// </summary>
		/// <returns>Must return the removed value: yet can be null.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		internal TValue NotifyRemoved()
		{
			throwIfNotAddedUnsafe();
			foreach (Owner owner in owners) {
				if (owner.TryGetTarget(out TOwner target))
					OnOwnerRemoved(target);
			}
			owners.Clear();
			value.TryGetTarget(out TValue removedValue);
			value.SetTarget(null);
			OnRemoved(removedValue);
			Interlocked.Exchange(ref _syncLock, new object());
			cache = null;
			return removedValue;
		}

		/// <summary>
		/// Internal implementation method must be invoked when this
		/// <paramref name="addOwner"/> is trying to be added under this Key.
		/// This method will always first check if this Value is alive
		/// with both Owners and a Value; and if not alive with both,
		/// this method will return false. In this case, if the value
		/// is not alive, the owner should not be added without ensuring
		/// a value first. And if there are no other owners, but there
		/// IS a value, then the value should be considered stale and
		/// be replaced. Otherwise the owner is added, and protected
		/// notification methods are then invoked. MUST be invoked under the
		/// <see cref="ICache.SyncLock"/>.
		/// </summary>
		/// <param name="addOwner">Not null.</param>
		/// <param name="hasLiveOwners">Always set true if any live owner
		/// is present when this returns.</param>
		/// <param name="currentValue">Always set to the current value.</param>
		/// <param name="alwaysAddIfValueIsAlive">Can be set true to allow
		/// adding this owner if the value is alive, even if this will
		/// be the only owner after being added to that value now
		/// --- this value with no prior owners is otherwise considered
		/// stale by default.</param>
		/// <param name="predicate">Optional: this will be invoked only if
		/// the <paramref name="addOwner"/> is about to be added (according
		/// to all other arguments), and if this returns false, the owner
		/// is not added and the method returns false. If null, this
		/// is ignored.</param>
		/// <returns>True if the <paramref name="addOwner"/> IS added
		/// --- which also implies that this entry IS now alive as well.
		/// Null if not added because this <paramref name="addOwner"/>
		/// is already added here; and the entry IS alive with a Value
		/// as well. False if entry is not alive and should be removed
		/// or the value replaced now.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		internal bool? TryAddOwner(
				TOwner addOwner,
				out bool hasLiveOwners,
				out TValue currentValue,
				bool alwaysAddIfValueIsAlive = false,
				Func<bool> predicate = null)
		{
			throwIfNotAddedUnsafe();
			if (addOwner == null)
				throw new ArgumentNullException(nameof(addOwner));
			if (tryFindAndCheckPrune(addOwner, false, out hasLiveOwners, out currentValue)) {
				return currentValue == null
					? false
					: (bool?)null;
			}
			if ((currentValue == null)
					|| (!hasLiveOwners
							&& !alwaysAddIfValueIsAlive)
					|| !(predicate?.Invoke() ?? true))
				return false;
			owners.Add(new Owner(addOwner, !isHoldWeakOwners, cache.OwnerComparer));
			OnOwnerAdded(addOwner);
			return true;
		}

		/// <summary>
		/// Internal implementation method must be invoked when this
		/// <paramref name="removeOwner"/> is trying to be removed
		/// from this Key. This will return true if this
		/// <paramref name="removeOwner"/> is found and removed;
		/// and returns false if the owner is not found. In both
		/// cases, the <paramref name="isEntryAlive"/>
		/// argument will be set true if this entry is now still
		/// alive with a Value and at least one Owner; and false
		/// if this entry should be removed.
		/// </summary>
		/// <param name="removeOwner">Not null.</param>
		/// <param name="isEntryAlive">Is always set
		/// true if this entry is now still
		/// alive with a Value and at least one Owner; and false
		/// if this entry should be removed.</param>
		/// <returns>True only if this <paramref name="removeOwner"/> is
		/// removed now; and false otherwise --- regardless of whether this
		/// entry is also still alive now.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		internal bool TryRemoveOwner(TOwner removeOwner, out bool isEntryAlive)
		{
			throwIfNotAddedUnsafe();
			if (removeOwner == null)
				throw new ArgumentNullException(nameof(removeOwner));
			bool result = tryFindAndCheckPrune(removeOwner, true, out bool hasLiveOwners, out TValue currentValue);
			isEntryAlive = hasLiveOwners && (currentValue != null);
			if (result)
				OnOwnerRemoved(removeOwner);
			return result;
		}

		/// <summary>
		/// Internal method must be invoked when the value for this entry
		/// is to be replaced. This will return true if this
		/// <paramref name="newValue"/> is replaced, and the entry
		/// is also either alive with at least one Owner, or also
		/// if <paramref name="forceReplace"/> is true. Note that the
		/// <paramref name="priorValue"/> may have been null.
		/// Returns false if the entry is not alive with at
		/// least one Owner, and <paramref name="forceReplace"/>
		/// is false; and the value is not replaced.
		/// </summary>
		/// <param name="newValue">Not null.</param>
		/// <param name="callbackOutsideLock">Optional callback that the
		/// cache must invoke outside of the Monitor lock.</param>
		/// <returns>True if this <paramref name="newValue"/> is replaced;
		/// and the entry is either alive with at least one Owner, or
		/// <paramref name="forceReplace"/> is true. Returns false if the
		/// entry is not alive with at least one Owner, and
		/// <paramref name="forceReplace"/> is false; and the value
		/// is not replaced.</returns>
		internal bool TryReplaceValue(
				TValue newValue,
				out TValue priorValue,
				out Action callbackOutsideLock,
				bool forceReplace = false)
		{
			throwIfNotAddedUnsafe();
			if (newValue == null)
				throw new ArgumentNullException(nameof(newValue));
			if (checkPrune(out bool hasLiveOwners, out priorValue)
					|| hasLiveOwners
					|| forceReplace) {
				value.SetTarget(newValue);
				callbackOutsideLock = OnValueReplaced(newValue, priorValue);
				return true;
			}
			callbackOutsideLock = null;
			return false;
		}


		/// <summary>
		/// Defaults to false: all added Owners are held by strong reference.
		/// If set true then weak references are held. Notice that with weak
		/// references, Owners may become collected before the cache becomes
		/// aware and removes the Key/Value --- i.e. Key/Values are not removed
		/// automatically when all weak Owners become collected (nor if a
		/// weak Value becomes collected). Notice also that changing
		/// this property will always cause the setter to re-check
		/// this entry, and this MAY be removed from the cache now if
		/// all Owners (or the Value) have become collected.
		/// </summary>
		public bool IsHoldWeakOwners
		{
			get {
				lock (SyncLock) {
					return isHoldWeakOwners;
				}
			}
			set {
				ICache thisCache;
				lock (SyncLock) {
					if (isHoldWeakOwners == value)
						return;
					isHoldWeakOwners = value;
					owners.RemoveWhere(Prune);
					if ((owners.Count != 0)
							&& this.value.IsAlive)
						return;
					thisCache = cache;
				}
				thisCache?.NotifyRemoveWeak(Key);
				bool Prune(Owner owner)
					=> !(isHoldWeakOwners
							? owner.ReleaseStrongReference()
							: owner.TryHoldStrongReference());
			}
		}

		/// <summary>
		/// Defaults to false: the Value is held by strong reference.
		/// If set true then a weak reference is held. Notice that with a weak
		/// reference, a Value may become collected before the cache becomes
		/// aware and removes the Key/Value --- i.e. Key/Values are not removed
		/// automatically when the weak Value become collected (nor when
		/// weak Owners become collected). Notice also that
		/// changing this property will always cause the setter to re-check
		/// this entry, and this MAY be removed from the cache now if
		/// the Value (or all Owners) has become collected.
		/// </summary>
		public bool IsHoldWeakValue
		{
			get {
				lock (SyncLock) {
					return isHoldWeakValue;
				}
			}
			set {
				ICache thisCache;
				lock (SyncLock) {
					if (isHoldWeakValue == value)
						return;
					isHoldWeakValue = value;
					if ((isHoldWeakValue
									? this.value.ReleaseStrongReference()
									: this.value.TryHoldStrongReference())
							&& checkPrune(out _, out _, true))
						return;
					thisCache = cache;
				}
				thisCache?.NotifyRemoveWeak(Key);
			}
		}

		/// <summary>
		/// Applies if <see cref="IsHoldWeakOwners"/> or
		/// <see cref="IsHoldWeakValue"/> is true. Defaults to false.
		/// If set true then this entry will prune weak references on every
		/// write operation. Note that if false, then entries are not
		/// removed, but are still checked for live references.
		/// Notice also that
		/// changing this property will always cause the setter to re-check
		/// this entry, and this MAY be removed from the cache now if
		/// the Value (or all Owners) has become collected.
		/// </summary>
		public bool IsPruneOnEveryMutation
		{
			get {
				lock (SyncLock) {
					return isPruneOnEveryMutation;
				}
			}
			set {
				ICache thisCache;
				lock (SyncLock) {
					if (isPruneOnEveryMutation == value)
						return;
					isPruneOnEveryMutation = value;
					if (checkPrune(out _, out _, true))
						return;
					thisCache = cache;
				}
				thisCache?.NotifyRemoveWeak(Key);
			}
		}


		/// <summary>
		/// This non-null unique entry Key.
		/// </summary>
		public TKey Key { get; }

		/// <summary>
		/// Returns the set of current Owners for this entry.
		/// Not null, and not empty if this entry is not <see cref="IsRemoved"/>.
		/// Yet, notice that if the cache is holding weak references
		/// to Owners, then in fact this MAY return an empty
		/// collection while this entry IS NOT <see cref="IsRemoved"/>
		/// --- before the collected Owners are recongnized and
		/// this entry is removed from the cache.
		/// </summary>
		public IReadOnlyCollection<TOwner> Owners
		{
			get {
				lock (SyncLock) {
					List<TOwner> result = new List<TOwner>(owners.Count);
					foreach (ConvertibleWeakReference<TOwner> owner in owners) {
						if (owner.TryGetTarget(out TOwner target))
							result.Add(target);
					}
					return result;
				}
			}
		}

		/// <summary>
		/// This non-null Value. Notice that this IS null before this
		/// entry is added; AND becomes null when removed. And
		/// ALSO, if the cache is holding weak references
		/// to Values, then in fact, this MAY ALSO return null
		/// while this entry IS NOT <see cref="IsRemoved"/>
		/// --- before the collected Value is recongnized and
		/// this entry is removed from the cache.
		/// </summary>
		public TValue Value
		{
			get {
				lock (SyncLock) {
					value.TryGetTarget(out TValue result);
					return result;
				}
			}
		}


		/// <summary>
		/// Returns false only when this entry is added to the
		/// owner cache, and before this is removed --- regardless of
		/// whether any wekly-held Owners or Value have become
		/// collected. Will return true when removed; and before added.
		/// </summary>
		public bool IsRemoved
		{
			get {
				lock (SyncLock) {
					return cache == null;
				}
			}
		}

		/// <summary>
		/// Returns true if the Value and at least one Owner is alive
		/// now; AND <see cref="IsRemoved"/> must also be false.
		/// </summary>
		public bool IsAlive
		{
			get {
				lock (SyncLock) {
					return !IsRemoved
							&& value.IsAlive
							&& isAnyOwnerAlive();
				}
			}
		}

		/// <summary>
		/// This method will remove all weak references from this entry that have
		/// been collected (if <see cref="IsHoldWeakOwners"/> or <see cref="IsHoldWeakValue"/>
		/// is true). The method then returns TRUE if this entry IS still alive,
		/// AND is NOT <see cref="IsRemoved"/>. Notice that this method may
		/// also cause this entry to be removed now before returning a false
		/// result.
		/// </summary>
		/// <param name="removeNowIfCollected">Note that this defaults to true.
		/// This specifies if this entry will
		/// be removed from the cache now if references are collected.
		/// If true, then if this is no longer alive, the entry will be
		/// removed now. If null, then the effective value is
		/// <see cref="IsPruneOnEveryMutation"/>. If false, this will
		/// NOT be removed here now.</param>
		/// <returns>True if NOT <see cref="IsRemoved"/>, and at least one
		/// Owner IS still present, and the Value is alive.</returns>
		public bool Prune(bool? removeNowIfCollected = true)
		{
			ICache thisCache;
			lock (SyncLock) {
				if (checkPrune(out _, out _, removeNowIfCollected == true))
					return !IsRemoved;
				switch (removeNowIfCollected) {
					case true:
					case null when isPruneOnEveryMutation:
						thisCache = cache;
						break;
					default:
						return false;
				}
			}
			thisCache?.NotifyRemoveWeak(Key);
			return false;
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName(true)}"
					+ "["
					+ $"{(IsRemoved ? $"[{nameof(IsRemoved)}] - " : string.Empty)}"
					+ $"{Key} / {Value}"
					+ "]";
	}
}
