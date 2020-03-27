using System;


namespace Sc.Abstractions.System
{
	/// <summary>
	/// Defines a dictionary of key/value par "Tags" on an object. The object
	/// can manage an arbitrary Tag for any given Key.
	/// </summary>
	public interface ITaggable
	{
		/// <summary>
		/// Sets the tag <paramref name="value"/> for the given
		/// <paramref name="key"/>. Notice that if this <paramref name="value"/>
		/// is null, then this <paramref name="key"/> is removed:
		/// pass a null value to this method to remove the key.
		/// Otherwise the key/value is set. This method will
		/// always return the PRIOR value for this key: note
		/// that in all cases, with this method, the returned
		/// value is now removed (or replaced) --- it will be
		/// the same value as this provided <paramref name="value"/>
		/// if that is reference-equal to the existing value;
		/// and otherwise is the now-removed prior value.
		/// </summary>
		/// <param name="key">The key: not null. This will be matched
		/// by value equality.</param>
		/// <param name="value">The value: if this argument is null, then
		/// this <paramref name="key"/> is removed.</param>
		/// <returns>Any PRIOR value for this key: may be null.
		/// Note that in all cases this return value has now been
		/// removed (or replaced) --- it will NOT be removed, and will be
		/// the same value as this provided <paramref name="value"/>
		/// if that is reference-equal to the existing value;
		/// and otherwise is the now-removed prior value.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		object Tag(object key, object value);

		/// <summary>
		/// Gets any current tag value for the given <paramref name="key"/>.
		/// May be null.
		/// </summary>
		/// <param name="key">The key: not null. This will be matched
		/// by value equality.</param>
		/// <returns>Any current value for this key: may be null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		object Tag(object key);
	}
}
