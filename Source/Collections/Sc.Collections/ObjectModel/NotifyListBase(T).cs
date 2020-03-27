using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections.ObjectModel;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// Complete <see cref="INotifyCollection{T}"/> <see cref="IList{T}"/> implementation
	/// that takes any <see cref="IList{T}"/> as a backing collection.
	/// Serializable.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <typeparam name="TList">Underlying list type.</typeparam>
	[DataContract]
	public class NotifyListBase<T, TList>
			: NotifyCollectionBase<T, TList, T>,
					IList<T>,
					INotifyCollection<T>
			where TList : class, IList<T>, IReadOnlyCollection<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">Required.</param>
		/// <param name="isReadOnly">If set true, the collection raises exceptions
		/// from write methods.</param>
		protected NotifyListBase(TList collection, bool isReadOnly = false)
				: base(collection, isReadOnly) { }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override T GetCollectionChangedValue(T element)
			=> element;


		/// <summary>
		/// Moves the item from the first index to the second.
		/// </summary>
		/// <param name="from">Index to move from.</param>
		/// <param name="to">Index to move to.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void Move(int from, int to)
		{
			ThrowIfReadOnly();
			T moved = Collection[from];
			Collection.Insert(to, moved);
			Collection.RemoveAt(
					from >= to
							? from + 1
							: from);
			if (EventHandler.CheckNextEvent())
				EventHandler.RaiseSingleItemEvents(NotifyCollectionChangedAction.Move, moved, moved, to, from);
		}

		public virtual T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Collection[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				ThrowIfReadOnly();
				T oldItem = Collection[index];
				Collection[index] = value;
				if (EventHandler.CheckNextEvent()) {
					EventHandler.RaiseSingleItemEvents(
							NotifyCollectionChangedAction.Replace,
							GetCollectionChangedValue(value),
							GetCollectionChangedValue(oldItem),
							index);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void Insert(int index, T item)
		{
			ThrowIfReadOnly();
			Collection.Insert(index, item);
			if (EventHandler.CheckNextEvent()) {
				EventHandler.RaiseSingleItemEvents(
						NotifyCollectionChangedAction.Add,
						GetCollectionChangedValue(item),
						default,
						index);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual void RemoveAt(int index)
		{
			ThrowIfReadOnly();
			T oldItem = Collection[index];
			Collection.RemoveAt(index);
			if (EventHandler.CheckNextEvent()) {
				EventHandler.RaiseSingleItemEvents(
						NotifyCollectionChangedAction.Remove,
						default,
						GetCollectionChangedValue(oldItem),
						index);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item)
			=> Collection.IndexOf(item);
	}
}
