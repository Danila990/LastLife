using UnityEngine;

namespace Core.Services
{
	public class MarketPointProvider : ISpawnPointProvider
	{
		private readonly IMapService _mapService;

		public MarketPointProvider(IMapService mapService)
		{
			_mapService = mapService;
		}

		public Transform GetInitialSpawnPoint()
		{
			return GetSafeSpawnPoint();
		}
		
		public Transform GetSafeSpawnPoint()
		{
			return _mapService.MapObject.CharacterSpawnPoint.MainSpanPoint;
		}
		
		public bool InBoundsMap(Vector3 position)
		{
			return true;
		}
	}
}
