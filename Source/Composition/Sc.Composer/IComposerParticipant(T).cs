namespace Sc.Composer
{
	/// <summary>
	/// Defines a common interface for <see cref="IComposer{TTarget}"/> participants.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target handled
	/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
	// ReSharper disable once UnusedTypeParameter
	public interface IComposerParticipant<in TTarget> { }
}
