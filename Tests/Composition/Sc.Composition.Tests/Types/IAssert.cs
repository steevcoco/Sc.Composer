using System;


namespace Sc.Composition.Tests.Types
{
	public interface IAssert
			: IDisposable
	{
		bool IsAnyDisposed { get; }

		bool IsAllDisposed { get; }

		/// <summary>
		/// Assert test conditions.
		/// </summary>
		void AssertConstruction();
	}
}
