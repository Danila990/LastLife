using System;
using System.Collections.Generic;
using Core.Entity.Repository;
using Core.Factory;
using FileManagerSystem;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.SaveSystem.SaveAdapters.EntitySave
{
	public class EntitySaveRepositoryAdapter : IAutoLoadAdapter
	{
		private readonly IEntityRepository _entityRepository;
		private readonly IObjectFactory _objectFactory;
		public bool CanSave => true;
		public string SaveKey => "EntitySave";

		public EntitySaveRepositoryAdapter(IEntityRepository entityRepository, IObjectFactory objectFactory)
		{
			_entityRepository = entityRepository;
			_objectFactory = objectFactory;
		}
		
		public string CreateSave()
		{
			var entityPool = ListPool<EntitySaveData>.Get();
			foreach (var entityContext in _entityRepository.EntityContext)
			{
				if (entityContext is ISavableEntity { CanSave: true } savableEntity)
				{
					entityPool.Add(new EntitySaveData(savableEntity.FactoryId, entityContext.MainTransform));
				}
			}
			var resultString = "";

			try
			{
				resultString = JsonConvert.SerializeObject(entityPool);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(EntitySaveRepositoryAdapter)}]" + e.Message);
			}
			
			ListPool<EntitySaveData>.Release(entityPool);
			return resultString;
		}
		
		public void LoadSave(string value)
		{
			List<EntitySaveData> list = null;
			try
			{
				list = JsonConvert.DeserializeObject<List<EntitySaveData>>(value);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(EntitySaveRepositoryAdapter)}]" + e.Message);
			}
			
			if (list is null)
				return;
			
			foreach (var saveData in list)
			{
				_objectFactory.CreateObject(saveData.FactoryId, saveData.Transform.Position.AsVec(), saveData.Transform.Rotation.AsQuat());
			}
		}
		
		
		[Serializable]
		private struct EntitySaveData
		{
			public string FactoryId;
			public SimpleSavedTransform Transform;
			
			public EntitySaveData(string factoryId, SimpleSavedTransform transform)
			{
				FactoryId = factoryId;
				Transform = transform;
			}
		}
	}

	public interface ISavableEntity
	{
		string FactoryId { get; set; }
		bool CanSave { get; set; }
	}
}