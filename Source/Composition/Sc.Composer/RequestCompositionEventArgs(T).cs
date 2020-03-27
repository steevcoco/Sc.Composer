using System;
using System.ComponentModel;
using Sc.Composer.Providers;


namespace Sc.Composer
{
	/// <summary>
	/// Event args raised by <see cref="IRequestComposition{TTarget}"/> to
	/// request a composition. This event allows the sender to provide
	/// an <see cref="IComposerParticipant{TTarget}"/> <see cref="Participant"/>,
	/// and <see cref="Request"/> for the <see cref="IComposer{TTarget}"/>
	/// that handles this event to process the given participant.
	/// </summary>
	/// <typeparam name="TTarget">The composition target type.</typeparam>
	public class RequestCompositionEventArgs<TTarget>
			: EventArgs
	{
		/// <summary>
		/// Enumerates values for <see cref="RequestCompositionEventArgs{TTarget}.Request"/>,
		/// as a request to the <see cref="IComposer{TTarget}"/> that handles
		/// this event to process the <see cref="RequestCompositionEventArgs{TTarget}.Participant"/>
		/// according to this specified action.
		/// </summary>
		public enum ParticipantRequest
				: byte
		{
			None,
			Add,
			AddOneTimeOnly,
			Remove
		}


		/// <summary>
		/// Default constructor sets no <see cref="Participant"/>; and no
		/// <see cref="Request"/>.
		/// </summary>
		public RequestCompositionEventArgs() { }

		/// <summary>
		/// Constructor sets the <see cref="Participant"/>; and the
		/// <see cref="Request"/>.
		/// </summary>
		/// <param name="participant">Required in this constructor.</param>
		/// <param name="request">Required in this constructor; and must not be
		/// <see cref="ParticipantRequest.None"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidEnumArgumentException"></exception>
		public RequestCompositionEventArgs(IComposerParticipant<TTarget> participant, ParticipantRequest request)
		{
			if (!Enum.IsDefined(typeof(ParticipantRequest), request)
					|| (request == ParticipantRequest.None))
				throw new InvalidEnumArgumentException(nameof(request), (int)request, typeof(ParticipantRequest));
			Participant = participant ?? throw new ArgumentNullException(nameof(participant));
			Request = request;
		}


		/// <summary>
		/// This method provides an implementation for an <see cref="IComposer{TTarget}"/>
		/// to handle this event. This checks the <see cref="Participant"/>, and the
		/// <see cref="Request"/>, and will invoke your <paramref name="composer"/>
		/// as specified.
		/// </summary>
		/// <typeparam name="T">Your <paramref name="composer"/>
		/// actual (covariant) type.</typeparam>
		/// <param name="composer">Not null.</param>
		/// <returns>True if any method was invoked on the <paramref name="composer"/>.
		/// False if not invoked.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool Handle<T>(IComposer<T> composer)
				where T : TTarget
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			if (Participant == null)
				return false;
			IComposer<TTarget> targetComposer = composer as IComposer<TTarget>;
			switch (Request) {
				case ParticipantRequest.Add :
					if (targetComposer != null) {
						targetComposer.Participate(Participant);
						return true;
					}
					composer.ParticipateAs(Participant);
					return true;
				case ParticipantRequest.AddOneTimeOnly :
					if (targetComposer != null) {
						targetComposer.Participate(Participant, true);
						return true;
					}
					composer.ParticipateAs(Participant, true);
					return true;
				case ParticipantRequest.Remove :
					if (targetComposer != null) {
						targetComposer.Remove(Participant);
						return true;
					}
					composer.Remove(new VariantParticipant<TTarget, T>(Participant));
					return true;
			}
			return false;
		}


		/// <summary>
		/// Applies if the <see cref="Participant"/> is provided.
		/// </summary>
		public ParticipantRequest Request { get; }

		/// <summary>
		/// This property holds an optional <see cref="IComposerParticipant{TTarget}"/>,
		/// that will be processed by the <see cref="IComposer{TTarget}"/> that handles
		/// this event. If this participant is not null, it will be handled according
		/// to the <see cref="Request"/> value.
		/// </summary>
		public IComposerParticipant<TTarget> Participant { get; }
	}
}
