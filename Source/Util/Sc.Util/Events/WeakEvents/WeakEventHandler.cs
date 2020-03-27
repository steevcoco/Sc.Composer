using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Sc.Util.Collections;
using Sc.Util.Threading;


namespace Sc.Util.Events.WeakEvents
{
	/// <summary>
	/// Provides static helper methods for <see cref="WeakEventHandler{TEventArgs}"/>.
	/// </summary>
	public static class WeakEventHandler
	{
		/// <summary>
		/// Returns the default event window for <see cref="OnEventWindow{TEventArgs}"/>:
		/// 15 milliseconds.
		/// </summary>
		public static TimeSpan DefaultEventWindow
			=> TimeSpan.FromMilliseconds(15D);


		/// <summary>
		/// Creates and subscribes a new <see cref="WeakEventHandler{TEventArgs}"/>
		/// on your given event source object, for the named event.
		/// If the <c>weakReference</c> is given, it is used as a weak reference that will
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
		/// <c>eventHandler</c></param>
		/// <param name="eventBindingFlags">Optional flags that are used to locate the event on
		/// the <c>eventSource</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If the <c>eventSource</c> does not define an
		/// accessible <c>eventName</c> event; or if the handler's type is not compatible..</exception>
		public static WeakEventHandler<TEventArgs> Subscribe<TEventArgs>(
				object eventSource,
				string eventName,
				EventHandler<TEventArgs> eventHandler,
				object weakReference = null,
				BindingFlags eventBindingFlags
						= BindingFlags.Public
						| BindingFlags.Instance
						| BindingFlags.FlattenHierarchy)
			=> new WeakEventHandler<TEventArgs>(eventSource, eventName, eventHandler, weakReference, eventBindingFlags);


		/// <summary>
		/// Creates and subscribes a new <see cref="WeakEventHandler{TEventArgs}"/>
		/// for the <see cref="INotifyPropertyChanged"/> event
		/// on your given event source object.
		/// If the <c>weakReference</c> is given, it is used as a weak reference that will
		/// release both it and the <c>eventHandler</c> when it becomes released. Otherwise this will test
		/// the Target object of the <c>eventHandler</c>, and if not null, uses that. Note that if
		/// both are null then the weak reference used IS the <c>eventHandler</c> --- and the
		/// code that constructed that must hold a strong reference (the Target is null if the delegate
		/// is a static method; and a lambda as an event handler delegate may be immediately
		/// collected).
		/// </summary>
		/// <param name="eventSource">Not null. Will be weakly held here.</param>
		/// <param name="eventHandler">Not null. Will be weakly held here.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>eventHandler</c></param>
		/// <param name="eventBindingFlags">Optional flags that are used to locate the event on
		/// the <c>eventSource</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If the <c>eventSource</c> does not define an
		/// accessible <c>eventName</c> event; or if the handler's type is not compatible..</exception>
		public static WeakEventHandler<PropertyChangedEventArgs> ObserveWeak(
				this INotifyPropertyChanged eventSource,
				EventHandler<PropertyChangedEventArgs> eventHandler,
				object weakReference = null,
				BindingFlags eventBindingFlags
						= BindingFlags.Public
						| BindingFlags.Instance
						| BindingFlags.FlattenHierarchy)
			=> new WeakEventHandler<PropertyChangedEventArgs>(
					eventSource,
					nameof(INotifyPropertyChanged.PropertyChanged),
					eventHandler,
					weakReference,
					eventBindingFlags);

		/// <summary>
		/// Creates and subscribes a new <see cref="WeakEventHandler{TEventArgs}"/>
		/// for the <see cref="INotifyCollectionChanged"/> event
		/// on your given event source object.
		/// If the <c>weakReference</c> is given, it is used as a weak reference that will
		/// release both it and the <c>eventHandler</c> when it becomes released. Otherwise this will test
		/// the Target object of the <c>eventHandler</c>, and if not null, uses that. Note that if
		/// both are null then the weak reference used IS the <c>eventHandler</c> --- and the
		/// code that constructed that must hold a strong reference (the Target is null if the delegate
		/// is a static method; and a lambda as an event handler delegate may be immediately
		/// collected).
		/// </summary>
		/// <param name="eventSource">Not null. Will be weakly held here.</param>
		/// <param name="eventHandler">Not null. Will be weakly held here.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>eventHandler</c>.</param>
		/// <param name="eventBindingFlags">Optional flags that are used to locate the event on
		/// the <c>eventSource</c>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If the <c>eventSource</c> does not define an
		/// accessible <c>eventName</c> event; or if the handler's type is not compatible..</exception>
		public static WeakEventHandler<NotifyCollectionChangedEventArgs> ObserveWeak(
				this INotifyCollectionChanged eventSource,
				EventHandler<NotifyCollectionChangedEventArgs> eventHandler,
				object weakReference = null,
				BindingFlags eventBindingFlags
						= BindingFlags.Public
						| BindingFlags.Instance
						| BindingFlags.FlattenHierarchy)
			=> new WeakEventHandler<NotifyCollectionChangedEventArgs>(
					eventSource,
					nameof(INotifyCollectionChanged.CollectionChanged),
					eventHandler,
					weakReference,
					eventBindingFlags);


		/// <summary>
		/// This method will create a <see cref="WeakEventHandler"/> for this event
		/// source, and on the first event, starts a delayed <see cref="Task"/> that
		/// will invoke your Action once after the given window timeout. Any events
		/// raised within the window will not trigger the Action again until after
		/// the first timeout fires --- thereby coalescing all events within the window
		/// into a single Action. After the delegate is notified, then any
		/// further events again trigger the delayed <see cref="Task"/> that
		/// will fire once again after the window. Also, any events raised while the
		/// delegate is executing will cause the delayed <see cref="Task"/>
		/// to start again immediately when your delegate is complete.
		/// Dispose the returned object
		/// to unsubscribe the handler: your delegate is held weakly, and would
		/// also be released as defined by <see cref="WeakEventHandler"/>.
		/// If the <c>weakReference</c> is given, it is used as a weak reference that will
		/// release both it and your delegate when it becomes released. Otherwise this will test
		/// the Target object of the <c>delegate</c>, and if not null, uses that. Note that if
		/// both are null then the weak reference used IS the <c>delegate</c> --- and the
		/// code that constructed that must hold a strong reference (the Target is null if the delegate
		/// is a static method; and a lambda as a delegate may be immediately
		/// collected).
		/// </summary>
		/// <param name="eventSource">Required.</param>
		/// <param name="eventName">Required: the name of the event to observe.</param>
		/// <param name="onRaised">Required.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>onRaised</c> delegate.</param>
		/// <param name="eventFilter">Can provide an optional event filter: if the
		/// delegate returns false, then the event IS IGNORED. If this is null,
		/// then the default delegate always returns true.</param>
		/// <param name="window">If not null, must be at least one millisecond.
		/// If null, <see cref="DefaultEventWindow"/> is used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="InvalidOperationException">If the <c>eventSource</c> does not define an
		/// accessible <c>eventName</c> event; or if the handler's type is not compatible..</exception>
		public static WeakEventHandler<TEventArgs> OnEventWindow<TEventArgs>(
				object eventSource,
				string eventName,
				Action onRaised,
				object weakReference = null,
				Func<TEventArgs, bool> eventFilter = null,
				TimeSpan? window = null)
			=> WeakEventHandler.windowChanges(eventSource, eventName, eventFilter, onRaised, weakReference, window);


		/// <summary>
		/// Please see <see cref="OnEventWindow{TEventArgs}"/>: this method is a
		/// shortcut to create a "windowed" event handler specifically for an
		/// <see cref="INotifyPropertyChanged"/> event source.
		/// </summary>
		/// <param name="eventSource">Required.</param>
		/// <param name="onRaised">Required.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>onRaised</c> delegate.</param>
		/// <param name="window">If not null, must be at least one millisecond.
		/// If null, <see cref="DefaultEventWindow"/> is used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public static WeakEventHandler<PropertyChangedEventArgs> OnChangeWindow(
				this INotifyPropertyChanged eventSource,
				Action onRaised,
				object weakReference,
				TimeSpan? window = null)
			=> WeakEventHandler.windowChanges<PropertyChangedEventArgs>(
					eventSource,
					nameof(INotifyPropertyChanged.PropertyChanged),
					null,
					onRaised,
					weakReference,
					window);

		/// <summary>
		/// Please see <see cref="OnEventWindow{TEventArgs}"/>: this method is a
		/// shortcut to create a "windowed" event handler specifically for an
		/// <see cref="INotifyPropertyChanged"/> event source; and also takes
		/// a predicate to filter events that will will trigger the window.
		/// </summary>
		/// <param name="eventSource">Required.</param>
		/// <param name="propertyNames">Required list of property names that
		/// will only trigger the event action as documented --- if the
		/// raised event's property name is not in this list, then the
		/// event is ignored.</param>
		/// <param name="onRaised">Required.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>onRaised</c> delegate.</param>
		/// <param name="window">If not null, must be at least one millisecond.
		/// If null, <see cref="DefaultEventWindow"/> is used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public static WeakEventHandler<PropertyChangedEventArgs> OnChangeWindow(
				this INotifyPropertyChanged eventSource,
				IEnumerable<string> propertyNames,
				Action onRaised,
				object weakReference,
				TimeSpan? window = null)
		{
			if (propertyNames == null)
				throw new ArgumentNullException(nameof(propertyNames));
			string[] events = propertyNames.ToArray();
			if (events.Length == 0) {
				throw new ArgumentException(
						events.ToStringCollection()
								.ToString(),
						nameof(propertyNames));
			}
			return WeakEventHandler.windowChanges<PropertyChangedEventArgs>(
					eventSource,
					nameof(INotifyPropertyChanged.PropertyChanged),
					Predicate,
					onRaised,
					weakReference,
					window);
			bool Predicate(PropertyChangedEventArgs eventArgs)
				=> events.Any(propertyName => string.Equals(propertyName, eventArgs.PropertyName));
		}

		/// <summary>
		/// Please see <see cref="OnEventWindow{TEventArgs}"/>: this method is a
		/// shortcut to create a "windowed" event handler specifically for an
		/// <see cref="INotifyCollectionChanged"/> event source.
		/// </summary>
		/// <param name="eventSource">Required.</param>
		/// <param name="onRaised">Required.</param>
		/// <param name="weakReference">Optional object will determine the weak retention of the
		/// <c>onRaised</c> delegate.</param>
		/// <param name="window">If not null, must be at least one millisecond.
		/// If null, <see cref="DefaultEventWindow"/> is used.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public static WeakEventHandler<NotifyCollectionChangedEventArgs> OnChangeWindow(
				this INotifyCollectionChanged eventSource,
				Action onRaised,
				object weakReference,
				TimeSpan? window = null)
			=> WeakEventHandler.windowChanges<NotifyCollectionChangedEventArgs>(
					eventSource,
					nameof(INotifyCollectionChanged.CollectionChanged),
					null,
					onRaised,
					weakReference,
					window);


		[SuppressMessage("ReSharper", "AccessToModifiedClosure")]
		private static WeakEventHandler<TEventArgs> windowChanges<TEventArgs>(
				object eventSource,
				string eventName,
				Func<TEventArgs, bool> eventFilter,
				Action onRaised,
				object weakReference,
				TimeSpan? window)
		{
			if (eventSource == null)
				throw new ArgumentNullException(nameof(eventSource));
			if (string.IsNullOrWhiteSpace(eventName))
				throw new ArgumentNullException(nameof(eventName));
			if (onRaised == null)
				throw new ArgumentNullException(nameof(onRaised));
			if (!window.HasValue)
				window = WeakEventHandler.DefaultEventWindow;
			else if (window.Value.TotalMilliseconds < 1D)
				throw new ArgumentException(nameof(window), window.ToString());
			object syncLock = new object();
			bool isDelayRunning = false;
			bool isCallbackRunning = false;
			bool didEventRaiseWhileRunning = false;
			WeakEventHandler<TEventArgs> weakHandler = null;
			weakHandler = WeakEventHandler.Subscribe<TEventArgs>(
					eventSource,
					eventName,
					HandleEventSourceEvent,
					weakReference ?? onRaised.Target ?? onRaised);
			return weakHandler;
			void HandleEventSourceEvent(object sender, TEventArgs eventArgs)
			{
				lock (syncLock) {
					Debug.Assert(weakHandler != null, "weakHandler != null");
					if (!weakHandler.IsAlive
							|| !(eventFilter?.Invoke(eventArgs) ?? true)) {
						return;
					}
					if (isCallbackRunning) {
						didEventRaiseWhileRunning = true;
						return;
					}
					if (isDelayRunning)
						return;
					isDelayRunning = true;
					StartDelay(SynchronizationContext.Current);
					void StartDelay(SynchronizationContext synchronizationContext)
					{
						Task.Delay(window.Value)
								.ConfigureAwait(false)
								.GetAwaiter()
								.OnCompleted(ContinueAfterDelay);
						void ContinueAfterDelay()
						{
							lock (syncLock) {
								if (!weakHandler.IsAlive)
									return;
								isCallbackRunning = true;
							}
							try {
								if ((synchronizationContext == null)
										|| synchronizationContext.CheckInvoke()) {
									onRaised();
								} else {
									synchronizationContext.Send(onRaised);
								}
							} finally {
								lock (syncLock) {
									isCallbackRunning = false;
									if (didEventRaiseWhileRunning) {
										didEventRaiseWhileRunning = false;
										StartDelay(synchronizationContext);
									} else {
										isDelayRunning = false;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
