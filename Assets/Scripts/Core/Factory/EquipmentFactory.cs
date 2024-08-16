using System.Collections.Generic;
using System.Linq;
using Core.Entity;
using Core.Entity.Repository;
using Core.Equipment;
using Core.Equipment.Impl;
using Core.Factory.DataObjects;
using UnityEngine;
using VContainer.Unity;

namespace Core.Factory
{
	public class EquipmentFactory : IEquipmentFactory, IInitializable
	{
		private readonly IFactoryData _factoryData;
		private readonly IEntityRepository _repository;
		private readonly IObjectFactory _factory;
		private readonly Dictionary<string, EntityContext> _objects = new();

		private const string FAT_POSTFIX = "_FAT";
		
		public EquipmentFactory(
			IFactoryData factoryData,
			IEntityRepository repository,
			IObjectFactory factory
		)
		{
			_factoryData = factoryData;
			_repository = repository;
			_factory = factory;
		}

		public EquipmentEntityContext CreateObject(string key, bool isFat)
		{
			var fatKey = isFat ? $"{key}{FAT_POSTFIX}" : string.Empty;
			if (isFat && _objects.ContainsKey(fatKey))
			{
				return CreateObject(fatKey);
			}

			if (!_objects.ContainsKey(key))
			{
				Debug.LogError($"Factory doesn't contains {key} object!");
				return null;
			}

			return CreateObject(key);
		}

		private EquipmentEntityContext CreateObject(string key)
		{
			
			var instance = _factory.CreateObject(key);
			if (instance is EquipmentEntityContext context)
			{
				return context;
			}
			
			Debug.LogError($"Instance doesn't match EquipmentEntityContext!");
			return null;
		}

		public EquipmentEntityContext CreateFromPrefab(EquipmentEntityContext prefab, string key)
		{
			var instance = _factory.CreateFromPrefab(prefab, key);
			if (instance is EquipmentEntityContext context)
			{
				return context;
			}
			
			Debug.LogError($"Instance doesn't match EquipmentEntityContext!");
			return null;
		}
		
		public void Initialize()
		{
			var jetPacks = _factoryData.Objects.Where(x => x.Type == EntityType.Equipment);
			foreach (var obj in jetPacks)
			{
				if (_objects.ContainsKey(obj.Key))
				{
					Debug.LogError($"Factory already contains {obj.Key}!");
					continue;
				}
				_objects.Add(obj.Key,obj.Object);
			}
		}
	}

}
