namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines an object that must be <see cref="Initialize"/>.
	/// </summary>
	public interface IInitialize
	{
		/// <summary>
		/// Initializes the component.
		/// </summary>
		void Initialize();
	}
}
