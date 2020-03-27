using System;
using System.Collections.Generic;


namespace Sc.Collections
{
	/// <summary>
	/// Base interface defines the Keyed methods for
	/// <see cref="IOwnerCache{TKey, TOwner, TValue}"/>. Please see
	/// <see cref="IOwnerCache{TKey, TOwner, TValue}"/>.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TOwner">The owner type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	public interface IOwnerCacheBase<TKey, TOwner, TValue>
			where TOwner : class
			where TValue : class
	{
		/// <summary>
		/// The non-null comparer used to identify unique Keys.
		/// </summary>
		IEqualityComparer<TKey> KeyComparer { get; }

		/// <summary>
		/// The non-null comparer used to identify unique Owners.
		/// </summary>
		IEqualityComparer<TOwner> OwnerComparer { get; }


		/// <summary>
		/// Locates a cached <typeparamref name="TValue"/> for this
		/// <paramref name="key"/>, and also returns all current owners.
		/// Note that this method will operate atomically on the current
		/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}"/> for
		/// this Key; whereas
		/// <see cref="TryGetEntry(TKey, out OwnerCacheEntry{TKey, TOwner, TValue})"/>
		/// does not guarantee to return live Owners and/or a Value.
		/// At the same time, owners may still be removed after this returns
		/// the values.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="owners">Not null or empty if the method returns true.</param>
		/// <param name="value">The result.</param>
		/// <returns>True if there is a cached value and at least one owner
		/// at this time.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		bool TryGetValue(TKey key, out IReadOnlyCollection<TOwner> owners, out TValue value);

		/// <summary>
		/// Locates a cached <typeparamref name="TValue"/> for this
		/// <paramref name="key"/>.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="value">The result.</param>
		/// <returns>True if there is a cached value and at least one owner
		/// at this time.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		bool TryGetValue(TKey key, out TValue value);

		/// <summary>
		/// Locates a current
		/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}"/>
		/// for this <paramref name="key"/>. Notice that this method does not
		/// operate atomically if the entry is holding weak Owners and/or a
		/// weak Value: this will return the entry, which is current and
		/// alive at this moment, but then your next access to the
		/// returned <paramref name="entry"/> does not guarantee to return
		/// live Owners and/or a Value at that time.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="entry">The result.</param>
		/// <returns>True if there is a cached value and at least one owner
		/// at this time.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		bool TryGetEntry(TKey key, out OwnerCacheEntry<TKey, TOwner, TValue> entry);


		/// <summary>
		/// Locates a current
		/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}"/>
		/// for this <paramref name="key"/>; or adds a new entry.
		/// If the key is not found, must also now add a value.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="owner">Not null.</param>
		/// <param name="addOrReplaceValue">Not null. This is invoked if the
		/// value must be added now: the delegate always receives the current
		/// value; and in this case, it is always null --- a value must be added
		/// now for this first owner. Otherwise, if <paramref name="forceReplace"/>
		/// is set true, then this will be invoked with the current value, and
		/// must return the new value (or you may return a non-null existing value
		/// after inspecting it here if it is not to be replaced).
		/// Cannot return null.</param>
		/// <param name="forceReplace">Defaults to false. If set true, then the
		/// value is always replaced now if there is any existing value.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If the
		/// <paramref name="addOrReplaceValue"/> delegate returns null.</exception>
		OwnerCacheEntry<TKey, TOwner, TValue> GetOrAdd(
				TKey key,
				TOwner owner,
				Func<TValue, TValue> addOrReplaceValue,
				bool forceReplace = false);

		/// <summary>
		/// Adds this <paramref name="owner"/> under this <paramref name="key"/>;
		/// and, if the key is not found, must also now add a value.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="owner">Not null.</param>
		/// <param name="addOrReplaceValue">Not null. This is invoked if the
		/// value must be added now: the delegate always receives the current
		/// value; and in this case, it is always null --- a value must be added
		/// now for this first owner. Otherwise, if <paramref name="forceReplace"/>
		/// is set true, then this will be invoked with the current value, and
		/// must return the new value (or you may return a non-null existing value
		/// after inspecting it here if it is not to be replaced).
		/// Cannot return null.</param>
		/// <param name="forceReplace">Defaults to false. If set true, then the
		/// value is always replaced now if there is any existing value.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If the
		/// <paramref name="addOrReplaceValue"/> delegate returns null.</exception>
		TValue AddOwner(TKey key, TOwner owner, Func<TValue, TValue> addOrReplaceValue, bool forceReplace = false);

		/// <summary>
		/// Removes this <paramref name="owner"/> under this <paramref name="key"/>.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="owner">Not null.</param>
		/// <returns>True if this <paramref name="key"/> is present;
		/// and the owner has been removed.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		bool RemoveOwner(TKey key, TOwner owner);


		/// <summary>
		/// Tries to replace the value, keyed under the
		/// <paramref name="key"/>; only if there are existing owners
		/// and a value for this key.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="replaceValue">Not null: is invoked if the
		/// value must be replaced now. If invoked, the argument is
		/// the existing value, and is not null. Cannot return null.</param>
		/// <returns>True if this <paramref name="key"/> is found; and
		/// the value has been replaced.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">If the
		/// <paramref name="replaceValue"/> delegate returns null.</exception>
		bool TryReplace(TKey key, Func<TValue, TValue> replaceValue);

		/// <summary>
		/// Removes the value and all owners under this <paramref name="key"/>.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>False if this <paramref name="key"/> is not found
		/// and removed now.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		bool Remove(TKey key);
	}


	/// <summary>
	/// Defines a Dictionary collection, that holds keyed values, and also
	/// holds a collection of "owners" for each key/value. A value is added with
	/// at least one owner under a given key. Owners can then be added or removed
	/// under that key. If all owners are removed, then the value is removed.
	/// The value is always present if at least one owner is present; and
	/// is always removed if all owners are removed. Usage notes:
	/// All Keys, Owners, and Values must be non-null. Each Key/Value
	/// is held in the cache within an
	/// <see cref="OwnerCacheEntry{TKey, TOwner, TValue}"/>, which holds
	/// the actual Key and Value, and can return the current Owners.
	/// In addition, you may specify a factory for your own Entry
	/// subclass type, and that class provides notification methods
	/// for added and removed Owners and other events on that Key.
	/// The generic types are not constrained, except that the Owner and
	/// Value types must be reference types. An equality comparer must
	/// be specified for the Keys and the Owners. The cache also
	/// supports holding weak references to Owners and/or Values,
	/// which can also be specified per-Key entry instance.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TOwner">The owner type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	public interface IOwnerCache<TKey, TOwner, TValue>
			: IOwnerCacheBase<TKey, TOwner, TValue>
			where TOwner : class
			where TValue : class
	{
		/// <summary>
		/// Clears all keys. This method will invoke
		/// <see cref="IOwnerCache{TKey,TOwner,TValue}.Remove"/>
		/// with all current keys --- so that each key is handled
		/// by handlers as it is removed.
		/// </summary>
		void Clear();


		/// <summary>
		/// Returns a count of all current entries.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Returns all current keys.
		/// </summary>
		/// <returns>Not null, may be empty.</returns>
		IEnumerable<TKey> GetKeys();
	}
}
