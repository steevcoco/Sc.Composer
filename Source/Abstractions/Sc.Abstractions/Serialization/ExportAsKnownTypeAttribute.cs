using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Sc.Abstractions.Serialization
{
	/// <summary>
	/// Provides an <see cref="Attribute"/> that can be used to apply to
	/// a type, where a container can locate attributed types
	/// THEMSELVES, and consider them as additions to a known type
	/// collection --- e.g. <see cref="IKnownTypeCollector"/>.
	/// The intent is to mark THIS attributed type as a
	/// contribution to the list of known types.
	/// </summary>
	[AttributeUsage(
			AttributeTargets.Class
					| AttributeTargets.Delegate
					| AttributeTargets.Enum
					| AttributeTargets.Interface
					| AttributeTargets.Struct,
			AllowMultiple = false,
			Inherited = false)]
	public class ExportAsKnownTypeAttribute
			: Attribute
	{
		/// <summary>
		/// Static helper method will search an <see cref="Assembly"/> for all
		/// Types that have an <see cref="ExportAsKnownTypeAttribute"/>,
		/// and create a new <see cref="GetKnownTypes"/> delegate that
		/// returns the types. Note that the collection of types is
		/// assembled one time only now: the delegate then always
		/// returns this fixed collection
		/// </summary>
		/// <param name="assembly">Required Assembly to search.</param>
		/// <param name="publicOnly">Optional and defaults to false:
		/// non-public types will be included.</param>
		/// <returns>The delegate.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static GetKnownTypes FindTypes(Assembly assembly, bool publicOnly = false)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));
			Type[] knownTypes = assembly
					.GetTypes()
					.Where(TypePredicate)
					.ToArray();
			return GetKnownTypes;
			bool TypePredicate(Type type)
				=> !type.IsAbstract
						&& !type.IsInterface
						&& type.GetCustomAttributes<ExportAsKnownTypeAttribute>()
								.Any()
						&& (!publicOnly
								|| type.IsPublic);
			IEnumerable<Type> GetKnownTypes()
				=> knownTypes;
		}
	}
}
