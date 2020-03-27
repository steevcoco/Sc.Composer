using System;


namespace Sc.Diagnostics
{
	/// <summary>
	/// Simple class the implements <see cref="ITraceSourceSelector"/> with
	/// provided delegates. Note: this overrides <see cref="Equals"/>;
	/// and compares ONLY the <see cref="Selector"/>.
	/// </summary>
	public sealed class DelegateTraceSourceSelector
			: ITraceSourceSelector
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="selector">Required <see cref="ITraceSourceSelector.Select"/> delegate.</param>
		/// <param name="remove">Optional delegate for <see cref="ITraceSourceSelector.Remove"/>.</param>
		public DelegateTraceSourceSelector(Action<SimpleTraceSource> selector, Action<SimpleTraceSource> remove = null)
		{
			Selector = selector ?? throw new ArgumentNullException(nameof(selector));
			RemoveDelegate = remove;
		}


		/// <summary>
		/// This is the delegate invoked by <see cref="Select"/>.
		/// Not null.
		/// </summary>
		public Action<SimpleTraceSource> Selector { get; }

		/// <summary>
		/// This is the optional delegate invoked by <see cref="Remove"/>.
		/// May be null.
		/// </summary>
		public Action<SimpleTraceSource> RemoveDelegate { get; }


		public void Select(SimpleTraceSource traceSource)
			=> Selector(traceSource);

		public void Remove(SimpleTraceSource traceSource)
			=> RemoveDelegate?.Invoke(traceSource);


		public override int GetHashCode()
			=> Selector.GetHashCode();

		public override bool Equals(object obj)
			=> obj is DelegateTraceSourceSelector other
					&& object.Equals(Selector, other.Selector);

		public override string ToString()
			=> $"{GetType().Name}"
					+ "["
					+ $"{nameof(DelegateTraceSourceSelector.Selector)}: {Selector}"
					+ $", {nameof(DelegateTraceSourceSelector.RemoveDelegate)}: {RemoveDelegate}"
					+ "]";
	}
}
