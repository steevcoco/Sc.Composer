namespace Sc.Abstractions.ObjectModel
{
	/// <summary>
	/// Defines an object with a unique string <see cref="Id"/>.
	/// </summary>
	public interface IHasId
	{
		/// <summary>
		/// This object's Id --- is a key.
		/// </summary>
		string Id { get; }
	}
}
