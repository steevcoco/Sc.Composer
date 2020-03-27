using System;


namespace Sc.Composer.Providers
{
	/// <summary>
	/// Implements a <see cref="ProvidePartsEventArgs{TTarget}"/> of
	/// <typeparamref name="TTarget"/>, that does not add callbacks, and
	/// instead ads them to a given event implementing a covariant type
	/// <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="TTarget">This implemented (contravariant) type.</typeparam>
	/// <typeparam name="T">The delegate event's (covariant) type.</typeparam>
	public class DelegateProvidePartsEventArgs<TTarget, T>
			: ProvidePartsEventArgs<TTarget>
			where T : TTarget
	{
		private readonly ProvidePartsEventArgs<T> eventArgs;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="eventArgs">Required.</param>
		/// <exception cref="ArgumentNullException"/>
		public DelegateProvidePartsEventArgs(ProvidePartsEventArgs<T> eventArgs)
				: base(eventArgs.Target, out _)
			=> this.eventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));


		public override void CallbackWithAllParts(Action<TTarget> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			void Callback(T t)
				=> callback(t);
			eventArgs.CallbackWithAllParts(Callback);
		}
	}
}
