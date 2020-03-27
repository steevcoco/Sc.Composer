using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.Specialized;
using Sc.Util.Collections;


namespace Sc.Collections.Specialized
{
	/// <summary>
	/// An <see cref="ISequenceView{T}"/> implementation that wraps a collection and a delegate
	/// <see cref="VariantFunc"/>; and returns variant types. NOTICE: this collection is serializable,
	/// but the Func will not serialize: you may reset it upon deserialization.
	/// </summary>
	/// <typeparam name="TIn">Source type.</typeparam>
	/// <typeparam name="T">Target type.</typeparam>
	/// <typeparam name="TSequence">Your delegate <see cref="ISequenceView{T}"/> type.</typeparam>
	[DataContract]
	[KnownType(nameof(VariantSequenceView<TIn, T, TSequence>.getKnownTypes))]
	public sealed class VariantSequenceView<TIn, T, TSequence>
			: ISequenceView<T>
			where TSequence : ISequenceView<TIn>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static IEnumerable<Type> getKnownTypes()
			=> Sequence<TIn>.GetKnownTypes();


		[DataMember(Name = nameof(VariantSequenceView<TIn, T, TSequence>.Collection))]
		private TSequence collection;

		private Func<TIn, T> variantFunc;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">Not null.</param>
		/// <param name="variantFunc">Not null.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public VariantSequenceView(TSequence collection, Func<TIn, T> variantFunc)
		{
			Collection = collection;
			VariantFunc = variantFunc;
		}


		/// <summary>
		/// The parent collection.
		/// </summary>
		public TSequence Collection
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => collection;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set {
				if (value == null)
					throw new ArgumentNullException(nameof(VariantSequenceView<TIn, T, TSequence>.Collection));
				collection = value;
			}
		}

		/// <summary>
		/// This func converts all <see cref="TIn"/> values to the target <see cref="T"/> type.
		/// </summary>
		public Func<TIn, T> VariantFunc
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => variantFunc;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => variantFunc
					= value
					?? throw new ArgumentNullException(nameof(VariantSequenceView<TIn, T, TSequence>.VariantFunc));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
			=> Collection.GetEnumerator()
					.Select(variantFunc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator(int startIndex, int rangeCount)
		{
			foreach (TIn element in Collection.EnumerateRange(startIndex, rangeCount)) {
				yield return variantFunc(element);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator()
			=> GetReverseEnumerator(Count - 1, Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetReverseEnumerator(int startIndex, int rangeCount)
			=> Collection.GetReverseEnumerator(startIndex, rangeCount)
					.Select(variantFunc);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> CreateReadOnlyView(int startIndex, int rangeCount)
			=> new ReadOnlySequence<T>(this, startIndex, rangeCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable once ParameterHidesMember
		public ISequenceView<TOut> CreateVariantView<TOut>(Func<T, TOut> variantFunc)
			=> new VariantSequenceView<T, TOut, VariantSequenceView<TIn, T, TSequence>>(this, variantFunc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceChain<TChain> CreateChain<TChain>(bool asStack, params ISequenceView<TChain>[] chain)
			=> new SequenceChain<TChain>(asStack, chain);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ISequenceView<T> Clone()
			=> new ImmutableSequence<T>(IsStack, this);

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Collection.Count;
		}

		public bool IsReadOnly
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => true;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => PeekAt(index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(Array destination, int destinationIndex = 0)
			=> CopyRangeTo(0, destination, destinationIndex, Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyRangeTo(int startIndex, Array destination, int destinationIndex, int rangeCount)
		{
			Sequence<T>.CheckDestinationRangeIndex(
					Count,
					startIndex,
					destination.Length,
					destinationIndex,
					rangeCount);
			foreach (TIn element in Collection.EnumerateRange(startIndex, rangeCount)) {
				destination.SetValue(variantFunc(element), destinationIndex);
				++destinationIndex;
			}
		}


		public bool IsStack
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Collection.IsStack;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek()
			=> variantFunc(Collection.Peek());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T PeekAt(int index)
			=> variantFunc(Collection.PeekAt(index));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Poke()
			=> variantFunc(Collection.Poke());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray()
			=> ToArray(0, Count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T[] ToArray(int startIndex, int rangeCount)
		{
			T[] result = new T[rangeCount];
			int index = 0;
			foreach (TIn element in Collection.EnumerateRange(startIndex, rangeCount)) {
				result[index] = variantFunc(element);
				++index;
			}
			return result;
		}

		public int Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Collection.Version;
		}
	}
}
