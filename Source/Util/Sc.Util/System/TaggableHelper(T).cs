using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Sc.Abstractions.System;


namespace Sc.Util.System
{
	/// <summary>
	/// This class provides a generic helper for classes to implement
	/// <see cref="ITaggable"/> --- please also see <see cref="TaggableHelper"/>
	/// for a non-generic version. This generic implementation is provided
	/// so that your implementing class could delegate the interface
	/// implementation and provide key or value wrappers of your own
	/// type to implement other functionality. Or also optionally
	/// restrict the allowed key or value types: this will raise
	/// <see cref="InvalidOperationException"/> for object keys
	/// and values that do not conform to the defined types.
	/// The interface can be implemented by delegating to methods on this object.
	/// Thread safe. Also implements <see cref="IDisposable"/>; and
	/// is <see cref="DataContractAttribute"/>.
	/// Can be subclassed; and provides <see cref="TagsChanged"/>.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	[DataContract]
	public class TaggableHelper<TKey, TValue>
			: IDisposable
			where TValue : class
	{
		/// <summary>
		/// The actual tags: must be protected by locking this collection.
		/// </summary>
		[DataMember]
		protected Dictionary<TKey, TValue> Tags = new Dictionary<TKey, TValue>(1);


		/// <summary>
		/// Implementation kelper: this throws if the <paramref name="key"/>
		/// is null, or is not <typeparamref name="TKey"/>.
		/// </summary>
		/// <param name="key">User-provided key.</param>
		/// <returns>The argument as <typeparamref name="TKey"/> if it
		/// is of the correct type.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		protected TKey CheckKey(object key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (!(key is TKey tKey)) {
				throw new InvalidOperationException(
						$"Key '{key.GetType().GetFriendlyFullName()}'"
						+ $" is not of required type '{typeof(TKey).GetFriendlyFullName()}'.");
			}
			return tKey;
		}

		/// <summary>
		/// Implementation kelper: this throws if the <paramref name="key"/>
		/// is null, or is not <typeparamref name="TKey"/>.
		/// </summary>
		/// <param name="key">User-provided key.</param>
		/// <returns>The argument as <typeparamref name="TKey"/> if it
		/// is of the correct type.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		protected TValue CheckValue(object value)
		{
			if (value == null)
				return null;
			if (!(value is TValue tValue)) {
				throw new InvalidOperationException(
						$"Value '{value.GetType().GetFriendlyFullName()}'"
						+ $" is not of required type '{typeof(TValue).GetFriendlyFullName()}'.");
			}
			return tValue;
		}


		/// <summary>
		/// Raises <see cref="TagsChanged"/>.
		/// </summary>
		protected virtual void RaiseTagsChanged()
			=> TagsChanged?.Invoke(this, EventArgs.Empty);


		/// <summary>
		/// Implements <see cref="ITaggable.Tag(object, object)"/>; AND restricts
		/// the types to the defined types.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="value">Can be null: as with <see cref="ITaggable"/>.</param>
		/// <returns>As with <see cref="ITaggable"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public object TaggableTag(object key, object value)
			=> Tag(CheckKey(key), CheckValue(value));

		/// <summary>
		/// Implements <see cref="ITaggable.Tag(object)"/>; AND restricts
		/// the type to the defined type.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>As with <see cref="ITaggable"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public object TaggableTag(object key)
			=> Tag(CheckKey(key));


		/// <summary>
		/// Provides a generic version of <see cref="ITaggable.Tag(object, object)"/>.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="value">Can be null.</param>
		/// <returns>The prior value; now removed (or replaced)
		/// --- unless this <paramref name="value"/> is reference-equal
		/// to the existing value.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public TValue Tag(TKey key, TValue value)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			TValue priorValue;
			lock (Tags) {
				if (Tags.TryGetValue(key, out priorValue)) {
					if (value == null)
						Tags.Remove(key);
					else
						Tags[key] = value;
				} else if (value != null)
					Tags[key] = value;
				else
					return null;
			}
			RaiseTagsChanged();
			return priorValue;
		}

		/// <summary>
		/// Provides a generic version of <see cref="ITaggable.Tag(object)"/>.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <returns>The existing value: may be null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public TValue Tag(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			lock (Tags) {
				return Tags.TryGetValue(key, out TValue result)
						? result
						: null;
			}
		}

		/// <summary>
		/// Provides a method that allows you to provide a predicate
		/// that will be invoked with any current value, and if your
		/// predicate reutrns true, then the value is ALWAYS set to
		/// your new value --- even when that is null, which REMOVES
		/// this <paramref name="key"/>. Please note that this return
		/// value is different from <see cref="Tag(TKey,TValue)"/>:
		/// that method ALWAYS removes or replaces the current value,
		/// and always returns a value that has been removed
		/// (or replaced). This method ALWAYS returns the now
		/// CURRENT value --- which MAY be null if your predicate
		/// removes it, or makes no changes yet there is no current
		/// value.
		/// </summary>
		/// <param name="key">Not null.</param>
		/// <param name="predicate">Not null. Will be invoked with any
		/// existing value, even if null. If this returns true, then
		/// the predicate's returned new value is ALWAYS set, even if
		/// null --- which REMOVES this key. If the predicate returns
		/// false then the current value is not changhedl even if null.</param>
		/// <returns>Always returns the now-current value;
		/// which still may be null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public TValue TryTag(TKey key, ValuePredicate<TValue> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			TValue newValue;
			lock (Tags) {
				TValue currentValue = Tag(key);
				if (!predicate(currentValue, out newValue))
					return currentValue;
				if (newValue == null)
					Tags.Remove(key);
				else
					Tags[key] = newValue;
			}
			RaiseTagsChanged();
			return newValue;
		}


		/// <summary>
		/// Returns the count of current tags.
		/// </summary>
		public int Count
		{
			get {
				lock (Tags) {
					return Tags.Count;
				}
			}
		}

		/// <summary>
		/// Returns a new array of all Keys.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		public TKey[] GetKeys()
		{
			lock (Tags) {
				return Tags.Keys.ToArray();
			}
		}

		/// <summary>
		/// Returns a new array of all Values.
		/// </summary>
		/// <returns>Not null; may be empty.</returns>
		public TValue[] GetValues()
		{
			lock (Tags) {
				return Tags.Values.ToArray();
			}
		}

		/// <summary>
		/// Removes all tags.
		/// </summary>
		public void Clear()
		{
			lock (Tags) {
				Tags.Clear();
			}
			RaiseTagsChanged();
		}


		/// <summary>
		/// Sets <see cref="TagsChanged"/> to null.
		/// </summary>
		public void ClearTagsChanged()
			=> TagsChanged = null;

		/// <summary>
		/// Raised when a tag is added, changed or cleared.
		/// </summary>
		[field: NonSerialized]
		public event EventHandler TagsChanged;


		/// <summary>
		/// Invoked from <see cref="Dispose"/>.
		/// </summary>
		/// <param name="isDisposing">False if invoked from a finalizer.</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!isDisposing)
				return;
			lock (Tags) {
				ClearTagsChanged();
				Clear();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
