using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UnityEngine;
using Utils;

namespace Core.Inventory.Items.Weapon
{
	public class ProjectileWeaponBulletPatterns : SimpleProjectileWeaponContext
	{
		[SerializeField] private PatternDataSO[] _patternsData;
		private List<ShootPattern> _allPatterns = new List<ShootPattern>();

		private void Awake()
		{
			_allPatterns = _patternsData.Select(x=>x.GetPattern()).ToList();
		}
		
		public override void Shoot()
		{
			ShootTask().Forget();
		}

		private async UniTaskVoid ShootTask()
		{
			await UniTask.Delay(0.25f.ToSec(), cancellationToken:destroyCancellationToken);
			var pattern = _allPatterns.GetRandom();
			await foreach (var patternData in pattern.Shoot(GetOrigin(), GetOrigin(), destroyCancellationToken))
			{
				CreateProjectile(patternData.Position, patternData.Dir,GetInaccuracy(GetOrigin()));
				if (patternData.ShouldEffect)
				{
					OnShoot();
				}
			}
			OnShoot();
		}
	}
}