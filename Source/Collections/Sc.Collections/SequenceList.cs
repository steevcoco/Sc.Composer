using System;
using System.Collections;
using System.Runtime.CompilerServices;


namespace Sc.Collections
{
	public partial class Sequence<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T element)
		{
			if (IsStack)
				Push(element);
			else
				Enqueue(element);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			unchecked {
				++Version;
				if (!isElementTypeValueType) {
					if (head < tail)
						Array.Clear(array, head, count);
					else {
						Array.Clear(array, head, array.Length - head);
						if (tail > 0)
							Array.Clear(array, 0, tail);
					}
				}
				head = tail = count = 0;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T item)
			=> IndexOf(item) >= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(T[] destination, int destinationIndex = 0)
			=> CopyTo((Array)destination, destinationIndex);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index < 0)
				return false;
			RemoveAt(index);
			return true;
		}

		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(T item)
			=> findIndexOf(item);

		/// <summary>
		/// Implements <see cref="IndexOf"/> by iterating.
		/// </summary>
		/// <param name="value">Can be null.</param>
		/// <returns>May be <c>-1</c>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int findIndexOf(object value)
		{
			int index = 0;
			foreach (T element in this) {
				if (object.Equals(value, element))
					return index;
				++index;
			}
			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(int index, T item)
		{
			if ((index < 0)
					|| (index > count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index),
						index,
						$"Must be >= 0, <= {count}.");
			}
			if (index == 0) {
				Push(item);
				return;
			}
			if (index == count) {
				Enqueue(item);
				return;
			}
			unchecked {
				if (!TryGrowCapacity())
					++Version;
				if (preferTailShift(index, true, out int pointer)) {
					if (pointer < tail) {
						Array.Copy(array, pointer, array, pointer + 1, tail - pointer);
						if (tail == (array.Length - 1))
							tail = 0;
						else
							++tail;
					} else {
						Array.Copy(array, 0, array, 1, tail);
						array[0] = array[array.Length - 1];
						++tail;
						if (pointer < (array.Length - 1))
							Array.Copy(array, pointer, array, pointer + 1, array.Length - (pointer + 1));
					}
				} else {
					if (head == 0) {
						array[array.Length - 1] = array[0];
						Array.Copy(array, 1, array, 0, pointer - 1);
						head = array.Length - 1;
						--pointer;
					} else {
						if (pointer > head) {
							Array.Copy(array, head, array, head - 1, pointer - head);
							--pointer;
						} else {
							Array.Copy(array, head, array, head - 1, array.Length - head);
							if (pointer > 0) {
								array[array.Length - 1] = array[0];
								Array.Copy(array, 1, array, 0, pointer - 1);
								--pointer;
							} else
								pointer = array.Length - 1;
						}
						--head;
					}
				}
				array[pointer] = item;
				++count;
			}
		}

		/// <summary>
		/// For insert and remove: picks the head or tail to shift, by choosing the smaller count of
		/// elements to copy. Do not pass index 0 or count - 1; and do not invoke when count == 0.
		/// </summary>
		/// <param name="index">The logical INDEX.</param>
		/// <param name="pointer">The <c>index</c> converted to a POINTER here.</param>
		/// <param name="isRemove">True if this is RemoveAt.</param>
		/// <returns>True if the tail should be shifted.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool preferTailShift(int index, bool isRemove, out int pointer)
		{
			pointer = getPointerAt(index);
			int middle = (int)((count / 2F) - .5F);
			if ((index < middle)
					|| ((index == middle)
					&& ((count % 2) == 0)))
				return false;
			if (index > middle)
				return true;
			return (pointer <= head) || (!isRemove && (head == 0));
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			if ((index < 0)
					|| (index >= count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index),
						index,
						$"Must be >= 0, < {count}.");
			}
			if (index == 0) {
				Dequeue();
				return;
			}
			if (index == (count - 1)) {
				Drop();
				return;
			}
			unchecked {
				++Version;
				if (preferTailShift(index, true, out int pointer)) {
					if (pointer < tail) {
						Array.Copy(array, pointer + 1, array, pointer, tail - (pointer + 1));
						--tail;
					} else {
						if (pointer < (array.Length - 1))
							Array.Copy(array, pointer + 1, array, pointer, array.Length - (pointer + 1));
						if (tail == 0)
							tail = array.Length - 1;
						else {
							array[array.Length - 1] = array[0];
							--tail;
							Array.Copy(array, 1, array, 0, tail);
						}
					}
					if (!isElementTypeValueType)
						Array.Clear(array, tail, 1);
				} else {
					if (pointer > head)
						Array.Copy(array, head, array, head + 1, pointer - head);
					else {
						if (pointer > 0)
							Array.Copy(array, 0, array, 1, pointer);
						array[0] = array[array.Length - 1];
						if (head < (array.Length - 1))
							Array.Copy(array, head, array, head + 1, array.Length - (head + 1));
					}
					if (!isElementTypeValueType)
						Array.Clear(array, head, 1);
					if (head == (array.Length - 1))
						head = 0;
					else
						++head;
				}
				--count;
			}
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => PeekAt(index);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Set(index, value);
		}
	}


	public partial class Sequence<T>
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


		bool ICollection.IsSynchronized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => false;
		}

		object ICollection.SyncRoot
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int IList.Add(object value)
		{
			Add(getT(value));
			return IsStack
					? 0
					: Count - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IList.Contains(object value)
			=> findIndexOf(value) >= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int IList.IndexOf(object value)
			=> findIndexOf(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IList.Insert(int index, object value)
			=> Insert(index, getT(value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IList.Remove(object value)
			=> RemoveAt(findIndexOf(value));

		bool IList.IsFixedSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => false;
		}

		object IList.this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this[index];
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this[index] = getT(value);
		}
	}
}
