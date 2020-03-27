using System;
using System.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sc.Composition.Tests.Types;


namespace Sc.Composition.Tests.Parts
{
	[Export(typeof(IParent))]
	internal class ParentMultiConstructor
			: AssertBase,
					IParent
	{
		public ParentMultiConstructor() { }

		public ParentMultiConstructor(IChild c)
			=> Child = c ?? throw new ArgumentNullException(nameof(c));

		[ImportingConstructor]
		public ParentMultiConstructor(ChildMultiConstructor c, BabyDefault b)
		{
			Child = c ?? throw new ArgumentNullException(nameof(c));
			Baby = b ?? throw new ArgumentNullException(nameof(b));
		}


		public IChild Child { get; }

		public IBaby Baby { get; }


		public override void AssertConstruction()
		{
			Assert.IsFalse(IsAnyDisposed);
			Assert.IsNotNull(Child);
			Assert.IsNotNull(Baby);
			Child.AssertConstruction();
			Baby.AssertConstruction();
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
