namespace Sc.Diagnostics
{
	/// <summary>
	/// Implements a <see cref="SimpleTraceSource"/> selector; which is
	/// supported by <see cref="TraceSources"/>. The selector implements
	/// two methods: the <see cref="Select"/> method is invoked with each
	/// newly-constructed <see cref="SimpleTraceSource"/> for configuration;
	/// and is able to inspect and directly configure the source. This
	/// interface also implements a <see cref="Remove"/> method: if THIS
	/// <see cref="ITraceSourceSelector"/> is removed from <see cref="TraceSources"/>,
	/// then it will be invoked with all live trace sources and can remove any
	/// configurations that it might have made. Note that the
	/// <see cref="SimpleTraceSource"/> instances are cached weakly by
	/// <see cref="TraceSources"/>: when the source goes out of scope,
	/// it will be collected, and this <see cref="Remove"/> mwthod
	/// IS NOT invoked at that time.
	/// </summary>
	public interface ITraceSourceSelector
	{
		/// <summary>
		/// Implements a selector for each newly-constructed <see cref="SimpleTraceSource"/>.
		/// </summary>
		/// <param name="traceSource">Not null.</param>
		void Select(SimpleTraceSource traceSource);

		/// <summary>
		/// This method will be invoked with each live <see cref="SimpleTraceSource"/>
		/// instance when THIS selector is removed from <see cref="TraceSources"/>.
		/// </summary>
		/// <param name="traceSource">Not null.</param>
		void Remove(SimpleTraceSource traceSource);
	}
}
