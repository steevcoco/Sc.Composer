using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Collections
{
	/// <summary>
	/// Wraps an <see cref="IList{T}"/> and returns and adds elements in reverse order.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed partial class ReversedList<T>
			: IList<T>,
					IList
	{
		private readonly IList<T> list;
		private readonly bool isElementTypeValueType;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="list">Required</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ReversedList(IList<T> list)
		{
			this.list = list ?? throw new ArgumentNullException(nameof(list));
			isElementTypeValueType = typeof(T).IsValueType;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int getListIndex(int index)
			=> list.Count - 1 - index;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = Count - 1; i >= 0; --i) {
				yield return list[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
			=> list.Insert(0, item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
			=> list.Clear();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item)
			=> list.Contains(item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(T[] array, int arrayIndex)
		{
			for (int i = Count - 1; i >= 0; --i, ++arrayIndex) {
				if (arrayIndex >= array.Length)
					return;
				array[arrayIndex] = list[i];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index < 0)
				return false;
			list.RemoveAt(index);
			return true;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => list.Count;
		}

		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => list.IsReadOnly;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item)
			=> getListIndex(list.IndexOf(item));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(int index, T item)
			=> list.Insert(getListIndex(index), item);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
			=> list.RemoveAt(getListIndex(index));

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => list[getListIndex(index)];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => list[getListIndex(index)] = value;
		}
	}


	public sealed partial class ReversedList<T>
	{
		/// <summary>
		/// Casts the object or throws.
		/// </summary>
		/// <param name="value">Can be null: this will throw if <see cref="T"/>
		/// cannot be null.</param>
		/// <returns>The value as <see cref="T"/> --- can again be null.</returns>
		/// <exception cref="ArgumentException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T getT(object value)
			=> value is T element
					? element
					: (value == null)
					&& (!isElementTypeValueType
					|| (Nullable.GetUnderlyingType(typeof(T)) != null))
							? default(T)
							: throw new ArgumentException();


		void ICollection.CopyTo(Array array, int index)
		{
			for (int i = list.Count - 1; i >= 0; --i, ++index) {
				if (index >= array.Length)
					return;
				array.SetValue(list[i], index);
			}
		}

		bool ICollection.IsSynchronized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ((IList)list).IsSynchronized;
		}

		object ICollection.SyncRoot
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ((IList)list).SyncRoot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int IList.Add(object value)
		{
			Add(getT(value));
			return Count - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IList.Contains(object value)
			=> Contains(getT(value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int IList.IndexOf(object value)
			=> IndexOf(getT(value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IList.Insert(int index, object value)
			=> Insert(index, getT(value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IList.Remove(object value)
			=> Remove(getT(value));

		bool IList.IsFixedSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ((IList)list).IsFixedSize;
		}

		object IList.this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this[getListIndex(index)];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this[getListIndex(index)] = getT(value);
		}
	}
}
