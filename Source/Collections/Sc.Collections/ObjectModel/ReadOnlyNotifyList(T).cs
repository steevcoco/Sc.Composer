using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using Sc.Abstractions.Collections.ObjectModel;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// Provides a readonly implementation for <see cref="INotifyCollection{T}"/>,
	/// wrapping an <see cref="IReadOnlyList{T}"/>. Can be used for a readonly
	/// colection when the binding interfaces
	/// are needed; but the  implementation will be readonly.
	/// The underlying collection CAN ALSO be changed; and this class
	/// is serializable.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	[DataContract]
	public class ReadOnlyNotifyList<T>
			: ReadOnlyNotifyCollectionBase<T, IReadOnlyList<T>, T>,
					INotifyCollection<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ReadOnlyNotifyList(IReadOnlyList<T> collection)
				: base(collection) { }


		public T this[int index]
			=> Collection[index];

		/// <summary>
		/// This method allows changing the underlying actual collection instance.
		/// This will raise a Reset event. The value cannot be null.
		/// </summary>
		/// <param name="collection">The new actual backing collection.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void SetCollection(IReadOnlyList<T> collection)
			=> Collection = collection;

		/// <summary>
		/// This method allows you to raise the <see cref="NotifyCollectionChangedAction.Reset"/>
		/// at any time. Notice that if you invoke <see cref="SetCollection"/>, this
		/// event WILL be raised by that method.
		/// </summary>
		public void RaiseCollectionReset()
			=> EventHandler.RaiseResetEvents();
	}
}
