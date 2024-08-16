using Core.Services;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Cheats.Impl
{
	public class SetBossTimerCheatCommand : ICheatCommandProvider
	{
		public string ButtonText => "speed up \n boss";
		public bool IsToggle => false;
		
		public void Execute(bool status)
		{
			var scope = LifetimeScope.Find<LifetimeScope>(SceneManager.GetActiveScene());
			var service = scope.Container.Resolve<IBossSpawnService>();
			if (service.CurrentSpanTimer.Value > 0)
			{
				service.SetTimerTime(0);
			}
			else if (service is BossSpawnService bossSpawnService)
			{
				bossSpawnService.ManualSkipBoss();
			}
		}
	}

}