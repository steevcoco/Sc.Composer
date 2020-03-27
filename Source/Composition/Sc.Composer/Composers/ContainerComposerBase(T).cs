using System;


namespace Sc.Composer.Composers
{
	/// <summary>
	/// This class provides a <see cref="IComposer{TTarget}"/> implementation for
	/// Container composition, that can also be subclassed. This class extends
	/// <see cref="Composer{TTarget}"/>, and provides simple container-oriented
	/// features. You are expected to add participants that
	/// will contribute registrations or members to the container; YET, the
	/// type of container is NOT restricted here. This class will set the
	/// <see cref="Container"/> property to the result of each composition
	/// when the composition completes --- so the container instance is available
	/// here; but note that it is not set until the whole composition completes.
	/// AND: since <see cref="IComposer{TTarget}"/>
	/// supports multiple compositions: if your container is a singleton, you
	/// can either restrict the added participants to instances that will NOT
	/// request re-composition; or otherwise ensure that participants ARE able to
	/// re-compose into the SAME container instance; AND, this class adds
	/// the <see cref="AllowRecomposition"/> property, which defaults to
	/// FALSE: this will override all events raised by any added
	/// <see cref="IRequestComposition{TTarget}"/> participants, and
	/// requests for re-composition will be ELIDED; AND direct
	/// invokers of <see cref="IComposer{TTarget}.Compose"/> WILL
	/// RAISE <see cref="InvalidOperationException"/>. You must set this
	/// true if your composer will support re-composition.
	/// </summary>
	/// <typeparam name="TContainer">The specific type of the Container target.</typeparam>
	public class ContainerComposerBase<TContainer>
			: Composer<TContainer>
	{
		private bool allowRecomposition;
		private TContainer container;
		private bool hasComposed;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTarget">Required: this will be invoked in
		/// <see cref="Composer{TTarget}.GetTarget"/>,
		/// and can provide a complete implementation.
		/// You MUST otherwise use the protected constructor and
		/// override <see cref="Composer{TTarget}.GetTarget"/>.</param>
		/// <param name="disposeProvidersWithThis">Specifies how added providers are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ContainerComposerBase(Func<TContainer> getTarget, bool disposeProvidersWithThis = true)
				: base(getTarget, disposeProvidersWithThis) { }

		/// <summary>
		/// Protected constructor for subclasses. NOTICE that you MUST
		/// override <see cref="Composer{TTarget}.GetTarget"/>.
		/// </summary>
		/// <param name="disposeProvidersWithThis">Specifies how added providers are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		protected ContainerComposerBase(bool disposeProvidersWithThis = true)
				: base(disposeProvidersWithThis) { }


		/// <summary>
		/// Overridden here to test <see cref="AllowRecomposition"/>; and will THROW
		/// if false and this has composed already.
		/// </summary>
		protected override TContainer HandleCompose()
		{
			lock (SyncLock) {
				if (!allowRecomposition
						&& hasComposed)
					throw new InvalidOperationException($"Composer does not support re-composition: {this}.");
				hasComposed = true;
			}
			return base.HandleCompose();
		}

		/// <summary>
		/// This method is overridden here to implement <see cref="AllowRecomposition"/>;
		/// and if false and <see cref="HasComposed"/> is true, this will not invoke base.
		/// </summary>
		protected override void HandleCompositionRequested(
				object sender,
				RequestCompositionEventArgs<TContainer> eventArgs)
		{
			lock (SyncLock) {
				if (!allowRecomposition
						&& hasComposed)
					return;
			}
			base.HandleCompositionRequested(sender, eventArgs);
		}

		/// <summary>
		/// Overridden to set this <see cref="Container"/> every time.
		/// </summary>
		protected override void AfterComposed(TContainer newTarget)
			=> container = newTarget;


		/// <summary>
		/// Defaults to FALSE: any event raised by an added
		/// <see cref="IRequestComposition{TTarget}"/> participant will be overridden
		/// and discarded. If set true, any participant that raises an event WILL
		/// invoke a new composition
		/// </summary>
		public virtual bool AllowRecomposition
		{
			get {
				lock (SyncLock) {
					return allowRecomposition;
				}
			}
			set {
				lock (SyncLock) {
					allowRecomposition = value;
				}
			}
		}

		/// <summary>
		/// Notice: this is null until composition completes;
		/// and, if your implementation supports re-composition,
		/// this will be re-set on every composition.
		/// This is set each time in <see cref="Composer{TTarget}.AfterComposed"/>.
		/// Note also that this is NOT set null NOR DISPOSED when this composer is disposed.
		/// </summary>
		public TContainer Container
		{
			get {
				lock (SyncLock) {
					return container;
				}
			}
			protected set {
				lock (SyncLock) {
					container = value;
				}
			}
		}

		/// <summary>
		/// Note: unlike the <see cref="Container"/>, this property is set at the
		/// START of the first composition.
		/// This property will be set true on the first composition here; and will
		/// not be set false again; UNLESS the subclass DOES reset the property.
		/// </summary>
		public bool HasComposed
		{
			get {
				lock (SyncLock) {
					return hasComposed;
				}
			}
			protected set {
				lock (SyncLock) {
					hasComposed = value;
				}
			}
		}


		public override string ToString()
		{
			lock (SyncLock) {
				return $"{base.ToString()}"
						+ ", ["
						+ $"{nameof(ContainerComposerBase<TContainer>.AllowRecomposition)}: {AllowRecomposition}"
						+ $", {nameof(ContainerComposerBase<TContainer>.HasComposed)}: {HasComposed}"
						+ $", {nameof(ContainerComposerBase<TContainer>.Container)}: {Container}"
						+ "]";
			}
		}
	}
}
