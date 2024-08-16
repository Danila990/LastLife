using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
	public abstract class ShootPattern
	{
		public abstract IAsyncEnumerable<ProjectilePatternData> Shoot(Transform origin, Transform tDir, CancellationToken destroyCancellationToken);
		
		public readonly struct ProjectilePatternData
		{
			public readonly Vector3 Position;
			public readonly Vector3 Dir;
			public readonly bool ShouldEffect;
			
			public ProjectilePatternData(
				Vector3 position,
				Vector3 dir,
				bool shouldEffect)
			{
				Position = position;
				Dir = dir;
				ShouldEffect = shouldEffect;
			}
		}
		
	}
	
	[Serializable]	
	public class BurstShootPattern : ShootPattern
	{
		public int ShootCount;
		public int ShootDelay;
		
		public async override IAsyncEnumerable<ProjectilePatternData> Shoot(Transform origin, Transform tDir, [EnumeratorCancellation] CancellationToken destroyCancellationToken)
		{
			for (var i = 0; i < ShootCount-1; i++)
			{
				yield return new ProjectilePatternData(origin.position, tDir.forward,true);
				await UniTask.Delay(ShootDelay, cancellationToken: destroyCancellationToken);
			}
			yield return new ProjectilePatternData(origin.position, tDir.forward,false);
			await UniTask.NextFrame(cancellationToken: destroyCancellationToken);
		}
	}
	
	[Serializable]	
	public class ShapeShootPattern : ShootPattern
	{
		public Vector3[] Offsets;
		
		public async override IAsyncEnumerable<ProjectilePatternData> Shoot(Transform origin, Transform tDir, [EnumeratorCancellation] CancellationToken destroyCancellationToken)
		{
			
			foreach (var offset in Offsets)
			{
				yield return new ProjectilePatternData(origin.position + origin.rotation * offset, tDir.forward,false);
			}
			await UniTask.NextFrame(cancellationToken: destroyCancellationToken);
		}
	}
	
	[Serializable]	
	public class CircleShootPatter : ShootPattern
	{
		public float RadiusStart;
		public int Count;
		public float RangeAngle;
		public bool CircleCenter;
		[ReadOnly] public float DeltaAngle;
		[ReadOnly] public float RadiusDelta;

		public async override IAsyncEnumerable<ProjectilePatternData> Shoot(Transform origin, Transform tDir, [EnumeratorCancellation] CancellationToken destroyCancellationToken)
		{
			var intervalCount = Count - 1;
			if (intervalCount > 0)
			{
				DeltaAngle = RangeAngle / intervalCount;
			}
			else
			{
				DeltaAngle = 0;
			}
			
			var mAngleStart = RangeAngle * 0.5f;

			for (int i = 0; i < Count; i++)
			{
				var delta = i * DeltaAngle;
				var angle = mAngleStart - delta;
				
				var pos = new Vector3(
					Mathf.Cos(angle * Mathf.Deg2Rad),
					Mathf.Sin(angle * Mathf.Deg2Rad),
					0.0f);
				var offset = pos * (RadiusStart + (i * RadiusDelta));
				yield return new ProjectilePatternData(origin.position + origin.rotation * offset, tDir.forward,false);
			}

			if (CircleCenter)
			{
				yield return new ProjectilePatternData(origin.position, tDir.forward,false);
			}

			await UniTask.NextFrame(cancellationToken: destroyCancellationToken);
		}
	}
}