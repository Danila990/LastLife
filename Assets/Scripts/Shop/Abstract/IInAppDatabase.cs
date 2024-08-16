using System.Collections.Generic;
using Shop.Models.Product;

namespace Shop.Abstract
{
	public interface IInAppDatabase
	{
		IList<InAppProduct> GetProducts();
		InAppProduct GetProductById(string inAppID);
	}
}