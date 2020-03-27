using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sc.Util.Collections;
using Sc.Util.Text;


namespace Sc.Util.System
{
	/// <summary>
	/// Static helpers for <see cref="Type"/>.
	/// </summary>
	public static class TypeHelper
	{
		/// <summary>
		/// Returns a new instance of the default value for this Type:
		/// <c>Activator.CreateInstance(type)</c> for a value type,
		/// and otherwise null.
		/// </summary>
		/// <param name="type">Required.</param>
		/// <returns>The default value.</returns>
		public static object GetDefaultValue(this Type type)
		{
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			return type.IsValueType
					? Activator.CreateInstance(type)
					: null;
		}


		/// <summary>
		/// Given an object <c>obj</c>, this method will determine if the object implements
		/// a specified generic interface, or extends a specified generic class --- either
		/// of which is given by this <c>genericType</c>. If the object implements or extends this
		/// generic type, then the generic arguments declared by the <c>obj</c> are returned.
		/// Additionally, if this <c>genericType</c> is an interface, the object is checked
		/// for all closed implementations of the interface: the out argument may contain
		/// more than one array.
		/// </summary>
		/// <param name="genericType">Not null. Must be generic. May be a class or an interface;
		/// and may be an open or closed generic type.</param>
		/// <param name="obj">Not null.</param>
		/// <param name="genericArguments">Will be set if the method returns true. Tf this
		/// <c>genericType</c> is an interface, and the object implements more than one
		/// definition of this interface, then the arguments from each definition are
		/// included in the list.</param>
		/// <returns>True if the <c>obj</c> implements or extends this generic type.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"><c>genericType</c> must be an open or closed
		/// generic type.</exception>
		public static bool TryGetGenericArguments(
				this Type genericType,
				object obj,
				out List<Type[]> genericArguments)
			=> genericType.TryGetGenericArguments(
					obj?.GetType()
					?? throw new ArgumentNullException(nameof(obj)),
					out genericArguments);

		/// <summary>
		/// Given a <see cref="Type"/>, this method will determine if the <c>implementingType</c>
		/// implements a specified generic interface, or extends a specified generic class --- either
		/// of which is given by this <c>genericType</c>. If the <c>implementingType</c>
		/// implements or extends this generic type, then the generic arguments declared by the
		/// <c>implementingType</c> are returned. Additionally, if this <c>genericType</c> is an
		/// interface, the <c>implementingType</c> is checked for all closed implementations of the
		/// interface: the out argument may contain more than one array.
		/// </summary>
		/// <param name="genericType">Not null. Must be generic. May be a class or an interface;
		/// and may be an open or closed generic type.</param>
		/// <param name="implementingType">Not null.</param>
		/// <param name="genericArguments">Will be set if the method returns true. Tf this
		/// <c>genericType</c> is an interface, and the <c>implementingType</c> implements more
		/// than one definition of this interface, then the arguments from each definition are
		/// included in the list.</param>
		/// <returns>True if the <c>implementingType</c> implements or extends this generic type.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"><c>genericType</c> must be an open or closed
		/// generic type.</exception>
		public static bool TryGetGenericArguments(
				this Type genericType,
				Type implementingType,
				out List<Type[]> genericArguments)
		{
			if (genericType == null)
				throw new ArgumentNullException(nameof(genericType));
			if (implementingType == null)
				throw new ArgumentNullException(nameof(implementingType));
			if (!genericType.IsGenericType) {
				throw new ArgumentException(
						$"{genericType.GetFriendlyName()} is not a generic type.", nameof(genericType));
			}
			if (!genericType.IsGenericTypeDefinition)
				genericType = genericType.GetGenericTypeDefinition();
			List<Type> baseTypes;
			if (genericType.IsInterface) {
				baseTypes
						= implementingType
								.GetInterfaces()
								.ToList();
			} else {
				baseTypes = new List<Type>(8);
				Type baseType = implementingType;
				while ((baseType != typeof(object))
						&& (baseType != null)) {
					baseTypes.Add(baseType);
					baseType = baseType.BaseType;
				}
			}
			genericArguments = new List<Type[]>(baseTypes.Count);
			foreach (Type baseType in baseTypes) {
				if (!baseType.IsGenericType)
					continue;
				if (baseType.GetGenericTypeDefinition() == genericType)
					genericArguments.Add(baseType.GetGenericArguments());
			}
			return genericArguments.Count != 0;
		}


		/// <summary>
		/// This method locates the name of a public static Field given the field's value.
		/// This will search the public static fields defined on
		/// <paramref name="definedOnType"/>, and returns the name of the first one
		/// whose value Equals the <paramref name="staticFieldValue"/>.
		/// </summary>
		/// <param name="definedOnType">Required type that defines the
		/// <paramref name="staticFieldValue"/>.</param>
		/// <param name="staticFieldValue">REQUIRED value of the static field. Note
		/// that this cannot be null.</param>
		/// <param name="flattenHierarchy">Defaults to FALSE: specifies if
		/// <see cref="BindingFlags.FlattenHierarchy"/> is specified --- OR otherwise,
		/// <see cref="BindingFlags.DeclaredOnly"/> is specified.</param>
		/// <returns>The field name on your Type; or null if not found.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string GetStaticFieldName(
				Type definedOnType,
				object staticFieldValue,
				bool flattenHierarchy = false)
		{
			if (definedOnType == null)
				throw new ArgumentNullException(nameof(definedOnType));
			if (staticFieldValue == null)
				throw new ArgumentNullException(nameof(staticFieldValue));
			foreach (
					FieldInfo fieldInfo
					in definedOnType.GetFields(
							BindingFlags.Public
							| BindingFlags.Static
							| BindingFlags.GetField
							| (flattenHierarchy
									? BindingFlags.FlattenHierarchy
									: BindingFlags.DeclaredOnly))) {
				if (object.Equals(fieldInfo.GetValue(null), staticFieldValue))
					return fieldInfo.Name;
			}
			return null;
		}


		/// <summary>
		/// Static helper method that will strip the C# default backtick 
		/// "<c>`</c>" character from a Type name, and will return all generic
		/// type argument names, and angle brackets. Optionally, if
		/// <paramref name="omitGenericArguments"/> is true, this method will
		/// return a name that is just the simple name. E.G. for <c>List&lt;object></c>,
		/// this returns either "<c>List&lt;Object></c>", or just "<c>List</c>".
		/// if <paramref name="omitGenericArguments"/> is set null, then only
		/// the angle brackets are returned --- e.g. <c>KeyValuePair&lt;object, string></c>
		/// will return "<c>KeyValuePair&lt',></c>".
		/// </summary>
		/// <param name="type">CAN be null: if so, this returns
		/// <see cref="Convert"/> <c>Convert.ToString(null)</c>.</param>
		/// <param name="omitGenericArguments">Defauts to false: all
		/// generic argument types will be made "friendly". If null, only angle
		/// brackets and commas are return for generic argument types.
		/// If true, only the simple name is returned</param>
		/// <returns>Not null.</returns>
		public static string GetFriendlyName(this Type type, bool? omitGenericArguments = false)
		{
			if (type == null)
				return Convert.ToString(null);
			return appendFriendlyName(new EnumerableStringBuilder(), type, omitGenericArguments)
					.ToString();
		}

		private static EnumerableStringBuilder appendFriendlyName(
				EnumerableStringBuilder friendlyName,
				Type type,
				bool? omitGenericArguments)
		{
			AppendTypeName(friendlyName, false, type, omitGenericArguments);
			return friendlyName;
			static void AppendTypeName(EnumerableStringBuilder builder, bool isRecursive, Type t, bool? omitGenerics)
			{
				bool isArray = t.IsArray;
				if (isArray)
					t = t.GetElementType();
				int index = builder.Count;
				if (!isRecursive
						|| omitGenerics.HasValue)
					builder.StringBuilder.Append(t.Name);
				if (t.IsGenericType) {
					int backtick = builder.FindIndex(IsBackTick, index);
					if (backtick > 0)
						builder.StringBuilder.Remove(backtick, builder.Count - backtick);
					if (omitGenerics != true) {
						builder.StringBuilder.Append('<');
						foreach (Type genericArgument in t.GetGenericArguments()) {
							if (builder[builder.Count - 1] != '<')
								builder.StringBuilder.Append(',');
							AppendTypeName(builder, true, genericArgument, omitGenerics);
						}
						builder.StringBuilder.Append('>');
					}
				}
				if (isArray) {
					for (int i = 0; i < t.GetArrayRank(); ++i) {
						builder.StringBuilder.Append("[]");
					}
				}
				static bool IsBackTick(char c)
					=> c == '`';
			}
		}

		/// <summary>
		/// Please see <see cref="GetFriendlyName(Type, bool?)"/>. This method also builds
		/// a friendly display name for this <paramref name="type"/> by stripping the
		/// C# default backtick  "<c>`</c>" character from a Type name, and will
		/// optionally return all generic type argument names; and angle brackets.
		/// This method will include the Type's Namespace.
		/// </summary>
		/// <param name="type">CAN be null: if so, this returns
		/// <see cref="Convert"/> <c>Convert.ToString(null)</c>.</param>
		/// <param name="omitGenericArguments">Defauts to false: all
		/// generic argument types will be made "friendly". If null, only angle
		/// brackets and commas are return for generic argument types.
		/// If true, only the simple name is returned</param>
		/// <returns>Not null.</returns>
		public static string GetFriendlyFullName(this Type type, bool? omitGenericArguments = false)
		{
			if (type == null)
				return Convert.ToString(null);
			EnumerableStringBuilder friendlyName = new EnumerableStringBuilder(type.Namespace);
			friendlyName.StringBuilder.Append('.');
			return appendFriendlyName(friendlyName, type, omitGenericArguments)
					.ToString();
		}
	}
}
