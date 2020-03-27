using System.Collections.Generic;


namespace Sc.Abstractions.Collections.Specialized
{
	/// <summary>
	/// A ReadOnly view of a dictionary that contains more than one value per key.
	/// An enumeration over the dictionary yields every value for every key: the
	/// enumerator returns <see cref="KeyValuePair{TKey,TValue}"/>, where the Key
	/// is each key in the dictionary in sequence, and for each key, the enumeration
	/// will yield each value, and then for the next key, each value, etc. Note that this
	/// api returns values as <see cref="IReadOnlyList{TValue}"/>, which is how the values
	/// are stored; and though mutations cannot be tracked, a collection created with
	/// only add operations will retain the add order for values under each key
	/// --- the list of values for each key respects the List interface
	/// --- and the implementation may provide other guarantees. Note also that
	/// methods may return the ACTUAL unterlying list of elements.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys.</typeparam>
	/// <typeparam name="TValue">The type of the list contents.</typeparam>
	public interface IMultiDictionary<TKey, TValue>
			: IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		/// <summary>
		/// Returns the actual collection of all keys.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		IReadOnlyCollection<TKey> Keys { get; }

		/// <summary>
		/// Returns THE ACTUAL list of all current values under the key.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="values">Not null or empty if the method returns true.</param>
		/// <returns>True if the key is found.</returns>
		bool TryGetValues(TKey key, out IReadOnlyList<TValue> values);

		/// <summary>
		/// This method yields an enumeration of THE ACTUAL list of all
		/// values under each key.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetListEnumerator();

		/// <summary>
		/// This method yields an enumeration of all values, under any key.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		IEnumerator<TValue> GetValueEnumerator();
	}
}
