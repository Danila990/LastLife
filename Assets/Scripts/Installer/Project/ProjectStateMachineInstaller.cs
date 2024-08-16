using GameStateMachine.States.Impl.Project;
using VContainer;
using VContainer.Extensions;

namespace Installer.Project
{
	public class ProjectStateMachineInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
			builder.Register<ProjectInitializeState>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ProjectLoadSceneState>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<ProjectStateMachine>(Lifetime.Singleton)
				.AsImplementedInterfaces()
				.AsSelf();
		}
	}
}