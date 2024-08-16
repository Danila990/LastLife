using Core.Services;
using Db.Map;
using UnityEngine;
using VContainer.Unity;

namespace Core.Zones
{
    public class MapZoneService : IStartable, IMapZoneService
    {
        private readonly IMapService _mapService;

        public MapZoneService(
            IMapService mapService
        )
        {
            _mapService = mapService;
        }

        public int TryGetZones(ZoneType type, ref ZoneBoundsObject[] array)
        {
            var count = 0;
            var max = Mathf.Min(array.Length, _mapService.MapObject.Zones.Length);
            for (var i = 0; i < max; i++)
            {
                var zone = _mapService.MapObject.Zones[i];
                if ((zone.ZoneType & type) <= 0) continue;
                array[count] = zone;
                count++;
            }
            return count;
        }
		
        public void Start()
        {
			
        }
    }
}