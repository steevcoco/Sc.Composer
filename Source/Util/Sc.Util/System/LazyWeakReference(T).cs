using System;
using SystemWeakReference = System.WeakReference;


namespace Sc.Util.System
{
	/// <summary>
	/// Extends <see cref="ConvertibleWeakReference{T}"/> to wrap a
	/// <see cref="WeakReference{T}"/>; and takes a factory that constructs the
	/// target object. Each time the target is requested from <see cref="Get"/>,
	/// the factory will be invoked if the current instance has been collected.
	/// The constructed target is held weakly; and the factory is invoked each
	/// time the target is requested but has been collected. PLEASE notice also
	/// that the base methods are not changed: this <see cref="Get"/> method WILL
	/// always invoke the factory if needed, but the base
	/// <see cref="ConvertibleWeakReference.TryGetTarget"/> method WILL NOT (and
	/// only returns the last constructed instance if that is still alive). SIMILARLY,
	/// the base <see cref="ConvertibleWeakReference{T}.SetTarget"/>
	/// method WILL replace the weakly-held instance, and that CAN also be null;
	/// and the base <see cref="ConvertibleWeakReference.TryHoldStrongReference"/>
	/// will ONLY succeed if the last-constructed instance is still alive. You
	/// may use this <see cref="HoldStrongReference"/> method to ensure a value and
	/// a strong reference. Thread safe.
	/// </summary>
	/// <typeparam name="T">The target object type. Must be a reference type.</typeparam>
	public class LazyWeakReference<T>
			: ConvertibleWeakReference<T>
			where T : class
	{
		/// <summary>
		/// The Factory.
		/// </summary>
		protected readonly Func<T> Factory;


		/// <summary>
		/// Constructor: allows creating the <see cref="WeakReference{T}"/> with
		/// <see cref="SystemWeakReference.TrackResurrection"/> set true.
		/// </summary>
		/// <param name="factory">Not null.</param>
		/// <param name="holdStrongReferenceNow">Optional: defaults to false: a weak reference
		/// is held now only. If set true, this will now invoke <see cref="HoldStrongReference"/>.</param>
		/// <param name="trackResurrection">Defaults to false: if you set this true,
		/// then the <see cref="WeakReference{T}"/> is created with
		/// <see cref="SystemWeakReference.TrackResurrection"/> set true.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public LazyWeakReference(Func<T> factory, bool holdStrongReferenceNow = false, bool trackResurrection = false)
				: base(null, false, trackResurrection)
		{
			Factory = factory ?? throw new ArgumentNullException(nameof(factory));
			if (holdStrongReferenceNow)
				HoldStrongReference();
		}


		/// <summary>
		/// Always returns the value, invoking the Factory now if needed.
		/// Note that if your factory CAN return a null value, then this result CAN be null.
		/// </summary>
		/// <param name="convertStrongReference">If null --- the default --- then the
		/// strong/weak reference state is retained. If true, then this will invoke
		/// <see cref="ConvertibleWeakReference.TryHoldStrongReference"/>;
		/// and if false, this will invoke
		/// <see cref="ConvertibleWeakReference.ReleaseStrongReference"/>.</param>
		/// <returns>Always non-null IF your factory always returns a non-null value:
		/// returns the now current value.</returns>
		public T Get(bool? convertStrongReference = null)
		{
			lock (WeakReference) {
				bool isHoldingStrongReference = IsHoldingStrongReference;
				if (!WeakReference.TryGetTarget(out T value)) {
					ReleaseStrongReference();
					value = Factory();
					WeakReference.SetTarget(value);
					if (convertStrongReference ?? isHoldingStrongReference)
						TryHoldStrongReference();
					return value;
				}
				switch (convertStrongReference) {
					case true :
						TryHoldStrongReference();
						break;
					case false :
						ReleaseStrongReference();
						break;
				}
				return value;
			}
		}

		/// <summary>
		/// Always ensures that there is a target --- invoking the factory if needed now
		/// --- and then invokes <see cref="ConvertibleWeakReference.TryHoldStrongReference"/>
		/// if needed, always ensuring a strong reference is held to the now current target.
		/// Note still that if your factory CAN return a null target, then this result CAN be null.
		/// </summary>
		/// <returns>Always non-null IF your factory always returns a non-null target:
		/// returns the now current target.</returns>
		public T HoldStrongReference()
		{
			lock (WeakReference) {
				return Get(true);
			}
		}
	}
}
