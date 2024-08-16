using Shop.Models;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Shop.ItemProvider
{

	public abstract class ScriptableShopItem<T> : ScriptableObject where T : ShopItemModel
	{
		[InlineProperty]
		[HideLabel]
		public T Model;
		
#if UNITY_EDITOR
		[Button]
		public void Rename()
		{
			if (Model == null)
				return;
			var newName = Model.InAppId + "_ShopItem";
			name = newName;
			UnityEditor.AssetDatabase.RenameAsset(UnityEditor.AssetDatabase.GetAssetPath(this), newName);
			UnityEditor.AssetDatabase.SaveAssets();
		}
		
		[Button]
		public void ParseMoney()
		{
			if (Model == null)
				return;
			
			float.TryParse(Model.Price.Replace("$", "").Trim().Replace(".", ","), out var result);
			Model.PriceUSDNumber = result;
		}
#endif
	}

}