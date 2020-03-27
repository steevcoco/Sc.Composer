using System;
using System.Collections.Generic;
using Sc.Util.Collections;


namespace Sc.Composer
{
	/// <summary>
	/// Event args raised by <see cref="IComposer{TTarget}"/> for
	/// <see cref="IProvideParts{TTarget}"/> to handle.
	/// </summary>
	/// <typeparam name="TTarget">The composer's target type.</typeparam>
	public class ProvidePartsEventArgs<TTarget>
			: ComposerEventArgs<TTarget>
	{
		/// <summary>
		/// The actual list of all added callbacks.
		/// </summary>
		protected readonly List<Action<TTarget>> Callbacks = new List<Action<TTarget>>(4);


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="target">Required.</param>
		/// <param name="getCallbacks">This will be set to a <see cref="Func{TResult}"/>
		/// that will return all added callbacks. You must invoke this delegate
		/// to retrieve all callbacks added to this event.</param>
		public ProvidePartsEventArgs(TTarget target, out Func<IReadOnlyCollection<Action<TTarget>>> getCallbacks)
				: base(target)
		{
			IReadOnlyCollection<Action<TTarget>> GetCallbacks()
				=> Callbacks.ToArray();
			getCallbacks = GetCallbacks;
		}


		/// <summary>
		/// Adds a callback to be invoked with the composition target after all
		/// <see cref="IProvideParts{TTarget}"/> participants have run
		/// --- and before <see cref="IBootstrap{TTarget}"/> participants run.
		/// </summary>
		/// <param name="callback">Required.</param>
		public virtual void CallbackWithAllParts(Action<TTarget> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			Callbacks.Add(callback);
		}


		public override string ToString()
			=> $"{base.ToString()}"
					+ "["
					+ $", {nameof(ProvidePartsEventArgs<TTarget>.Callbacks)}{Callbacks.ToStringCollection()}"
					+ "]";
	}
}
