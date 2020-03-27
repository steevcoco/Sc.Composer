using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Sc.Tests.Shared
{
	/// <summary>
	/// Contains Assembly initialize and Cleanup methods to instrument
	/// TraceSources with verbose output.
	/// </summary>
	[TestClass]
	public class AssemblySetup
	{
		private static IDisposable resetTraceSources;


		[AssemblyInitialize]
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
				"Style",
				"IDE0060:Remove unused parameter",
				Justification = "MSTest signature.")]
		public static void AssemblyInitialize(TestContext testContext)
			=> AssemblySetup.resetTraceSources = TestHelper.TraceAllVerbose();

		[AssemblyCleanup]
		public static void AssemblyCleanup()
			=> AssemblySetup.resetTraceSources?.Dispose();


		[TestMethod]
		public void NoOpAssemblySetupTestMethod() { }
	}
}
