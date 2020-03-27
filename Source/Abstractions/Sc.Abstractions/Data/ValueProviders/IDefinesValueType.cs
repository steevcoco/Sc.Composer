using System;


namespace Sc.Abstractions.Data.ValueProviders
{
	/// <summary>
	/// An object that defines the Type of a value.
	/// </summary>
	public interface IDefinesValueType
	{
		/// <summary>
		/// The type of the provided value.
		/// </summary>
		Type ValueType { get; }
	}
}
