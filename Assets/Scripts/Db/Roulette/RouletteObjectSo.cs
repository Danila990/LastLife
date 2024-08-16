using Db.ObjectData.Impl;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Db.Roulette
{
	[CreateAssetMenu(menuName = "Settings/RouletteObjectSo", fileName = "RouletteObjectData")]
	public class RouletteObjectSo : ScriptableObject
	{
		[OnValueChanged("Rename")] public CharacterDataSo ObjectSo;
		[EnumPaging] public RouletteObjectRarity Rarity;

#if UNITY_EDITOR
		public void Rename()
		{
			if (!ObjectSo)
				return;
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), "Roulette_" + ObjectSo.ObjectData.Id);
		}
#endif
	}
	
	public enum RouletteObjectRarity
	{
		Common,
		Rare,
		Epic,
		Legendary
	}
}