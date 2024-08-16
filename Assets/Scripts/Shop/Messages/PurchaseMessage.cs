using Shop.Models.Product;

namespace Shop.Messages
{
	public readonly struct PurchaseMessage
	{
		public readonly InAppProduct Product;
        
		public PurchaseMessage(InAppProduct productId)
		{
			Product = productId;
		}
	}
}