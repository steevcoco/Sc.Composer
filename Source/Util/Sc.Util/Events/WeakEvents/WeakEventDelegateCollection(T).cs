using System;
using System.Collections.Generic;
using System.Threading;
using Sc.Abstractions.Threading;
using Sc.Abstractions.Lifecycle;
using Sc.Util.Threading;


namespace Sc.Util.Events.WeakEvents
{
	/// <summary>
	/// Maintains a collection of <see cref="WeakEventDelegate{TEventArgs}"/>
	/// instances of a given event type. --- Used by an event source to hold weak listeners.
	/// Note that this collection is constrained only by the event type: handlers may be added by ANY
	/// event source, and for any event name. A linear list is maintained. The list is purged on
	/// each add and remove; and if <see cref="TryInvoke"/> is invoked. You may manually purge the
	/// list with <see cref="Purge"/>. Any removed handler is always disposed.
	/// Each handler is also watched for the <see cref="IRaiseDisposed.Disposed"/>
	/// event, and removed at that time.
	/// This class also implements <see cref="ISynchronizationContextAware"/>,
	/// and if a non-null <see cref="SynchronizationContext"/> is set, the handlers will be
	/// invoked by Posting or optionally Sending to that context.
	/// </summary>
	/// <typeparam name="TEventArgs"></typeparam>
	public class WeakEventDelegateCollection<TEventArgs>
			: ISynchronizationContextAware
	{
		/// <summary>
		/// This holds the actual list of handlers. Access must be protected by locking this list.
		/// </summary>
		protected readonly List<WeakEventDelegate<TEventArgs>> EventDelegates
				= new List<WeakEventDelegate<TEventArgs>>(2);

		private SynchronizationContext synchronizationContext;


		private void handleWeakEventDelegateDisposed(object sender, EventArgs eventArgs)
			=> Purge();

		private IEnumerable<WeakEventDelegate<TEventArgs>> enumerateLiveDelegates()
		{
			WeakEventDelegate<TEventArgs>[] GetDelegates()
			{
				lock (EventDelegates) {
					return EventDelegates.ToArray();
				}
			}
			foreach (WeakEventDelegate<TEventArgs> weakEventDelegate in GetDelegates()) {
				if (weakEventDelegate.IsAlive) {
					yield return weakEventDelegate;
					continue;
				}
				lock (EventDelegates) {
					EventDelegates.Remove(weakEventDelegate);
				}
				weakEventDelegate.Disposed -= handleWeakEventDelegateDisposed;
				weakEventDelegate.Dispose();
			}
		}


		// ReSharper disable once ParameterHidesMember
		public void SetSynchronizationContext(SynchronizationContext synchronizationContext)
		{
			lock (EventDelegates) {
				this.synchronizationContext = synchronizationContext;
				foreach (WeakEventDelegate<TEventArgs> weakEventDelegate in enumerateLiveDelegates()) {
					weakEventDelegate.SetSynchronizationContext(synchronizationContext);
				}
			}
		}

		public SynchronizationContext SynchronizationContext
		{
			get {
				lock (EventDelegates) {
					return synchronizationContext;
				}
			}
		}


		/// <summary>
		/// Adds an event handler. If the <paramref name="weakReference"/>
		/// is given, it is used as a weak reference that
		/// will release both it and the <paramref name="eventHandler"/>
		/// when it becomes released. Otherwise this will test
		/// the Target object of the <paramref name="eventHandler"/>,
		/// and if not null, uses that. Note that if
		/// both are null then the weak reference used IS the <c>eventHandler</c> --- and the
		/// code that constructed that must hold a strong reference (the Target is null if the delegate
		/// is a static method; and a lambda as an event handler delegate may be immediately
		/// collected).
		/// </summary>
		/// <param name="eventHandler">NOT null. Will be weakly held here.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>eventHandler</c>.</param>
		/// <returns>The actual <see cref="WeakEventDelegate{TEventArgs}"/> that has been created
		/// and added here.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public WeakEventDelegate<TEventArgs> AddHandler(
				EventHandler<TEventArgs> eventHandler,
				object weakReference = null)
		{
			if (eventHandler == null)
				throw new ArgumentNullException(nameof(eventHandler));
			Purge();
			lock (EventDelegates) {
				WeakEventDelegate<TEventArgs> weakEventDelegate
						= new WeakEventDelegate<TEventArgs>(eventHandler, weakReference);
				EventDelegates.Add(weakEventDelegate);
				weakEventDelegate.Disposed += handleWeakEventDelegateDisposed;
				return weakEventDelegate;
			}
		}

		/// <summary>
		/// Removes an event handler.
		/// </summary>
		/// <param name="eventHandler">NOT null.</param>
		public void RemoveHandler(EventHandler<TEventArgs> eventHandler)
		{
			if (eventHandler == null)
				throw new ArgumentNullException(nameof(eventHandler));
			foreach (WeakEventDelegate<TEventArgs> weakEventDelegate in enumerateLiveDelegates()) {
				if (weakEventDelegate.TryGetTarget(out EventHandler<TEventArgs> handler)
						&& !eventHandler.Equals(handler)) {
					continue;
				}
				lock (EventDelegates) {
					EventDelegates.Remove(weakEventDelegate);
				}
				weakEventDelegate.Disposed -= handleWeakEventDelegateDisposed;
				weakEventDelegate.Dispose();
			}
		}

		/// <summary>
		/// This method will remove all weak event handlers that are no longer alive.
		/// </summary>
		public void Purge()
				// ReSharper disable once IteratorMethodResultIsIgnored
			=> enumerateLiveDelegates();

		/// <summary>
		/// This method will remove and dispose all weak event handlers now.
		/// </summary>
		public void RemoveAll()
		{
			foreach (WeakEventDelegate<TEventArgs> weakEventDelegate in enumerateLiveDelegates()) {
				weakEventDelegate.Dispose();
			}
		}

		/// <summary>
		/// This method will try to fetch all live event handlers,
		/// and invokes each handler with the arguments.
		/// If a non-null <see cref="SynchronizationContext"/> has been set, it is checked
		/// by each handler.
		/// </summary>
		/// <param name="sender">NOT tested here.</param>
		/// <param name="eventArgs">NOT tested here.</param>
		/// <param name="sendOrPostPolicy">Invoke selection. Defaults to
		/// <see cref="SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown"/>.</param>
		/// <returns>False if NO handlers are alive and the event is NOT invoked.
		/// True if the live handlers are all completed when this returns.
		/// Null if any live handler is Posted to the <see cref="SynchronizationContext"/>.</returns>
		public bool? TryInvoke(
				object sender,
				TEventArgs eventArgs,
				SendOrPostPolicy sendOrPostPolicy = SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown)
		{
			bool? result = false;
			foreach (WeakEventDelegate<TEventArgs> weakEventDelegate in enumerateLiveDelegates()) {
				switch (weakEventDelegate.TryInvoke(sender, eventArgs, sendOrPostPolicy)) {
					case null :
						result = null;
						break;
					case true :
						if (result == false)
							result = true;
						break;
				}
			}
			return result;
		}

		/// <summary>
		/// This method is provided to find a <see cref="WeakEventDelegate{TEventArgs}"/>
		/// that was added here by passing this <paramref name="weakReference"/>
		/// to <see cref="AddHandler"/> --- specifying to use this object as the
		/// weak reference to retain this <see cref="EventHandler{TEventArgs}"/>. If
		/// an explicit object was passed to the method as the object used to
		/// retain the weak reference here, then this returns true if this is the object.
		/// Note that if <see langword="null"/> was passed as the weak reference, then
		/// this has used the <see cref="EventHandler{TEventArgs}"/>
		/// <see cref="Delegate.Target"/> as the weak reference --- and so if that
		/// Target is passed to this method, this will return true. Note also
		/// that if the weak reference is no longer alive then this returns false.
		/// </summary>
		/// <param name="weakReference">Not null.</param>
		/// <param name="weakEventDelegate">The result if found.</param>
		/// <returns>True if this object WAS the object, or
		/// <see cref="Delegate.Target"/> passed to <see cref="AddHandler"/> to hold
		/// the weak reference here --- and this reference IS still alive.</returns>
		public bool TryGetKeyedDelegate(object weakReference, out WeakEventDelegate<TEventArgs> weakEventDelegate)
		{
			if (weakReference == null)
				throw new ArgumentNullException(nameof(weakReference));
			foreach (WeakEventDelegate<TEventArgs> liveDelegate in enumerateLiveDelegates()) {
				if (liveDelegate.IsKeyedByWeakReference(weakReference)) {
					weakEventDelegate = liveDelegate;
					return true;
				}
			}
			weakEventDelegate = null;
			return false;
		}
	}
}
