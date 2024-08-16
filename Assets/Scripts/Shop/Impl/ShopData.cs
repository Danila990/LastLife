using System.Collections.Generic;
using System.Linq;
using Shop.Abstract;
using Shop.ItemProvider;
using Shop.Models;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Shop.Impl
{
	[CreateAssetMenu(menuName = SoNames.SHOP + nameof(ShopData), fileName = nameof(ShopData))]
	public class ShopData : ScriptableObject, IShopData
	{
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private ScriptableTicketShopItem[] _ticketShopItems;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private CharacterShopItem[] _characterShopItems;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private ScriptableShopBulletproofItem[] _shopBulletproofItems;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private ScriptableShopJetPackItem[] _shopJetpackItems;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private ScriptableShopBootsItem[] _shopBootsItems;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private ScriptableShopHatItem[] _shopHatItems;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private ScriptableBoostShopItem[] _bootsItems;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private BundleScriptableShopItem[] _bundleScriptable;
		[SerializeField, AssetSelector(Paths = "Assets/Settings/Data/Shop/ShopItems")] private RemoveAdsScriptableItem _removeAdsModel;
		
		public IReadOnlyList<ScriptableTicketShopItem> TicketShopItems => _ticketShopItems;
		public IReadOnlyList<CharacterShopItem> CharacterShopItems => _characterShopItems;
		public IReadOnlyList<ScriptableShopBulletproofItem> ShopBulletproofItems => _shopBulletproofItems;
		public IReadOnlyList<ScriptableShopJetPackItem> ShopJetpackItems => _shopJetpackItems;
		public IReadOnlyList<ScriptableShopBootsItem>  ShopBootsItems => _shopBootsItems;
		public IReadOnlyList<ScriptableShopHatItem> ShopHatItems => _shopHatItems;
		public IReadOnlyList<ScriptableBoostShopItem> BoostsItems => _bootsItems;
		public IReadOnlyList<BundleScriptableShopItem> BundleScriptable => _bundleScriptable;
		public RemoveAdsScriptableItem RemoveAdsModel => _removeAdsModel;

		public IEnumerable<ShopItemModel> GetAllShopItemModels()
		{
			foreach (var item in _ticketShopItems.Select(item => item.Model))
			{
				yield return item;
			}

			foreach (var item in _characterShopItems.Select(item => item.Model))
			{
				yield return item;
			}
			
			foreach (var item in _shopBulletproofItems.Select(item => item.Model))
			{
				yield return item;
			}
			
			foreach (var item in _shopJetpackItems.Select(item => item.Model))
			{
				yield return item;
			}
			
			foreach (var item in _shopBootsItems.Select(item => item.Model))
			{
				yield return item;
			}
			
			foreach (var item in _shopHatItems.Select(item => item.Model))
			{
				yield return item;
			}
			
			foreach (var item in _bootsItems.Select(item => item.Model))
			{
				yield return item;
			}
			
			yield return _removeAdsModel.Model;
		}
	}
}