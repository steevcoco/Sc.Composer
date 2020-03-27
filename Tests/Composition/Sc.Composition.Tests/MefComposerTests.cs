using System.Collections.Generic;
using System.Composition.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sc.Composer;
using Sc.Composer.Mef;
using Sc.Composer.Mef.Providers;
using Sc.Composition.Tests.Types;
using Sc.IO.Files;
using Sc.Util.Collections;


namespace Sc.Composition.Tests
{
	[TestClass]
	public class MefComposerTests
	{
		[TestMethod]
		public void TestMefAssemblyPartProvider()
		{
			// Shared
			MefComposer composer = new MefComposer(Helper.NewParentConventions(true));
			composer.Participate(
					new MefAssemblyPartProvider(
							GetType()
									.Assembly.AsSingle()));
			assertExportsAndDispose(composer);

			// Not Shared
			composer = new MefComposer(Helper.NewParentConventions(false));
			composer.Participate(
					new MefAssemblyPartProvider(
							GetType()
									.Assembly.AsSingle()));
			assertExportsAndDispose(composer);
		}

		[TestMethod]
		public void TestMefDirectoryPartWatcher()
		{
			using (Helper.GetTempDir(
					nameof(MefComposerTests.TestMefDirectoryPartWatcher),
					out string tempDir)) {
				// Shared
				MefComposer composer = new MefComposer(Helper.NewParentConventions(true));
				assertWithMefDirectoryPartWatcher(
						new MefDirectoryPartWatcher(new DirectoryWatcher(tempDir, "*.dll")),
						composer);

				// Not Shared
				composer = new MefComposer(Helper.NewParentConventions(false));
				assertWithMefDirectoryPartWatcher(
						new MefDirectoryPartWatcher(new DirectoryWatcher(tempDir, "*.dll")),
						composer);
			}
		}


		private void assertWithMefDirectoryPartWatcher(MefDirectoryPartWatcher contributor, MefComposer composer)
		{
			int compositionRequestedCount = 0;
			contributor.CompositionRequested += (sender, compositionEventArgs) => ++compositionRequestedCount;
			composer.Participate(contributor);
			assertExportsAndDispose(composer);
			Assert.AreEqual(0, compositionRequestedCount); // Zero in both cases
		}

		private void assertExportsAndDispose(MefComposer composer)
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

			CompositionHost compositionHost = lastComposedEventArgs.Target.CreateContainer();
			List<IParent> parts = new List<IParent>(3);
			parts.AddRange(compositionHost.GetExports<IParent>());
			Assert.AreEqual(3, parts.Count);

			Helper.AssertValidParents(parts);
			composer.Dispose();
			Helper.AssertValidParents(parts);
			compositionHost.Dispose();
			Helper.AssertDisposedParents(parts);
		}
	}
}
