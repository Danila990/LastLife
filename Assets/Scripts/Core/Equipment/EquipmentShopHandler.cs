using System;
using Core.Entity.Characters;
using Core.Equipment.Data;
using Core.InputSystem;
using Core.Services;
using Db.ObjectData;
using MessagePipe;
using UniRx;
using VContainer.Unity;

namespace Core.Equipment
{
	public class EquipmentShopHandler : IPostInitializable, IDisposable
	{
		private readonly IItemUnlockService _unlockService;
		private readonly ISubscriber<PlayerContextChangedMessage> _subscriber;
		private readonly IEquipmentInventoryService _equipmentInventoryService;

		private CharacterContext _currentContext;

		private IDisposable _contextSub;
		private IDisposable _unlockSub;
		
		public EquipmentShopHandler(
			IItemUnlockService unlockService,
			ISubscriber<PlayerContextChangedMessage> subscriber,
			IEquipmentInventoryService equipmentInventoryService)
		{
			_unlockService = unlockService;
			_subscriber = subscriber;
			_equipmentInventoryService = equipmentInventoryService;
		}
		
		public void PostInitialize()
		{
			_contextSub = _subscriber.Subscribe(OnContextChanged);
		}

		public void Dispose()
		{
			_contextSub?.Dispose();
			_unlockSub?.Dispose();
		}
		
		private void OnContextChanged(PlayerContextChangedMessage msg)
		{
			_currentContext = msg.CharacterContext;
			
			if(!msg.Created)
				return;
			
			_unlockSub?.Dispose();
			_unlockService.OnItemUnlock.Subscribe(OnItemUnlock);
		}
		
		private void OnItemUnlock(ObjectData data)
		{
			if(_currentContext == null)
				return;

			if (data is EquipmentItemData equipmentItemData)
				_equipmentInventoryService.AddNewEquipment(equipmentItemData.Args);
		}

	}

}
