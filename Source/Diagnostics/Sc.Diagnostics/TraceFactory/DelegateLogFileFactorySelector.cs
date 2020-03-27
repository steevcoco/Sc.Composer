using System;
using System.Diagnostics;


namespace Sc.Diagnostics.TraceFactory
{
	/// <summary>
	/// Simple class the implements <see cref="ILogFileFactorySelector"/> with
	/// a provided delegate. This overrides <see cref="Equals"/>;
	/// compares the <see cref="Selector"/>.
	/// </summary>
	public sealed class DelegateLogFileFactorySelector
			: ILogFileFactorySelector
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="selector">Required.</param>
		public DelegateLogFileFactorySelector(
				Func<TraceSource, LogFileFactorySelection, SourceLevels, LogFileFactorySelection> selector)
			=> Selector = selector ?? throw new ArgumentNullException(nameof(selector));


		/// <summary>
		/// This is the delegate invoked by <see cref="Select"/>.
		/// Not null.
		/// </summary>
		public Func<TraceSource, LogFileFactorySelection, SourceLevels, LogFileFactorySelection> Selector { get; }


		public LogFileFactorySelection Select(
				TraceSource traceSource,
				LogFileFactorySelection factoryDefault,
				SourceLevels selectedSwitchLevel)
			=> Selector(traceSource, factoryDefault, selectedSwitchLevel);


		public override int GetHashCode()
			=> Selector.GetHashCode();

		public override bool Equals(object obj)
			=> obj is DelegateLogFileFactorySelector other
					&& object.Equals(Selector, other.Selector);

		public override string ToString()
			=> $"{GetType().Name}"
					+ "["
					+ $"{nameof(DelegateLogFileFactorySelector.Selector)}: {Selector}"
					+ "]";
	}
}
