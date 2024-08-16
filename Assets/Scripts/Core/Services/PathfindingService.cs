using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using VContainer.Unity;

namespace Core.Services
{
	public interface IPathfindingService
	{
		void GetPath(IList<Vector3> result, Vector3 start, Vector3 end);
	}
	
	public class TestPathfindingService : IPathfindingService
	{
		public void GetPath(IList<Vector3> result, Vector3 start, Vector3 end)
		{
			result.Clear();
			result.Add(new Vector3(0, 10, 0));
		}
	}

	public class PathfindingService : IStartable, IPathfindingService
	{
		private readonly IMapService _mapService;

		public PathfindingService(IMapService mapService)
		{
			_mapService = mapService;
		}
		
		public void Start()
		{
			_mapService.MapObject.GridBuilder.Build();
			_mapService.MapObject.GridBuilder.ForceUpdateGrid();
		}

		public void GetPath(IList<Vector3> result, Vector3 start, Vector3 end)
		{
			_mapService.MapObject.GridBuilder.Pathfinding(result, start, end);
		}
	}
}