using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Sc.Abstractions.Threading;
using Sc.Util.System;
using Sc.Util.Threading;


namespace Sc.Util.Events.WeakEvents
{
	/// <summary>
	/// Implements a weak <see cref="EventHandler"/>. This object holds a weak reference to your given
	/// <see cref="EventHandler{T}"/>; and that handler is kept alive by a weak reference
	/// to a target that you may specify: by default it is the handler's Target object.
	/// This then adds itself as a listener on the event source. The source will
	/// hold a strong reference to this object; and this will invoke your handler when the source event
	/// is raised. Dispose this object to remove the handler and all references. This class can be used
	/// by an event subscriber when the source does not provide some weak registration --- i.e. for any
	/// normal event subscribed with "<c>+-</c>" syntax. The event source may wind up with a "leaked"
	/// reference to this object, but not to your Target or your event handler.
	/// This class also implements <see cref="ISynchronizationContextAware"/>,
	/// and if a non-null <see cref="SynchronizationContext"/> is set, the handler will be
	/// invoked by Posting or optionally Sending to that context.
	/// </summary>
	public class WeakEventHandler<TEventArgs>
			: WeakEventDelegate<TEventArgs>
	{
		private string eventName;
		private readonly WeakReference<object> eventSourceWeak = new WeakReference<object>(null);
		private Delegate thisEventHandlerDelegate;
		private volatile SendOrPostPolicy sendOrPostPolicy = SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown;


		/// <summary>
		/// Constructor. If the <c>weakReference</c> is given, it is used as a weak reference that will
		/// release both it and the <c>eventHandler</c> when it becomes released. Otherwise this will test
		/// the Target object of the <c>eventHandler</c>, and if not null, uses that. Note that if
		/// both are null then the weak reference used IS the <c>eventHandler</c> --- and the
		/// code that constructed that must hold a strong reference (the Target is null if the delegate
		/// is a static method; and a lambda as an event handler delegate may be immediately
		/// collected).
		/// </summary>
		/// <param name="eventSource">Not null. Will be weakly held here.</param>
		/// <param name="eventName">Not null.</param>
		/// <param name="eventHandler">Not null. Will be weakly held here.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>eventHandler</c>.</param>
		/// <param name="eventBindingFlags">Optional flags that are used to locate the event on
		/// the <c>eventSource</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If the <c>eventSource</c> does not define an
		/// accessible <c>eventName</c> event; or if the handler's type is not compatible..</exception>
		public WeakEventHandler(
				object eventSource,
				string eventName,
				EventHandler<TEventArgs> eventHandler,
				object weakReference = null,
				BindingFlags eventBindingFlags
						= BindingFlags.Public
						| BindingFlags.NonPublic
						| BindingFlags.Instance
						| BindingFlags.Static
						| BindingFlags.FlattenHierarchy)
			=> Initialize(eventSource, eventName, eventHandler, weakReference, eventBindingFlags);

		/// <summary>
		/// This protected constructor is provided for subclasses: this instance will
		/// have no handler when this constructor completes: you MUST immediately
		/// initialize this instance by invoking <see cref="Initialize"/>; and please
		/// note that you must invoke the initialize method defined here on
		/// <see cref="WeakEventHandler"/> --- with five arguments.
		/// </summary>
		protected WeakEventHandler() { }


		/// <summary>
		/// This protected initialization method is provided to support the protected
		/// default constructor.
		/// </summary>
		/// <param name="eventSource">Not null. Will be weakly held here.</param>
		/// <param name="eventName">Not null.</param>
		/// <param name="eventHandler">Not null. Will be weakly held here.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>eventHandler</c>.</param>
		/// <param name="eventBindingFlags">Optional flags that are used to locate the event on
		/// the <c>eventSource</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If the <c>eventSource</c> does not define an
		/// accessible <c>eventName</c> event; or if the handler's type is not compatible;
		/// or iff this has already been initialized...</exception>
		protected void Initialize(
				object eventSource,
				// ReSharper disable once ParameterHidesMember
				string eventName,
				EventHandler<TEventArgs> eventHandler,
				object weakReference = null,
				BindingFlags eventBindingFlags
						= BindingFlags.Public
						| BindingFlags.NonPublic
						| BindingFlags.Instance
						| BindingFlags.Static
						| BindingFlags.FlattenHierarchy)
		{
			if (eventSource == null)
				throw new ArgumentNullException(nameof(eventSource));
			this.eventName = string.IsNullOrWhiteSpace(eventName)
					? throw new ArgumentNullException(nameof(eventName))
					: eventName;
			Initialize(eventHandler, weakReference);
			Type observedType = eventSource.GetType();
			EventInfo eventInfo = observedType.GetEvent(eventName, eventBindingFlags);
			if (eventInfo == null) {
				throw new InvalidOperationException(
						$"Observed object {observedType.GetFriendlyFullName()} does not define accessible event"
						+ $" '{eventName}'.");
			}
			try {
				MethodInfo handleEventMethod
						= typeof(WeakEventHandler<TEventArgs>).GetMethod(
								nameof(WeakEventHandler<TEventArgs>.handleEvent),
								BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
				thisEventHandlerDelegate
						= Delegate.CreateDelegate(
								eventInfo.EventHandlerType,
								this,
								// ReSharper disable once AssignNullToNotNullAttribute
								handleEventMethod);
				if (thisEventHandlerDelegate == null)
					throw new NotSupportedException($"Could not create local event delegate {handleEventMethod}.");
				eventInfo.GetAddMethod(true)
						.Invoke(eventSource, new object[] { thisEventHandlerDelegate });
				eventSourceWeak.SetTarget(eventSource);
			} catch (Exception exception) {
				eventSourceWeak.SetTarget(null);
				thisEventHandlerDelegate = null;
				throw new InvalidOperationException(
						$"Cannot bind to event '{eventName}'"
						+ $" on object to observe {observedType.GetFriendlyFullName()}.",
						exception);
			}
		}


		private void handleEvent(object sender, TEventArgs eventArgs)
			=> TryInvoke(sender, eventArgs, sendOrPostPolicy);


		protected override void OnInvokeHandler(EventHandler<TEventArgs> handler, object sender, TEventArgs eventArgs)
		{
			try {
				handler(sender, eventArgs);
			} catch (TargetInvocationException targetInvocationException)
					when (targetInvocationException.InnerException != null) {
				throw targetInvocationException.InnerException;
			}
		}


		/// <summary>
		/// Defaults to <see cref="Threading.SendOrPostPolicy.InvokeSafePostSafeOrInvokeUnknown"/>.
		/// Provides a value that is used to invoke the handler when the event is raised:
		/// this is passed to <see cref="WeakEventDelegate{TEventArgs}.TryInvoke"/>.
		/// if a non-null <see cref="SynchronizationContext"/> is set, it is checked,
		/// and this will Post, Send, or Invoke the handler, checking the current
		/// <see cref="SynchronizationContext"/> and this <see cref="Threading.SendOrPostPolicy"/>.
		/// </summary>
		public SendOrPostPolicy SendOrPostPolicy
		{
			get => sendOrPostPolicy;
			set => sendOrPostPolicy = value;
		}

		/// <summary>
		/// This method is provided to try to fetch the weakly-held event source here.
		/// </summary>
		/// <param name="eventSource">Not null if the method returns true.</param>
		/// <returns>True if the reference is still alive here.</returns>
		public bool TryGetEventSource(out object eventSource)
		{
			if (eventSourceWeak.TryGetTarget(out eventSource))
				return true;
			Dispose();
			return false;
		}

		public override bool IsAlive
		{
			get {
				if (!base.IsAlive)
					return false;
				if (eventSourceWeak.TryGetTarget(out _))
					return true;
				Dispose();
				return false;
			}
		}


		protected override void Dispose(bool isDisposing)
		{
			if (!isDisposing) {
				base.Dispose(false);
				return;
			}
			try {
				Delegate thisHandler = thisEventHandlerDelegate;
				thisEventHandlerDelegate = null;
				if ((thisHandler != null)
						&& eventSourceWeak.TryGetTarget(out object eventSource)) {
					eventSource.GetType()
							.GetEvent(eventName)
							?.RemoveEventHandler(eventSource, thisHandler);
				}
			} catch (Exception exception) {
				Trace.TraceError("Catching exception removing event handler: {0}.", exception.Message);
				Trace.WriteLine(exception);
			} finally {
				eventSourceWeak.SetTarget(default);
				base.Dispose(true);
			}
		}
	}
}
