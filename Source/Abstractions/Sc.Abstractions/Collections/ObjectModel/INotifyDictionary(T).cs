using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;


namespace Sc.Abstractions.Collections.ObjectModel
{
	/// <summary>
	/// An <see cref="IReadOnlyDictionary{TKey,TValue}"/> that is observable: implements
	/// <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	public interface INotifyDictionary<TKey, TValue>
			: IReadOnlyDictionary<TKey, TValue>,
					INotifyCollectionChanged,
					INotifyPropertyChanged { }
}
