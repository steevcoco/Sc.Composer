namespace Sc.Abstractions.Data.Periodicity
{
	/// <summary>
	/// Defines a collection of periodic data. Data is stored and fetched by
	/// a <typeparamref name="TPeriod"/> type defining the periods.
	/// The stored data is defined by <typeparamref name="TPeriodicData"/>.
	/// </summary>
	/// <typeparam name="TPeriod">Defines how periods are stored and
	/// fetched from the collection.</typeparam>
	/// <typeparam name="TPeriodicData">Defines the data returned for each
	/// period.</typeparam>
	public interface IPeriodicDataCollection<in TPeriod, TPeriodicData>
	{
		/// <summary>
		/// Fetches the <see cref="TPeriodicData"/> at the <see cref="TPeriod"/>, if any.
		/// </summary>
		/// <param name="period">Required.</param>
		/// <param name="data">Set of there is data defined for the <c>period</c>.</param>
		/// <returns>True if the data is found.</returns>
		bool TryGetPeriod(TPeriod period, out TPeriodicData data);

		/// <summary>
		/// Fetches the first <see cref="TPeriodicData"/> in the collection, if any.
		/// </summary>
		/// <param name="data">Set of there is data defined for the first <c>period</c>.</param>
		/// <returns>True if the data is found.</returns>
		bool TryGetFirstPeriod(out TPeriodicData data);

		/// <summary>
		/// Fetches the last <see cref="TPeriodicData"/> in the collection, if any.
		/// </summary>
		/// <param name="data">Set of there is data defined for the Alast <c>period</c>.</param>
		/// <returns>True if the data is found.</returns>
		bool TryGetLastPeriod(out TPeriodicData data);
	}
}
