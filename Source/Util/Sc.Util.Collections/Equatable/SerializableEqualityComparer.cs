using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace Sc.Util.Collections.Equatable
{
	/// <summary>
	/// An <see cref="IEqualityComparer{T}"/> that simply delegates to
	/// <see cref="EqualityComparer{T}.Default"/> of the specified Type; and is serializable.
	/// </summary>
	/// <typeparam name="T">The element type.</typeparam>
	[Serializable]
	public sealed class SerializableEqualityComparer<T>
			: IEqualityComparer<T>
	{
		[NonSerialized]
		private IEqualityComparer<T> comparer = EqualityComparer<T>.Default;


		[OnDeserialized]
		private void onDeSerialized(StreamingContext _)
			=> comparer = EqualityComparer<T>.Default;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(T x, T y)
			=> comparer.Equals(x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetHashCode(T obj)
			=> comparer.GetHashCode(obj);
	}
}
