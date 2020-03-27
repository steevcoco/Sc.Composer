using System.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sc.Composition.Tests.Types;


namespace Sc.Composition.Tests.Parts
{
	[Export]
	internal class ChildDefault
			: AssertBase,
					IChild
	{
		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IBaby Baby { get; }

		public override void AssertConstruction()
		{
			Assert.IsFalse(IsAnyDisposed);
			Assert.IsNull(Baby);
		}

		public override bool IsAnyDisposed
			=> base.IsAnyDisposed
					|| (Baby?.IsAnyDisposed ?? false);

		public override bool IsAllDisposed
			=> base.IsAnyDisposed
					&& (Baby?.IsAnyDisposed ?? true);

		public override void Dispose()
		{
			base.Dispose();
			Baby?.Dispose();
		}
	}
}
