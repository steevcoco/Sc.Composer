using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Sc.Util.Collections.Collections
{
	/// <summary>
	/// Implements an <see cref="IReadOnlyCollection{T}"/> that takes a delegate collection,
	/// and crops a given count of elements from the head; and optionally crops the tail also.
	/// Note that both arguments may exceed the count of the source collection: if the
	/// underlying collection changes, the cropped counts remain the same --- and so
	/// the resulting view may change. The counts are ALSO mutable here.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	public sealed class CroppedCollection<T>
			: IReadOnlyCollection<T>
	{
		private int cropFromHead;
		private int thisCount;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="sourceCollection">Not null.</param>
		/// <param name="cropFromHead">Count of elements omitted from the head of the
		/// <paramref name="sourceCollection"/>. Not negative. Notice that this MAY be zero;
		/// AND may exceed the current count of the source collection.</param>
		/// <param name="thisCount">The count to return in this collection. If this is null,
		/// then all elements are returned from the source, less the
		/// <paramref name="cropFromHead"/> elements. Note also that this MAY be zero;
		/// AND may exceed the current count of the source collection.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException">Only if an int argument is negative:
		/// either may exceed the count of the source collection.</exception>
		public CroppedCollection(
				IReadOnlyCollection<T> sourceCollection,
				int cropFromHead,
				int? thisCount = null)
		{
			SourceCollection = sourceCollection ?? throw new ArgumentNullException(nameof(sourceCollection));
			CropFromHead = cropFromHead;
			ThisCount = thisCount;
		}


		/// <summary>
		/// The underlying source collection.
		/// </summary>
		public IReadOnlyCollection<T> SourceCollection
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
		}

		/// <summary>
		/// The count of elements cropped from the head of the <see cref="SourceCollection"/>.
		/// Must be non-negative, but CAN exceed the current count of the source collection.
		/// The actual returned elements are always coerced within the available bounds
		/// </summary>
		public int CropFromHead
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => cropFromHead;
			set => cropFromHead
					= value < 0
							? throw new ArgumentOutOfRangeException(
									nameof(CroppedCollection<T>.CropFromHead),
									value,
									value.ToString())
							: value;
		}

		/// <summary>
		/// The count of elements to return from this collection. Must be non-negative,
		/// but CAN exceed the current count of the <see cref="SourceCollection"/>.
		/// The actual returned count is always coerced within the available bounds
		/// --- and so DO NOT use this value as an actual count
		/// (use <see cref="IReadOnlyCollection{T}.Count"/>).
		/// </summary>
		public int? ThisCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => thisCount;
			set => thisCount
					= !value.HasValue
							? -1
							: value.Value < 0
									? throw new ArgumentOutOfRangeException(
											nameof(CroppedCollection<T>.ThisCount),
											value.Value,
											value.Value.ToString())
									: value.Value;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
		{
			if ((thisCount == 0)
					|| (SourceCollection.Count == 0)) {
				yield break;
			}
			int index = -CropFromHead;
			int maxIndex = thisCount < 0
					? SourceCollection.Count - 1
					: (CropFromHead + thisCount) - 1;
			foreach (T value in SourceCollection) {
				if (index < 0) {
					++index;
					continue;
				}
				if (index > maxIndex)
					yield break;
				yield return value;
				++index;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Math.Max(
					0,
					thisCount < 0
							? SourceCollection.Count - CropFromHead
							: Math.Min(
									SourceCollection.Count - CropFromHead,
									thisCount));
		}
	}
}
