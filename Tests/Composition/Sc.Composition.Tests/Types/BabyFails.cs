using System;
using System.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Sc.Composition.Tests.Types
{
	[Export]
	internal class BabyFails
			: AssertBase,
					IBaby
	{
		[ImportingConstructor]
		public BabyFails(string s)
			=> BabyFailsString = string.IsNullOrWhiteSpace(s)
					? throw new ArgumentNullException(nameof(s))
					: s;


		public string BabyFailsString { get; }

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public ArgumentException Ex { get; }

		public override void AssertConstruction()
		{
			Assert.IsNull(Ex);
			Assert.IsFalse(string.IsNullOrWhiteSpace(BabyFailsString));
		}
	}
}
