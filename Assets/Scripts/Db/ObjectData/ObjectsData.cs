using System.Collections.Generic;
using System.Linq;
using Core.Equipment.Data;
using Db.ObjectData.Impl;
using UnityEngine;
using Utils;

namespace Db.ObjectData
{
	
	[CreateAssetMenu(menuName = SoNames.SETTINGS + nameof(ObjectsData), fileName = nameof(ObjectsData))]
	public class ObjectsData : ScriptableObject, IObjectsData
	{
		[field:SerializeField] public ItemObjectDataSo DefaultSelected { get; set; }

		public CharacterDataSo[] CharacterDataSo;
		public ItemObjectDataSo[] ItemsObjectDataSo;
		public InventoryObjectDataSo[] InventoryObjectData;
		public BoostItemObjectDataSo[] BoostItemObjectDataSo;
		public AbilityItemObjectDataSo[] AbilityItemObjectDataSo;

		public IReadOnlyList<CharacterDataSo> CharactersData => CharacterDataSo;
		public IReadOnlyList<ItemObjectDataSo> ItemsData => ItemsObjectDataSo;
		public IReadOnlyList<InventoryObjectDataSo> InventoryData => InventoryObjectData;
		public IReadOnlyList<BoostItemObjectDataSo> BoostItemObjectData => BoostItemObjectDataSo;
		public IReadOnlyList<AbilityItemObjectDataSo> AbilityItemObjectData => AbilityItemObjectDataSo;

#if UNITY_EDITOR
		/// <summary>
		/// Editor Only
		/// </summary>
		public static ObjectsData EditorInstance;

		public static string[] AllIds;
		
		public void UpdateValues()
		{
			AllIds = GetNames().ToArray();
		}

		private IEnumerable<string> GetNames()
		{
			foreach (var data in CharacterDataSo)
			{
				yield return data.ObjectData.Id;
			}

			foreach (var data in ItemsData)
			{
				yield return data.ObjectData.Id;
			}
			
			foreach (var data in InventoryData)
			{
				yield return data.ObjectData.Id;
			}
			
			foreach (var data in BoostItemObjectDataSo)
			{
				yield return data.ObjectData.Id;
			}
			
			foreach (var data in AbilityItemObjectData)
			{
				yield return data.ObjectData.Id;
			}

			foreach (var data in EquipmentItemsData.EditorInstance.GetNames())
			{
				yield return data;
			}
		}

		public static IEnumerable<string> GetCharacters()
		{
			return EditorInstance.CharacterDataSo.Select(data => data.ObjectData.Id);
		}
		
		public static IEnumerable<string> GetAbilities()
		{
			return EditorInstance.AbilityItemObjectData.Select(data => data.ObjectData.Id);
		}

		private void OnEnable()
		{
			EditorInstance = this;
		}
#endif
	}

	public interface IObjectsData
	{
		ItemObjectDataSo DefaultSelected { get; }
		IReadOnlyList<CharacterDataSo> CharactersData { get; }
		IReadOnlyList<ItemObjectDataSo> ItemsData { get; }
		IReadOnlyList<InventoryObjectDataSo> InventoryData { get; }
		IReadOnlyList<BoostItemObjectDataSo> BoostItemObjectData { get; }
		IReadOnlyList<AbilityItemObjectDataSo> AbilityItemObjectData { get; }
	}
}