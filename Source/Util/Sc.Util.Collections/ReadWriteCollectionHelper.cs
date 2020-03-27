using System;
using System.Collections.Generic;


namespace Sc.Util.Collections
{
	/// <summary>
	/// Static constructor methods for <see cref="ReadWriteCollection{TCollection}"/>.
	/// </summary>
	public static class ReadWriteCollectionHelper
	{
		/// <summary>
		/// Constructs a new <see cref="ReadWriteCollection{TCollection}"/> of <see cref="List{T}"/>.
		/// Which will have a new empty <see cref="List{T}"/> instance.
		/// </summary>
		/// <typeparam name="T">The <see cref="List{T}"/> element type.</typeparam>
		/// <param name="copyCollection">Optional: if provided this <c>Func</c> will be passed the current
		/// Collection value; and it must construct a new shallow clone of the Collection: a new
		/// instance containing all elements from the argument --- e.g.
		/// <c>(list) => new List&lt;T>(list)</c>. If null, a default instance is created as just
		/// described.</param>
		/// <returns>Not null.</returns>
		public static ReadWriteCollection<List<T>> List<T>(Func<List<T>, List<T>> copyCollection = null)
		{
			return new ReadWriteCollection<List<T>>(new List<T>(0), copyCollection ?? CopyCollection);
			static List<T> CopyCollection(List<T> list)
			{
				List<T> copy = new List<T>(list.Count + 1);
				copy.AddRange(list);
				return copy;
			}
		}

		/// <summary>
		/// Constructs a new <see cref="ReadWriteCollection{TCollection}"/> of
		/// <see cref="Dictionary{TKey,TValue}"/>. Which will have a new empty
		/// <see cref="Dictionary{TKey,TValue}"/> instance.
		/// </summary>
		/// <typeparam name="TKey">The <see cref="Dictionary{TKey,TValue}"/> Key type.</typeparam>
		/// <typeparam name="TValue">The <see cref="Dictionary{TKey,TValue}"/> Value type.</typeparam>
		/// <param name="copyCollection">Optional: if provided this <c>Func</c> will be passed the current
		/// Collection value; and it must construct a new shallow clone of the Collection: a new
		/// instance containing all elements from the argument --- e.g.
		/// <c>(dictionary) => new Dictionary&lt;TKey, TValue>(list)</c>. If null, a default instance is
		/// created as just described.</param>
		/// <returns>Not null.</returns>
		public static ReadWriteCollection<Dictionary<TKey, TValue>> Dictionary<TKey, TValue>(
				Func<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>> copyCollection = null)
		{
			return new ReadWriteCollection<Dictionary<TKey, TValue>>(
					new Dictionary<TKey, TValue>(0),
					copyCollection ?? CopyCollection);
			static Dictionary<TKey, TValue> CopyCollection(Dictionary<TKey, TValue> dictionary)
				=> new Dictionary<TKey, TValue>(dictionary, dictionary.Comparer);
		}
	}
}
