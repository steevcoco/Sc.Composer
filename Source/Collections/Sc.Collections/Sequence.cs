using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using Sc.Abstractions.Collections;
using Sc.Collections.Specialized;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Collections
{
	/// <summary>
	/// <see cref="ISequence{T}"/> implementation. This implementation implements both <see cref="IStack{T}"/>
	/// and <see cref="IQueue{T}"/>, as well as the base <see cref="ISequence{T}"/>; and also implements
	/// <see cref="IList{T}"/>. <see cref="Sequence{T}"/> performs quickly: in tests, it is faster than the
	/// .NET <see cref="Queue{T}"/> and <see cref="Stack{T}"/>; and provides more methods. Internally, the
	/// collection is implemented in a circular Array buffer. In any mode, the buffer has a "Head" and "Tail"
	/// pointer that both can move in either direction: when you Push, you move the Head back and insert there;
	/// and when you Enqueue or Lift, you move the Tail out and insert there --- therefore, the underlying
	/// schema is the same fora Queue or Stack, and the method names indicate the semantics. Since this class
	/// implements both the Queue and Stack interfaces, you should expose the object as the intended interface
	/// type, and otherwise be sure to use methods that implement the intended behavior. NOTICE ALSO that you
	/// may PREFER to use the methods defined on <see cref="ISequence{T}"/>: ALL of those methods on will
	/// operate based on the underlying implementation, which allows you to "swap" implementations.
	/// </summary>
	/// <typeparam name="T">Element type. Unbounded.</typeparam>
	[DataContract]
	public partial class Sequence<T>
			: IStack<T>,
					IQueue<T>,
					IList<T>,
					IList
	{
		/// <summary>
		/// The max size of an array: the collection will raise exceptions if it becomes this large.
		/// </summary>
		public const int ArrayMaxLength = 0X7FEFFFFF;


		/// <summary>
		/// This method returns known implementation types for this element type.
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<Type> GetKnownTypes()
		{
			yield return typeof(Sequence<T>);
			yield return typeof(FixedSizeSequence<T>);
			yield return typeof(ImmutableSequence<T>);
			yield return typeof(ReadOnlySequence<T>);
			yield return typeof(SequenceChain<T>);
		}


		/// <summary>
		/// Static method checks range arguments against the <see cref="ISequenceView{T}.Count"/>.
		/// </summary>
		/// <param name="checkSequenceCount">The Count of the collection to check.</param>
		/// <param name="startIndex">Start index within the <c>collection</c>.</param>
		/// <param name="rangeCount">Count within the <c>collection</c> beginning at <c>startIndex</c>.</param>
		/// <param name="isReverse">Should be true if this test is testing a reverse enumeration.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected internal static void CheckRangeIndex(
				int checkSequenceCount,
				int startIndex,
				int rangeCount,
				bool isReverse = false)
		{
			if ((startIndex < 0)
					|| (checkSequenceCount != 0
							? startIndex >= checkSequenceCount
							: startIndex > 0)) {
				throw new ArgumentOutOfRangeException(
						nameof(startIndex),
						startIndex,
						$"Must be >= 0, < {(checkSequenceCount > 0 ? checkSequenceCount.ToString() : "1")}");
			}
			if (isReverse) {
				if ((rangeCount < 0)
						|| (checkSequenceCount != 0
								? rangeCount > (startIndex + 1)
								: rangeCount > 0)) {
					throw new ArgumentOutOfRangeException(
							nameof(rangeCount),
							rangeCount,
							$"Must be >= 0, < {(checkSequenceCount > 0 ? (startIndex + 2).ToString() : "1")}");
				}
			} else {
				if ((rangeCount < 0)
						|| (checkSequenceCount != 0
								? rangeCount > (checkSequenceCount - startIndex)
								: rangeCount > 0)) {
					throw new ArgumentOutOfRangeException(
							nameof(rangeCount),
							rangeCount,
							"Must be >= 0, < "
							+ $"{(checkSequenceCount > 0 ? ((checkSequenceCount - startIndex) + 1).ToString() : "1")}");
				}
			}
		}

		/// <summary>
		/// Static method checks range arguments against the <see cref="ISequenceView{T}.Count"/>
		/// and the parameters for a destination array to be copied into. This method ALWAYS
		/// first invokes <see cref="CheckRangeIndex"/>.
		/// </summary>
		/// <param name="checkSequenceCount">The Count of the collection to check.</param>
		/// <param name="startIndex">Start index within the <c>collection</c>.</param>
		/// <param name="destinationLength">The Length of the destination Array.</param>
		/// <param name="destinationIndex">The index in the destination Array tobegin copying.</param>
		/// <param name="rangeCount">Count within the <c>collection</c> beginning at <c>startIndex</c>.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected internal static void CheckDestinationRangeIndex(
				int checkSequenceCount,
				int startIndex,
				int destinationLength,
				int destinationIndex,
				int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(checkSequenceCount, startIndex, rangeCount);
			if ((destinationIndex < 0)
					|| (destinationLength > 0
							? destinationIndex >= destinationLength
							: destinationIndex > 0)) {
				throw new ArgumentOutOfRangeException(
						nameof(destinationIndex),
						destinationIndex,
						$@"{nameof(destinationLength)}={destinationLength}");
			}
			if (rangeCount
					> (destinationLength > 0
							? destinationLength - destinationIndex
							: 0)) {
				throw new ArgumentException(
						nameof(rangeCount),
						$"{nameof(rangeCount)}={rangeCount}"
						+ $", {nameof(startIndex)}={startIndex}"
						+ $", {nameof(IReadOnlyCollection<T>.Count)}={checkSequenceCount}"
						+ $", {nameof(destinationIndex)}={destinationIndex}"
						+ $", {nameof(destinationLength)}={destinationLength}");
			}
		}


		private bool isElementTypeValueType;

		[DataMember]
		private ReadOnlySequence<T> asReadOnly;

		[DataMember]
		private T[] array;

		[DataMember]
		private int head;

		[DataMember]
		private int tail; // points after the last valid element

		[DataMember]
		private int count;


		/// <summary>
		/// Default constructor; creates an empty Collection as a Queue, with the default initial
		/// capacity (32) and grow factor (2x).
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sequence()
				: this(false) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="isStack">Sets the mode of this Collection, which will be honored by
		/// interface implementations: if true, this will be a Stack, otherwise a Queue.</param>
		/// <param name="capacity">The initial buffer capacity.</param>
		/// <param name="growFactor">When the array must grow, it's current size is multiplied
		/// by this value to create a new array. &gt; 1.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sequence(bool isStack, int capacity = 32, float growFactor = 2F)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));
			if (growFactor <= 1F)
				throw new ArgumentOutOfRangeException(nameof(growFactor));
			array = new T[capacity];
			GrowFactor = growFactor;
			IsStack = isStack;
			setElementType();
		}

		/// <summary>
		/// Creates a Collection by copying the elements from the argument.
		/// <see cref="ISequenceView{T}.IsStack"/> will match the argument. The enumeration order
		/// will be retained: the current first item in the argument will be the first element in this
		/// collection.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="growFactor">When the array must grow, it's current size is multiplied
		/// by this value to create a new array. &gt; 1.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sequence(ISequenceView<T> collection, float growFactor = 2F)
		{
			if (growFactor <= 1F)
				throw new ArgumentOutOfRangeException(nameof(growFactor));
			array = collection?.ToArray() ?? throw new ArgumentNullException(nameof(collection));
			count = array.Length;
			GrowFactor = growFactor;
			IsStack = collection.IsStack;
			setElementType();
		}

		/// <summary>
		/// Creates a Collection by copying the elements from the argument. The enumeration order
		/// will be retained: the current first item in the argument will be the first element in this
		/// collection.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="isStack">Sets the mode of this Collection, which will be honored by
		/// interface implementations: if true, this will be a Stack, otherwise a Queue.</param>
		/// <param name="growFactor">When the array must grow, it's current size is multiplied
		/// by this value to create a new array. &gt; 1.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sequence(IEnumerable<T> collection, bool isStack, float growFactor = 2F)
		{
			if (growFactor <= 1F)
				throw new ArgumentOutOfRangeException(nameof(growFactor));
			array = collection?.ToArray() ?? throw new ArgumentNullException(nameof(collection));
			count = array.Length;
			GrowFactor = growFactor;
			IsStack = isStack;
			setElementType();
		}

		/// <summary>
		/// Creates a Collection by EITHER copying the elements from the argument array, OR retaining
		/// THE ACTUAL ARRAY as the current backing store. Note that the array will be released and
		/// recreated if the collection grows or is trimmed. The enumeration order will be retained:
		/// the current first item in the argument will be the first element in this collection.
		/// </summary>
		/// <param name="array">Not null.</param>
		/// <param name="keepArray">If true, the the actual array is used now; and if false, elements
		/// are copied now to a new array.</param>
		/// <param name="isStack">Sets the mode of this Collection, which will be honored by
		/// interface implementations: if true, this will be a Stack, otherwise a Queue.</param>
		/// <param name="growFactor">When the array must grow, it's current size is multiplied
		/// by this value to create a new array. &gt; 1.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException">If the array is incompatible.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Sequence(T[] array, bool keepArray, bool isStack, float growFactor = 2F)
		{
			if (growFactor <= 1F)
				throw new ArgumentOutOfRangeException(nameof(growFactor));
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (array.Rank != 1)
				throw new ArgumentException(nameof(array));
			if (keepArray)
				this.array = array;
			else {
				this.array = new T[array.Length];
				Array.Copy(array, this.array, array.Length);
			}
			count = this.array.Length;
			GrowFactor = growFactor;
			IsStack = isStack;
			setElementType();
		}


		[OnSerializing]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void onSerializing(StreamingContext _)
			=> TrimToSize();

		[OnDeserialized]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void onDeserialized(StreamingContext _)
		{
			asReadOnly?.ResetCollection(this);
			setElementType();
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void setElementType()
			=> isElementTypeValueType = typeof(T).IsValueType;


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}{this.ToStringCollection(0)}";
	}


	public partial class Sequence<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> AsReadOnly()
		{
			if (asReadOnly == null) {
				Interlocked.CompareExchange(
						ref asReadOnly,
						new ReadOnlySequence<T>(this),
						null);
			}
			return asReadOnly;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange(
				ISequenceView<T> collection,
				bool enumerateInOrder = false,
				int? addCount = null,
				bool countInOrder = false)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (addCount.HasValue) {
				if (addCount.Value < 0)
					throw new ArgumentOutOfRangeException(nameof(addCount), addCount, addCount.ToString());
				addCount = Math.Min(collection.Count, addCount.Value);
			} else
				addCount = collection.Count;
			if (addCount.Value == 0)
				return;
			if (array.Length < (count + addCount.Value))
				SetCapacity(count + addCount.Value);
			if (enumerateInOrder
					|| !IsStack) {
				int countToAdd = addCount.Value;
				foreach (T element in collection) {
					if (countToAdd == 0)
						break;
					Add(element);
					--countToAdd;
				}
			} else {
				using (IEnumerator<T> enumerator
						= countInOrder
								? collection.GetReverseEnumerator(addCount.Value - 1, addCount.Value)
								: collection.GetReverseEnumerator(collection.Count - 1, addCount.Value)) {
					while (enumerator.MoveNext()) {
						Add(enumerator.Current);
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InsertOldest(T element)
		{
			if (IsStack)
				Lift(element);
			else
				Push(element);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(T element)
		{
			unchecked {
				if (!TryGrowCapacity())
					++Version;
				if (head == 0)
					head = array.Length - 1;
				else
					--head;
				array[head] = element;
				++count;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Append(T element)
			=> Enqueue(element);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange(IEnumerable<T> collection, int? addCount = null)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			int countToAdd;
			if (addCount.HasValue) {
				if (addCount.Value < 0)
					throw new ArgumentOutOfRangeException(nameof(addCount), addCount, addCount.ToString());
				countToAdd = addCount.Value;
			} else
				countToAdd = Sequence<T>.ArrayMaxLength - count;
			foreach (T element in collection) {
				if (countToAdd == 0)
					break;
				Add(element);
				--countToAdd;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InsertRangeOldest(
				ISequenceView<T> collection,
				bool? enumerateInOrder = null,
				int? addCount = null,
				bool countInOrder = false)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			if (addCount.HasValue) {
				if (addCount.Value < 0)
					throw new ArgumentOutOfRangeException(nameof(addCount), addCount, addCount.ToString());
				addCount = Math.Min(collection.Count, addCount.Value);
			} else
				addCount = collection.Count;
			if (addCount.Value == 0)
				return;
			if (!enumerateInOrder.HasValue)
				enumerateInOrder = IsStack;
			if (array.Length < (count + addCount.Value))
				SetCapacity(count + addCount.Value);
			if (enumerateInOrder.Value) {
				int countToAdd = addCount.Value;
				foreach (T element in collection) {
					if (countToAdd == 0)
						break;
					InsertOldest(element);
					--countToAdd;
				}
			} else {
				using (IEnumerator<T> enumerator
						= countInOrder
								? collection.GetReverseEnumerator(addCount.Value - 1, addCount.Value)
								: collection.GetReverseEnumerator(collection.Count - 1, addCount.Value)) {
					while (enumerator.MoveNext()) {
						InsertOldest(enumerator.Current);
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void InsertRangeOldest(IEnumerable<T> collection, int? addCount = null)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			int countToAdd;
			if (addCount.HasValue) {
				if (addCount.Value < 0)
					throw new ArgumentOutOfRangeException(nameof(addCount), addCount, addCount.ToString());
				countToAdd = addCount.Value;
			} else
				countToAdd = Sequence<T>.ArrayMaxLength - count;
			foreach (T element in collection) {
				if (countToAdd == 0)
					break;
				InsertOldest(element);
				--countToAdd;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int index, T element)
		{
			if ((index < 0)
					|| (index >= count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index),
						index,
						$"Must be >= 0, < {count}.");
			}
			array[getPointerAt(index)] = element;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Exchange(int index, Func<T, T> swap, bool returnNewValue = false)
		{
			if ((index < 0)
					|| (index >= count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index),
						index,
						$"Must be >= 0, < {count}.");
			}
			unchecked {
				index = getPointerAt(index);
				T result;
				if (returnNewValue) {
					result = swap(array[index]);
					array[index] = result;
				} else {
					result = array[index];
					array[index] = swap(result);
				}
				return result;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Exchange(int index, T newValue)
		{
			if ((index < 0)
					|| (index >= count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index),
						index,
						$"Must be >= 0, < {count}.");
			}
			unchecked {
				index = getPointerAt(index);
				T result = array[index];
				array[index] = newValue;
				return result;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Swap(int index1, int index2)
		{
			if ((index1 < 0)
					|| (index1 >= count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index1),
						index1,
						$"Must be >= 0, < {count}.");
			}
			if ((index2 < 0)
					|| (index2 >= count)) {
				throw new ArgumentOutOfRangeException(
						nameof(index2),
						index2,
						$"Must be >= 0, < {count}.");
			}
			unchecked {
				index1 = getPointerAt(index1);
				index2 = getPointerAt(index2);
				T element2 = array[index2];
				array[index2] = array[index1];
				array[index1] = element2;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveNext()
			=> Dequeue();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] RemoveNextRange(int rangeCount)
			=> IsStack
					? PopRange(count)
					: DequeueRange(rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Drop()
		{
			if (count == 0)
				throw new InvalidOperationException("Collection is empty.");
			unchecked {
				++Version;
				if (tail == 0)
					tail = array.Length - 1;
				else
					--tail;
				--count;
				T removed = array[tail];
				if (!isElementTypeValueType)
					array[tail] = default;
				return removed;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void TrimToSize()
			=> SetCapacity(count);
	}
}
