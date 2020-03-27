using System;
using System.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sc.Composition.Tests.Types;


namespace Sc.Composition.Tests.Parts
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
