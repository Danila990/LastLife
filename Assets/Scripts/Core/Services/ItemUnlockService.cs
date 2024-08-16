using System;
using Db.ObjectData;
using SharedUtils.PlayerPrefs;
using Shop;
using Shop.Abstract;
using Shop.Models;
using Shop.Models.Product;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Core.Services
{
	public interface IItemUnlockService
	{
		void UnlockItemOnPurchase(ObjectData objectData, InAppProduct purchasable);
		void UnlockItem(ObjectData objectData);
		/// <summary>
		/// Only used in cheats
		/// </summary>
		/// <param name="objectData"></param>
		void LockItem(ObjectData objectData);
		bool IsUnlocked(ObjectData objectData);
		bool IsPurchased(IPurchasable purchasable);
		IObservable<ObjectData> OnItemUnlock { get; }
		void UnlockBundle(BundleShopItemModel value);
		bool BundleIsUnlocked(BundleShopItemModel value);
	}
	
	public class ItemUnlockService : IItemUnlockService, IDisposable
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly IInAppDatabase _inAppDatabase;
		private readonly IShopStorage _shopStorage;
		private readonly ReactiveCommand<ObjectData> _onItemUnlock = new ReactiveCommand<ObjectData>();

		public IObservable<ObjectData> OnItemUnlock => _onItemUnlock;

		public ItemUnlockService(
			IPlayerPrefsManager playerPrefsManager,
			IInAppDatabase inAppDatabase,
			IShopStorage shopStorage)
		{
			_playerPrefsManager = playerPrefsManager;
			_inAppDatabase = inAppDatabase;
			_shopStorage = shopStorage;
		}

		public void UnlockItemOnPurchase(ObjectData objectData, InAppProduct purchasable)
		{
			_playerPrefsManager.SetValue(objectData.UnlockKey, true);
			if (purchasable.ProductType == ProductType.NonConsumable)
			{
				_playerPrefsManager.SetValue(purchasable.InAppID, true);
			}
			_onItemUnlock.Execute(objectData);
		}
		
		public void UnlockBundle(BundleShopItemModel value)
		{
			foreach (var item in value.ShopItemIds)
			{
				if (_shopStorage.ObjectItemModels.TryGetValue(item, out var itemModel))
				{
					UnlockItemOnPurchase(itemModel.ObjectData, _inAppDatabase.GetProductById(item));
				}
				else
				{
					Debug.LogWarning($"Cant unlock bundle item {item}");
				}
			}
		}

		public bool BundleIsUnlocked(BundleShopItemModel value)
		{
			foreach (var item in value.ShopItemIds)
			{
				if (_shopStorage.ObjectItemModels.TryGetValue(item, out var itemModel))
				{
					var isUnlocked = IsUnlocked(itemModel.ObjectData) || IsPurchased(itemModel.ShopItemModel);
					if (!isUnlocked)
					{
						return false;
					}
				}
			}
			
			return true;
		}
		
		public void UnlockItem(ObjectData objectData)
		{
#if UNITY_EDITOR
			if (!objectData.IsUnlocked && string.IsNullOrEmpty(objectData.UnlockKey))
			{
				Debug.LogWarning($"[UNLOCK SERVICE] unlock key not fill for {objectData.Id}");
			}
#endif
			_playerPrefsManager.SetValue(objectData.UnlockKey, true);
			_onItemUnlock.Execute(objectData);
		}

		public void LockItem(ObjectData objectData)
		{
			_playerPrefsManager.DeleteKey(objectData.UnlockKey);
		}

		public bool IsUnlocked(ObjectData objectData)
		{
#if UNITY_EDITOR
			if (!objectData.IsUnlocked && string.IsNullOrEmpty(objectData.UnlockKey))
			{
				Debug.LogWarning($"[UNLOCK SERVICE] unlock key not fill for {objectData.Id}");
			}
#endif
			return objectData.IsUnlocked || _playerPrefsManager.GetValue(objectData.UnlockKey, false);
		}
		
		public bool IsPurchased(IPurchasable purchasable)
		{
			return _playerPrefsManager.GetValue(purchasable.InAppId, false);
		}
		
		public void Dispose()
		{
			_onItemUnlock?.Dispose();
		}
	}
}