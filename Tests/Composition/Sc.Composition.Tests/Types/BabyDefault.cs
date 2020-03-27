using System;
using System.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Sc.Composition.Tests.Types
{
	[Export]
	internal class BabyDefault
			: AssertBase,
					IBaby
	{
		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public ArgumentException Ex { get; }

		public override void AssertConstruction()
			=> Assert.IsNull(Ex);
	}
}
