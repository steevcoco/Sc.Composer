namespace Sc.Abstractions.ObjectModel
{
	/// <summary>
	/// Defines an object with a user-visible <see cref="Name"/>.
	/// </summary>
	public interface IHasName
	{
		/// <summary>
		/// The object's name.
		/// </summary>
		string Name { get; }
	}
}
