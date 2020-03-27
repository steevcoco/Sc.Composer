using System;


namespace Sc.Diagnostics.TraceFactory
{
	/// <summary>
	/// Defines selections for <see cref="ILogFileFactorySelector"/>.
	/// </summary>
	[Flags]
	public enum LogFileFactorySelection
			: byte
	{
		/// <summary>
		/// Indicates to the factory that it should perform
		/// its default configurations on the selected source.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Indicates to the factory that it should perform NO
		/// configuration on the selected source.
		/// </summary>
		None = 1,

		/// <summary>
		/// Indicates to the factory that it should perform
		/// switch and filter level configurations on the selected source.
		/// </summary>
		SwitchAndFilterLevels = 2,

		/// <summary>
		/// Indicates to the factory that it should perform
		/// log file output configurations on the selected source.
		/// </summary>
		LogFileOutput = 4,

		/// <summary>
		/// Selects all flags.
		/// </summary>
		All = 255,
	}
}
