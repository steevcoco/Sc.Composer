using System.ComponentModel;
using System.Diagnostics;


namespace Sc.Diagnostics.TraceFactory
{
	/// <summary>
	/// Defines simple POCO properties to configure a <see cref="LogFileFactory"/>.
	/// </summary>
	public interface ILogFileFactoryConfig
	{
		/// <summary>
		/// Provides the <see cref="LogFileFactory"/>
		/// <see cref="LogFileFactory.DefaultTraceSourceSelection"/>
		/// property value.
		/// </summary>
		[DefaultValue(LogFileFactorySelection.All)]
		LogFileFactorySelection DefaultTraceSourceSelection { get; set; }

		/// <summary>
		/// Provides the <see cref="LogFileFactory"/>
		/// <see cref="LogFileFactory.SelectedSwitchLevel"/>
		/// property value.
		/// </summary>
		[DefaultValue(SourceLevels.Warning)]
		SourceLevels SelectedSwitchLevel { get; set; }

		/// <summary>
		/// Provides the <see cref="LogFileFactory"/>
		/// <see cref="LogFileFactory.LogFileFilterLevel"/>
		/// property value.
		/// </summary>
		[DefaultValue(SourceLevels.Information)]
		SourceLevels LogFileFilterLevel { get; set; }

		/// <summary>
		/// Provides the <see cref="LogFileFactory"/>
		/// <see cref="LogFileFactory.WatchConfigFileChanges"/>
		/// property value.
		/// </summary>
		[DefaultValue(true)]
		bool WatchConfigFileChanges { get; set; }

		/// <summary>
		/// Provides the <see cref="LogFileFactory"/>
		/// <see cref="LogFileFactory.ToggleLogFile"/>
		/// property value.
		/// </summary>
		[DefaultValue(true)]
		bool ToggleLogFile { get; set; }
	}
}