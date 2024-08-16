using System.Buffers;
using System.Threading;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using Db.Map;
using SharedUtils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Etc
{
	public class DeathTrigger : MonoBehaviour
	{
		public BoundsObject BoundsObject;
		[ValueDropdown("@LayerMasks.GetLayersMasks()")]
		[SerializeField] private int _layer;
		[SerializeField] private Transform _detectionPoint;
		[SerializeField] private ParticleSystem _particleSystem;

		[Button]
		public void DeathAsync()
		{
			Death(destroyCancellationToken).Forget();
		}
		
		private async UniTaskVoid Death(CancellationToken token)
		{
			_particleSystem.Play();
			await UniTask.Delay(0.5f.ToSec(), cancellationToken: token);
			CastDeathBox();
			
			await UniTask.Delay(1f.ToSec(), cancellationToken: token);
			CastDeathBox();
			
			await UniTask.Delay(5f.ToSec(), cancellationToken: token);
			CastDeathBox();
		}
		
		private void CastDeathBox()
		{

			var cashedIds = ListPool<uint>.Get();
			var pool = ArrayPool<Collider>.Shared.Rent(20);

			var hitCount = Physics.OverlapBoxNonAlloc(
				_detectionPoint.position,
				BoundsObject.Size,
				pool,
				Quaternion.identity,
				_layer
			);

			if (hitCount <= 0)
				return;

			for (var i = 0; i < hitCount; i++)
			{
				var col = pool[i];
				col.transform.TryGetComponent(out IEntityAdapter entityAdapter);

				if (entityAdapter is {Entity: CharacterContext characterContext})
				{
					var args = new DamageArgs
					{
						Damage = 9999,
						DamageType = DamageType.Generic,
						DismemberDamage = 100
					};
					characterContext.DoDamage(ref args, DamageType.Generic);
				}
			}


			ListPool<uint>.Release(cashedIds);
			ArrayPool<Collider>.Shared.Return(pool);
		}
	}
}