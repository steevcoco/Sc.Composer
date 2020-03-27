using System;
using System.Collections.Generic;


namespace Sc.Abstractions.Serialization
{
	/// <summary>
	/// Defines a delegate that provides serializer known typee.
	/// </summary>
	/// <returns>Not null; may be empty.</returns>
	public delegate IEnumerable<Type> GetKnownTypes();
}
