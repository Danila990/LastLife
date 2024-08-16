using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Projectile
{
	public class KnifeProjectile : SimpleProjectile
	{
		public Transform KnifeModel;
		public float RotationTime;
		public LoopType LoopType;
		public Vector3 RotationTarget;
		private MotionHandle _handle;

		public override void Release()
		{
			_handle.IsActiveCancel();
			if (Particle)
			{
				Particle.ParticleSystem.Stop(false);
			}
			if (ReleaseDelay > 0 && LastInteraction == LastInteractionType.Environment)
			{
				DelayRelease().Forget();
			}
			else
			{
				Pool.Return(this);
			}
		}
		
		public override void HandleHit(
			RaycastHit hit,
			out bool isBlocking)
		{
			base.HandleHit(hit, out isBlocking);
			if (isBlocking)
			{
				KnifeModel.up = -hit.normal;
			}
		}
		
		private void OnDisable()
		{
			_handle.IsActiveCancel();
		}

		public override void OnRent()
		{
			base.OnRent();
			Rotate();
		}
		
		[Button]
		private void Stop()
		{
			KnifeModel.localEulerAngles = Vector3.zero;
			_handle.IsActiveCancel();
		}
		
		[Button]
		private void Rotate()
		{
			Stop();
			
			_handle = LMotion
				.Create(Vector3.zero, RotationTarget, RotationTime)
				.WithLoops(-1, LoopType)
				.BindToLocalEulerAngles(KnifeModel);
		}
	}
}