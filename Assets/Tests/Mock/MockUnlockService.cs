using System;
using Core.Services;
using Db.ObjectData;
using Shop;
using Shop.Models;
using Shop.Models.Product;
using UniRx;

namespace Tests.Mock
{

	public class MockUnlockService : IItemUnlockService, IDisposable
	{
		private readonly ReactiveCommand<ObjectData> _onItemUnlock = new ReactiveCommand<ObjectData>();

		public IObservable<ObjectData> OnItemUnlock => _onItemUnlock;
	
		public void UnlockBundle(BundleShopItemModel value)
		{
			return;
		}
		
		public bool BundleIsUnlocked(BundleShopItemModel value)
		{
			return false;
		}

		public void UnlockItemOnPurchase(ObjectData objectData, InAppProduct purchasable)
		{
			_onItemUnlock.Execute(objectData);
		}
		
		public void UnlockItem(ObjectData objectData)
		{
			_onItemUnlock.Execute(objectData);
		}
		
		public void LockItem(ObjectData objectData)
		{
		}
		
		public bool IsUnlocked(ObjectData objectData)
		{
			return true;
		}
		
		public bool IsPurchased(IPurchasable purchasable)
		{
			return true;
		}
		
		public void Dispose()
		{
			_onItemUnlock?.Dispose();
		}
	}
}