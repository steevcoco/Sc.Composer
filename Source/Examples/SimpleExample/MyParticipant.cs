using System;
using Sc.Composer;


namespace SimpleExample
{
	internal class MyParticipant
		: IProvideParts<MyTarget>,
		IBootstrap<MyTarget>
	{
		public void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
			where T : MyTarget
		{
			eventArgs.Target.AddPart(new MyPart());
			eventArgs.Target.AddPart(new MyPart());
			eventArgs.Target.AddPart(new MyPart());
		}

		public void HandleBootstrap<T>(ComposerEventArgs<T> eventArgs)
			where T : MyTarget
			=> eventArgs.Target.Bootstrap();
	}
}
