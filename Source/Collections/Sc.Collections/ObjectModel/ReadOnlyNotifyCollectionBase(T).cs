using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Sc.Util.Collections;
using Sc.Util.System;


namespace Sc.Collections.ObjectModel
{
	/// <summary>
	/// Serves as a base <see cref="INotifyCollectionChanged"/> implementation,
	/// which is readonly here, yet still provides the implementation for the
	/// event handlers. This class includes a protected
	/// class that can be subclassed to augment the event behaviors.
	/// This class requires a backing <see cref="IReadOnlyCollection{T}"/>
	/// to implement the collection interface. An instance of this class
	/// can be constructed to provide a readonly collection that still implements
	/// the notification interfaces for binding requirements.
	/// Note that this class declares separate types for the collection's
	/// element type, and the type that is raise on collection changed
	/// events --- this can provide support for e.g. dictionary implementations.
	/// Serializable.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	/// <typeparam name="TCollection">Underlying collection type.</typeparam>
	/// <typeparam name="TCollectionChangedValue">Is the type of the values raised in
	/// <see cref="NotifyCollectionChangedEventArgs"/> events.</typeparam>
	[DataContract]
	public class ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue>
			: IReadOnlyCollection<T>,
					INotifyCollectionChanged,
					INotifyPropertyChanged
			where TCollection : class, IReadOnlyCollection<T>
	{
		/// <summary>
		/// This class implements all events for the
		/// <see cref="ReadOnlyNotifyCollectionBase{T, TCollection, TCollectionChangedValue}"/>.
		/// A singleton instance is held by the parent collection.
		/// When the instance is disposed, events are raised. Subclasses
		/// can provide custom implementations, and if so, must
		/// override and implement <see cref="CopyStateFrom"/>,
		/// <see cref="Clear"/>, and <see cref="HandleDispose"/>
		/// (and the parent <see cref="NewEventHandler"/> method).
		/// Your collection implementaytion
		/// must invoke the methods here to raise all events.
		/// Collection implementations SHOULD invoke
		/// <see cref="RaiseSingleItemEvents"/>,
		/// <see cref="RaiseMultiItemEvents"/>,
		/// and <see cref="RaiseResetEvents"/> from their implementation where
		/// possible --- those methods raise both the Property Changed and
		/// Collection Changed events appropriate for the action. Other methods
		/// are provided to raise individual events, and can also be safely
		/// invoked at any time to raise that event. Please also see
		/// <see cref="CheckNextEvent"/> to possibly optimize your events.
		/// This provides support for "Suspending" events, which causes
		/// the class to cache all invoked events, and then raise them
		/// when the client disposes the returned object --- which is
		/// handled in this <see cref="HandleDispose"/> method: the parent
		/// collection provides the <see cref="SuspendEvents"/> method to
		/// support this behavior. When events are suspended, then if there is
		/// more than one CollectionChanged event, it will be coalesced
		/// into a single <see cref="NotifyCollectionChangedAction.Reset"/>
		/// event. This class also provides support for raising ONLY
		/// <see cref="NotifyCollectionChangedAction.Reset"/> events, by
		/// setting <see cref="RaiseOnlyResetEvents"/> true: please note
		/// that your implementation SHOULD STILL invoke the
		/// <see cref="RaiseSingleItemEvents"/>,
		/// <see cref="RaiseMultiItemEvents"/> methods to raise events:
		/// those methos will check the setting and raise or cache the
		/// appropriate event --- and that setting is mutable.
		/// </summary>
		protected class NotifyEventHandler
				: IDisposable
		{
			private readonly ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue> parent;
			private NotifyCollectionChangedEventArgs collectionChangedEventArgs;
			private bool raiseCount;
			private bool raiseIndexer;
			private List<string> raisePropertyChanged;


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="parent">Not null.</param>
			public NotifyEventHandler(ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue> parent)
				=> this.parent = parent ?? throw new ArgumentNullException(nameof(parent));


			/// <summary>
			/// This method is invoked by the parent when a new instance is constructed,
			/// and this must copy all current state to this instance from the argument.
			/// </summary>
			/// <param name="notifyEventHandler">Not null.</param>
			public virtual void CopyStateFrom(NotifyEventHandler notifyEventHandler)
			{
				if (notifyEventHandler == null)
					throw new ArgumentNullException(nameof(notifyEventHandler));
				collectionChangedEventArgs = notifyEventHandler.collectionChangedEventArgs;
				raiseCount = notifyEventHandler.raiseCount;
				raiseIndexer = notifyEventHandler.raiseIndexer;
				raisePropertyChanged = notifyEventHandler.raisePropertyChanged.IsNullOrEmpty()
						? null
						: new List<string>(notifyEventHandler.raisePropertyChanged);
				IsSuspended = notifyEventHandler.IsSuspended;
				IsRaiseAllEventsAsResetEvents = notifyEventHandler.IsRaiseAllEventsAsResetEvents;
			}


			/// <summary>
			/// Defaults to false: if set true, this object will only raise
			/// <see cref="NotifyCollectionChangedAction.Reset"/> events.
			/// The parent collection WILL serialize and restore this value.
			/// </summary>
			public bool IsRaiseAllEventsAsResetEvents
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				set;
			}


			/// <summary>
			/// Returns the current suspended state.
			/// </summary>
			public bool IsSuspended
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private set;
			}

			/// <summary>
			/// Sets the suspended state to true: this handler will now begin to
			/// cache all events. Invoke <see cref="IDisposable.Dispose"/> to
			/// raise all events and reset the suspended state.
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Suspend()
			{
				if (IsSuspended)
					throw new InvalidOperationException($"{GetType().GetFriendlyName()} events are already suspended.");
				IsSuspended = true;
			}


			/// <summary>
			/// This is an "optimization" method that is provided for implementations
			/// when raising collection changed events --- especially when raising
			/// multiple events. If this method returns TRUE then you MUST invoke
			/// your event. Please note that this method may raise an event now.
			/// The purpose is to check the current event state
			/// early and report whether a next posted event will in fact not
			/// be raised nor cached. If this returns false then you can skip your
			/// next event since state is cached and will not be changed by
			/// your event. This happens when events have been suspended
			/// and a Reset event HAS already been cached. In this case, further
			/// posted events will not be raised nor cached until the event
			/// handler is disposed and suspended state is cleared. Multiple raised
			/// events while suspended are colesced into the single Reset event,
			/// and if that IS cached now then your next event is a no-op.
			/// If this returns TRUE than your event MUST be invoked to
			/// raise or cache propert state. Please note that in addition,
			/// this method checks if there is a cached event and if that will
			/// be converted into a Reset event on the next event; AND also
			/// checks the same for <see cref="IsRaiseAllEventsAsResetEvents"/>;
			/// and if possible then a Reset event is properly coerced now
			/// and the method will return false. This is invoked in
			/// <see cref="RaiseSingleItemEvents"/>, 
			/// and <see cref="RaiseMultiItemEvents"/>, to check this state;
			/// and you CAN invoke this first to avoid having to pass the
			/// arguments to those methods. Please lastly note also that this
			/// method MAY raise an event now.
			/// </summary>
			public bool CheckNextEvent()
			{
				if (!IsSuspended
						|| (collectionChangedEventArgs == null)) {
					if (IsRaiseAllEventsAsResetEvents) {
						RaiseResetEvents();
						return false;
					}
					return true;
				}
				if (collectionChangedEventArgs.Action != NotifyCollectionChangedAction.Reset)
					RaiseResetEvents();
				return false;

			}


			/// <summary>
			/// Collection changed event method for the collection, which
			/// raises or caches <see cref="INotifyPropertyChanged.PropertyChanged"/> and
			/// <see cref="INotifyCollectionChanged.CollectionChanged"/> events for a single
			/// item change. If <see cref="RaiseOnlyResetEvents"/> is true, this instead
			/// invokes <see cref="RaiseResetEvents"/>.
			/// </summary>
			/// <param name="action">Required.</param>
			/// <param name="newItem">Required unless this is a Remove.</param>
			/// <param name="oldItem">Required if this is a Replace or Remove.</param>
			/// <param name="index">Required.</param>
			/// <param name="oldIndex">Required if this is a Move.</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RaiseSingleItemEvents(
					NotifyCollectionChangedAction action,
					TCollectionChangedValue newItem = default,
					TCollectionChangedValue oldItem = default,
					int index = -1,
					int oldIndex = -1)
			{
				if (!CheckNextEvent())
					return;
				switch (action) {
					case NotifyCollectionChangedAction.Add:
						RaiseCountChanged();
						if (index != (parent.Count - 1))
							RaiseIndexerChanged();
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, index));
						break;
					case NotifyCollectionChangedAction.Remove:
						RaiseCountChanged();
						if (index != parent.Count)
							RaiseIndexerChanged();
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, oldItem, index));
						break;
					case NotifyCollectionChangedAction.Replace:
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
						break;
					case NotifyCollectionChangedAction.Move:
						RaiseIndexerChanged();
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, index, oldIndex));
						break;
					default:
						throw new NotSupportedException(
								$"{nameof(ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue>)}"
								+ " does not support the"
								+ $" {action} event.");
				}
			}

			/// <summary>
			/// Collection changed event method for the collection, which
			/// raises or caches <see cref="INotifyPropertyChanged.PropertyChanged"/> and
			/// <see cref="INotifyCollectionChanged.CollectionChanged"/> events for a
			/// multiple-item change. If <see cref="RaiseOnlyResetEvents"/> is true, this instead
			/// invokes <see cref="RaiseResetEvents"/>.
			/// </summary>
			/// <param name="action">Required.</param>
			/// <param name="newItems">Required unless this is a Remove.</param>
			/// <param name="oldItems">Required if this is a Replace or Remove.</param>
			/// <param name="index">Required.</param>
			/// <param name="oldIndex">Required if this is a Move.</param>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RaiseMultiItemEvents(
					NotifyCollectionChangedAction action,
					IEnumerable<TCollectionChangedValue> newItems = default,
					IEnumerable<TCollectionChangedValue> oldItems = default,
					int index = -1,
					int oldIndex = -1)
			{
				if (!CheckNextEvent())
					return;
				IList NewItems()
					=> newItems as IList ?? newItems?.ToArray();
				IList OldItems()
					=> oldItems as IList ?? oldItems?.ToArray();
				switch (action) {
					case NotifyCollectionChangedAction.Add:
						RaiseCountChanged();
						if (index != (parent.Count - 1))
							RaiseIndexerChanged();
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, NewItems(), index));
						break;
					case NotifyCollectionChangedAction.Remove:
						RaiseCountChanged();
						if (index != parent.Count)
							RaiseIndexerChanged();
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, OldItems(), index));
						break;
					case NotifyCollectionChangedAction.Replace:
						RaiseCollectionChanged(
								new NotifyCollectionChangedEventArgs(action, NewItems(), OldItems(), index));
						break;
					case NotifyCollectionChangedAction.Move:
						RaiseIndexerChanged();
						RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(action, NewItems(), index, oldIndex));
						break;
					default:
						throw new NotSupportedException(
								$"{nameof(ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue>)}"
								+ " does not support the"
								+ $" {action} event.");
				}
			}

			/// <summary>
			/// Collection changed event method for the collection, which
			/// raises or caches <see cref="INotifyPropertyChanged.PropertyChanged"/> and
			/// <see cref="INotifyCollectionChanged.CollectionChanged"/> events for
			/// <see cref="NotifyCollectionChangedAction.Reset"/>.
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RaiseResetEvents()
			{
				RaiseCountChanged();
				RaiseIndexerChanged();
				RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}


			/// <summary>
			/// This is the event implementation method that is invoked to raise
			/// or cache ONLY an <see cref="INotifyCollectionChanged"/> event.
			/// This is invoked in <see cref="RaiseSingleItemEvents"/>,
			/// <see cref="RaiseMultiItemEvents"/>, and <see cref="RaiseResetEvents"/>.
			/// Please notice: if <see cref="RaiseOnlyResetEvents"/> is true,
			/// then this converts the event into a Reset event if needed
			/// --- and WILL raise the needed property changed events
			/// (as with <see cref="RaiseResetEvents"/>).
			/// This method checkes the state of <see cref="IsSuspended"/>,
			/// and caches the event if suspended.
			/// </summary>
			/// <param name="eventArgs">Not null.</param>
			/// <exception cref="ArgumentNullException"></exception>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
			{
				if (eventArgs == null)
					throw new ArgumentNullException(nameof(eventArgs));
				if (IsSuspended) {
					if (collectionChangedEventArgs == null) {
						if (IsRaiseAllEventsAsResetEvents) {
							collectionChangedEventArgs = eventArgs.Action == NotifyCollectionChangedAction.Reset
									? eventArgs
									: new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
							raiseCount = true;
							raiseIndexer = true;
						} else {
							collectionChangedEventArgs = eventArgs;
							if (collectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset) {
								raiseCount = true;
								raiseIndexer = true;
							}
						}
					} else if (collectionChangedEventArgs.Action != NotifyCollectionChangedAction.Reset) {
						collectionChangedEventArgs
								= eventArgs.Action == NotifyCollectionChangedAction.Reset
										? eventArgs
										: new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
						raiseCount = true;
						raiseIndexer = true;
					}
				} else if (IsRaiseAllEventsAsResetEvents) {
					RaiseCountChanged();
					RaiseIndexerChanged();
					parent.CollectionChanged?.Invoke(
							parent,
							eventArgs.Action == NotifyCollectionChangedAction.Reset
									? eventArgs
									: new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				} else
					parent.CollectionChanged?.Invoke(parent, eventArgs);
			}

			/// <summary> 
			/// This is the event implementation method that is invoked to raise
			/// or cache ONLY an <see cref="INotifyPropertyChanged"/> event
			/// for the Count.
			/// This is invoked in <see cref="RaiseSingleItemEvents"/>,
			/// <see cref="RaiseMultiItemEvents"/>, and <see cref="RaiseResetEvents"/>.
			/// This method checkes the state of <see cref="IsSuspended"/>,
			/// and caches the event if suspended.
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RaiseCountChanged()
			{
				if (IsSuspended)
					raiseCount = true;
				else {
					parent.PropertyChanged?.Invoke(
							parent,
							new PropertyChangedEventArgs(nameof(IReadOnlyCollection<T>.Count)));
				}
			}

			/// <summary>
			/// This is the event implementation method that is invoked to raise
			/// or cache ONLY an <see cref="INotifyPropertyChanged"/> event
			/// for the Indexer property ("Item[]").
			/// This is invoked in <see cref="RaiseSingleItemEvents"/>,
			/// <see cref="RaiseMultiItemEvents"/>, and <see cref="RaiseResetEvents"/>.
			/// This method checkes the state of <see cref="IsSuspended"/>,
			/// and caches the event if suspended.
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RaiseIndexerChanged()
			{
				if (IsSuspended)
					raiseIndexer = true;
				else {
					parent.PropertyChanged?.Invoke(
							parent,
							new PropertyChangedEventArgs(CollectionsHelper.CollectionIndexerItemPropertyName));
				}
			}

			/// <summary>
			/// This is an event implementation method that can be invoked to raise
			/// or cache ONLY an <see cref="INotifyPropertyChanged"/> event
			/// for some arbitrary property name. This implementation does not
			/// invoke this method (the Count and Indexer are raised with
			/// <see cref="RaiseCountChanged"/> and <see cref="RaiseIndexerChanged"/>,
			/// which are slightly more performant).
			/// This method checks the state of <see cref="IsSuspended"/>,
			/// and caches the event if suspended.
			/// </summary>
			/// <param name="propertyName">Not null.</param>
			/// <exception cref="ArgumentNullException"></exception>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
			{
				if (propertyName == null)
					throw new ArgumentNullException(nameof(propertyName));
				if (IsSuspended) {
					if (raisePropertyChanged == null) {
						raisePropertyChanged = new List<string>(1)
						{
							propertyName
						};
					} else if (!raisePropertyChanged.Contains(propertyName))
						raisePropertyChanged.Add(propertyName);
				} else
					parent.PropertyChanged?.Invoke(parent, new PropertyChangedEventArgs(propertyName));
			}


			/// <summary>
			/// This virtual method is provided to clear all cached events; and also
			/// sets <see cref="IsSuspended"/> to false.
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public virtual void Clear()
			{
				raiseCount = false;
				raiseIndexer = false;
				collectionChangedEventArgs = null;
				raisePropertyChanged = null;
				IsSuspended = false;
			}


			/// <summary>
			/// Sets <see cref="IsSuspended"/> to false, and raises and then clears
			/// all cached events. This method is virtual.
			/// </summary>
			protected virtual void HandleDispose()
			{
				IsSuspended = false;
				if (raiseCount) {
					parent.PropertyChanged?.Invoke(
							parent,
							new PropertyChangedEventArgs(nameof(IReadOnlyCollection<T>.Count)));
					raiseCount = false;
				}
				if (raiseIndexer) {
					parent.PropertyChanged?.Invoke(
							parent,
							new PropertyChangedEventArgs(CollectionsHelper.CollectionIndexerItemPropertyName));
					raiseIndexer = false;
				}
				if (collectionChangedEventArgs != null) {
					parent.CollectionChanged?.Invoke(parent, collectionChangedEventArgs);
					collectionChangedEventArgs = null;
				}
				if (!raisePropertyChanged.IsNullOrEmpty()) {
					PropertyChangedEventHandler propertyChanged = parent.PropertyChanged;
					if (propertyChanged != null) {
						foreach (string propertyName in raisePropertyChanged) {
							propertyChanged.Invoke(parent, new PropertyChangedEventArgs(propertyName));
						}
					}
					raisePropertyChanged = null;
				}
			}

			/// <summary>
			/// Sets <see cref="IsSuspended"/> to false, and raises and then clears
			/// all cached events.
			/// </summary>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
				try {
					parent.EventHandler = parent.NewEventHandler();
					HandleDispose();
				} finally {
					Clear();
				}
			}
		}


		[DataMember(Name = nameof(ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue>.Collection))]
		private TCollection collection;

		[DataMember]
		private bool wasRaiseAllEventsAsResetEvents;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="collection">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		protected ReadOnlyNotifyCollectionBase(TCollection collection)
		{
			this.collection = collection ?? throw new ArgumentNullException(nameof(collection));
			EventHandler = NewEventHandler();
		}


		[OnSerializing]
		private void onSerializing(StreamingContext _)
			=> wasRaiseAllEventsAsResetEvents = EventHandler.IsRaiseAllEventsAsResetEvents;

		[OnDeserialized]
		private void onDeserialized(StreamingContext _)
		{
			EventHandler = NewEventHandler();
			EventHandler.IsRaiseAllEventsAsResetEvents = wasRaiseAllEventsAsResetEvents;
		}


		/// <summary>
		/// This collection holds the actual values. This is serialized.
		/// Notice that this CAN be changed by the subclass: this setter will raise
		/// (or enqueue) a Reset event; and the value cannot be null.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		protected TCollection Collection
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => collection;
			set {
				collection = value
						?? throw new ArgumentNullException(
								nameof(ReadOnlyNotifyCollectionBase<T, TCollection, TCollectionChangedValue>
										.Collection));
				EventHandler.RaiseResetEvents();
			}
		}

		/// <summary>
		/// This object implements all events for this collection.
		/// You must invoke the methods here to raise all events.
		/// This object is a singleton instance; BUT, the collection
		/// makes copies when returning from <see cref="SuspendEvents"/>.
		/// This class can be extended for subclasses to augment the event
		/// behaviors: to provide a custom implementation, you must oiverride
		/// and implement <see cref="NewEventHandler"/>. Please notice also that
		/// this object itself is not serialized here: this collection
		/// will reset this to a new default instance when deserialized.
		/// This collection WILL serialize the state of
		/// <see cref="NotifyEventHandler.IsRaiseAllEventsAsResetEvents"/>, 
		/// and restore that on the new instance when deserialized.
		/// </summary>
		protected NotifyEventHandler EventHandler
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get;
			private set;
		}

		/// <summary>
		/// This method is virtual, and is responsible for constructing
		/// this collection's <see cref="NotifyEventHandler"/> instance(s).
		/// Please also notice that this method is invoked in this
		/// (base class) constructor: it must be ready to run at that time.
		/// </summary>
		/// <returns>Not null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual NotifyEventHandler NewEventHandler()
			=> new NotifyEventHandler(this);


		/// <summary>
		/// Defaults to false. If set true, then this collection will only raise
		/// <see cref="NotifyCollectionChangedAction.Reset"/> events
		/// --- even when an event would otherwise raise only a single-item
		/// change event.
		/// </summary>
		public bool IsRaiseAllEventsAsResetEvents
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => EventHandler.IsRaiseAllEventsAsResetEvents;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => EventHandler.IsRaiseAllEventsAsResetEvents = value;
		}

		/// <summary>
		/// This method provides support for suspending events.
		/// Notice that this method is ONLY safe for a single thread: if your thread invokes
		/// this method, and then makes mutations, you must ensure that no intervening
		/// code observes the changes that are now taking place on this collection. Invoking this
		/// will cause events to be cached; and if there is more than one notify event, it will
		/// be coalesced into a single <see cref="NotifyCollectionChangedAction.Reset"/> event.
		/// Make mutations, and then you MUST ensure that the return value is guaranteed to be
		/// disposed: this returns a disposable object that will clear the suspended state
		/// and raise the events when disposed --- use this method in a <see langword="using"/>
		/// block around your mutations.
		/// </summary>
		/// <returns>Not null: MUST be disposed to clear the suspended state and raise events.</returns>
		/// <exception cref="InvalidOperationException">Events are already suspended.</exception>
		public IDisposable SuspendEvents()
		{
			EventHandler.Suspend();
			return EventHandler;
		}


		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Collection.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator()
			=> Collection.GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// This method sets <see cref="CollectionChanged"/> and <see cref="PropertyChanged"/>
		/// null, and also clears any suspended events.
		/// </summary>
		public void ClearEventHandlers()
		{
			CollectionChanged = null;
			PropertyChanged = null;
			EventHandler.Clear();
		}


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}{this.ToStringCollection()}";
	}
}
