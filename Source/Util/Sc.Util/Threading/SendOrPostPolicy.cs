using System.Threading;


namespace Sc.Util.Threading
{
	/// <summary>
	/// Values used to select the policy for Actions invoked on a
	/// <see cref="SynchronizationContext"/>.
	/// </summary>
	public enum SendOrPostPolicy
			: byte
	{
		/// <summary>
		/// When used, callers may invoke the same action at different times synchronously or
		/// asynchronously based on SynchronizationContext equality. If this
		/// <see cref="SynchronizationContext"/> is not null: then if the Current context IS
		/// equal to this context, the action is invoked synchronously; and if not equal, it
		/// is Posted to this context. If this context is null, the action is invoked synchronously.
		/// I.E. the action will only be Posted if this context is not null, and does
		/// not compare equal to the Current context; and otherwise is synchronously
		/// invoked.
		/// </summary>
		InvokeSafePostSafeOrInvokeUnknown,

		/// <summary>
		/// When used, callers may invoke the same action at different times synchronously or
		/// asynchronously based on SynchronizationContext equality. If this
		/// <see cref="SynchronizationContext"/> is not null: then even if the Current context IS
		/// equal to this context, the action is always Posted to this non-null context.
		/// If this context is null, the action is invoked synchronously.
		/// </summary>
		PostSafeAlwaysOrInvokeUnknown,

		/// <summary>
		/// Actions are ALWAYS Posted: either on this non-null <see cref="SynchronizationContext"/>,
		/// OR on the ThreadPool.
		/// </summary>
		PostAlwaysSafeOrUnknown,
	}
}
