using System.Collections.Generic;
using Core.Equipment;
using Core.Equipment.Data;
using Core.ResourcesSystem;
using Db.ObjectData;
using VContainer.Unity;

namespace Core.Services
{
	public interface IItemStorage
	{
		IReadOnlyDictionary<string, ObjectData> All { get; }
		IReadOnlyDictionary<string, InventoryObjectData> InventoryItems { get; }
		IReadOnlyDictionary<EquipmentPartType, IReadOnlyList<EquipmentItemData>> EquipmentByType { get; }
		IReadOnlyDictionary<string, CharacterObjectData> Characters { get; }
		IReadOnlyDictionary<string, BoostObjectData> BoostObjects { get; }
		IReadOnlyDictionary<string, BoostObjectData> BoostsByBoostType { get; }
		IReadOnlyDictionary<string, AbilityObjectData> AbilityObjects { get; }
	}
	
	public class ItemStorage : IInitializable, IItemStorage
	{
		private readonly IObjectsData _objectsData;
		private readonly IEquipmentItemsData _equipmentItemsData;
		
		private readonly Dictionary<string, CharacterObjectData> _characters = new Dictionary<string, CharacterObjectData>();
		private readonly Dictionary<string, ObjectData> _all = new Dictionary<string, ObjectData>();
		private readonly Dictionary<string, InventoryObjectData> _inventoryItems = new Dictionary<string, InventoryObjectData>();
		private readonly Dictionary<string, BoostObjectData> _boosts = new Dictionary<string, BoostObjectData>();
		/// <summary>
		/// Key is BoostType
		/// </summary>
		private readonly SortedList<string, BoostObjectData> _boostsByBoostType = new SortedList<string, BoostObjectData>();
		private readonly SortedList<string, AbilityObjectData> _abilityObjects = new SortedList<string, AbilityObjectData>();
		
		private readonly Dictionary<EquipmentPartType, IReadOnlyList<EquipmentItemData>> _equipmentByType = new Dictionary<EquipmentPartType, IReadOnlyList<EquipmentItemData>>();
		private readonly Dictionary<ResourceType, IReadOnlyList<EquipmentItemData>> _resourceByType = new Dictionary<ResourceType, IReadOnlyList<EquipmentItemData>>();

		public IReadOnlyDictionary<EquipmentPartType, IReadOnlyList<EquipmentItemData>> EquipmentByType => _equipmentByType;
		public IReadOnlyDictionary<ResourceType, IReadOnlyList<EquipmentItemData>> ResourceByType => _resourceByType;
		public IReadOnlyDictionary<string, CharacterObjectData> Characters => _characters;
		public IReadOnlyDictionary<string, ObjectData> All => _all;
		public IReadOnlyDictionary<string, InventoryObjectData> InventoryItems => _inventoryItems;
		
		public IReadOnlyDictionary<string, BoostObjectData> BoostObjects => _boosts;
		public IReadOnlyDictionary<string, BoostObjectData> BoostsByBoostType => _boostsByBoostType;
		public IReadOnlyDictionary<string, AbilityObjectData> AbilityObjects => _abilityObjects;
		
		public ItemStorage(IObjectsData objectsData, IEquipmentItemsData equipmentItemsData)
		{
			_objectsData = objectsData;
			_equipmentItemsData = equipmentItemsData;
		}
		
		public void Initialize()
		{
			foreach (var characterDataSo in _objectsData.CharactersData)
			{
				_characters.Add(characterDataSo.Model.Id, characterDataSo.Model);
				_all.Add(characterDataSo.Model.Id, characterDataSo.Model);
			}
			
			foreach (var characterDataSo in _objectsData.ItemsData)
			{
				_all.Add(characterDataSo.Model.Id, characterDataSo.Model);
			}
			
			foreach (var boostItemObjectData in _objectsData.BoostItemObjectData)
			{
				_all.Add(boostItemObjectData.Model.Id, boostItemObjectData.Model);
				_boosts.Add(boostItemObjectData.Model.Id, boostItemObjectData.Model);
				_boostsByBoostType.Add(boostItemObjectData.Model.BoostArgs.Type, boostItemObjectData.Model);
			}
			
			foreach (var characterDataSo in _objectsData.InventoryData)
			{
				_inventoryItems.Add(characterDataSo.Model.Id, characterDataSo.Model);
				_all.Add(characterDataSo.Model.Id, characterDataSo.Model);
			}
			
			foreach (var abilityItem in _objectsData.AbilityItemObjectData)
			{
				_all.Add(abilityItem.Model.Id, abilityItem.Model);
				_abilityObjects.Add(abilityItem.Model.Id, abilityItem.Model);
			}
			
			EquipmentItemData(_equipmentItemsData.JetPacks, EquipmentPartType.JetPack);
			EquipmentItemData(_equipmentItemsData.BulletProofItems, EquipmentPartType.Body);
			EquipmentItemData(_equipmentItemsData.BootsItemData, EquipmentPartType.Foot);
			EquipmentItemData(_equipmentItemsData.HatsItemData, EquipmentPartType.Hat);
		}
		
		private void EquipmentItemData<T>(IReadOnlyCollection<EquipmentSo<T>> equipmentItemData, EquipmentPartType partType)
			where T : EquipmentItemData
		{
			var list = new List<EquipmentItemData>(equipmentItemData.Count);
			foreach (var jetPacks in equipmentItemData)
			{
				list.Add(jetPacks.Model);
				_all.Add(jetPacks.Model.Id, jetPacks.Model);
			}
			_equipmentByType.Add(partType, list);
		}
	}
}