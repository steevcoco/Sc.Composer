using System.Reflection;
using System.Linq;

namespace Sc.Util.Reflection
{
	/// <summary>
	/// Static helpers for reflection.
	/// </summary>
	public static class ReflectionHelper
	{
		/// <summary>
		/// Returns the unqualified name of this <paramref name="member"/>,
		/// whose <see cref="MemberInfo.Name"/> may contain e.g. an interface
		/// qualifier if this is an explicit interface member implementation.
		/// </summary>
		/// <param name="member">Not null.</param>
		/// <returns>Not null.</returns>
		public static string GetUnQualifiedName(this MemberInfo member)
		{
			if (member == null)
				throw new global::System.ArgumentNullException(nameof(member));
			return member.Name.Split('.').Last();
		}

		/// <summary>
		/// Returns true if the <see cref="MemberInfo.Name"/> of this
		/// <paramref name="member"/> is a qualified name, as with e.g. an interface
		/// qualifier if this is an explicit interface member implementation.
		/// </summary>
		/// <param name="member">Not null.</param>
		/// <returns>Not null.</returns>
		public static bool IsQualifiedName(this MemberInfo member)
		{
			if (member == null)
				throw new global::System.ArgumentNullException(nameof(member));
			return member.Name.Contains('.');
		}

		/// <summary>
		/// Returns true if this <paramref name="property"/> is defined static
		/// </summary>
		/// <param name="property">Not null.</param>
		/// <returns>True if static.</returns>
		public static bool IsStatic(this PropertyInfo property)
		{
			if (property == null)
				throw new global::System.ArgumentNullException(nameof(property));
			return property.GetAccessors(true)
					.Any(accessor => accessor.IsStatic);
		}
	}
}
