using System;
using Sc.Abstractions.ServiceLocator;


namespace Sc.Composer.Composers
{
	/// <summary>
	/// A complete <see cref="IComposer{TTarget}"/> implementation
	/// for <see cref="IContainerBase"/> composition, that can also be subclassed.
	/// This class extends <see cref="ContainerComposerBase{TContainer}"/>,
	/// which provides the <see cref="ContainerComposerBase{TContainer}.AllowRecomposition"/>
	/// property, which defaults to FALSE: this will override all events raised
	/// by any added <see cref="IRequestComposition{TTarget}"/> participants and
	/// discard the event; AND raise exceptions if more than one composition
	/// is attempted. You can set this property true if your composer can support
	/// re-composition. Please see <see cref="ContainerComposerBase{TContainer}"/>
	/// for more.
	/// </summary>
	/// <typeparam name="TContainer">The specific type of the
	/// <see cref="IContainerBase"/>.</typeparam>
	public class ContainerComposer<TContainer>
			: ContainerComposerBase<TContainer>
			where TContainer : IContainerBase
	{
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
		public ContainerComposer(Func<TContainer> getTarget, bool disposeProvidersWithThis = true)
				: base(getTarget, disposeProvidersWithThis) { }

		/// <summary>
		/// Protected constructor for subclasses. NOTICE that you MUST
		/// override <see cref="Composer{TTarget}.GetTarget"/>.
		/// </summary>
		/// <param name="disposeProvidersWithThis">Specifies how added providers are handled when
		/// this object is disposed: notice that this defaults to true.</param>
		protected ContainerComposer(bool disposeProvidersWithThis = true)
				: base(disposeProvidersWithThis) { }
	}
}
