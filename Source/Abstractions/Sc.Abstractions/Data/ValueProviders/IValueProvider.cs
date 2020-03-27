namespace Sc.Abstractions.Data.ValueProviders
{
	/// <summary>
	/// Defines an object that yields a value when requested. Types can be defined by
	/// <see cref="ValueProviderDescriptor"/>; and return refined values with
	/// <see cref="IValueProvider{TValue}"/>.
	/// </summary>
	public interface IValueProvider
	{
		/// <summary>
		/// The provided value.
		/// </summary>
		object GetProviderValue();
	}
}
