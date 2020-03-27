using System.Collections.ObjectModel;


namespace Sc.Util.Collections.Collections
{
	/// <summary>
	/// Implements a <see cref="Collection{T}"/> that does not add elements.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public sealed class VoidCollection<T>
			: Collection<T>
	{
		protected override void InsertItem(int index, T item) { }

		protected override void SetItem(int index, T item) { }
	}
}
