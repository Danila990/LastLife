using Core.ResourcesSystem;
using Db.MerchantData;
using Db.ObjectData.Impl;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Editor.Windows
{
	public class MerchantItemsPage : BasePageEditor
	{
		public ScriptableObject ObjectDataSo;

		public MerchantShopItemData MerchantShopItemData;
		
		[FolderPath(RequireExistingPath = true)]
		public string PathAt = "Assets/Settings/Data";

		public override void Init()
		{
			PathAt = EditorPrefs.GetString("MerchantItemsPath", PathAt);
		}

		[Button]
		public void Clone()
		{
			var provider = ObjectDataSo as IObjectDataProvider;
			Debug.Assert(provider is not null);
			var model = provider.ObjectData;
			MerchantShopItemData = new MerchantShopItemData()
			{
				Ico = model.Ico,
				Id = model.Id + "_MerchantItem", 
				Name = model.Name,
				IsUnlocked = model.IsUnlocked,
				UnlockKey = model.UnlockKey + "_MerchantItem",
				ObjectDataIdToBuy = model.Id,
				ResourceType = ResourceType.GoldTicket,
				StoreItemType = StoreItemType.NonConsumable,
				Price = 1
			};
		}
		
		[Button]
		public void CreateItem()
		{
			EditorPrefs.SetString("MerchantItemsPath", PathAt);
			var instance = ScriptableObject.CreateInstance<MerchantShopItemDataSo>();
			var path = PathAt + $"/{MerchantShopItemData.Id}.asset";
			AssetDatabase.CreateAsset(instance, AssetDatabase.GenerateUniqueAssetPath(path));
			AssetDatabase.SaveAssets();
            
			instance.EditorSet(MerchantShopItemData);
			EditorUtility.SetDirty(instance);
			Selection.activeObject = instance;
		}
	}
}