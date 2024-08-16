using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Equipment.Data
{
	[CreateAssetMenu(menuName = SoNames.EQUIPMENT_DATA + nameof(EquipmentItemsData), fileName = "AllEquipment")]
	public class EquipmentItemsData : SerializedScriptableObject, IEquipmentItemsData
	{
		[SerializeField] private List<ScriptableJetPackItemDataSo> _jetPacksData;
		[SerializeField] private List<ScriptableBulletproofItemDataSo> _bulletproofItemData;
		[SerializeField] private List<ScriptableBootsItemDataSo> _bootsItemData;
		[SerializeField] private List<ScriptableHatItemDataSo> _hatsItemData;
	

		public IReadOnlyList<ScriptableJetPackItemDataSo> JetPacks => _jetPacksData;
		public IReadOnlyList<ScriptableBulletproofItemDataSo> BulletProofItems => _bulletproofItemData;
		public IReadOnlyList<ScriptableBootsItemDataSo> BootsItemData => _bootsItemData;
		public IReadOnlyList<ScriptableHatItemDataSo> HatsItemData => _hatsItemData;
		
#if UNITY_EDITOR
		public static EquipmentItemsData EditorInstance;
		
		private void OnEnable()
		{
			EditorInstance = this;
		}
		
		public IEnumerable<string> GetNames()
		{
			foreach (var data in _jetPacksData)
			{
				yield return data.ObjectData.Id;
			}

			foreach (var data in _bulletproofItemData)
			{
				yield return data.ObjectData.Id;
			}
			
			foreach (var data in _bootsItemData)
			{
				yield return data.ObjectData.Id;
			}
			
			foreach (var data in _hatsItemData)
			{
				yield return data.ObjectData.Id;
			}
		}
		
		public IEquipmentArgs FindArgs(string factoryId)
		{
			foreach (var item in JetPacks)
			{
				if (item.Model.Args.FactoryId == factoryId)
					return item.Model.Args.GetCopy();
			}
			foreach (var item in BulletProofItems)
			{
				if (item.Model.Args.FactoryId == factoryId)
					return item.Model.Args.GetCopy();
			}
			foreach (var item in BootsItemData)
			{
				if (item.Model.Args.FactoryId == factoryId)
					return item.Model.Args.GetCopy();
			}
			foreach (var item in HatsItemData)
			{
				if (item.Model.Args.FactoryId == factoryId)
					return item.Model.Args.GetCopy();
			}

			return null;
		} 
#endif
	}
	
	public interface IEquipmentItemsData
	{
		IReadOnlyList<ScriptableJetPackItemDataSo> JetPacks { get; }
		IReadOnlyList<ScriptableBulletproofItemDataSo> BulletProofItems { get; }
		IReadOnlyList<ScriptableBootsItemDataSo> BootsItemData { get; }
		IReadOnlyList<ScriptableHatItemDataSo> HatsItemData { get; }
	}
}
