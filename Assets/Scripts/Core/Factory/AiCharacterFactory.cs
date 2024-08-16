using System;
using System.Collections.Generic;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.Factory.DataObjects;
using Db.ObjectData;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Core.Factory
{

	public class AiCharacterFactory : IAiCharacterFactory, IInitializable
	{
		private readonly IObjectFactory _objectFactory;
		private readonly IObjectResolver _resolver;
		private readonly IAiFactoryData _factoryData;
		private readonly IEntityRepository _repository;
		private readonly Dictionary<string, AiBindedCharacter> _characters = new();

		public Type AdapterType => typeof(AiCharacterAdapter);
		public AiType AiType => AiType.Character;

		public AiCharacterFactory(
			IObjectFactory objectFactory,
			IObjectResolver resolver,
			IAiFactoryData factoryData,
			IEntityRepository repository
		)
		{
			_objectFactory = objectFactory;
			_resolver = resolver;
			_factoryData = factoryData;
			_repository = repository;
		}

		public void Initialize()
		{
			foreach (var character in _factoryData.Characters)
			{
				if (!_characters.TryAdd(character.CharacterId, character))
				{
					Debug.LogError($"AI Factory already contains char with id {character.CharacterId}");
					continue;
				}
			}
		}
		
		public IEntityAdapter Create(string key)
		{
			return CreateObject(key);
		}
		
		public IEntityAdapter Create(string key, Vector3 pos)
		{
			return CreateObject(key, pos);
		}
		
		public IEntityAdapter Create(string key, Vector3 pos, Quaternion rotation)
		{
			return CreateObject(key, pos, rotation);
		}

		public AiCharacterAdapter CreateObject(string key, Vector3 pos, Quaternion rot)
		{
			if (!_characters.ContainsKey(key))
			{
				Debug.LogError($"Ai factory dosen't contains {key}");
				return null;
			}
			
			var aichar = _characters[key];
			var character = _objectFactory.CreateObject(aichar.CharacterId, pos, false);
			var charContext = character as CharacterContext;
			var adapter = Object.Instantiate(aichar.AiAdapter, pos, rot);
			_resolver.Inject(adapter);
			adapter.Init();
			adapter.SetEntityContext(charContext);
			_repository.AddGenericEntity(charContext);

			return adapter;
		}

		public AiCharacterAdapter CreateObject(string key, Vector3 pos)
		{
			return CreateObject(key, pos, Quaternion.identity);
		}

		public AiCharacterAdapter CreateObject(string key)
		{
			return CreateObject(key, Vector3.zero, Quaternion.identity);
		}
	}
}