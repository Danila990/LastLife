// using System;
// using System.Threading;
// using Adv.Providers;
// using Adv.Services.Interfaces;
// using Cysharp.Threading.Tasks;
// using UniRx;
// using VContainer.Unity;
//
// namespace Adv.Services
// {
//     public class SandboxAdvService : IInitializable, IDisposable, ISandboxAdvService
// 	{
// 		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
// 		private readonly float _advTimer = 100;
//         private readonly IAdvProvider _advProvider;
//         private readonly IRemoveAdsService _removeAdsService;
//         private IDisposable _disposable;
// 		private CancellationTokenSource _restartSource;
//
// 		public SandboxAdvService(
//             IAdvProvider advProvider,
// 			IRemoveAdsService removeAdsService)
// 		{
//             _advProvider = advProvider;
//             _removeAdsService = removeAdsService;
// 		}
//
// 		public void Initialize()
// 		{
// 			_restartSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
// 			if (_removeAdsService.IsRemoveAdsEnabled)
// 				return;
// 			
// 			//_removeAdsService.StopAdv += RemoveAdsServiceOnStopAdv;
// 			_advProvider.ShowedReward.Subscribe(OnRewardShown).AddTo(_cts.Token);
// 			_advProvider.HidedReward.Subscribe(OnHideReward).AddTo(_cts.Token);
// 			
// 			StartAdvTimer(_restartSource.Token).Forget();
// 		}
//
// 		
// 		public void RewardRequest(Action action, string rewardString) => _advProvider.ShowReward(action, rewardString);
//
// 		public void StopAdv() => PauseAdv();
//
// 		public void PauseAdv()
// 		{
// 			if (_restartSource is { IsCancellationRequested : false})
// 			{
// 				_restartSource.Cancel();
// 				_restartSource.Dispose();
// 			}
// 		}
//
// 		public void RestartAdv()
// 		{
// 			PauseAdv();
// 			
// 			if (_removeAdsService.IsRemoveAdsEnabled)
// 				return;
// 			
// 			_restartSource = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
// 			StartAdvTimer(_restartSource.Token).Forget();
// 		}
//         
// 		private void OnHideReward(Unit obj) => RestartAdv();
// 		private void OnRewardShown(Unit obj) => StopAdv();
// 		private void OnInterstitialAdClosed() => RestartAdv();
// 		private void RemoveAdsServiceOnStopAdv() => StopAdv();
//
// 		private async UniTask StartAdvTimer(CancellationToken restartSourceToken)
// 		{
// 			if (_removeAdsService.IsRemoveAdsEnabled)
// 				return;
// 			
// 			await UniTask.Delay(TimeSpan.FromSeconds(_advTimer),true, cancellationToken: restartSourceToken);
//
// 			if (!_advProvider.InterstitialIsLoaded)
// 			{
// 				if (restartSourceToken.IsCancellationRequested)
// 					return;
// 				StartAdvTimer(restartSourceToken).Forget();
// 				return;
// 			}
// 			
// 			if (restartSourceToken.IsCancellationRequested)
// 				return;
// 			
// 			_advProvider.ShowInterstitial(OnInterstitialAdClosed);
// 		}
//
// 		public void Dispose()
// 		{
// 			if (!_cts.IsCancellationRequested)
// 			{
// 				_cts.Cancel();
// 				_cts.Dispose();
// 			}
// 			_disposable?.Dispose();
// 		}
// 	}
// }