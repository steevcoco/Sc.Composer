using System;
using Sc.Util.System;


namespace Sc.Composer
{
	/// <summary>
	/// Exception that is created when a participant raises an exception.
	/// Each instance of this exception always has a non-null
	/// <see cref="Exception.InnerException"/>.
	/// The <see cref="IComposer{TTarget}"/> sets the
	/// <see cref="IComposer{TTarget}.LastComposeErrors"/> to an aggregate
	/// exception containing instances of this type from any participant
	/// that raised an exception.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target handled
	/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
	public class ParticipantException<TTarget>
			: ComposerException
	{
		private static string getMessage(
				Exception exception,
				IComposerParticipant<TTarget> participant,
				object callbackDelegateTarget)
		{
			if (exception == null)
				throw new ArgumentNullException(nameof(exception));
			return $"'{exception.Message}' --- Raised by: "
					+ (participant != null
							? $"{nameof(participant)}: {participant}."
							: $"{nameof(callbackDelegateTarget)}: {callbackDelegateTarget}.");
		}


		/// <summary>
		/// Constructor: NOTICE that exactly one (and only one) of
		/// <paramref name="participant"/> or <paramref name="callbackDelegateTarget"/>
		/// must be non-null. Also, the <paramref name="exception"/> is required.
		/// If the participant raising the exception is an
		/// <see cref="IComposerParticipant{TTarget}"/> itself, provide
		/// the <paramref name="participant"/> argument. Otherwise it is expected
		/// that a callback raised the exception
		/// (a <see cref="ProvidePartsEventArgs{TTarget}"/> callback):
		/// in this case, pass the <see cref="Delegate"/>
		/// <see cref="Delegate.Target"/> in the <paramref name="callbackDelegateTarget"/>
		/// argument. This constructor creates the <see cref="Exception.Message"/>
		/// from the arguments; and they are not retained here.
		/// </summary>
		/// <param name="exception">Required.</param>
		/// <param name="participant">One is required.</param>
		/// <param name="callbackDelegateTarget">One is required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public ParticipantException(
				Exception exception,
				IComposerParticipant<TTarget> participant,
				object callbackDelegateTarget)
				: base(
						ParticipantException<TTarget>.getMessage(
								exception,
								participant,
								callbackDelegateTarget),
						exception)
		{
			if (participant == null == (callbackDelegateTarget == null))
				throw new ArgumentException();
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $"'{Message}' --- {nameof(Exception.InnerException)}: {InnerException}"
					+ "]";
	}
}
