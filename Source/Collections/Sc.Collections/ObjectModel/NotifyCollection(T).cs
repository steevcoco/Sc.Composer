using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.ObjectModel;
using Sc.Collections.Specialized;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// Complete mutable <see cref="INotifyCollection{T}"/> <see cref="IList{T}"/>
	/// implementation; using an underlying <see cref="Sequence{T}"/>.
	/// Serializable.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	[DataContract]
	public class NotifyCollection<T>
			: NotifyListBase<T, Sequence<T>>
	{
		/// <summary>
		/// Initializes a new instance that is empty and has default initial capacity.
		/// </summary>
		public NotifyCollection()
				: this(false) { }

		/// <summary>
		/// Initializes a new instance that is empty and has the default initial capacity.
		/// </summary>
		/// <param name="isReadOnly">If set true, the collection raises exceptions
		/// from write methods.</param>
		public NotifyCollection(bool isReadOnly)
				: base(new Sequence<T>(), isReadOnly) { }

		/// <summary>
		/// Initializes a new instance that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="initialCapacity">The collection's initial capacity.</param>
		/// <param name="isReadOnly">If set true, the collection raises exceptions
		/// from write methods.</param>
		public NotifyCollection(int initialCapacity, bool isReadOnly = false)
				: base(new Sequence<T>(false, initialCapacity), isReadOnly) { }

		/// <summary>
		/// Initializes a new instance that contains elements copied from the specified collection,
		/// and has sufficient capacity to accommodate the number of elements copied.
		/// </summary>
		/// <param name="collection">The collection whose elements are copied to the new list.</param>
		/// <param name="isReadOnly">If set true, the collection raises exceptions
		/// from write methods.</param>
		/// <exception cref="ArgumentNullException"> collection is a null reference </exception>
		public NotifyCollection(IEnumerable<T> collection, bool isReadOnly = false)
				: base(
						new Sequence<T>(collection ?? throw new ArgumentNullException(nameof(collection)), false),
						isReadOnly) { }


		/// <summary>
		/// This defaults to false: the additional methods defined here that
		/// perform multi-item changes will raise a
		/// <see cref="NotifyCollectionChangedAction.Reset"/> event.
		/// If set true, then multi-item events will be raised.
		/// </summary>
		public bool IsRaiseMultiItemEvents { get; set; }

		/// <summary>
		/// This method inserts all elements from the argument at the beginning
		/// of this collection. Note that the order in which elements are
		/// added here delends on the <paramref name="enumerateAndInsertInOrder"/>
		/// argument. The default is FALSE. When false, then the added elements
		/// will be enumerated in reverse, effectively "chaining" the argument
		/// before this collection. Otherwise elements are inserted here in the order
		/// returned by that collection's enumerator.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="enumerateAndInsertInOrder">Defaults to false: the argument
		/// is enumerated in reverse, to effect pushing that whole collection in
		/// order before this collection. If set true, elements are effectively
		/// enumerated and inserted here one by one in the order returned.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void InsertRange(IEnumerable<T> collection, bool enumerateAndInsertInOrder = false)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			ThrowIfReadOnly();
			ISequenceView<T> sequence = collection as ISequenceView<T> ?? new ImmutableSequence<T>(false, collection);
			if (sequence.Count == 0)
				return;
			Collection.InsertRangeOldest(sequence, enumerateAndInsertInOrder);
			if (EventHandler.CheckNextEvent()) {
				if (IsRaiseMultiItemEvents) {
					EventHandler.RaiseMultiItemEvents(
							NotifyCollectionChangedAction.Add,
							enumerateAndInsertInOrder
									? sequence.EnumerateInReverse()
									: sequence,
							null,
							0);
				} else
					EventHandler.RaiseResetEvents();
			}
		}

		/// <summary>
		/// This method adds all elements from the argument to this collection.
		/// Elements are always added here in the order returned by that collection's
		/// enumerator.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void AddRange(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			ThrowIfReadOnly();
			if (!EventHandler.CheckNextEvent()) {
				Collection.AddRange(collection);
				return;
			}
			int priorCount = Collection.Count;
			if (IsRaiseMultiItemEvents) {
				IReadOnlyCollection<T> range = (collection as IReadOnlyCollection<T>) ?? collection.ToArray();
				if (range.Count == 0)
					return;
				Collection.AddRange(range);
				EventHandler.RaiseMultiItemEvents(
						NotifyCollectionChangedAction.Add,
						range,
						null,
						priorCount);
			} else {
				Collection.AddRange(collection);
				if (Collection.Count != priorCount)
					EventHandler.RaiseResetEvents();
			}
		}

		/// <summary>
		/// Removes the specified number of elements from the beginning of this collection
		/// and returns them. The elements in the returned array are in the order they
		/// were in this collection. Does not trim the capacity.
		/// </summary>
		/// <param name="rangeCount">The number of elements to remove and return.
		/// >= 0, &lt;= Count.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Not null, may be empty.</returns>
		public T[] PopRange(int rangeCount)
		{
			ThrowIfReadOnly();
			T[] removed = Collection.PopRange(rangeCount);
			switch (removed.Length) {
				case 0:
					return removed;
				case 1:
					if (EventHandler.CheckNextEvent()) {
						EventHandler.RaiseSingleItemEvents(
								NotifyCollectionChangedAction.Remove,
								default,
								removed[0],
								0);
					}
					break;
				default: {
					if (EventHandler.CheckNextEvent()) {
						if (IsRaiseMultiItemEvents) {
							EventHandler.RaiseMultiItemEvents(
									NotifyCollectionChangedAction.Remove,
									null,
									removed,
									0);
						} else
							EventHandler.RaiseResetEvents();
					}
					break;
				}
			}
			return removed;
		}

		/// <summary>
		/// Removes the specified number of elements from the end of this collection
		/// and returns them. The elements in the returned array are in the order they
		/// were in this collection. Does not trim the capacity.
		/// </summary>
		/// <param name="rangeCount">The number of elements to remove and return.
		/// >= 0, &lt;= Count.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <returns>Not null, may be empty.</returns>
		public T[] DropRange(int rangeCount)
		{
			ThrowIfReadOnly();
			T[] removed = Collection.DropRange(rangeCount);
			switch (removed.Length) {
				case 0 :
					return removed;
				case 1 :
					if (EventHandler.CheckNextEvent()) {
						EventHandler.RaiseSingleItemEvents(
								NotifyCollectionChangedAction.Remove,
								default,
								removed[0],
								Collection.Count);
					}
					break;
				default : {
					if (EventHandler.CheckNextEvent()) {
						if (IsRaiseMultiItemEvents) {
							EventHandler.RaiseMultiItemEvents(
									NotifyCollectionChangedAction.Remove,
									null,
									removed,
									Collection.Count);
						} else
							EventHandler.RaiseResetEvents();
					}
					break;
				}
			}
			return removed;
		}
	}
}
