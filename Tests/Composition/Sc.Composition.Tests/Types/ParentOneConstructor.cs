using System;
using System.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Sc.Composition.Tests.Types
{
	[Export(typeof(IParent))]
	internal class ParentOneConstructor
			: AssertBase,
					IParent
	{
		[ImportingConstructor]
		public ParentOneConstructor(ChildDefault c)
			=> Child = c ?? throw new ArgumentNullException(nameof(c));


		public IChild Child { get; }

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public IBaby Baby { get; }

		public override void AssertConstruction()
		{
			Assert.IsFalse(IsAnyDisposed);
			Assert.IsNotNull(Child);
			Assert.IsNull(Baby);
			Child.AssertConstruction();
		}

		public override bool IsAnyDisposed
			=> base.IsAnyDisposed
					|| (Child?.IsAnyDisposed ?? false)
					|| (Baby?.IsAnyDisposed ?? false);

		public override bool IsAllDisposed
			=> base.IsAnyDisposed
					&& (Child?.IsAnyDisposed ?? true)
					&& (Baby?.IsAnyDisposed ?? true);

		public override void Dispose()
		{
			base.Dispose();
			Child?.Dispose();
			Baby?.Dispose();
		}
	}
}
