using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections;
using Sc.Abstractions.Collections.Specialized;


namespace Sc.Collections.Specialized
{
	/// <summary>
	/// <see cref="ISequenceChain{T}"/> implementation; which extends
	/// <see cref="SequenceChainBase{T,TSequence}"/> and declares the
	/// Chain sequence type as <see cref="ISequenceView{T}"/>.
	/// </summary>
	/// <typeparam name="T">The sequence element type.</typeparam>
	[DataContract]
	[KnownType(nameof(SequenceChain<T>.getKnownTypes))]
	public class SequenceChain<T>
			: SequenceChainBase<T, ISequenceView<T>>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static IEnumerable<Type> getKnownTypes()
			=> Sequence<T>.GetKnownTypes();


		/// <summary>
		/// Default constructor creates an empty instance.
		/// </summary>
		/// <param name="asStack">Specifies the mode of THIS collection.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceChain(bool asStack)
				: base(asStack) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="asStack">Specifies the mode of THIS collection.</param>
		/// <param name="chain">Can be null or empty.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceChain(bool asStack, IEnumerable<ISequenceView<T>> chain)
				: base(asStack, chain) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="asStack">Specifies the mode of THIS collection.</param>
		/// <param name="chain">Can be null or empty.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SequenceChain(bool asStack, params ISequenceView<T>[] chain)
				: base(asStack, chain) { }
	}
}
