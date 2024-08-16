using System;
using System.Collections.Generic;
using Core.Equipment.Data;
using UniRx;

namespace Core.Equipment.Inventory
{
	public class AllEquipment : IDisposable
	{
		#region Fields
		private readonly Dictionary<(EquipmentPartType Type, string Id), RuntimeEquipmentArgs> _storageByTypeId;
		private readonly Dictionary<EquipmentPartType, Dictionary<string, RuntimeEquipmentArgs>> _storageByType;

		private readonly ReactiveCommand<RuntimeEquipmentArgs> _onAdd;
		private readonly ReactiveCommand<RuntimeEquipmentArgs> _onRemove;
		#endregion

		#region Props
		public IEnumerable<RuntimeEquipmentArgs> Items => _storageByTypeId.Values;
		public IReadOnlyDictionary<(EquipmentPartType Type, string Id), RuntimeEquipmentArgs> EquipmentByTypeId => _storageByTypeId;
		public IReadOnlyDictionary<EquipmentPartType, Dictionary<string, RuntimeEquipmentArgs>> EquipmentByType => _storageByType;
		
		public IReactiveCommand<RuntimeEquipmentArgs> OnAdd => _onAdd;
		public IReactiveCommand<RuntimeEquipmentArgs> OnRemove => _onRemove;
		#endregion

		public AllEquipment()
		{
			_storageByTypeId = new Dictionary<(EquipmentPartType Type, string Id), RuntimeEquipmentArgs>();
			_storageByType = new Dictionary<EquipmentPartType, Dictionary<string, RuntimeEquipmentArgs>>();
			_onAdd = new ReactiveCommand<RuntimeEquipmentArgs>();
			_onRemove = new ReactiveCommand<RuntimeEquipmentArgs>();
		}
		
		public void Dispose()
		{
			_onAdd?.Dispose();
			_onRemove?.Dispose();
		}
		
		/// <summary>
		/// Don't use it directly. Better use a <b>IEquipmentInventoryService.AddEquipment</b>
		/// </summary>
		/// <param name="args"></param>
		public void AddPart(RuntimeEquipmentArgs args)
		{
			var key = (args.EquipmentArgs.PartType, args.EquipmentArgs.Id);
			
			if (_storageByTypeId.ContainsKey(key))
				return;

			if (!_storageByType.ContainsKey(key.PartType))
				_storageByType[key.PartType] = new();
			
			_storageByType[key.PartType].Add(args.EquipmentArgs.Id, args);
			_storageByTypeId.Add(key, args);
			_onAdd.Execute(args);
		}

		public void ChangePart(RuntimeEquipmentArgs args)
		{
			var key = (args.EquipmentArgs.PartType, args.EquipmentArgs.Id);
			if (HasEquipment(key.PartType, key.Item2))
			{
				_storageByType[key.PartType][args.EquipmentArgs.Id] = args;
				_storageByTypeId[key] = args;
			}
		}
		
		public void RemovePart(EquipmentPartType type, string id)
		{
			var key = (type, id);

			if (_storageByTypeId.ContainsKey(key))
			{
				_onRemove.Execute(_storageByTypeId[key]);
				
				_storageByType[key.type].Remove(key.id);
				_storageByTypeId.Remove(key);
			}
		}

		public void RemoveAll()
		{
			_storageByType.Clear();
			_storageByTypeId.Clear();
		}

		public bool HasEquipment(EquipmentPartType type, string id)
			=> _storageByTypeId.ContainsKey((type, id));
	}
}
