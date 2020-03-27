using System;
using System.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Sc.Composition.Tests.Types
{
	[Export]
	internal class ChildMultiConstructor
			: AssertBase,
					IChild
	{
		public ChildMultiConstructor() { }

		[ImportingConstructor]
		public ChildMultiConstructor(BabyDefault b)
			=> Baby = b ?? throw new ArgumentNullException(nameof(b));


		public IBaby Baby { get; }

		public override void AssertConstruction()
		{
			Assert.IsFalse(IsAnyDisposed);
			Assert.IsNotNull(Baby);
			Baby.AssertConstruction();
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
