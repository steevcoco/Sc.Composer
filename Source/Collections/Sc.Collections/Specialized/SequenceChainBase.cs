using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.Specialized;
using Sc.Util.System;


namespace Sc.Collections.Specialized
{
	/// <summary>
	/// <see cref="ISequenceChain{T}"/> implementation; which allows you
	/// to specify the actual Chain collection type, and exposes the
	/// <see cref="ChainInternal"/> as a mutable collection.
	/// </summary>
	/// <typeparam name="T">The sequence element type.</typeparam>
	/// <typeparam name="TSequence">The Chain collection type.</typeparam>
	[DataContract]
	[KnownType(nameof(SequenceChainBase<T, TSequence>.getKnownTypes))]
	public class SequenceChainBase<T, TSequence>
			: ISequenceChain<T>
			where TSequence : ISequenceView<T>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static IEnumerable<Type> getKnownTypes()
			=> Sequence<T>.GetKnownTypes();


		/// <summary>
		/// Default constructor creates an empty instance.
		/// </summary>
		/// <param name="asStack">Specifies the mode of THIS collection.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceChainBase(bool asStack)
		{
			ChainInternal = new Sequence<TSequence>(asStack, 2);
			setChain();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="asStack">Specifies the mode of THIS collection.</param>
		/// <param name="chain">Can be null or empty.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceChainBase(bool asStack, IEnumerable<TSequence> chain)
		{
			ChainInternal = chain != null
					? new Sequence<TSequence>(chain, asStack)
					: new Sequence<TSequence>(asStack, 2);
			setChain();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="asStack">Specifies the mode of THIS collection.</param>
		/// <param name="chain">Can be null or empty.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceChainBase(bool asStack, params TSequence[] chain)
		{
			ChainInternal = chain != null
					? new Sequence<TSequence>(chain, false, asStack)
					: new Sequence<TSequence>(asStack, 2);
			setChain();
		}


		private void setChain()
		{
			Chain = new VariantSequenceView<TSequence, ISequenceView<T>, Sequence<TSequence>>(
					ChainInternal,
					Selector);
			static ISequenceView<T> Selector(TSequence sequenceView)
				=> sequenceView;
		}

		[OnDeserialized]
		private void onDeserialized(StreamingContext c)
			=> setChain();


		/// <summary>
		/// This is the actual collection that backs the <see cref="Chain"/>;
		/// and is mutable.
		/// </summary>
		[DataMember]
		public Sequence<TSequence> ChainInternal
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		}
		

		/// <summary>
		/// This method will locate the collections within this <see cref="Chain"/>
		/// that contain your given <paramref name="startIndex"/> and
		/// <paramref name="rangeCount"/> of elements; and then construct a new
		/// <see cref="SequenceChainBase{T,TSequence}"/> with only that range of collections.
		/// The result then will contain the range of elements within this instance that
		/// you specify, and only the collections needed to cover the indicated range
		/// of elements. You must then use the returned <paramref name="startIndex"/>,
		/// and your <paramref name="rangeCount"/>, to locate the elements within the
		/// returned collection. Notice that the <paramref name="startIndex"/> parameter
		/// is passed as a <see langword="ref"/> argument: you must pass in your desired
		/// element start index within this collection, and this method will set that
		/// argument to the index within the new returned collection. If the exact
		/// range is not located, the method returns false. You may then wrap the
		/// returned collection with the returned start index and the range count in a
		/// <see cref="SequenceViewer{T}"/>; with the returned argument value.
		/// Note that this method will not raise exceptions for bad argument values:
		/// it will return false.
		/// </summary>
		/// <param name="startIndex">A ref argument: pass in your desired first element
		/// index within this collection; and this is set to the index within
		/// the new returned collection that is that element.</param>
		/// <param name="rangeCount">Your desired range count (is the same value for
		/// the returned collection).</param>
		/// <param name="result">The resulting new chain. Null if the method returns
		/// false.</param>
		/// <returns>True if the exact range is located.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryExtractRange(ref int startIndex, int rangeCount, out SequenceChainBase<T, TSequence> result)
		{
			if ((ChainInternal.Count == 0)
					|| (startIndex < 0)
					|| (rangeCount <= 0)) {
				result = null;
				return false;
			}
			int firstCollection = -1;
			int lastCollection = -1;
			foreach (TSequence sequence in ChainInternal) {
				if (lastCollection < 0) {
					++firstCollection;
					if (startIndex < sequence.Count) {
						lastCollection = firstCollection;
						rangeCount -= (sequence.Count - startIndex);
						if (rangeCount <= 0)
							break;
					} else
						startIndex -= sequence.Count;
					continue;
				}
				++lastCollection;
				rangeCount -= sequence.Count;
				if (rangeCount <= 0)
					break;
			}
			if ((lastCollection < 0)
					|| (rangeCount > 0)) {
				result = null;
				return false;
			}
			result = new SequenceChainBase<T, TSequence>(
					ChainInternal.IsStack,
					ChainInternal.EnumerateRange(firstCollection, (lastCollection - firstCollection) + 1));
			return true;
		}


		public ISequenceView<ISequenceView<T>> Chain
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
		{
			foreach (TSequence sequenceView in ChainInternal) {
				if (sequenceView == null)
					continue;
				foreach (T element in sequenceView) {
					yield return element;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator(int startIndex, int rangeCount)
		{
			if (startIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex.ToString());
			if (rangeCount < 0)
				throw new ArgumentOutOfRangeException(nameof(rangeCount), rangeCount.ToString());
			if (rangeCount == 0)
				yield break;
			int i = 0;
			foreach (TSequence sequenceView in ChainInternal) {
				if (sequenceView == null)
					continue;
				int currentCount = sequenceView.Count;
				if ((i + (currentCount - 1)) < startIndex) {
					i += currentCount;
					if ((i - startIndex) >= rangeCount)
						yield break;
					continue;
				}
				foreach (T element in sequenceView) {
					if (i >= startIndex)
						yield return element;
					++i;
					if ((i - startIndex) >= rangeCount)
						yield break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator()
		{
			int count = Count;
			return getReverseEnumerator(
					count,
					count == 0
							? 0
							: count - 1,
					count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator(int startIndex, int rangeCount)
			=> getReverseEnumerator(Count, startIndex, rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IEnumerator<T> getReverseEnumerator(int thisCount, int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(thisCount, startIndex, rangeCount, true);
			if (rangeCount == 0)
				yield break;
			int i = thisCount - 1;
			if (i < 0)
				yield break;
			foreach (TSequence chainSequence in ChainInternal.EnumerateInReverse()) {
				if (chainSequence == null)
					continue;
				int currentCount = chainSequence.Count;
				if ((i - (currentCount - 1)) > startIndex) {
					i -= currentCount;
					if ((startIndex - i) >= rangeCount)
						yield break;
					continue;
				}
				foreach (T element in chainSequence.EnumerateInReverse()) {
					if (i <= startIndex)
						yield return element;
					--i;
					if ((startIndex - i) >= rangeCount)
						yield break;
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> CreateReadOnlyView(int startIndex, int rangeCount)
			=> new ReadOnlySequence<T>(this, startIndex, rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<TOut> CreateVariantView<TOut>(Func<T, TOut> variantFunc)
			=> new VariantSequenceView<T, TOut, ISequenceView<T>>(this, variantFunc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceChain<TChain> CreateChain<TChain>(bool asStack, params ISequenceView<TChain>[] chain)
			=> new SequenceChain<TChain>(asStack, chain);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> Clone()
			=> new ImmutableSequence<T>(IsStack, this);

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				int count = 0;
				foreach (TSequence sequenceView in ChainInternal) {
					if (sequenceView == null)
						continue;
					count += sequenceView.Count;
				}
				return count;
			}
		}

		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => false;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => PeekAt(index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Array destination, int destinationIndex = 0)
		{
			int count = Count;
			copyRangeTo(
					count,
					0,
					destination,
					destinationIndex,
					Math.Min(count, destination.Length - destinationIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyRangeTo(int startIndex, Array destination, int destinationIndex, int rangeCount)
			=> copyRangeTo(Count, startIndex, destination, destinationIndex, rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void copyRangeTo(
				int thisCount,
				int startIndex,
				Array destination,
				int destinationIndex,
				int rangeCount)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));
			Sequence<T>.CheckDestinationRangeIndex(
					thisCount,
					startIndex,
					destination.Length,
					destinationIndex,
					rangeCount);
			copyToArray(startIndex, destination, destinationIndex, rangeCount);
		}


		public bool IsStack
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ChainInternal.IsStack;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek()
		{
			foreach (TSequence sequenceView in ChainInternal) {
				if (sequenceView == null)
					continue;
				if (sequenceView.Count != 0)
					return sequenceView.Peek();
			}
			throw new InvalidOperationException("Peek: Collection is empty.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T PeekAt(int index)
		{
			int i = 0;
			foreach (TSequence sequenceView in ChainInternal) {
				if (sequenceView == null)
					continue;
				int sCount = sequenceView.Count;
				if ((index - i) < sCount)
					return sequenceView.PeekAt(index - i);
				i += sCount;
			}
			throw new ArgumentOutOfRangeException(nameof(index), index.ToString());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Poke()
		{
			foreach (TSequence chainSequence in ChainInternal.EnumerateInReverse()) {
				if (chainSequence == null)
					continue;
				if (chainSequence.Count != 0)
					return chainSequence.Poke();
			}
			throw new InvalidOperationException("Poke: Collection is empty.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray()
		{
			int count = Count;
			T[] result = new T[count];
			copyToArray(0, result, 0, count);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray(int startIndex, int rangeCount)
		{
			Sequence<T>.CheckRangeIndex(Count, 0, rangeCount);
			T[] result = new T[rangeCount];
			copyToArray(startIndex, result, 0, rangeCount);
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void copyToArray(int startIndex, Array destination, int destinationIndex, int rangeCount)
		{
			startIndex = -startIndex;
			foreach (TSequence sequenceView in ChainInternal) {
				if (sequenceView == null)
					continue;
				int sCount = sequenceView.Count;
				if (startIndex < 0) {
					if (sCount <= -startIndex) {
						startIndex += sCount;
						continue;
					}
					startIndex = -startIndex;
				}
				sequenceView.CopyRangeTo(
						startIndex,
						destination,
						destinationIndex,
						Math.Min(sCount - startIndex, rangeCount));
				rangeCount -= sCount - startIndex;
				if (rangeCount <= 0)
					return;
				destinationIndex += sCount - startIndex;
				startIndex = 0;
			}
		}

		public int Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				return HashCodeHelper.Seed
						.Hash(ChainInternal.Count)
						.Hash(ChainInternal.Select(Selector));
				static ulong Selector(TSequence sequenceView)
					=> sequenceView == null
							? 0UL
							: ((ulong)sequenceView.GetHashCode() << 32)
							| (uint)sequenceView.Version;
			}
		}
	}
}
