using System;


namespace Sc.Abstractions.Serialization
{
	/// <summary>
	/// Implements a collection of <see cref="IProvideKnownTypes"/>
	/// and <see cref="GetKnownTypes"/> proividers. This interface also
	/// re-implements <see cref="IProvideKnownTypes"/>, and will provide
	/// the results from all providers.
	/// </summary>
	public interface IKnownTypeCollector
			: IProvideKnownTypes
	{
		/// <summary>
		/// Adds an <see cref="IProvideKnownTypes"/> provider to this collection.
		/// </summary>
		/// <param name="provideKnownTypes">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		void Add(IProvideKnownTypes provideKnownTypes);

		/// <summary>
		/// Adds a <see cref="GetKnownTypes"/> provider to this collection.
		/// </summary>
		/// <param name="getKnownTypes">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		void Add(GetKnownTypes getKnownTypes);
	}
}
