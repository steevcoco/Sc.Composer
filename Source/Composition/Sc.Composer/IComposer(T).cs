using System;
using Sc.Abstractions.Lifecycle;


namespace Sc.Composer
{
	/// <summary>
	/// Interface for a composition host that accepts and invokes
	/// <see cref="IProvideParts{TTarget}"/>, <see cref="IBootstrap{TTarget}"/>,
	/// <see cref="IHandleComposed{TTarget}"/>,
	/// and <see cref="IRequestComposition{TTarget}"/> participants.
	/// This object accepts participants in a composition, which is
	/// centered around a given <typeparamref name="TTarget"/> target object.
	/// When composed, a simple event sequence is followed:
	/// parts are gathered from <see cref="IProvideParts{TTarget}"/>
	/// participants; then <see cref="IBootstrap{TTarget}"/>
	/// participants run; and then <see cref="IHandleComposed{TTarget}"/>
	/// participants. More specifically, on each composition, the parts
	/// host gathers parts from all added
	/// <see cref="IProvideParts{TTarget}"/> participants, which are
	/// provided the <typeparamref name="TTarget"/> target to inspect and
	/// participate into. These participants may add callbacks here; and
	/// these will be invoked now, after all parts are participated.
	/// The "part" composition is then considered complete. This will then
	/// invoke all added <see cref="IBootstrap{TTarget}"/> participants,
	/// which also receive the <typeparamref name="TTarget"/> target;
	/// now holding all contributed parts. The full target composition
	/// is then considered complete, and this will then invoke all added
	/// <see cref="IHandleComposed{TTarget}"/> participants; which
	/// again receive the composed target. Notice that
	/// <see cref="IRequestComposition{TTarget}"/> participants
	/// raise an event that triggers the <see cref="IComposer{TTarget}"/>
	/// to begin a full composition sequence. The implementation
	/// determines if the given <typeparamref name="TTarget"/> target object
	/// is a singleton, or is a new instance for each composition; and
	/// any other parameters that participants must be guaranteed or provide.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target handled
	/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
	public interface IComposer<out TTarget>
			: IRaiseDisposed
	{
		/// <summary>
		/// Checks the type of your <see cref="IComposerParticipant{TTarget}"/>,
		/// and contributes all supported interfaces.
		/// </summary>
		/// <param name="participant">Not null.</param>
		/// <param name="oneTimeOnly">Optional, and defaults to false: will apply
		/// to all added participants.</param>
		/// <returns>This composer.</returns>
		/// <exception cref="ArgumentNullException"/>
		IComposer<TTarget> Participate(IComposerParticipant<TTarget> participant, bool oneTimeOnly = false);

		/// <summary>
		/// Removes an <see cref="IComposerParticipant{TTarget}"/> participant
		/// added with <see cref="Participate"/>.
		/// </summary>
		/// <param name="participant">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		void Remove(IComposerParticipant<TTarget> participant);

		/// <summary>
		/// This method will synchronously invoke a complete composition event sequence.
		/// All parts are gathered from <see cref="IProvideParts{TTarget}"/>
		/// participants; and then any callbacks from those
		/// participants are called back. <see cref="IBootstrap{TTarget}"/> participants
		/// are invoked. And then <see cref="IHandleComposed{TTarget}"/> participants.
		/// Note that also, any participants that
		/// raise an exception will contribute a <see cref="ParticipantException{TTarget}"/>
		/// to this <see cref="LastComposeErrors"/>
		/// --- an existing exception there will
		/// be removed, and a new instance with any errors from this composition is set,
		/// or else null.
		/// </summary>
		/// <returns>Not null.</returns>
		/// <exception cref="AggregateComposerException">If there is an unhandled
		/// <see cref="IComposer{TTarget}"/> error; or if the implementation specifies
		/// that it will throw on a participant error. Note that errors will
		/// always be aggregated into the <see cref="LastComposeErrors"/>
		/// in all cases --- and if thrown, that is the exception thrown here.</exception>
		TTarget Compose();

		/// <summary>
		/// Note: can be null. This property will be set to a new instance or null
		/// after each composition. If any participants raised exceptions on a composition,
		/// the exception is caught and aggregated here. The inner exceptions here
		/// will all be instances of <see cref="ParticipantException{TTarget}"/>
		/// for participant exceptions; and <see cref="ComposerException"/> for
		/// other composer errors.
		/// If there were no errors then this is set null. Notice that this error may not
		/// be available until all participants and callbacks have run;
		/// but the implementation may aggregate errors as they happen. This must be set
		/// null at the start of each composition, and must be set before the
		/// composition completes if there is an error.
		/// </summary>
		AggregateComposerException LastComposeErrors { get; }
	}
}
