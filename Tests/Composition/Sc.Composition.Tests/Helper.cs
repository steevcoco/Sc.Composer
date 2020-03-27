using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sc.Composition.Tests.Types;
using Sc.Util.System;


namespace Sc.Composition.Tests
{
	internal static class Helper
	{
		public static string PartsProjectName
			=> "Sc.Composition.Tests.Parts";


		public static IDisposable GetTempDir(string folderName, out string tempDir)
		{
			tempDir = Path.Combine(Path.GetTempPath(), $"{typeof(MefComposerTests).GetFriendlyName()}.{folderName}");
			string path = tempDir;
			void DisposeTempDir()
			{
				try {
					Directory.Delete(path, true);
				} catch {
					// Ignored
				}
			}
			DisposeTempDir();
			try {
				Directory.CreateDirectory(tempDir);
				string GetDllPath(bool isDebug)
				{
					string result = new Uri(typeof(MefComposerTests).Assembly.CodeBase).LocalPath;
					result = Path.GetDirectoryName(
							Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(result))));
					// ReSharper disable once AssignNullToNotNullAttribute
					result = Path.Combine(Path.Combine(result, Helper.PartsProjectName), "bin");
					result = Path.Combine(
							result,
							isDebug
									? "Debug"
									: "Release");
					return Path.Combine(result, Helper.PartsProjectName + ".dll");
				}
				bool useDebugPath =
#if DEBUG
						true;
#else
						false;
#endif
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				string partsPath = GetDllPath(useDebugPath);
				if (!File.Exists(partsPath))
						// ReSharper disable once ConditionIsAlwaysTrueOrFalse
					partsPath = GetDllPath(!useDebugPath);
				File.Copy(
						partsPath,
						Path.Combine(
								tempDir,
								Helper.PartsProjectName + ".dll"));
				return DelegateDisposable.With(DisposeTempDir);
			} catch (Exception exception) {
				DisposeTempDir();
				throw new AssertFailedException(
						$"Couldn't create temp assembly directory: {exception.Message}",
						exception);
			}
		}

		public static ConventionBuilder NewParentConventions(bool isShared)
		{
			ConventionBuilder conventions = new ConventionBuilder();
			if (isShared) {
				conventions.ForTypesDerivedFrom(typeof(IParent))
						.Export()
						.Shared();
			} else {
				conventions.ForTypesDerivedFrom(typeof(IParent))
						.Export();
			}
			return conventions;
		}

		public static void AssertValidParents(IReadOnlyCollection<IParent> exports)
		{
			Assert.AreEqual(3, exports.Count);
			foreach (IParent parent in exports) {
				Assert.IsNotNull(parent);
				parent.AssertConstruction();
				Assert.IsFalse(parent.IsAllDisposed);
			}
		}

		public static void AssertDisposedParents(IReadOnlyCollection<IParent> exports)
		{
			Assert.AreEqual(3, exports.Count);
			foreach (IParent parent in exports) {
				Assert.ThrowsException<AssertFailedException>(() => parent.AssertConstruction());
				Assert.IsTrue(parent.IsAllDisposed);
			}
		}
	}
}
