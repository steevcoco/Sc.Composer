namespace Sc.Composition.Tests.Types
{
	public abstract class AssertBase
			: IAssert
	{
		private bool isDisposed;


		public abstract void AssertConstruction();

		public virtual bool IsAnyDisposed
			=> isDisposed;

		public virtual bool IsAllDisposed
			=> isDisposed;

		public virtual void Dispose()
			=> isDisposed = true;
	}
}
