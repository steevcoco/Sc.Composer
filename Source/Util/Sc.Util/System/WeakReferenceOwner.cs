using System;
using System.Runtime.CompilerServices;
using Sc.Abstractions.Lifecycle;


namespace Sc.Util.System
{
	/// <summary>
	/// Implements a weak reference to a given target, that can be keyed
	/// weakly by another object. If a "weak owner"
	/// is given, it is used as a weak reference that will
	/// release both it and the given target
	/// when it becomes released. This class implements <see cref="IRaiseDisposed"/>:
	/// the event will be raised only one time, and is EITHER raised
	/// when this object is explicitly disposed, OR will also be raised
	/// if an attempt to fetch the target fails. This is provided for
	/// weak caches to remove the instance more eagerly when collected.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class WeakReferenceOwner<T>
			: IDispose
			where T : class
	{
		private readonly WeakReference<T> weakTarget = new WeakReference<T>(null);
		private volatile ConditionalWeakTable<object, T> weakOwner = new ConditionalWeakTable<object, T>();


		/// <summary>
		/// Constructor. If the <paramref name="weakOwner"/>
		/// is given, it is used as a weak reference that will
		/// release both it and the <paramref name="target"/>
		/// when it becomes released. Note that if that is
		/// null then the weak reference used IS the
		/// <paramref name="target"/> --- and the
		/// code that constructed that must hold a strong reference.
		/// </summary>
		/// <param name="target">Not null. Will be weakly held here.</param>
		/// <param name="weakOwner">Optional object will determine the weak retention of the
		/// <paramref name="target"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public WeakReferenceOwner(T target, object weakOwner = null)
			=> Initialize(target, weakOwner);

		/// <summary>
		/// This protected constructor is provided for subclasses: this instance will
		/// have no target when this constructor completes: you MUST immediately
		/// initialize this instance by invoking <see cref="Initialize"/>.
		/// </summary>
		protected WeakReferenceOwner() { }


		/// <summary>
		/// This protected initialization method is provided to support the protected
		/// default constructor.
		/// </summary>
		/// <param name="target">Not null. Will be weakly held here.</param>
		/// <param name="weakOwner">Optional object will determine the weak retention of the
		/// <paramref name="target"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException">If this has already been initialized.</exception>
		protected void Initialize(T target, object weakOwner = null)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (IsDisposed)
				throw new ObjectDisposedException(ToString());
			if (weakOwner == null)
				weakOwner = target;
			if (weakTarget.TryGetTarget(out _)
					|| (this.weakOwner?.TryGetValue(weakOwner, out _) ?? false)) {
				throw new InvalidOperationException("Instance has already been initialized.");
			}
			weakTarget.SetTarget(target);
			this.weakOwner.Add(weakOwner, target);
		}


		/// <summary>
		/// This property is provided to determine if this instance is no longer alive: this method will
		/// try to fetch the weakly-held target: if it fails, the method returns
		/// false. If false, the object cannot be resurrected.
		/// Please notice: getting this property MAY RAISE this <see cref="Disposed"/>
		/// event now.
		/// </summary>
		/// <returns>True if the reference reports that it is alive now. False if it is no longer
		/// alive.</returns>
		public virtual bool IsAlive
			=> TryGetTarget(out _);

		/// <summary>
		/// This method is provided to try to fetch the weakly-held target here.
		/// This will return false if the reference is no longer alive.
		/// Please notice: invoking this method MAY RAISE this <see cref="Disposed"/>
		/// event now.
		/// </summary>
		/// <param name="target">Not null if the method returns true.</param>
		/// <returns>True if the reference is still alive here.</returns>
		public bool TryGetTarget(out T target)
		{
			if (weakTarget.TryGetTarget(out target))
				return true;
			Dispose();
			return false;
		}

		/// <summary>
		/// This method is provided to identify a <see cref="WeakReferenceOwner{T}"/>
		/// that was constructed (or initialized) by passing this <paramref name="weakOwner"/>
		/// to the constructor (or initializer) --- specifying to use this object as the
		/// weak reference to retain this target. If
		/// an explicit object was passed to this instance as the object used to
		/// retain the weak reference here, then this returns true if this is the object.
		/// Note that if <see langword="null"/> was passed as the weak reference, then
		/// this has used the target as the weak reference --- and so if that
		/// Target is passed to this method, this will return true. Note also
		/// that if the weak reference is no longer alive then this returns false.
		/// Please notice: invoking this method MAY RAISE this <see cref="Disposed"/>
		/// event now.
		/// </summary>
		/// <param name="weakOwner">Not null.</param>
		/// <returns>True if this object WAS the object, or
		/// <see cref="Delegate.Target"/> passed to this constructor to hold
		/// the weak reference here --- and this reference IS still alive.</returns>
		public bool IsKeyedByWeakReference(object weakOwner)
		{
			if (weakOwner == null)
				throw new ArgumentNullException(nameof(weakOwner));
			if (this.weakOwner?.TryGetValue(weakOwner, out _) ?? false)
				return true;
			Dispose();
			return false;
		}


		public bool IsDisposed
			=> weakOwner == null;

		public event EventHandler Disposed;

		protected virtual void Dispose(bool isDisposing)
		{
			if (!isDisposing)
				return;
			weakOwner = null;
			weakTarget.SetTarget(default);
			EventHandler disposed = Disposed;
			Disposed = null;
			disposed?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
