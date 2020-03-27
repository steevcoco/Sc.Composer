using System;
using SystemWeakReference = System.WeakReference;


namespace Sc.Util.System
{
	/// <summary>
	/// Base non-generic class for <see cref="ConvertibleWeakReference{T}"/>. Wraps a
	/// <see cref="WeakReference{T}"/>, and allows holding a strong reference.
	/// Thread safe.
	/// </summary>
	public abstract class ConvertibleWeakReference
	{
		/// <summary>
		/// Returns the object that is used to synchronize all operations
		/// for this instance: you may coordinate atomic operations by locking
		/// this object; and you must be sure to avoid deadlocks.
		/// </summary>
		public abstract object SyncLock { get; }

		/// <summary>
		/// Returns true if the weak reference is not collected.
		/// </summary>
		public abstract bool IsAlive { get; }

		/// <summary>
		/// Returns the target only if it is currently alive.
		/// </summary>
		/// <param name="target">The target if alive now.</param>
		/// <param name="convertStrongReference">Applies only if the target is alive.
		/// If null --- the default --- then the strong/weak reference state is unchanged
		/// (and no further action is taken).  If true, then this will now invoke
		/// <see cref="TryHoldStrongReference"/>; and if false, this will invoke
		/// <see cref="ReleaseStrongReference"/>.</param>
		/// <returns><see langword="true"/>if alive now.</returns>
		public abstract bool TryGetTarget(out object target, bool? convertStrongReference = null);

		/// <summary>
		/// Returns true if <see cref="TryHoldStrongReference"/> has been invoked;
		/// and the target is alive.
		/// </summary>
		public abstract bool IsHoldingStrongReference { get; }

		/// <summary>
		/// This method will fetch the target if it is alive now, and will set a
		/// private field to the value --- converting the reference from weak to strong.
		/// </summary>
		/// <returns>FALSE if the target is not alive.</returns>
		public abstract bool TryHoldStrongReference();

		/// <summary>
		/// This method will release any strong reference to the target --- converting
		/// the reference from strong to weak.
		/// </summary>
		/// <returns>FALSE if the target is not alive.</returns>
		public abstract bool ReleaseStrongReference();

		/// <summary>
		/// When <see cref="TryHoldStrongReference"/> is invoked, and the target is alive,
		/// this is set to the Utc time.
		/// </summary>
		public abstract DateTime? StrongReferenceHeldAtUtc { get; }
	}


	/// <summary>
	/// Wraps a <see cref="WeakReference{T}"/>, and allows holding a strong reference.
	/// Thread safe.
	/// </summary>
	public class ConvertibleWeakReference<T>
			: ConvertibleWeakReference
			where T : class
	{
		/// <summary>
		/// The target: all operations must lock this object: this is returned
		/// from <see cref="SyncLock"/>.
		/// </summary>
		protected readonly WeakReference<T> WeakReference;

		/// <summary>
		/// The strong reference to the target.
		/// </summary>
		protected T StrongReference;

		private DateTime? strongReferenceHeldAtUtc;


		/// <summary>
		/// Constructor: allows creating the <see cref="WeakReference{T}"/> with
		/// <see cref="SystemWeakReference.TrackResurrection"/> set true.
		/// </summary>
		/// <param name="target">NOTICE: CAN be null.</param>
		/// <param name="holdStrongReferenceNow">Optional: defaults to false: a weak
		/// reference is held now only. If set true, and if the target is not null,
		/// this will now invoke <see cref="TryHoldStrongReference"/>.</param>
		/// <param name="trackResurrection">Defaults to false: if you set this true,
		/// then the <see cref="WeakReference{T}"/> is created with
		/// <see cref="SystemWeakReference.TrackResurrection"/> set true.</param>
		public ConvertibleWeakReference(T target, bool holdStrongReferenceNow = false, bool trackResurrection = false)
		{
			WeakReference = new WeakReference<T>(target, trackResurrection);
			if (holdStrongReferenceNow
					&& (target != null)) {
				TryHoldStrongReference();
			}
		}


		/// <summary>
		/// Replaces the target.
		/// </summary>
		/// <param name="target">NOTICE: CAN be null. The new target.</param>
		/// <param name="convertStrongReference">If null --- the default --- then the
		/// strong/weak reference state is retained with the new target, AND
		/// <see cref="StrongReferenceHeldAtUtc"/> will be updated for this new
		/// target if needed. If true, then this will invoke
		/// <see cref="TryHoldStrongReference"/> (and again the time is updated for
		/// this new target). If false, this will invoke
		/// <see cref="ReleaseStrongReference"/>.</param>
		/// <returns><see langword="true"/>if alive now.</returns>
		public void SetTarget(T target, bool? convertStrongReference = null)
		{
			lock (WeakReference) {
				bool isHoldingStrongReference = IsHoldingStrongReference;
				ReleaseStrongReference();
				WeakReference.SetTarget(target);
				if (convertStrongReference ?? isHoldingStrongReference)
					TryHoldStrongReference();
			}
		}

		public sealed override object SyncLock
			=> WeakReference;

		public sealed override bool IsAlive
		{
			get {
				lock (WeakReference) {
					return WeakReference.TryGetTarget(out _);
				}
			}
		}

		/// <summary>
		/// Returns the target only if it is currently alive.
		/// </summary>
		/// <param name="target">The target if alive now.</param>
		/// <param name="convertStrongReference">Applies if target is alive.
		/// If null --- the default --- then the
		/// strong/weak reference state is retained with the new target, and
		/// <see cref="StrongReferenceHeldAtUtc"/> will be updated.
		/// If true, then this will invoke
		/// <see cref="TryHoldStrongReference"/>; and if false, this will invoke
		/// <see cref="ReleaseStrongReference"/>.</param>
		/// <returns><see langword="true"/>if alive now.</returns>
		public bool TryGetTarget(out T target, bool? convertStrongReference = null)
		{
			lock (WeakReference) {
				if (!WeakReference.TryGetTarget(out target))
					return false;
				switch (convertStrongReference) {
					case true :
						TryHoldStrongReference();
						break;
					case false :
						ReleaseStrongReference();
						break;
				}
				return true;
			}
		}

		public sealed override bool TryGetTarget(out object target, bool? convertStrongReference = null)
		{
			bool result = TryGetTarget(out T tTarget, convertStrongReference);
			target = tTarget;
			return result;
		}

		public sealed override bool IsHoldingStrongReference
		{
			get {
				lock (WeakReference) {
					return StrongReference != null;
				}
			}
		}

		public sealed override bool TryHoldStrongReference()
		{
			lock (WeakReference) {
				if (StrongReference != null)
					return true;
				if (WeakReference.TryGetTarget(out StrongReference)) {
					strongReferenceHeldAtUtc = DateTime.UtcNow;
					return true;
				}
				strongReferenceHeldAtUtc = null;
				return false;
			}
		}

		public sealed override bool ReleaseStrongReference()
		{
			lock (WeakReference) {
				StrongReference = null;
				strongReferenceHeldAtUtc = null;
				return WeakReference.TryGetTarget(out _);
			}
		}

		public sealed override DateTime? StrongReferenceHeldAtUtc
		{
			get {
				lock (WeakReference) {
					return strongReferenceHeldAtUtc;
				}
			}
		}
	}
}
