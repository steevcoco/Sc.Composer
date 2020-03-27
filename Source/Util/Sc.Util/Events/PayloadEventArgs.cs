using System;
using Sc.Util.System;


namespace Sc.Util.Events
{
	/// <summary>
	/// A simple <see cref="EventArgs"/> that takes a <see cref="Payload"/> object.
	/// </summary>
	/// <typeparam name="TPayload">The <see cref="Payload"/> type.</typeparam>
	public class PayloadEventArgs<TPayload>
			: EventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="payload">The <see cref="Payload"/>: note that by default
		/// this cannot be null: depending on <paramref name="throwIfNull"/>.</param>
		/// <param name="throwIfNull">Defaults to <see langword="true"/>:
		/// this will throw if the <paramref name="payload"/> is <see langword="null"/>.</param>
		public PayloadEventArgs(TPayload payload, bool throwIfNull = true)
		{
			if (throwIfNull
					&& (payload == null)) {
				throw new ArgumentNullException(nameof(payload));
			}
			Payload = payload;
		}


		/// <summary>
		/// This event's Payload.
		/// </summary>
		public TPayload Payload { get; }


		public override string ToString()
			=> $"{GetType().GetFriendlyName()}[{nameof(PayloadEventArgs<TPayload>.Payload)}: {Payload}]";
	}
}
