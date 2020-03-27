namespace Sc.Abstractions.Lifecycle
{
	/// <summary>
	/// Defines a particpant that supports resetting.
	/// </summary>
	public interface IReset
	{
		/// <summary>
		/// Invoked to reset this state now.
		/// </summary>
		void Reset();
	}
}
