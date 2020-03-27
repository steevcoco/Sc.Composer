using System;
using System.Collections.Generic;
using Sc.Composer.Providers;


namespace Sc.Composer
{
	/// <summary>
	/// Static helpers for <see cref="IComposer{TComposedEventArgs}"/>.
	/// </summary>
	public static class ComposerHelper
	{
		/// <summary>
		/// Convenience method contributes each participant to your composer.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target handled
		/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="participants">Required.</param>
		/// <param name="oneTimeOnly">Optional.</param>
		/// <returns>Your composer.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static IComposer<TTarget> ParticipateRange<TTarget>(
				this IComposer<TTarget> composer,
				IEnumerable<IComposerParticipant<TTarget>> participants,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			if (participants == null)
				throw new ArgumentNullException(nameof(participants));
			foreach (IComposerParticipant<TTarget> composerParticipant in participants) {
				composer.Participate(composerParticipant, oneTimeOnly);
			}
			return composer;
		}


		/// <summary>
		/// Convenience method that constructs a new
		/// <see cref="AggregateComposerParticipant{TTarget}"/> from the
		/// given list, and participates it with your composer. All interfaces
		/// implemented by your participants will be invoked. Since this
		/// will construct a new participant, the actual
		/// added instance is returned here.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target handled
		/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="participants">Required.</param>
		/// <param name="oneTimeOnly">Optional.</param>
		/// <param name="disposeParticipantsWithAggregate">Defaults to true: the participants
		/// will be disposed with this new aggregate participant.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static AggregateComposerParticipant<TTarget> Aggregate<TTarget>(
				this IComposer<TTarget> composer,
				IEnumerable<IComposerParticipant<TTarget>> participants,
				bool oneTimeOnly = false,
				bool disposeParticipantsWithAggregate = true)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			AggregateComposerParticipant<TTarget> aggregateParticipant
					= new AggregateComposerParticipant<TTarget>(participants)
					{
						DisposeParticipantsWithThis = disposeParticipantsWithAggregate
					};
			composer.Participate(aggregateParticipant, oneTimeOnly);
			return aggregateParticipant;
		}


		/// <summary>
		/// Convenience method that constructs a new <see cref="IProvideParts{TTarget}"/>
		/// participant that will invoke your action, and participates it with this composer.
		/// Since this will construct a new participant, the new
		/// added instance is returned here.
		/// See also <see cref="DelegateComposerParticipant{TTarget}"/>.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target handled
		/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="provideParts">Required.</param>
		/// <param name="oneTimeOnly">Optional.</param>
		/// <returns>Not null: the added participant.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static DelegateComposerParticipant<TTarget> ParticipatePartsDelegate<TTarget>(
				this IComposer<TTarget> composer,
				Action<ProvidePartsEventArgs<TTarget>> provideParts,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			DelegateComposerParticipant<TTarget> delegateParticipant = new DelegateComposerParticipant<TTarget>(provideParts);
			composer.Participate(delegateParticipant, oneTimeOnly);
			return delegateParticipant;
		}

		/// <summary>
		/// Convenience method that constructs a new <see cref="IBootstrap{TTarget}"/>
		/// participant that will invoke your action, and participates it with this composer.
		/// Since this will construct a new participant, the new
		/// added instance is returned here.
		/// See also <see cref="DelegateComposerParticipant{TTarget}"/>.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target handled
		/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="bootstrap">Required.</param>
		/// <param name="oneTimeOnly">Optional.</param>
		/// <returns>Not null: the added participant.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static DelegateComposerParticipant<TTarget> ParticipateBootstrapDelegate<TTarget>(
				this IComposer<TTarget> composer,
				Action<ComposerEventArgs<TTarget>> bootstrap,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			DelegateComposerParticipant<TTarget> delegateParticipant = new DelegateComposerParticipant<TTarget>(null, bootstrap);
			composer.Participate(delegateParticipant, oneTimeOnly);
			return delegateParticipant;
		}

		/// <summary>
		/// Convenience method that constructs a new <see cref="IHandleComposed{TTarget}"/>
		/// participant that will invoke your action, and participates it with this composer.
		/// Since this will construct a new participant, the new
		/// added instance is returned here.
		/// See also <see cref="DelegateComposerParticipant{TTarget}"/>.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target handled
		/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="handleComposed">Required.</param>
		/// <param name="oneTimeOnly">Optional.</param>
		/// <returns>Not null: the added participant.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static DelegateComposerParticipant<TTarget> ParticipateHandleComposedDelegate<TTarget>(
				this IComposer<TTarget> composer,
				Action<ComposerEventArgs<TTarget>> handleComposed,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			DelegateComposerParticipant<TTarget> delegateParticipant
					= new DelegateComposerParticipant<TTarget>(null, null, handleComposed);
			composer.Participate(delegateParticipant, oneTimeOnly);
			return delegateParticipant;
		}

		/// <summary>
		/// Convenience method that constructs a new <see cref="IRequestComposition{TTarget}"/>
		/// participant that will raise the <see cref="IRequestComposition{TTarget}.CompositionRequested"/>
		/// event, and participates it with this composer.
		/// Since this will construct a new participant, the new
		/// added instance is returned here.
		/// See also <see cref="DelegateComposerParticipant{TTarget}"/>.
		/// </summary>
		/// <typeparam name="TTarget">The type of the target handled
		/// by the specific <see cref="IComposer{TTarget}"/>.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="requestComposition">Will be set to an action that will
		/// raise the <see cref="IRequestComposition{TTarget}.CompositionRequested"/>
		/// event from the returned participant.</param>
		/// <param name="oneTimeOnly">Optional.</param>
		/// <returns>Not null: the added participant.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static DelegateComposerParticipant<TTarget> ParticipateRequestCompositionDelegate<TTarget>(
				this IComposer<TTarget> composer,
				out Action<RequestCompositionEventArgs<TTarget>> requestComposition,
				bool oneTimeOnly = false)
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			DelegateComposerParticipant<TTarget> delegateParticipant
					= new DelegateComposerParticipant<TTarget>(out requestComposition);
			composer.Participate(delegateParticipant, oneTimeOnly);
			return delegateParticipant;
		}


		/// <summary>
		/// This helper method will check and construct a new variant participant, that
		/// implements the composer's covariant type, and contributes all supported interfaces to
		/// the composer. Since this will construct a new participant, the actual
		/// added instance is returned here.
		/// </summary>
		/// <typeparam name="TSuper">The delegate's actual type.</typeparam>
		/// <typeparam name="TTarget">This composer's implemented type.</typeparam>
		/// <param name="composer">Required.</param>
		/// <param name="participant">Required.</param>
		/// <param name="oneTimeOnly">Optional.</param>
		/// <returns>The actual added instance.</returns>
		/// <exception cref="ArgumentNullException"/>
		public static VariantParticipant<TSuper, TTarget> ParticipateAs<TSuper, TTarget>(
				this IComposer<TTarget> composer,
				IComposerParticipant<TSuper> participant,
				bool oneTimeOnly = false)
				where TTarget : TSuper
		{
			if (composer == null)
				throw new ArgumentNullException(nameof(composer));
			VariantParticipant<TSuper, TTarget> variantParticipant
					= new VariantParticipant<TSuper, TTarget>(participant);
			composer.Participate(variantParticipant, oneTimeOnly);
			return variantParticipant;
		}


		/// <summary>
		/// Static helper method will construct a new <see cref="Composer{TTarget}"/>
		/// now, with your target, and perform one complete composition, that will
		/// invoke your action.
		/// </summary>
		/// <typeparam name="TTarget">The composer's target type.</typeparam>
		/// <param name="target">Required.</param>
		/// <param name="provideParts">Required. Full support for callbacks is
		/// implemented.</param>
		/// <returns>Your given <c>target`</c>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="AggregateComposerException">If there is an unhandled
		/// <see cref="IComposer{TTarget}"/> error.</exception>
		public static TTarget ComposeNow<TTarget>(
				TTarget target,
				Action<ProvidePartsEventArgs<TTarget>> provideParts)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (provideParts == null)
				throw new ArgumentNullException(nameof(provideParts));
			TTarget GetTarget()
				=> target;
			using (Composer<TTarget> composer = new Composer<TTarget>(GetTarget)) {
				composer.ParticipatePartsDelegate(provideParts);
				return composer.Compose();
			}
		}

		/// <summary>
		/// Static helper method will construct a new <see cref="Composer{TTarget}"/>
		/// now, with your target, and participate all of your participants;
		/// and perform one complete composition.
		/// </summary>
		/// <typeparam name="TTarget">The composer's target type.</typeparam>
		/// <param name="target">Required.</param>
		/// <param name="participants">Required. Full support for callbacks is
		/// implemented.</param>
		/// <param name="disposeParticipants">Defaults to true: the participants
		/// will be disposed here.</param>
		/// <returns>Your given <c>target`</c>.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="AggregateComposerException">If there is an unhandled
		/// <see cref="IComposer{TTarget}"/> error.</exception>
		public static TTarget ComposeNow<TTarget>(
				TTarget target,
				IEnumerable<IComposerParticipant<TTarget>> participants,
				bool disposeParticipants = true)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (participants == null)
				throw new ArgumentNullException(nameof(participants));
			TTarget GetTarget()
				=> target;
			using (Composer<TTarget> composer = new Composer<TTarget>(GetTarget, disposeParticipants)) {
				composer.ParticipateRange(participants);
				return composer.Compose();
			}
		}
	}
}
