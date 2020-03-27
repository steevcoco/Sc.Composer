using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;


namespace Sc.Abstractions.Collections.ObjectModel
{
	/// <summary>
	/// Defines a non-generic interface for an <see cref="IEnumerable"/>
	/// that is <see cref="INotifyCollectionChanged"/> and
	/// <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	public interface INotifyCollection
			: IEnumerable,
					INotifyCollectionChanged,
					INotifyPropertyChanged { }
}
