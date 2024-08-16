using System.Collections.Generic;
using System.Linq;
using Db.ObjectData;
using Shop.Models;
using VContainer.Unity;

namespace Shop.Abstract
{
	public class ShopStorage : IShopStorage, IInitializable
	{
		private readonly IShopData _shopData;
		private readonly SortedList<string, ShopObjectItemModel<CharacterObjectData>> _characterShopItems = new SortedList<string, ShopObjectItemModel<CharacterObjectData>>();
		private readonly SortedList<string, IShopObjectItemModel> _shopObjectItemModels = new SortedList<string, IShopObjectItemModel>();
		public IReadOnlyDictionary<string, ShopObjectItemModel<CharacterObjectData>> CharacterShopItems => _characterShopItems;
		public IReadOnlyDictionary<string, IShopObjectItemModel> ObjectItemModels => _shopObjectItemModels;
		
		public ShopStorage(IShopData shopData)
		{
			_shopData = shopData;
		}
		
		public void Initialize()
		{
			foreach (var shopData in _shopData.CharacterShopItems)
			{
				_characterShopItems.Add(shopData.Model.Item.Model.Id, shopData.Model);
			}
			
			foreach (var shopItemModel in _shopData.GetAllShopItemModels().OfType<IShopObjectItemModel>())
			{
				_shopObjectItemModels.Add(shopItemModel.ShopItemModel.InAppId, shopItemModel);
			}
		}
	}
}