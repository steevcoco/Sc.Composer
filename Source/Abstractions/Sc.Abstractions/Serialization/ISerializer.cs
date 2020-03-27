using System;
using System.Collections.Generic;
using System.IO;


namespace Sc.Abstractions.Serialization
{
	/// <summary>
	/// An object that manages serialization. Note that this type
	/// extends <see cref="IProvideKnownTypes"/>: an instance can
	/// be created with a set of known serialization types; and also
	/// supports scoping.
	/// </summary>
	public interface ISerializer
			: IProvideKnownTypes
	{
		/// <summary>
		/// Creates a copy of this serializer that will use the given known types. The argument
		/// can be specified to add to any that are provided by this instance, or otherwise
		/// only use the given list. Notice also that if this instance is created as
		/// more than one nested instance, and if the parent types are included, then the types
		/// from all preants are included.
		/// </summary>
		/// <param name="knownTypes">Not null.</param>
		/// <param name="includeParentTypes">Specifies that the parent types should be included;
		/// or otherwise none are included.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		ISerializer Scope(GetKnownTypes knownTypes, bool includeParentTypes = true);


		/// <summary>
		/// Serializes the argument as <c>obj.GetType()</c>.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>The serialized result if successful. True for success. False for a serialization
		/// error; and any available error if the result is false.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		(string serialized, bool result, Exception exception) TrySerialize(
				object obj,
				IEnumerable<Type> knownTypes = null);

		/// <summary>
		/// Serializes the argument as <c>serializerType</c>.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <param name="serializerType">This is the Type used for the serializer.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>The serialized result if successful. True for success. False for a serialization
		/// error; and any available error if the result is false.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		(string serialized, bool result, Exception exception) TrySerialize(
				object obj,
				Type serializerType,
				IEnumerable<Type> knownTypes = null);

		/// <summary>
		/// Serializes the argument as <c>obj.GetType()</c> and writes it to the given stream.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <param name="targetStream">The target stream to write into. This is not closed.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>True for success. False for a serialization
		/// error. Null for an IO error. And any available error if unsuccessful.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		(bool? result, Exception exception) TrySerializeToStream(
				object obj,
				Stream targetStream,
				IEnumerable<Type> knownTypes = null);

		/// <summary>
		/// Serializes the argument as <c>serializerType</c> and writes it to the given stream.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <param name="serializerType">This is the Type used for the serializer.</param>
		/// <param name="targetStream">The target stream to write into. This is not closed.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>True for success. False for a serialization
		/// error. Null for an IO error. And any available error if unsuccessful.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		(bool? result, Exception exception) TrySerializeToStream(
				object obj,
				Type serializerType,
				Stream targetStream,
				IEnumerable<Type> knownTypes = null);


		/// <summary>
		/// Deserializes the argument as <c>T</c>.
		/// </summary>
		/// <typeparam name="T">The deserialized type.</typeparam>
		/// <param name="serialized">Not null.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>The deserialized result if successful. True for success. False for a serialization
		/// error; and any available error if the result is false.</returns>
		/// <exception cref="ArgumentNullException">If the arg is null or empty.</exception>
		(T deserialized, bool result, Exception exception) TryDeserialize<T>(
				string serialized,
				IEnumerable<Type> knownTypes = null);

		/// <summary>
		/// Deserializes the argument as <c>serializerType</c>, and returns the object as <c>T</c>.
		/// </summary>
		/// <typeparam name="T">The returned type.</typeparam>
		/// <param name="serialized">Not null.</param>
		/// <param name="serializerType">This is the Type used for the deserializer.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>The deserialized result if successful. True for success. False for a serialization
		/// error; and any available error if the result is false.</returns>
		/// <exception cref="ArgumentNullException">If either arg is null or empty.</exception>
		/// <exception cref="ArgumentException">If <c>!typeof(T).IsAssignableFrom(serializerType)</c>.</exception>
		(T deserialized, bool result, Exception exception) TryDeserialize<T>(
				string serialized,
				Type serializerType,
				IEnumerable<Type> knownTypes = null);

		/// <summary>
		/// Deserializes the argument from the stream as <c>T</c>.
		/// </summary>
		/// <typeparam name="T">The deserialized type.</typeparam>
		/// <param name="sourceStream">Source for the data.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>The deserialized result if successful. True for success. False for a serialization
		/// error. Null for an IO error. And any available error if unsuccessful.</returns>
		/// <exception cref="ArgumentNullException">If the arg is null.</exception>
		(T deserialized, bool? result, Exception exception) TryDeserializeFromStream<T>(
				Stream sourceStream,
				IEnumerable<Type> knownTypes = null);

		/// <summary>
		/// DeSerializes the argument from the stream as <c>serializerType</c>, and returns the object
		/// as <c>T</c>.
		/// </summary>
		/// <typeparam name="T">The returned type.</typeparam>
		/// <param name="sourceStream">Source for the data.</param>
		/// <param name="serializerType">This is the Type used for the deserializer.</param>
		/// <param name="knownTypes">Optional known types for the serializer.</param>
		/// <returns>The deserialized result if successful. True for success. False for a serialization
		/// error. Null for an IO error. And any available error if unsuccessful.</returns>
		/// <exception cref="ArgumentNullException">If either arg is null.</exception>
		/// <exception cref="ArgumentException">If <c>!typeof(T).IsAssignableFrom(serializerType)</c>.</exception>
		(T deserialized, bool? result, Exception exception) TryDeserializeFromStream<T>(
				Stream sourceStream,
				Type serializerType,
				IEnumerable<Type> knownTypes = null);
	}
}
