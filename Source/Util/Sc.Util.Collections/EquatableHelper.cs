using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sc.Util.Collections.Equatable;


namespace Sc.Util.Collections
{
	/// <summary>
	/// Static helpers for <see cref="IEquatable{T}"/> and
	/// <see cref="IEqualityComparer{T}"/>.
	/// </summary>
	public static class EquatableHelper
	{
		/// <summary>
		/// Returns an <see cref="IEqualityComparer{T}"/> that uses the <c>equalsFunc</c>
		/// to return Equals, and optionally the <c>hashCodeFunc</c> to return the HashCode.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="equalsFunc">Not null.</param>
		/// <param name="hashCodeFunc">Optional. If null, the comparer uses:
		/// <c>obj?.GetHashCode() ?? 0</c>.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEqualityComparer<T> ToEqualityComparer<T>(
				Func<T, T, bool> equalsFunc,
				Func<T, int> hashCodeFunc = null)
			=> new DelegateEqualityComparer<T>(equalsFunc, hashCodeFunc);


		/// <summary>
		/// Returns an <see cref="IEqualityComparer{T}"/> of <typeparamref name="T"/>,
		/// that takes a delegate comparer --- or uses the default --- and uses a provided
		/// selector to fetch the compared <typeparamref name="TSelection"/> value from
		/// each <see cref="T"/> object. Optionally also takes a hash code function.
		/// </summary>
		/// <typeparam name="T">This comparer's type.</typeparam>
		/// <typeparam name="TSelection">The selected value type.</typeparam>
		/// <param name="comparer">Optional <see cref="IEqualityComparer{T}"/> that is used
		/// to compare each selected value: if null, the default is used.</param>
		/// <param name="selector">Required value selector.</param>
		/// <param name="getHashCode">Optional function to return the hashcode for
		/// each <typeparamref name="TSelection"/> value. If null, <see cref="object.GetHashCode"/>
		/// is used.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEqualityComparer<T> ToEqualityComparer<T, TSelection>(
				this IEqualityComparer<TSelection> comparer,
				Func<T, TSelection> selector,
				Func<TSelection, int> getHashCode = null)
			=> new SelectingEqualityComparer<T, TSelection>(selector, comparer, getHashCode);

		/// <summary>
		/// Returns an <see cref="IEqualityComparer{T}"/> of <typeparamref name="T"/>,
		/// that takes a delegate comparer --- or uses the default --- and uses a provided
		/// selector to fetch the compared <typeparamref name="TSelection"/> value from
		/// each <see cref="T"/> object. Optionally also takes a hash code function.
		/// </summary>
		/// <typeparam name="T">This comparer's type.</typeparam>
		/// <typeparam name="TSelection">The selected value type.</typeparam>
		/// <param name="selector">Required value selector.</param>
		/// <param name="comparer">Optional <see cref="IEqualityComparer{T}"/> that is used
		/// to compare each selected value: if null, the default is used.</param>
		/// <param name="getHashCode">Optional function to return the hashcode for
		/// each <typeparamref name="TSelection"/> value. If null, <see cref="object.GetHashCode"/>
		/// is used.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEqualityComparer<T> ToEqualityComparer<T, TSelection>(
				this Func<T, TSelection> selector,
				IEqualityComparer<TSelection> comparer = null,
				Func<TSelection, int> getHashCode = null)
			=> new SelectingEqualityComparer<T, TSelection>(selector, comparer, getHashCode);


		/// <summary>
		/// This method creates an <see cref="IEqualityComparer{T}"/> of <c>object</c>
		/// --- which also extends the non-generic <see cref="IEqualityComparer"/>
		/// --- that delegates to <see cref="EqualityComparer{T}.Default"/> of the specified Type.
		/// This implementation uses the given <paramref name="elementType"/> to get
		/// the generic <see cref="IEqualityComparer"/> Type, and then it fetches the
		/// default <see cref="EqualityComparer{T}"/> form the <see cref="EqualityComparer{T}.Default"/>
		/// property (that is, as if by getting <c>EqualityComparer&lt;elementType&gt;.Default</c>
		/// --- using reflection here). That comparer is then wrapped in a func that uses a
		/// <see cref="MethodInfo"/> to invoke the <see cref="EqualityComparer{T}.Equals(T,T)"/>
		/// method every time. The returned comparer is also serializable.
		/// </summary>
		/// <param name="elementType">Not null.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEqualityComparer<object> GetDefaultEqualityComparer(this Type elementType)
			=> new DefaultEqualityComparer(elementType);


		/// <summary>
		/// This method creates an <see cref="IEqualityComparer{T}"/> that simply delegates to
		/// <see cref="EqualityComparer{T}.Default"/> of the specified Type; and is serializable.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <returns>Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEqualityComparer<T> SerializableEqualityComparer<T>()
			=> new SerializableEqualityComparer<T>();


		/// <summary>
		/// Returns an <see cref="IEqualityComparer{T}"/> that returns <see cref="object.ReferenceEquals"/>,
		/// and <see cref="object.GetHashCode"/>.
		/// </summary>
		/// <typeparam name="T">The element type.</typeparam>
		/// <returns>Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEqualityComparer<T> ReferenceEqualityComparer<T>()
		{
			return EquatableHelper.ToEqualityComparer<T>(EqualsFunc, HashCodeFunc);
			static bool EqualsFunc(T a, T b)
				=> object.ReferenceEquals(a, b);
			static int HashCodeFunc(T obj)
				=> obj.GetHashCode();
		}
	}
}
