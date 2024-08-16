using System;
using Db.MerchantData;
using Dialogue.Services.Interfaces;
using UniRx;

namespace Dialogue.Services.Modules.MerchantShop
{
	public abstract class AbstractMerchantShopPresenter : IDisposable
	{
		public abstract void OpenFor(IMerchantItemCollectionData collection, string merchantName, string msg, ShopDialogueModuleArgs instanceShopDialogueModuleArgs);
		protected abstract void CreatePresenter(MerchantShopItemDataSo item);
		public abstract void OnClickSubmit(Unit obj);
		public virtual void Dispose()
		{
		
		}
	}

}