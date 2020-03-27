using System;


namespace Sc.Util.Text
{
	/// <summary>
	/// A readonly struct that wraps a Func that
	/// returns the string for <see cref="object.ToString"/>.
	/// </summary>
	public readonly struct DelegateToString
	{
		private readonly Func<string> toString;


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="toString">Required: the delegate's result
		/// is returned from <see cref="ToString"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public DelegateToString(Func<string> toString)
			=> this.toString = toString ?? throw new ArgumentNullException(nameof(toString));


		public override string ToString()
			=> toString?.Invoke();
	}
}
