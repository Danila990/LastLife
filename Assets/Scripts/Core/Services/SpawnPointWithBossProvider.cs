using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using SharedUtils;
using SharedUtils.PlayerPrefs.Impl;
using UnityEngine;
using Utils;

namespace Core.Services
{
	public interface ISpawnPointProvider
	{
		Transform GetInitialSpawnPoint();
		Transform GetSafeSpawnPoint();
		bool InBoundsMap(Vector3 position);
	}
	
	public class SpawnPointWithBossProvider : ISpawnPointProvider
	{
		private readonly IBossProvider _bossProvider;
		private readonly IMapService _mapService;
		private readonly InMemoryPlayerPrefsManager _playerPrefsManager;
		private bool _isFirst;
		
		public SpawnPointWithBossProvider(IBossProvider bossProvider, IMapService mapService, InMemoryPlayerPrefsManager playerPrefsManager)
		{
			_bossProvider = bossProvider;
			_mapService = mapService;
			_playerPrefsManager = playerPrefsManager;
			_isFirst = true;
		}

		public Transform GetInitialSpawnPoint()
		{
			if (_playerPrefsManager.GetValue<bool>("MetroUsed", false))
			{
				return _mapService.MapObject.CharacterSpawnPoint.TrainSpawnPoint;
			}
			return GetSafeSpawnPoint();
		}
		
		public Transform GetSafeSpawnPoint()
		{
			var mainPoint = _mapService.MapObject.CharacterSpawnPoint.MainSpanPoint;
			var bossContext = _bossProvider.CurrentContext;
			if (_isFirst)
			{
				_isFirst = false;
				return mainPoint;
			}
			
			var spawnPoints = _mapService.MapObject.CharacterSpawnPoint.OtherSpawnPoints.ToList();
			spawnPoints.Add(mainPoint);
			var validPoints = new List<Transform>();
			if (!bossContext)
				return spawnPoints.GetRandom();

			foreach (var point in spawnPoints)
			{
				var arr = ArrayPool<Collider>.Shared.Rent(1);
				var size = Physics.OverlapSphereNonAlloc(point.position, 4f, arr, LayerMasks.Creatures, QueryTriggerInteraction.Ignore);
				if (arr.Length == 0)
				{
					validPoints.Add(point);
					Util.DrawSphere(point.position, Quaternion.identity, 4f, Color.green, 10f);
				}
				else
				{
					
					Util.DrawSphere(point.position, Quaternion.identity, 4f, Color.red, 10f);
				}
				ArrayPool<Collider>.Shared.Return(arr);
			}

			if (validPoints.Count == 0)
				return spawnPoints.GetRandom(); 
			
			var farPoint = validPoints[0];
			var maxDistance = (bossContext.transform.position - farPoint.position).sqrMagnitude;

			foreach (var point in validPoints)
			{
				var distance = (bossContext.transform.position - point.position).sqrMagnitude;
				if (distance > maxDistance)
				{
					maxDistance = distance;
					farPoint = point;
				}
			}

			return farPoint;
		}

		public bool InBoundsMap(Vector3 position)
		{
			var result = false;
			foreach (var bound in _mapService.MapObject.MapBounds)
			{
				result |= bound.InBounds(position);
				
				if(result)
					break;
			}

			return result;
		}
	}
	
}
