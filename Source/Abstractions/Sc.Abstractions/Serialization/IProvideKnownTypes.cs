using System;
using System.Collections.Generic;


namespace Sc.Abstractions.Serialization
{
	/// <summary>
	/// Defines an object that provides known types for serialization.
	/// </summary>
	public interface IProvideKnownTypes
	{
		/// <summary>
		/// Returns the provided serializer known typee.
		/// As with the <see cref="Serialization.GetKnownTypes"/> delegate.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		IEnumerable<Type> GetKnownTypes();
	}
}
