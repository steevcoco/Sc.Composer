using System;
using System.Collections.Generic;
using System.Threading;


namespace Sc.Util.Threading
{
	/// <summary>
	/// Implements a simple <see cref="ThreadLocal{T}"/> that also implements a
	/// predicate by Thread for each candidate Thread local value. When the
	/// <see cref="ThreadLocal{T}"/> factory must be invoked to create a value
	/// for some Thread, then first your <see cref="FactoryThreadPredicate"/>
	/// is invoked: if that returns null, then no value is created for that
	/// Thread. The factory (and predicate) is invoked on the given Thread.
	/// </summary>
	/// <typeparam name="TValue">The Thread local value type.</typeparam>
	public class ThreadLocalPredicate<TValue>
			where TValue : class
	{
		private readonly ThreadLocal<TValue> threadLocalCaches;
		private readonly Func<TValue> factory;
		private readonly Func<bool> factoryThreadPredicate;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="factory">Required: this will implement the
		/// factory for each new <typeparamref name="TValue"/> instance
		/// on each <see cref="Thread"/>.</param>
		/// <param name="factoryThreadPredicate">Optional: will implement
		/// <see cref="FactoryThreadPredicate"/> if provided. Otherwise you
		/// may override that method --- and if neither is provided, then
		/// there is NO predicate and all Threads will create a value.</param>
		/// <param name="trackAllValues">Allows passing the value to
		/// the underlying <see cref="ThreadLocal{T}"/> --- if true, then
		/// the ThreadLocal tracks all values; and you may fetch them
		/// from <see cref="TryGetAllValues(out IList{TValue})"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ThreadLocalPredicate(
				Func<TValue> factory,
				Func<bool> factoryThreadPredicate = null,
				bool trackAllValues = false)
				: this(trackAllValues)
		{
			this.factory = factory;
			this.factoryThreadPredicate = factoryThreadPredicate;
		}

		/// <summary>
		/// Protected constructor ONLY for a subclass that MUST at least
		/// override and implement <see cref="Factory"/>. Notice that if
		/// you do not override <see cref="FactoryThreadPredicate"/>,
		/// then there is NO predicate and all Threads will create a value.
		/// </summary>
		/// <param name="trackAllValues">Allows passing the value to
		/// the underlying <see cref="ThreadLocal{T}"/> --- if true, then
		/// the ThreadLocal tracks all values; and you may fetch them
		/// from <see cref="TryGetAllValues(out IList{TValue})"/>.</param>
		protected ThreadLocalPredicate(bool trackAllValues = false)
			=> threadLocalCaches = new ThreadLocal<TValue>(threadLocalFactory, trackAllValues);


		private TValue threadLocalFactory()
			=> FactoryThreadPredicate()
					? Factory()
					: null;


		/// <summary>
		/// Implements the factory for all new values. This method
		/// will invoke a delegate that was provided on construction,
		/// and return that result. And if that is null this returns null
		/// --- you must either provide a delegate or override this method.
		/// Note that THIS method does NOT check the
		/// <see cref="FactoryThreadPredicate"/> --- the
		/// actual underlying factory method first invokes that itself;
		/// and this method is only invoked if that HAS returned true.
		/// </summary>
		/// <returns>A new instance for this Thread.</returns>
		protected virtual TValue Factory()
			=> factory?.Invoke();

		/// <summary>
		/// This method provides a filter for the <see cref="Factory"/>
		/// method. This is invoked each time the factory is requested
		/// for a new <typeparamref name="TValue"/>, ON the
		/// requesting <see cref="Thread"/>. If this returns false,
		/// then the value is not constructed --- and the factory
		/// will return null for this Thread. This implementation
		/// will invoke a delegate provided on construction; or
		/// otherwise always returns true.
		/// </summary>
		/// <returns>False to instruct the factory to return null.</returns>
		protected virtual bool FactoryThreadPredicate()
			=> factoryThreadPredicate?.Invoke() ?? true;


		/// <summary>
		/// Returns the cached instance for THIS <see cref="Thread"/>; OR,
		/// null if no value is created for this Thread,
		/// and the factory does not construct
		/// a value for this Thread now.
		/// </summary>
		/// <param name="value">The result.</param>
		/// <returns>True if there is a current value for this
		/// Thread, or if one is created now.</returns>
		public bool TryGet(out TValue value)
		{
			value = threadLocalCaches.Value;
			if (value == null)
				threadLocalCaches.Value = value = threadLocalFactory();
			return value != null;
		}

		/// <summary>
		/// Removes the cache for THIS <see cref="Thread"/>.
		/// </summary>
		public void Remove()
			=> threadLocalCaches.Value = null;

		/// <summary>
		/// This method tries to get all <see cref="ThreadLocal{T}.Values"/>;
		/// which are only tracked if the setting was specified on construction.
		/// </summary>
		/// <param name="values">May be null or empty.</param>
		/// <returns>This method returns true ONLY if the <paramref name="values"/>
		/// is both not null AND not empty.</returns>
		public bool TryGetAllValues(out IList<TValue> values)
		{
			try {
				values = threadLocalCaches.Values;
			} catch {
				// Ignored.
				values = null;
			}
			return (values != null) && (values.Count != 0);
		}
	}
}
