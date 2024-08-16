using System.Buffers;
using System.Collections.Generic;
using Core.Services;
using UnityEngine;
using Utils;

namespace Core.Map
{
	public class PlayerTeleportWithBossProvider : PlayerTeleportProvider
	{
		private readonly IBossSpawnService _bossSpawnService;


		public PlayerTeleportWithBossProvider(
			ISpawnPointProvider spawnPointProvider,
			IBossSpawnService bossSpawnService
			) 
			: base(spawnPointProvider)
		{
			_bossSpawnService = bossSpawnService;
		}
		
		public override Vector3 GetTpPoint(Vector3 currentPosition, float radius)
			=> GetRandomNavMeshPositionWithBoss(currentPosition, radius);
		
		private Vector3 GetRandomNavMeshPositionWithBoss(Vector3 currentPosition, float radius)
		{
			if(_bossSpawnService.CurrentBoss == null || _bossSpawnService.CurrentBoss.Value == null)
				return base.GetTpPoint(currentPosition, radius);
			
			var secondObject = _bossSpawnService.CurrentBoss.Value.MainTransform;
			
			var distance = (currentPosition - secondObject.position).sqrMagnitude;
			
			if (distance <= radius * radius)
			{
				var buffer = ArrayPool<Vector3>.Shared.Rent(10);
				MathUtils.GetPointsAroundOriginAsArray(currentPosition, ref buffer, radius: radius);
				var point = FindFarthestPoint(buffer, secondObject.position);
				ArrayPool<Vector3>.Shared.Return(buffer);
				
				return NavMeshUtils.ProjectOnNavMesh(point, radius, out var result) ? 
					result 
					:
					base.GetTpPoint(currentPosition, radius);
			}

			return base.GetTpPoint(currentPosition, radius);
		}

		private static Vector3 FindFarthestPoint(IEnumerable<Vector3> buffer, Vector3 origin)
		{
			var farthestPoint = origin;
			var farthestDistance = 0f;

			foreach (var t in buffer)
			{
				var d = (origin - t).sqrMagnitude;
				if (d > farthestDistance)
				{
					farthestDistance = d;
					farthestPoint = t;
				}
			}

			return farthestPoint;
		}
	}

}
