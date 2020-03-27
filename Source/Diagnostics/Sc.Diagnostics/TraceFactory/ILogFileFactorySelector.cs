using System.Diagnostics;


namespace Sc.Diagnostics.TraceFactory
{
	/// <summary>
	/// Implements a selctor for individual <see cref="TraceSource"/>
	/// instances to be configured by a <see cref="LogFileFactory"/>.
	/// </summary>
	public interface ILogFileFactorySelector
	{
		/// <summary>
		/// Implements a selector for each newly-constructed <see cref="TraceSource"/>.
		/// </summary>
		/// <param name="traceSource">Not null.</param>
		/// <param name="factoryDefault">Will provide the factory's default
		/// <see cref="LogFileFactorySelection"/>; so that you can more
		/// easily return a value with flags addaed or removed from the default.</param>
		/// <param name="selectedSwitchLevel">Will prvide the facoty's
		/// switch lkevel that is applied if the method returns
		/// <see cref="LogFileFactorySelection.SwitchAndFilterLevels"/>.</param>
		LogFileFactorySelection Select(
				TraceSource traceSource,
				LogFileFactorySelection factoryDefault,
				SourceLevels selectedSwitchLevel);
	}
}
