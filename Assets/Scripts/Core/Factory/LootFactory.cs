using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Core.Entity;
using Core.Entity.Repository;
using Core.Factory.DataObjects;
using Core.Loot;
using SharedUtils;
using UnityEngine;
using Utils;
using VContainer.Unity;

namespace Core.Factory
{
	public class LootFactory : ILootFactory, IInitializable
	{
		private readonly IFactoryData _factoryData;
		private readonly IObjectFactory _factory;
		private readonly Dictionary<string, EntityContext> _objects = new();

		public LootFactory(
			IFactoryData factoryData,
			IObjectFactory factory
		)
		{
			_factoryData = factoryData;
			_factory = factory;
		}

		public LootEntity CreateObject(string key)
		{
			if (!_objects.ContainsKey(key))
			{
				Debug.LogError($"Factory doesn't contains {key} object!");
				return null;
			}

			var instance = _factory.CreateObject(key);
			if (instance is LootEntity context)
			{
				return context;
			}
			
			Debug.LogError($"Instance doesn't match LootEntity!");
			return null;
		}
		
		public LootEntity CreateObject(string key, Vector3 position)
		{
			var obj = CreateObject(key);
			obj.transform.position = position;
			obj.Init();
			return obj;
		}
		
		public LootEntity CreateObjectAroundPoint(string key, Vector3 position, float radius = 2)
		{
			var pool = ArrayPool<Vector3>.Shared.Rent(10);
			MathUtils.GetPointsAroundOriginAsArray(position, ref pool);
			var point = pool.GetRandom();
			ArrayPool<Vector3>.Shared.Return(pool);
			return CreateObject(key, point);
		}
		
		public void Initialize()
		{
			var loots = _factoryData.Objects.Where(x => x.Type == EntityType.Loot);
			foreach (var obj in loots)
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
