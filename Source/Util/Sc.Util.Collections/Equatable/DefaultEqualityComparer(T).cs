using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace Sc.Util.Collections.Equatable
{
	/// <summary>
	/// Implements an <see cref="IEqualityComparer{T}"/> of <c>object</c>
	/// --- which also extends the non-generic <see cref="IEqualityComparer"/>
	/// --- that delegates to <see cref="EqualityComparer{T}.Default"/> of the specified Type.
	/// This implementation uses a given <c>elementType</c> <see cref="Type"/> to get
	/// the generic <see cref="IEqualityComparer"/> Type, and then it fetches the
	/// default <see cref="EqualityComparer{T}"/> form the <see cref="EqualityComparer{T}.Default"/>
	/// property (that is, as if by getting <c>EqualityComparer&lt;elementType&gt;.Default</c>
	/// --- using reflection here). That comparer is then wrapped in a func that uses a
	/// <see cref="MethodInfo"/> to invoke the <see cref="EqualityComparer{T}.Equals(T,T)"/>
	/// method every time. This comparer is also serializable: the <see cref="ElementTypeName"/>
	/// will be serialized and the type will be restored (which is the full assembly qualified
	/// name).
	/// </summary>
	[Serializable]
	public sealed class DefaultEqualityComparer
			: IEqualityComparer<object>,
					IEqualityComparer
	{
		private static void getFuncs(
				Type elementType,
				out Func<object, object, bool> equals,
				out Func<object, int> hashCode)
		{
			Type equalityComparerType = typeof(EqualityComparer<>).MakeGenericType(elementType);
			PropertyInfo defaultProperty
					= equalityComparerType.GetProperty(
							nameof(EqualityComparer<object>.Default),
							BindingFlags.GetProperty
							| BindingFlags.Public
							| BindingFlags.Static
							| BindingFlags.FlattenHierarchy,
							null,
							equalityComparerType,
							new Type[0],
							null);
			if (defaultProperty == null) {
				throw new NotSupportedException(
						$"Failed to get {nameof(EqualityComparer<object>.Default)}"
						+ $" property from {typeof(EqualityComparer<>).FullName}");
			}
			object iEqualityComparer = defaultProperty.GetValue(equalityComparerType);
			MethodInfo equalsMethod
					= iEqualityComparer.GetType()
							.GetMethod(
									nameof(IEqualityComparer<object>.Equals),
									new[]
									{
											elementType, elementType
									});
			if (equalsMethod == null) {
				throw new NotSupportedException(
						$"Failed to get {nameof(IEqualityComparer<object>.Equals)}"
						+ $" method from {equalityComparerType.FullName}");
			}
			@equals = (x, y) => (bool)equalsMethod.Invoke(
					iEqualityComparer,
					new[]
					{
							x, y
					});
			MethodInfo hashCodeMethod
					= iEqualityComparer.GetType()
							.GetMethod(
									nameof(IEqualityComparer<object>.GetHashCode),
									new[]
									{
											elementType
									});
			if (hashCodeMethod == null) {
				throw new NotSupportedException(
						$"Failed to get {nameof(IEqualityComparer<object>.GetHashCode)}"
						+ $" method from {equalityComparerType.FullName}");
			}
			hashCode = obj => (int)hashCodeMethod.Invoke(
					iEqualityComparer,
					new[]
					{
							obj
					});
		}


		[NonSerialized]
		private Func<object, object, bool> equalsFunc;

		[NonSerialized]
		private Func<object, int> hashCodeFunc;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="elementType">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DefaultEqualityComparer(Type elementType)
		{
			if (elementType == null)
				throw new ArgumentNullException(nameof(elementType));
			ElementTypeName = elementType.AssemblyQualifiedName;
			DefaultEqualityComparer.getFuncs(elementType, out equalsFunc, out hashCodeFunc);
		}


		[OnDeserialized]
		private void onDeserialized(StreamingContext _)
		{
			Type elementType = Type.GetType(ElementTypeName);
			if (elementType == null) {
				throw new TypeLoadException(
						$"Cannot load serialized {nameof(DefaultEqualityComparer)} type '{ElementTypeName}'");
			}
			DefaultEqualityComparer.getFuncs(elementType, out equalsFunc, out hashCodeFunc);
		}


		/// <summary>
		/// Serialized element Type name.
		/// </summary>
		[DataMember]
		public string ElementTypeName { get; private set; }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetHashCode(object obj)
			=> hashCodeFunc(obj);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// ReSharper disable once MemberHidesStaticFromOuterClass
		public new bool Equals(object x, object y)
			=> equalsFunc(x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IEqualityComparer<object>.Equals(object x, object y)
			=> equalsFunc(x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IEqualityComparer.Equals(object x, object y)
			=> equalsFunc(x, y);
	}
}
