using System;
using System.Threading;
using System.Threading.Tasks;
using Sc.Abstractions.Threading;
using Sc.Util.System;
using Sc.Util.Threading;


namespace Sc.Util.Events.WeakEvents
{
	/// <summary>
	/// Implements a simple weak <see cref="EventHandler{TEventArgs}"/> reference. Used by an event source
	/// to hold a weak reference to a handler given to it. This object always holds a weak reference to
	/// the given <see cref="EventHandler{TEventArgs}"/>; and it is kept alive by a weak reference
	/// to a target that you may specify: by default it is the handler's
	/// <see cref="Delegate.Target"/> object. Dispose this object to remove the reference. You
	/// may use a <see cref="WeakEventDelegateCollection{TEventArgs}"/> as a handler
	/// collection. This class also implements <see cref="ISynchronizationContextAware"/>,
	/// and if a non-null <see cref="SynchronizationContext"/> is set, the handler will be
	/// invoked by Posting or optionally Sending to that context.
	/// </summary>
	public class WeakEventDelegate<TEventArgs>
			: WeakReferenceOwner<EventHandler<TEventArgs>>,
					ISynchronizationContextAware,
					IDisposable
	{
		private volatile SynchronizationContext synchronizationContext;


		/// <summary>
		/// Constructor. If the <paramref name="weakReference"/>
		/// is given, it is used as a weak reference that will
		/// release both it and the <paramref name="eventHandler"/>
		/// when it becomes released. Otherwise this will test
		/// the Target object of the <paramref name="eventHandler"/>,
		/// and if not null, uses that. Note that if
		/// both are null then the weak reference used IS the
		/// <paramref name="eventHandler"/> --- and the
		/// code that constructed that must hold a strong reference (the Target is null if the delegate
		/// is a static method; and a lambda as an event handler delegate may be immediately
		/// collected).
		/// </summary>
		/// <param name="eventHandler">Not null. Will be weakly held here.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>eventHandler</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public WeakEventDelegate(EventHandler<TEventArgs> eventHandler, object weakReference = null)
			: base(eventHandler, weakReference ?? eventHandler?.Target) { }

		/// <summary>
		/// This protected constructor is provided for subclasses: this instance will
		/// have no handler when this constructor completes: you MUST immediately
		/// initialize this instance by invoking <see cref="Initialize"/>.
		/// </summary>
		protected WeakEventDelegate() { }


		/// <summary>
		/// This protected virtual method is invoked in <see cref="TryInvoke"/> and must
		/// execute the given handler now with the given arguments. This implementation
		/// invokes <see cref="Delegate.DynamicInvoke"/> with the sender and event args.
		/// </summary>
		/// <param name="handler">Not null.</param>
		/// <param name="sender">Is the event sender argument provided to <see cref="TryInvoke"/>.</param>
		/// <param name="eventArgs">Is the event argument provided to <see cref="TryInvoke"/>.</param>
		protected virtual void OnInvokeHandler(EventHandler<TEventArgs> handler, object sender, TEventArgs eventArgs)
			=> handler(sender, eventArgs);


		// ReSharper disable once ParameterHidesMember
		public void SetSynchronizationContext(SynchronizationContext synchronizationContext)
			=> this.synchronizationContext = synchronizationContext;

		public SynchronizationContext SynchronizationContext
			=> synchronizationContext;


		/// <summary>
		/// This method will try to fetch the event handler now and invoke the handler with the arguments.
		/// If a non-null <see cref="SynchronizationContext"/> has been set, it is checked,
		/// and this will Post, Send, or Invoke the handler, checking the current
		/// <see cref="SynchronizationContext"/> and the <see cref="SendOrPostPolicy"/>.
		/// </summary>
		/// <param name="sender">NOT tested here.</param>
		/// <param name="eventArgs">NOT tested here.</param>
		/// <param name="sendOrPostPolicy">Invoke selection. Defaults to
		/// <see cref="SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown"/>.</param>
		/// <returns>False if the object is NOT alive and the handler is not invoked.
		/// True if the handler is completed when this returns.
		/// Null if the handler is Posted to the <see cref="SynchronizationContext"/>.</returns>
		public bool? TryInvoke(
				object sender,
				TEventArgs eventArgs,
				SendOrPostPolicy sendOrPostPolicy = SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown)
		{
			if (!TryGetTarget(out EventHandler<TEventArgs> eventHandler))
				return false;
			Task result = SynchronizationContext.SendOrPost(
					SendOrPostCallback,
					(eventHandler, sender, eventArgs),
					sendOrPostPolicy);
			void SendOrPostCallback((EventHandler<TEventArgs> handler, object source, TEventArgs args) state)
				=> OnInvokeHandler(state.handler, state.source, state.args);
			return result.IsCompleted
					? true
					: (bool?)null;
		}
	}
}
