namespace Sc.Composition.Tests.Types
{
	public interface IParent
			: IAssert
	{
		IChild Child { get; }

		IBaby Baby { get; }
	}
}
