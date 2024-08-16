using System;
using Common.SpawnPoint;
using Core.Factory;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using VContainer;

namespace Core.Map
{
	public class MapExterior : AbstractMapExterior
	{
		public ObjectSpawnPoint[] SpawnPoints;
		[Inject] protected readonly IObjectFactory ObjectFactory;

		public override void CreateObjects()
		{
			foreach (var spawnPoint in SpawnPoints)
			{
				if (!spawnPoint.gameObject.activeInHierarchy)
					continue;
				spawnPoint.CreateObject(ObjectFactory, true);
			}
			SpawnPoints = Array.Empty<ObjectSpawnPoint>();
		}
		
		public async override UniTask CreateObjectsAsync()
		{
			for (var index = 0; index < SpawnPoints.Length; index++)
			{
				var spawnPoint = SpawnPoints[index];
				if (!spawnPoint.gameObject.activeInHierarchy)
					continue;
				spawnPoint.CreateObject(ObjectFactory, true);
				if (index % 5 == 0)
				{
					await UniTask.Yield(destroyCancellationToken);
				}
			}
			SpawnPoints = Array.Empty<ObjectSpawnPoint>();
		}

#if UNITY_EDITOR
		[Button]
		public void GetSpawnPoints()
		{
			SpawnPoints = transform.GetComponentsInChildren<ObjectSpawnPoint>();
		}
#endif
	}

}