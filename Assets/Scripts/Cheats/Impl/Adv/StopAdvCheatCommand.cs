using Adv.Services;
using Adv.Services.Interfaces;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Cheats.Impl.Adv
{
	public class StopAdvCheatCommand : ICheatCommandProvider
	{
		public string ButtonText => "ads off";
		public bool IsToggle => true;
		
		public void Execute(bool on)
		{
			var removeAdsService = (RemoveAdsService)LifetimeScope.Find<LifetimeScope>(SceneManager.GetActiveScene()).Container.Resolve<IRemoveAdsService>();
			if (on)
			{
				removeAdsService.RemoveAds(true);
				removeAdsService.IsRemoveRewardAds = true;
			}
			else
			{
				removeAdsService.ReApplyAds();
			}
		}
	}
}