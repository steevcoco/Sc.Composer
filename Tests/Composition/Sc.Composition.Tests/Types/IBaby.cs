using System;


namespace Sc.Composition.Tests.Types
{
	public interface IBaby
			: IAssert
	{
		ArgumentException Ex { get; }
	}
}
