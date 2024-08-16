using Db.Map;

namespace Core.Zones
{
    public interface IMapZoneService
    {
        public int TryGetZones(ZoneType type, ref ZoneBoundsObject[] array);
    }
}