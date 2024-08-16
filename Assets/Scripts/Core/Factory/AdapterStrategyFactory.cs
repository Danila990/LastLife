using System;
using System.Collections.Generic;
using Core.Entity.Characters.Adapters;
using Db.ObjectData;
using UnityEngine;

namespace Core.Factory
{
	public interface IAdapterStrategyFactory
	{
		T CreateAiAdapter<T>(string key, Vector3 pos, Quaternion lookRotation = default) 
			where T : IEntityAdapter;

		IEntityAdapter CreateAiAdapter(IAiSpawnData aiSpawnData, Vector3 pos, Quaternion lookRotation = default);
		object CreateObject(ObjectData selectedItemValue, Vector3 currentHitPoint, Quaternion lookRotation);
	}
	
	public class AdapterStrategyFactory : IAdapterStrategyFactory
	{
		private readonly IObjectFactory _objectFactory;
		private readonly Dictionary<Type, IAiAdapterFactory> _adapterFactories;
		private readonly Dictionary<AiType, Type> _aiTypeProxy;
		
		public AdapterStrategyFactory(
			IEnumerable<IAiAdapterFactory> aiAdapterFactories,
			IObjectFactory objectFactory)
		{
			_objectFactory = objectFactory;
			
			_adapterFactories = new Dictionary<Type, IAiAdapterFactory>();
			_aiTypeProxy = new Dictionary<AiType, Type>();

			foreach (var adapterFactory in aiAdapterFactories)
			{
				AddFactory(adapterFactory, adapterFactory.AiType);
			}
		}

		private void AddFactory(IAiAdapterFactory factory, AiType aiType)
		{
			_adapterFactories.Add(factory.AdapterType, factory);
			_aiTypeProxy.Add(aiType, factory.AdapterType);
		}

		public T CreateAiAdapter<T>(string key, Vector3 pos, Quaternion lookRotation = default) 
			where T : IEntityAdapter
		{
			if (_adapterFactories.TryGetValue(typeof(T), out var factory))
			{
				return (T)factory.Create(key, pos, lookRotation);
			}
			
			throw new InvalidOperationException();
		}
		
		public IEntityAdapter CreateAiAdapter(IAiSpawnData aiSpawnData, Vector3 pos, Quaternion lookRotation = default)
		{
			var factory = _adapterFactories[_aiTypeProxy[aiSpawnData.AiType]];
			return factory.Create(aiSpawnData.AiAdapterId, pos, lookRotation);
		}
		
		public object CreateObject(ObjectData selectedItemValue, Vector3 currentHitPoint, Quaternion lookRotation)
		{
			return selectedItemValue switch
			{
				IAiSpawnData spawnData => CreateAiAdapter(spawnData, currentHitPoint, lookRotation),
				ItemObject itemObject => _objectFactory.CreateObject(itemObject.FactoryId, currentHitPoint, lookRotation),
				_ => throw new ArgumentOutOfRangeException(nameof(selectedItemValue), $"AdapterStrategy cant create {selectedItemValue.Id} type of ({selectedItemValue.GetType()})")
			};
		}
	}
}