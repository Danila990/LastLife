using System.Buffers;
using Core.Services;
using SharedUtils;
using UnityEngine;
using Utils;

namespace Core.Map
{
	public class PlayerTeleportProvider : IPlayerTeleportProvider
	{
		private readonly ISpawnPointProvider _spawnPointProvider;

		public PlayerTeleportProvider(ISpawnPointProvider spawnPointProvider)
		{
			_spawnPointProvider = spawnPointProvider;
		}

		public virtual Vector3 GetTpPoint(Vector3 currentPosition, float radius)
			=> GetRandomNavMeshPosition(currentPosition, radius);

		private Vector3 GetRandomNavMeshPosition(Vector3 currentPosition, float radius)
		{
			var buffer = ArrayPool<Vector3>.Shared.Rent(10);
			MathUtils.GetPointsAroundOriginAsArray(currentPosition, ref buffer, radius: radius);
			var point = buffer.GetRandom();
			ArrayPool<Vector3>.Shared.Return(buffer);

			return NavMeshUtils.ProjectOnNavMesh(point, radius, out var result) ? 
				result
				:
				_spawnPointProvider.GetSafeSpawnPoint().position;
		}
	}
}