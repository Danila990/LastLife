using Db.MerchantData;

namespace Dialogue.Services.Modules.MerchantShop
{
	public abstract class AbstractMerchantCardPresenter<T> 
		where T : MerchantShopItemData
	{
		public abstract int Price { get; }
		public abstract bool IsConsumable { get; }
		public T Data { get; }
		
		public AbstractMerchantCardPresenter(T data)
		{
			Data = data;
		}
	}
}