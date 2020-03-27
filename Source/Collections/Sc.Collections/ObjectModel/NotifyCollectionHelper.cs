using System.Collections.Generic;
using Sc.Abstractions.Collections.ObjectModel;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// Static helper methods for <see cref="INotifyCollection{T}"/>.
	/// </summary>
	public static class NotifyCollectionHelper
	{
		/// <summary>
		/// Constructs and returns a new <see cref="ReadOnlyNotifyList{T}"/>
		/// wrapping this readonly list. Note that the returned instance DOES
		/// support CHANGING the backing collection; and is serializable.
		/// </summary>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="readonlyList">Required.</param>
		/// <returns>Not null.</returns>
		public static ReadOnlyNotifyList<T> AsNotifyCollection<T>(this IReadOnlyList<T> readonlyList)
			=> new ReadOnlyNotifyList<T>(readonlyList);
	}
}
