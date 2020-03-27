using System;
using System.Collections;
using System.Threading;


namespace Sc.Util.Collections
{
	/// <summary>
	/// Simple wrapper around an <see cref="ICollection"/> that synchronizes mutations, and allows
	/// lock-free reads. Mutating the collection creates a new instance. You may access the actual
	/// Collection and read from it, but never mutate it: get the <see cref="Collection"/> property
	/// to read the Collection. You can safely expose that object as a readonly view at all times,
	/// but note that the reference WILL change.
	/// Fetching the reference fetches the state at that moment. To mutate the Collection,
	/// invoke <see cref="Mutate"/> or <see cref="Mutate{TResult}"/>. You will be passed a
	/// newly-created copied Collection instance with all current elements; which you can mutate
	/// arbitrarily. Notice that the <see cref="ThrowOnRecursiveMutation"/> property defaults to
	/// true, which will raise <see cref="InvalidOperationException"/> if a mutate method is invoked
	/// recursively; and the default is true since if set false, mutations can be lost. Note also
	/// that reads are not atomic, and so if a Thread updates while you inspect the list, then your
	/// next read may read from a different collection. To perform an atomic read operation,
	/// you can use a <see cref="ReadAtomic"/> method.
	/// </summary>
	/// <typeparam name="TCollection">The Collection type.</typeparam>
	public class ReadWriteCollection<TCollection>
	{
		private readonly object syncLock = new object();
		private TCollection collection;
		private bool throwOnRecursiveMutation;


		/// <summary>
		/// Constructor. You may also construct instances more easily with static methods on
		/// <see cref="ReadWriteCollectionHelper"/>.
		/// </summary>
		/// <param name="initialValue">Will set the current value of the <see cref="Collection"/>
		/// now. Not null.</param>
		/// <param name="copyCollection">Required: this <c>Func</c> will be passed the current
		/// Collection value; and it must construct a new shallow clone of the Collection: a new
		/// instance containing all elements from the argument.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ReadWriteCollection(TCollection initialValue, Func<TCollection, TCollection> copyCollection)
		{
			CopyCollection = copyCollection ?? throw new ArgumentNullException(nameof(copyCollection));
			if (initialValue == null)
				throw new ArgumentNullException(nameof(initialValue));
			collection = initialValue;
		}


		/// <summary>
		/// Is the actual current Collection of elements. You may read from this Collection;
		/// but you must not mutate it. It will never be mutated here.
		/// </summary>
		public TCollection Collection
		{
			get {
				Thread.MemoryBarrier();
				return collection;
			}
		}

		/// <summary>
		/// Provides access to the <c>copyCollection</c> Func specified on construction --- or the
		/// default instance that has been created. This delegate is responsible for re-creating
		/// the <see cref="Collection"/> on every mutation: it receives the current collection and
		/// creates a copy with all elements from that collection.
		/// </summary>
		public Func<TCollection, TCollection> CopyCollection { get; }

		/// <summary>
		/// This optional property defaults to true, and can be set false. By default the Mutate methods
		/// will raise <see cref="InvalidOperationException"/> if they are invoked recursively. This is
		/// the default since otherwise those recursive mutations would be lost.
		/// </summary>
		public bool ThrowOnRecursiveMutation
		{
			get {
				Thread.MemoryBarrier();
				return throwOnRecursiveMutation;
			}
			set {
				throwOnRecursiveMutation = value;
				Thread.MemoryBarrier();
			}
		}


		/// <summary>
		/// This virtual method is invoked in <see cref="Mutate{TResult}"/>
		/// and <see cref="Mutate(Action{TCollection})"/>, still under the writer mutex,
		/// and before the new collection has been set. The argument now contains the
		/// mutated new collection, after the user's function has run on it.
		/// You can mutate the argument.
		/// </summary>
		/// <param name="newCollection">Not <see langword="null"/>.</param>
		protected virtual void AfterMutate(TCollection newCollection) { }


		/// <summary>
		/// Invoke to mutate the <see cref="Collection"/>. This method allows your
		/// <see cref="Func{TIn,TResult}"/> to return a value back from this method.
		/// </summary>
		/// <typeparam name="TResult">Your own result type.</typeparam>
		/// <param name="mutate">Not null. This will be passed a copy of the current <see cref="Collection"/>
		/// value: you mutate the argument as desired, and that collection then becomes this
		/// new <see cref="Collection"/>. The result from the Func is defined by your own invoker:
		/// you return any arbitrary value that you may consume yourself.</param>
		/// <returns>The result of your own <see cref="Func{TIn,TResult}"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If <see cref="ThrowOnRecursiveMutation"/>
		/// is true and this is invoked recursively.</exception>
		public TResult Mutate<TResult>(Func<TCollection, TResult> mutate)
		{
			if (mutate == null)
				throw new ArgumentNullException(nameof(mutate));
			bool wasEntered = Monitor.IsEntered(syncLock);
			lock (syncLock) {
				if (wasEntered
						&& throwOnRecursiveMutation) {
					throw new InvalidOperationException("Collection cannot be mutated recursively.");
				}
				TCollection newCollection = CopyCollection(Collection);
				TResult result = mutate(newCollection);
				AfterMutate(newCollection);
				collection = newCollection;
				return result;
			}
		}

		/// <summary>
		/// Please see <see cref="Mutate(Action{TCollection})"/>: this method also
		/// performs a mutation under the monitor, but this will not invoke your
		/// action if the monitor is currently held --- regardless of the setting
		/// of <see cref="ThrowOnRecursiveMutation"/>.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <typeparam name="TResult">Your own result type.</typeparam>
		/// <param name="mutate">Not null. This will be passed a copy of the current <see cref="Collection"/>
		/// value: you mutate the argument as desired, and that collection then becomes this
		/// new <see cref="Collection"/>. The result from the Func is defined by your own invoker:
		/// you return any arbitrary value that you may consume yourself.</param>
		/// <param name="result">The result of your own <see cref="Func{TIn,TResult}"/>.</param>
		/// <returns>True if your delegate was invoked.</returns>
		public bool TryMutate<TResult>(Func<TCollection, TResult> mutate, out TResult result)
		{
			if (mutate == null)
				throw new ArgumentNullException(nameof(mutate));
			bool wasEntered = Monitor.IsEntered(syncLock);
			lock (syncLock) {
				if (wasEntered) {
					result = default;
					return false;
				}
				TCollection newCollection = CopyCollection(Collection);
				result = mutate(newCollection);
				AfterMutate(newCollection);
				collection = newCollection;
				return true;
			}
		}

		/// <summary>
		/// Invoke to mutate the <see cref="Collection"/>.
		/// </summary>
		/// <param name="mutate">Not null. This will be passed a copy of the current <see cref="Collection"/>
		/// value: you mutate the argument as desired, and that collection then becomes this
		/// new <see cref="Collection"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If <see cref="ThrowOnRecursiveMutation"/>
		/// is true and this is invoked recursively.</exception>
		public void Mutate(Action<TCollection> mutate)
		{
			if (mutate == null)
				throw new ArgumentNullException(nameof(mutate));
			bool wasEntered = Monitor.IsEntered(syncLock);
			lock (syncLock) {
				if (wasEntered
						&& throwOnRecursiveMutation) {
					throw new InvalidOperationException("Collection cannot be mutated recursively.");
				}
				TCollection newCollection = CopyCollection(Collection);
				mutate(newCollection);
				AfterMutate(newCollection);
				collection = newCollection;
			}
		}

		/// <summary>
		/// Invoke to perform an atomic read on the <see cref="Collection"/>.
		/// This method allows your
		/// <see cref="Func{TIn,TResult}"/> to return a value back from this method.
		/// </summary>
		/// <typeparam name="TResult">Your own result type.</typeparam>
		/// <param name="read">Not null. This will be passed the current
		/// <see cref="Collection"/> value. The result from the Func is defined by your own invoker:
		/// you return any arbitrary value that you may consume yourself.</param>
		/// <returns>The result of your own <see cref="Func{TIn,TResult}"/>.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public TResult ReadAtomic<TResult>(Func<TCollection, TResult> read)
		{
			if (read == null)
				throw new ArgumentNullException(nameof(read));
			lock (syncLock) {
				return read(collection);
			}
		}

		/// <summary>
		/// Invoke to perform an atomic read on the <see cref="Collection"/>.
		/// </summary>
		/// <param name="read">Not null. This will be passed the current
		/// <see cref="Collection"/> value.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void ReadAtomic(Action<TCollection> read)
		{
			if (read == null)
				throw new ArgumentNullException(nameof(read));
			lock (syncLock) {
				read(collection);
			}
		}
	}
}
