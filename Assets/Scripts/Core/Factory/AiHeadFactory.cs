using System;
using System.Collections.Generic;
using Core.Entity;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using Core.Entity.Repository;
using Core.Factory.DataObjects;
using Db.ObjectData;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Core.Factory
{
	public class AiHeadFactory : IAiHeadFactory, IInitializable
	{
		private readonly IObjectFactory _objectFactory;
		private readonly IObjectResolver _resolver;
		private readonly IAiFactoryData _factoryData;
		private readonly IEntityRepository _repository;
		private readonly Dictionary<string, AiBindedHead> _heads = new Dictionary<string, AiBindedHead>();
		
		public Type AdapterType => typeof(AiHeadAdapter);
		public AiType AiType => AiType.Head;

		public AiHeadFactory(
			IObjectFactory objectFactory, 
			IObjectResolver resolver,
			IAiFactoryData factoryData,
			IEntityRepository repository)
		{
			_objectFactory = objectFactory;
			_resolver = resolver;
			_factoryData = factoryData;
			_repository = repository;
		}
		
		public void Initialize()
		{
			foreach (var head in _factoryData.Heads)
			{
				if (!_heads.TryAdd(head.CharacterId, head))
				{
					Debug.LogError($"AI Factory already contains char with id {head.CharacterId}");
					continue;
				}
			}
		}
		
		public IEntityAdapter Create(string key)
		{
			return null;
		}
		
		public IEntityAdapter Create(string key, Vector3 pos, Quaternion rot = default)
		{
			if (!_heads.TryGetValue(key, out var aichar))
			{
				Debug.LogError($"Ai factory dosen't contains {key}");
				return null;
			}

			var character = _objectFactory.CreateObject(aichar.CharacterId, pos, rot);
			var charContext = character as HeadContext;
			var adapter = Object.Instantiate(aichar.AiAdapter, pos, rot);
			_resolver.Inject(adapter);
			adapter.Init();
			adapter.SetEntityContext(charContext);
			//_repository.AddGenericEntity((LifeEntity)charContext);
			return adapter;
		}
	}
}