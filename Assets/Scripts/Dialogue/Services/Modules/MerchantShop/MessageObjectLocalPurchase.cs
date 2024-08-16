namespace Dialogue.Services.Modules.MerchantShop
{
	public readonly struct MessageObjectLocalPurchase
	{
		public readonly string BoughtItemId;
		public readonly int Quantity;
		public readonly object MetaData;
		
		public MessageObjectLocalPurchase(string boughtItemId, int quantity, object metaData)
		{
			BoughtItemId = boughtItemId;
			Quantity = quantity;
			MetaData = metaData;
		}
	}
}