using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;


namespace Sc.Abstractions.Collections.ObjectModel
{
	/// <summary>
	/// Interface for a collection that is <see cref="IReadOnlyList{T}"/>,
	/// <see cref="INotifyCollectionChanged"/>, and <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	/// <typeparam name="T">The element Type.</typeparam>
	public interface INotifyCollection<out T>
			: INotifyCollection,
					IReadOnlyList<T> { }
}
