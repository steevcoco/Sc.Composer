using System;


namespace Sc.Composer.Providers
{
	/// <summary>
	/// <see cref="IComposerParticipant{TTarget}"/> that can invoke delegate
	/// implementation methods. Implements <see cref="IProvideParts{TTarget}"/>,
	/// <see cref="IBootstrap{TTarget}"/>, and
	/// <see cref="IHandleComposed{TTarget}"/>, and invokes one or more
	/// <see cref="Action{T}"/> delegates. Also
	/// provides a callback for <see cref="IRequestComposition{TTarget}"/>.
	/// </summary>
	/// <typeparam name="TTarget">The composer target type.</typeparam>
	public class DelegateComposerParticipant<TTarget>
			: IProvideParts<TTarget>,
					IBootstrap<TTarget>,
					IHandleComposed<TTarget>,
					IRequestComposition<TTarget>,
					IDisposable
	{
		private readonly Action<ProvidePartsEventArgs<TTarget>> provideParts;
		private readonly Action<ComposerEventArgs<TTarget>> bootstrap;
		private readonly Action<ComposerEventArgs<TTarget>> handleComposed;
		private bool isDisposed;


		/// <summary>
		/// Constructor: note that at least one delegate is required.
		/// </summary>
		/// <param name="provideParts">Optional.</param>
		/// <param name="bootstrap">Optional.</param>
		/// <param name="handleComposed">Optional.</param>
		/// <exception cref="ArgumentException"></exception>
		public DelegateComposerParticipant(
				Action<ProvidePartsEventArgs<TTarget>> provideParts = null,
				Action<ComposerEventArgs<TTarget>> bootstrap = null,
				Action<ComposerEventArgs<TTarget>> handleComposed = null)
		{
			if ((provideParts == null)
					&& (bootstrap == null)
					&& (handleComposed == null))
				throw new ArgumentException();
			this.provideParts = provideParts;
			this.bootstrap = bootstrap;
			this.handleComposed = handleComposed;
		}

		/// <summary>
		/// Constructor: note that with this constructor, the delegates are not required.
		/// </summary>
		/// <param name="requestComposition">Will be set to an action that will raise this
		/// <see cref="CompositionRequested"/> event</param>
		/// <param name="provideParts">Optional.</param>
		/// <param name="bootstrap">Optional.</param>
		/// <param name="handleComposed">Optional.</param>
		/// <exception cref="ArgumentException"></exception>
		public DelegateComposerParticipant(
				out Action<RequestCompositionEventArgs<TTarget>> requestComposition,
				Action<ProvidePartsEventArgs<TTarget>> provideParts = null,
				Action<ComposerEventArgs<TTarget>> bootstrap = null,
				Action<ComposerEventArgs<TTarget>> handleComposed = null)
		{
			requestComposition = this.requestComposition;
			this.provideParts = provideParts;
			this.bootstrap = bootstrap;
			this.handleComposed = handleComposed;
		}


		private void requestComposition(RequestCompositionEventArgs<TTarget> eventArgs)
			=> CompositionRequested?.Invoke(this, eventArgs);


		private void checkDisposed()
		{
			if (isDisposed)
				throw new ObjectDisposedException(ToString());
		}


		public virtual void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
				where T : TTarget
		{
			checkDisposed();
			provideParts?.Invoke(new DelegateProvidePartsEventArgs<TTarget, T>(eventArgs));
		}

		public void HandleBootstrap<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget
		{
			checkDisposed();
			bootstrap?.Invoke(new ComposerEventArgs<TTarget>(eventArgs.Target));
		}

		public void HandleComposed<T>(ComposerEventArgs<T> eventArgs)
				where T : TTarget
		{
			checkDisposed();
			handleComposed?.Invoke(new ComposerEventArgs<TTarget>(eventArgs.Target));
		}

		public event EventHandler<RequestCompositionEventArgs<TTarget>> CompositionRequested;


		public void Dispose()
		{
			CompositionRequested = null;
			isDisposed = true;
		}
	}
}
