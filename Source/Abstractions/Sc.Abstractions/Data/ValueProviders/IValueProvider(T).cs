namespace Sc.Abstractions.Data.ValueProviders
{
	/// <summary>
	/// Refined generic interface for a <see cref="IValueProvider"/>.
	/// </summary>
	/// <typeparam name="TValue">The refined <see cref="IValueProvider"/> value
	/// type.</typeparam>
	public interface IValueProvider<out TValue>
			: IValueProvider
	{
		/// <summary>
		/// The provided value.
		/// </summary>
		TValue ProviderValue { get; }
	}
}
