using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Util.Collections;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// A base <see cref="INotifyCollectionChanged"/> implementation that takes an
	/// <see cref="ICollection{T}"/> as a backing collection -- and that collection
	/// must also implement <see cref="IReadOnlyCollection{T}"/>. Notice that
	/// your implementation MUST check this default implementation of
	/// <see cref="GetCollectionChangedValue"/>. This provides support
	/// for <see cref="ICollection{T}.IsReadOnly"/>, and will
	/// raise exceptions from the mutate methods if true.
	/// Serializable.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <typeparam name="TCollection">Underlying collection type.</typeparam>
	/// <typeparam name="TCollectionChangedValue">Is the type of the values raised in
	/// <see cref="NotifyCollectionChangedEventArgs"/> events.</typeparam>
	[DataContract]
	public class NotifyCollectionBase<T, TCollection, TCollectionChangedValue>
			: ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue>,
			ICollection<T>
			where TCollection : class, ICollection<T>, IReadOnlyCollection<T>
	{
		[DataMember(Name = nameof(ICollection<T>.IsReadOnly))]
		private bool isReadOnly;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">Required.</param>
		/// <param name="isReadOnly">If set true, the collection raises exceptions
		/// from write methods.</param>
		protected NotifyCollectionBase(TCollection collection, bool isReadOnly = false)
				: base(collection)
			=> this.isReadOnly = isReadOnly;


		/// <summary>
		/// If <see cref="IsReadOnly"/>, throws <see cref="InvalidOperationException"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void ThrowIfReadOnly()
		{
			if (IsReadOnly) {
				throw new InvalidOperationException(
						$"Cannot modify a read only collection {ToString()}");
			}
		}


		/// <summary>
		/// This method is responsible for returning the element that will be
		/// raised on an <see cref="INotifyCollectionChanged"/> event --- which
		/// is allowed to differ from this <see cref="T"/> element type in this
		/// implementation. This will be invoked with added and removed elements.
		/// NOTICE that this implementation CASTS the element to
		/// <see cref="TCollectionChangedValue"/> --- you should override
		/// this and return the correct type in the most efficient manner.
		/// </summary>
		/// <param name="element">This is the added or removed element.
		/// This IS NOT tested for null here.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual TCollectionChangedValue GetCollectionChangedValue(T element)
			=> (TCollectionChangedValue)(object)element;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void Add(T item)
		{
			ThrowIfReadOnly();
			Collection.Add(item);
			if (EventHandler.CheckNextEvent()) {
				EventHandler.RaiseSingleItemEvents(
						NotifyCollectionChangedAction.Add,
						GetCollectionChangedValue(item),
						default,
						Count - 1);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual bool Remove(T item)
		{
			ThrowIfReadOnly();
			int index = Collection.FindIndex(item);
			if (index < 0)
				return false;
			Collection.Remove(item);
			if (EventHandler.CheckNextEvent()) {
				EventHandler.RaiseSingleItemEvents(
						NotifyCollectionChangedAction.Remove,
						default,
						GetCollectionChangedValue(item),
						index);
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			ThrowIfReadOnly();
			if (Count == 0)
				return;
			Collection.Clear();
			EventHandler.RaiseResetEvents();
		}


		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => isReadOnly;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual bool Contains(T item)
			=> Collection.Contains(item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(T[] array, int arrayIndex)
			=> Collection.CopyTo(array, arrayIndex);
	}
}
