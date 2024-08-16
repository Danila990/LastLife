using System;
using Analytic;
using Core.Actions.SpecialAbilities;
using Core.InputSystem;
using Core.Services;
using MessagePipe;
using VContainer.Unity;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class LocalAbilityPurchase : IInitializable, IDisposable
	{
		private readonly ISubscriber<MessageObjectLocalPurchase> _purchaseSubscriber;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IAbilitiesControllerService _abilitiesControllerService;
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly IAnalyticService _analyticService;
		private IDisposable _disposable;
		
		public LocalAbilityPurchase(
			ISubscriber<MessageObjectLocalPurchase> purchaseSubscriber,
			IItemUnlockService itemUnlockService,
			IAbilitiesControllerService abilitiesControllerService,
			IPlayerSpawnService playerSpawnService,
			IAnalyticService analyticService
		)
		{
			_purchaseSubscriber = purchaseSubscriber;
			_itemUnlockService = itemUnlockService;
			_abilitiesControllerService = abilitiesControllerService;
			_playerSpawnService = playerSpawnService;
			_analyticService = analyticService;
		}
		
		public void Initialize()
		{
			_disposable = _purchaseSubscriber.Subscribe(OnPurchase);
		}
		
		private void OnPurchase(MessageObjectLocalPurchase obj)
		{
			if (_abilitiesControllerService.AbilityControllers.TryGetValue(obj.BoughtItemId, out var abilityController))
			{
				var context = _playerSpawnService.PlayerCharacterAdapter.CurrentContext;
				_abilitiesControllerService.ConnectAbilityToPlayer(context, obj.BoughtItemId, true);
				_abilitiesControllerService.SaveAbilityConnection(_playerSpawnService.ActiveCharacterData.Id, obj.BoughtItemId);
				_itemUnlockService.UnlockItem(abilityController.Data);
				_analyticService.SendEvent($"LocalPurchase:Modificator:{obj.BoughtItemId}");
			}
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}