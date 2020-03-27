using System;
using System.Diagnostics.CodeAnalysis;


namespace Sc.Abstractions.Threading.Timers
{
	/// <summary>
	/// Provides a configuration to construct an
	/// <see cref="IWindowTimerFactory"/>. Notice that this
	/// implements <see cref="IEquatable{T}"/>, but the members are
	/// mutable: the hash code WILL change if the properties change.
	/// </summary>
	public class WindowTimerFactoryConfig
			: IEquatable<WindowTimerFactoryConfig>
	{
		/// <summary>
		/// This defaults to TRUE.
		/// Sets the constructor value for whether the factory is initialized
		/// immediately when constructed; or otherwise must be explicitly
		/// initialized.
		/// </summary>
		public bool InitializeNow { get; set; } = true;

		/// <summary>
		/// Defaults to null.
		/// Sets the factory's initial capacity. If null, the factory's
		/// default is used.
		/// </summary>
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
			=> Equals(obj as WindowTimerFactoryConfig);

		public bool Equals(WindowTimerFactoryConfig other)
			=> (other != null)
					&& (InitializeNow == other.InitializeNow)
					&& (InitialCapacity == other.InitialCapacity);


		public override string ToString()
			=> $"{GetType().Name}"
					+ $"["
					+ $"{nameof(WindowTimerFactoryConfig.InitializeNow)}: {InitializeNow}"
					+ $", {nameof(WindowTimerFactoryConfig.InitialCapacity)}: {InitialCapacity}"
					+ $"]";
	}
}
