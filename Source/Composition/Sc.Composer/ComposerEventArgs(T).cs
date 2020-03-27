using System;
using Sc.Util.System;


namespace Sc.Composer
{
	/// <summary>
	/// Event args holding the composition <see cref="Target"/>.
	/// </summary>
	/// <typeparam name="TTarget">The composition target type</typeparam>
	public class ComposerEventArgs<TTarget>
			: EventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="target">Required.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ComposerEventArgs(TTarget target)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			Target = target;
		}


		/// <summary>
		/// The composition target for this event.
		/// </summary>
		public TTarget Target { get; }


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}"
					+ "["
					+ $", {nameof(ComposerEventArgs<TTarget>.Target)}: {Target}"
					+ "]";
	}
}
