using System;
using System.Collections;
using System.Collections.Generic;


namespace Sc.Util.Collections
{
	/// <summary>
	/// Implements an <see cref="ICollection"/> of <see cref="WeakReference{T}"/>.
	/// The collection must take an element type that is a reference type;
	/// and null elements are not allowed. By default the collection invokes
	/// <see cref="Purge"/> before each add, remove, or clear; removing all
	/// <see cref="WeakReference{T}"/> instances from the underlying collection
	/// that are not alive. The <see cref="Count"/>, and enumerations
	/// will always only return live instances when invoked. Protected
	/// methods are available for subclasses to handle added and removed members.
	/// You may also provide the actual underlying <see cref="ICollection{T}"/>
	/// implementation holding the weak references. Not synchronized.
	/// </summary>
	/// <typeparam name="T">Collection member type. Must be a reference type.</typeparam>
	public class WeakReferenceCollection<T>
			: ICollection<T>
			where T : class
	{
		/// <summary>
		/// The actual collection of references.
		/// </summary>
		protected readonly ICollection<WeakReference<T>> Collection;


		/// <summary>
		/// Constructor creates an underlying <see cref="List{T}"/> to hold
		/// the weak references.
		/// </summary>
		/// <param name="initialCapacity">Optional: if null the underlying
		/// <see cref="List{T}"/> is created with its' default capacity; and
		/// otherwise this capacity.</param>
		public WeakReferenceCollection(int? initialCapacity = null)
			=> Collection = initialCapacity.HasValue
					? new List<WeakReference<T>>(initialCapacity.Value)
					: new List<WeakReference<T>>();

		/// <summary>
		/// Constructor allows a you to provide the actual underlying
		/// <see cref="ICollection{T}"/> implementation that will hold
		/// the weak references.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public WeakReferenceCollection(ICollection<WeakReference<T>> collection)
			=> Collection = collection ?? throw new ArgumentNullException(nameof(collection));


		private void checkPurgeBeforeEachMutation()
		{
			if (PurgeBeforeEachMutation)
				Purge();
		}


		/// <summary>
		/// This protected virtual method will be invoked when each
		/// new member is added.
		/// </summary>
		/// <param name="member">Will not be null.</param>
		protected virtual void HandleMemberAdded(T member) { }

		/// <summary>
		/// This protected virtual method will be invoked when a
		/// member is removed.
		/// </summary>
		/// <param name="member">Will not be null.</param>
		protected virtual void HandleMemberRemoved(T member) { }

		/// <summary>
		/// This protected virtual method will be invoked when this
		/// collection is cleared. Note that for efficiency, the given
		/// weak references are NOT checked: it is not known what members
		/// are alive. No actual <see cref="WeakReference{T}"/> instances
		/// will be null: this is the actual current content from the
		/// <see cref="Collection"/> which has now been cleared.
		/// </summary>
		/// <param name="members">Will not be null or empty.</param>
		protected virtual void HandleCleared(WeakReference<T>[] members) { }


		/// <summary>
		/// THis defaults to true: this collection will invoke <see cref="Purge"/>
		/// before each add, remove, or clear.
		/// </summary>
		public bool PurgeBeforeEachMutation { get; set; } = true;

		/// <summary>
		/// Removes all non-alive weak references from the underlying collection.
		/// </summary>
		public void Purge()
		{
			WeakReference<T>[] array = new WeakReference<T>[Collection.Count];
			Collection.CopyTo(array, 0);
			foreach (WeakReference<T> weakReference in array) {
				if (!weakReference.TryGetTarget(out _))
					Collection.Remove(weakReference);
			}
		}


		public void Add(T item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			checkPurgeBeforeEachMutation();
			Collection.Add(new WeakReference<T>(item));
			HandleMemberAdded(item);
		}

		public bool Remove(T item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			foreach (WeakReference<T> weakReference in Collection) {
				if (!weakReference.TryGetTarget(out T member)
						|| !object.Equals(item, member)) {
					continue;
				}
				Collection.Remove(weakReference);
				HandleMemberRemoved(member);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			if (Collection.Count == 0)
				return;
			WeakReference<T>[] array = new WeakReference<T>[Collection.Count];
			Collection.CopyTo(array, 0);
			Collection.Clear();
			HandleCleared(array);
		}


		public bool IsReadOnly
			=> false;

		public IEnumerator<T> GetEnumerator()
		{
			foreach (WeakReference<T> weakReference in Collection) {
				if (weakReference.TryGetTarget(out T element))
					yield return element;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public int Count
		{
			get {
				int count = 0;
				foreach (WeakReference<T> weakReference in Collection) {
					if (weakReference.TryGetTarget(out _))
						++count;
				}
				return count;
			}
		}

		public bool Contains(T item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			foreach (WeakReference<T> weakReference in Collection) {
				if (weakReference.TryGetTarget(out T element)
						&& object.Equals(item, element)) {
					return true;
				}
			}
			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			using (IEnumerator<T> enumerator = GetEnumerator()) {
				for (; arrayIndex < array.Length; ++arrayIndex) {
					if (!enumerator.MoveNext())
						return;
					array[arrayIndex] = enumerator.Current;
				}
			}
		}


		public override string ToString()
			=> $"{GetType().Name}[{Count} / {Collection.Count}]";
	}
}
