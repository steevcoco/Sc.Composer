using System.Composition.Hosting;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sc.Composer;
using Sc.Composer.Mef;
using Sc.Composer.Mef.Composers;
using Sc.Composer.Mef.Providers;
using Sc.Composition.Tests.Types;
using Sc.IO.Files;
using Sc.Util.Collections;


namespace Sc.Composition.Tests
{
	[TestClass]
	public class MefSingletonComposerTests
	{
		[TestMethod]
		public void TestMefSingletonComposerWithAssemblies()
		{
			MefSingletonComposer composer = MefSingletonComposer.ForSingleType<IParent>(true);
			composer.Participate(
					new MefAssemblyPartProvider(
							GetType()
									.Assembly.AsSingle()));
			assertExportsAndDispose(composer);

			// Don't dispose
			composer = MefSingletonComposer.ForSingleType<IParent>(true, false);
			composer.Participate(
					new MefAssemblyPartProvider(
							GetType()
									.Assembly.AsSingle()));
			assertExportsAndDispose(composer);
		}

		[TestMethod]
		public void TestMefSingletonComposerWithDirectory()
		{
			using (Helper.GetTempDir(
					nameof(MefSingletonComposerTests.TestMefSingletonComposerWithDirectory),
					out string tempDir)) {
				// Dispose
				MefSingletonComposer composer = MefSingletonComposer.ForSingleType<IParent>(true);
				assertWithMefDirectoryPartWatcher(
						new MefDirectoryPartWatcher(new DirectoryWatcher(tempDir, "*.dll")),
						composer);

				// Don't dispose
				composer = MefSingletonComposer.ForSingleType<IParent>(true, false);
				assertWithMefDirectoryPartWatcher(
						new MefDirectoryPartWatcher(new DirectoryWatcher(tempDir, "*.dll")),
						composer);
			}
		}



		private void assertWithMefDirectoryPartWatcher(
				MefDirectoryPartWatcher contributor,
				MefSingletonComposer composer)
		{
			int compositionRequestedCount = 0;
			contributor.CompositionRequested += (sender, compositionEventArgs) => ++compositionRequestedCount;
			composer.Participate(contributor);
			assertExportsAndDispose(composer);
			Assert.AreEqual(0, compositionRequestedCount); // Zero in both cases
		}

		private void assertExportsAndDispose(MefSingletonComposer composer)
		{
			int composedCount = 0;
			MefComposer lastComposer = null;
			ComposerEventArgs<ContainerConfiguration> lastComposedEventArgs = null;
			composer.Composed += (s, e) =>
			{
				++composedCount;
				lastComposer = s as MefComposer;
				lastComposedEventArgs = e;
			};

			Assert.AreSame(composer.Compose(), lastComposedEventArgs.Target);
			Assert.AreEqual(1, composedCount);
			Assert.IsNotNull(lastComposedEventArgs);
			Assert.IsNotNull(lastComposedEventArgs.Target);
			Assert.IsNotNull(lastComposer);
			Assert.AreSame(composer, lastComposer);

			IParent[] exports
					= composer.ExportsList.OfType<IParent>()
							.ToArray();
			Helper.AssertValidParents(exports);
			composer.Dispose();
			if (composer.DisposeCompositionHostsWithThis)
				Helper.AssertDisposedParents(exports);
			else
				Helper.AssertValidParents(exports);
		}
	}
}
