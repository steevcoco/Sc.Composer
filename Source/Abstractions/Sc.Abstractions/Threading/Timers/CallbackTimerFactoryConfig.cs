using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;


namespace Sc.Abstractions.Threading.Timers
{
	/// <summary>
	/// Provides a configuration to construct an
	/// <see cref="ICallbackTimerFactory"/>. Notice that this
	/// implements <see cref="IEquatable{T}"/>, but the members are
	/// mutable: the hash code WILL change if the properties change.
	/// </summary>
	[DataContract]
	public class CallbackTimerFactoryConfig
			: IEquatable<CallbackTimerFactoryConfig>
	{
		/// <summary>
		/// This defaults to TRUE.
		/// Sets the constructor value for whether the factory is initialized
		/// immediately when constructed; or otherwise must be explicitly
		/// initialized.
		/// </summary>
		[DataMember]
		public bool InitializeNow { get; set; } = true;

		/// <summary>
		/// Defaults to null.
		/// Sets the factory's initial capacity. If null, the factory's
		/// default is used.
		/// </summary>
		[DataMember]
		public int? InitialCapacity { get; set; }


		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
		public override int GetHashCode()
			=> (((23 * 37)
									+ (InitializeNow
											? 1
											: 0))
							* 37)
					+ (InitialCapacity ?? 0);

		public override bool Equals(object obj)
			=> Equals(obj as CallbackTimerFactoryConfig);

		public bool Equals(CallbackTimerFactoryConfig other)
			=> (other != null)
					&& (InitializeNow == other.InitializeNow)
					&& (InitialCapacity == other.InitialCapacity);


		public override string ToString()
			=> $"{GetType().Name}"
					+ $"["
					+ $"{nameof(CallbackTimerFactoryConfig.InitializeNow)}: {InitializeNow}"
					+ $", {nameof(CallbackTimerFactoryConfig.InitialCapacity)}: {InitialCapacity}"
					+ $"]";
	}
}
