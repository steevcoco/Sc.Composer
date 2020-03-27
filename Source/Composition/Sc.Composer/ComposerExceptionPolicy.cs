namespace Sc.Composer
{
	/// <summary>
	/// Specifies the policy for throwing the
	/// <see cref="Composer{TTarget}.LastComposeErrors"/>
	/// from the <see cref="IComposer{TTarget}.Compose"/> method. Note that in all cases,
	/// all exceptions will always be aggregated into the
	/// <see cref="Composer{TTarget}.LastComposeErrors"/>; and then it may be thrown.
	/// </summary>
	public enum ComposerExceptionPolicy
	{
		ThrowNone,
		ThrowUnhandledComposerException,
		ThrowParticipantException,
		ThrowAny,
	}
}
