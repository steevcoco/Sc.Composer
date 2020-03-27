using System.Composition;
using ComponentInterfaces;
using Sc.Abstractions.ServiceLocator;
using Sc.Composer;


namespace Component2
{
	[Export(typeof(IComposerParticipant<IContainerBase>))]
	internal class ContainerParticipant
		: IProvideParts<IContainerBase>
	{
		public void ProvideParts<T>(ProvidePartsEventArgs<T> eventArgs)
			where T : IContainerBase
			=> eventArgs.Target.RegisterType<IComponent2, Component2>();
	}
}
