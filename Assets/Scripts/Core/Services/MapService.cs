using System;
using Db.Map;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Core.Services
{
	public interface IMapService
	{
		MapObject MapObject { get; }
	}
	
	public class MapService : IInitializable, IDisposable, IMapService
	{
		private readonly IMapDataProvider _mapDataProvider;
		
		private MapData _selectedMap;
		private MapObject _mapObject;
		
		public MapObject MapObject => _mapObject;
		
		public MapService(IMapDataProvider mapDataProvider)
		{
			_mapDataProvider = mapDataProvider;
		}
		
		public void Initialize()
		{
			_selectedMap = _mapDataProvider.MapData;
			_mapObject = Object.Instantiate(_selectedMap.MapObject);
		}
		
		public void Dispose()
		{
			
		}
	}
}