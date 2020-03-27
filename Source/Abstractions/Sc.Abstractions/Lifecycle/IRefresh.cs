namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines an object that provides a <see cref="Refresh"/> method.
	/// </summary>
	public interface IRefresh
	{
		/// <summary>
		/// Refreshes this object's refreshable state.
		/// </summary>
		void Refresh();
	}
}
