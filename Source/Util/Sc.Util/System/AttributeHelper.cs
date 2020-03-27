using System;
using System.Linq;
using System.Reflection;


namespace Sc.Util.System
{
	/// <summary>
	/// Static helpers for <see cref="Attribute"/>.
	/// </summary>
	public static class AttributeHelper
	{
		/// <summary>
		/// Fetches the first given Attribute from the <see cref="object"/>. If the object IS a Type, then
		/// the attribute is always fetched from the Type. If the object is an Enum MEMBER, then this will
		/// first try to fetch the attribute from the member. Otherwise the return value is from the object's
		/// Type.
		/// </summary>
		/// <typeparam name="T">The attribute type to fetch.</typeparam>
		/// <param name="obj">Must not be null.</param>
		/// <param name="attribute">Not null if the method returns true: the first found instance.</param>
		/// <param name="inherit">Defaults to true.</param>
		/// <returns>True if the attribute is found.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryGetAttribute<T>(object obj, out T attribute, bool inherit = true)
				where T : Attribute
		{
			attribute = AttributeHelper.TryGetAttribute(obj, typeof(T), out Attribute attributeType, inherit)
					? attributeType as T
					: null;
			return attribute != null;
		}

		/// <summary>
		/// Fetches the first given Attribute from the <see cref="object"/>. If the object IS a Type, then
		/// the attribute is always fetched from the Type. If the object is an Enum MEMBER, then this will
		/// first try to fetch the attribute from the member. Otherwise the return value is from the object's
		/// Type.
		/// </summary>
		/// <param name="obj">Must not be null.</param>
		/// <param name="attributeType">Required: the Attribute type to fetch.</param>
		/// <param name="attribute">Not null if the method returns true: the first found instance.</param>
		/// <param name="inherit">Defaults to true.</param>
		/// <returns>True if the attribute is found.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static bool TryGetAttribute(object obj, Type attributeType, out Attribute attribute, bool inherit = true)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			if (obj is Type objType
					|| !(objType = obj.GetType()).IsEnum
					|| !AttributeHelper.TryGetAttribute(
							objType,
							attributeType,
							Enum.IsDefined(objType, obj)
									? Enum.GetName(objType, obj)
									: obj.ToString(),
							out attribute,
							inherit)) {
				attribute
						= objType.GetCustomAttributes(attributeType, inherit)
								.FirstOrDefault() as Attribute;
			}
			return attribute != null;
		}

		/// <summary>
		/// Fetches the Attribute from the member on the <see cref="Type"/>.
		/// </summary>
		/// <typeparam name="T">The attribute type to fetch.</typeparam>
		/// <param name="type">Must not be null.</param>
		/// <param name="memberName">Must not be null or whitespace.</param>
		/// <param name="attribute">Not null if the method returns true: the first found instance.</param>
		/// <param name="inherit">Defaults to true.</param>
		/// <returns>True if the attribute is found.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="AmbiguousMatchException"/>
		public static bool TryGetAttribute<T>(Type type, string memberName, out T attribute, bool inherit = true)
				where T : Attribute
		{
			attribute = AttributeHelper.TryGetAttribute(
					type,
					typeof(T),
					memberName,
					out Attribute attributeType,
					inherit)
					? attributeType as T
					: null;
			return attribute != null;
		}

		/// <summary>
		/// Fetches the Attribute from the member on the <see cref="Type"/>.
		/// </summary>
		/// <param name="type">Must not be null.</param>
		/// <param name="attributeType">Required: the Attribute type to fetch.</param>
		/// <param name="memberName">Must not be null or whitespace.</param>
		/// <param name="attribute">Not null if the method returns true: the first found instance.</param>
		/// <param name="inherit">Defaults to true.</param>
		/// <returns>True if the attribute is found.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="AmbiguousMatchException"/>
		public static bool TryGetAttribute(
				Type type,
				Type attributeType,
				string memberName,
				out Attribute attribute,
				bool inherit = true)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (string.IsNullOrWhiteSpace(memberName))
				throw new ArgumentNullException(nameof(memberName));
			MemberInfo[] members
					= type.GetMember(
							memberName,
							(inherit
									? BindingFlags.FlattenHierarchy
									: BindingFlags.DeclaredOnly)
							| BindingFlags.NonPublic
							| BindingFlags.Public
							| BindingFlags.Instance
							| BindingFlags.Static);
			if (members.Length != 1) {
				attribute = null;
				return false;
			}
			attribute = members[0]
					.GetCustomAttributes(attributeType, inherit)
					.FirstOrDefault() as Attribute;
			return attribute != null;
		}
	}
}
