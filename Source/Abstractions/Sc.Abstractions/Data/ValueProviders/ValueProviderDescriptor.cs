using System;
using System.Runtime.Serialization;
using Sc.Abstractions.Internal;


namespace Sc.Abstractions.Data.ValueProviders
{
	/// <summary>
	/// A descriptor for <see cref="IValueProvider"/> objects. Implements
	/// <see cref="IDefinesValueType"/>; where the <see cref="IDefinesValueType.ValueType"/>
	/// is the type of the value to be returned by the described <see cref="IValueProvider"/>
	/// <see cref="IValueProvider{TValue}.ProviderValue"/> property (and specifically is not the
	/// type of an <see cref="IValueProvider"/> itself). Implements <see cref="IEquatable{T}"/>:
	/// overrides <see cref="GetHashCode"/> and <see cref="Equals(object)"/>, and also defines
	/// a <see cref="GetParametersHashCode"/> method that can include specific instance parameters
	/// in the hash code and equals implementation. Notice that
	/// this implementation by default captures the <see cref="ValueTypeAssemblyQualifiedName"/>
	/// for <see cref="IEquatable{T}"/>; and serializes and restores that value: the <see cref="ValueType"/>
	/// property is virtual and can instead be overridden; and you must do that if the defined
	/// type may not serialize and restore correctly based on the assembly name.
	/// </summary>
	[DataContract]
	public class ValueProviderDescriptor
			: IDefinesValueType,
					IEquatable<ValueProviderDescriptor>
	{
		/// <summary>
		/// Static factory method creates and returns a new instance with your generic type.
		/// This object will serialize and restore the <see cref="Type.AssemblyQualifiedName"/>
		/// to implement the <see cref="ValueType"/> property.
		/// </summary>
		/// <typeparam name="T">The type for <see cref="ValueType"/>.</typeparam>
		/// <returns>Not null.</returns>
		public static ValueProviderDescriptor Create<T>()
			=> new ValueProviderDescriptor(typeof(T));


		/// <summary>
		/// Constructor that will capture this Type's <see cref="Type.AssemblyQualifiedName"/>:
		/// sets this <see cref="ValueTypeAssemblyQualifiedName"/>; and this <see cref="ValueType"/>
		/// returns the type dynamicaly by fetching this type by its name.
		/// </summary>
		/// <param name="valueType">Not null.</param>
		public ValueProviderDescriptor(Type valueType)
			=> ValueTypeAssemblyQualifiedName
					= valueType?.AssemblyQualifiedName
					?? throw new ArgumentNullException(nameof(valueType));

		/// <summary>
		/// Constructor for subclasses: you MUST override and implement <see cref="ValueType"/>;
		/// or set the <see cref="ValueTypeAssemblyQualifiedName"/> --- if the name is set,
		/// the <see cref="ValueType"/> returns the type dynamicaly by fetching this type by its name.
		/// </summary>
		protected ValueProviderDescriptor() { }


		/// <summary>
		/// Holds the <see cref="Type.AssemblyQualifiedName"/> of the captured type of <see cref="ValueType"/>.
		/// </summary>
		[DataMember]
		protected string ValueTypeAssemblyQualifiedName;

		public virtual Type ValueType
			=> Type.GetType(ValueTypeAssemblyQualifiedName);


		/// <summary>
		/// Provided for the descriptor to define parameters that may further define the
		/// described <see cref="IValueProvider"/>, and must be included in equality comparisons.
		/// </summary>
		/// <returns>Hash code of any parameters.</returns>
		public virtual int GetParametersHashCode()
			=> 0;

		/// <summary>
		/// Returns the <see cref="ValueType"/> HashCode , hashed with this
		/// <see cref="GetParametersHashCode"/>.
		/// </summary>
		public override int GetHashCode()
			=> (((23 * 37)
					+ ValueType.GetHashCode())
					* 37)
					+ GetParametersHashCode();

		/// <summary>
		/// Returns <see cref="Equals(ValueProviderDescriptor)"/>
		/// </summary>
		public override bool Equals(object obj)
			=> Equals(obj as ValueProviderDescriptor);

		/// <summary>
		/// Returns true if the <see cref="ValueType"/> is the same, and
		/// <see cref="GetParametersHashCode"/> is equal.
		/// </summary>
		/// <param name="other">Can be null.</param>
		/// <returns>True if the <see cref="ValueType"/> and <see cref="GetParametersHashCode"/>
		/// are equal.</returns>
		public virtual bool Equals(ValueProviderDescriptor other)
			=> (other != null)
					&& (ValueType == other.ValueType)
					&& (GetParametersHashCode() == other.GetParametersHashCode());

		public override string ToString()
			=> $"{GetType().GetFriendlyFullName()}[{ValueType}]";
	}
}
