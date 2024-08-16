using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Core.Entity.Head
{
	public class JetHeadContext : HeadContext
	{
		public ParticleSystem[] _fxs;
		public Transform LeftDoor;
		public Transform RightDoor;
		public Vector3 OpenedRotationLeft;
		public Vector3 OpenedRotationRight;
		private Vector3 _closedRotationLeft;
		private Vector3 _closedRotationRight;

		
		private void Start()
		{
			_closedRotationLeft = LeftDoor.localEulerAngles;
			_closedRotationRight = RightDoor.localEulerAngles;
		}

		public UniTask OpenDoors()
		{
			LMotion
				.Create(LeftDoor.localEulerAngles, OpenedRotationLeft, 0.5f)
				.BindToLocalEulerAngles(LeftDoor)
				.AddTo(this);
			
			return LMotion
				.Create(RightDoor.localEulerAngles, OpenedRotationRight, 0.5f)
				.BindToLocalEulerAngles(RightDoor)
				.ToUniTask(destroyCancellationToken);
		}

		public UniTask CloseDoors()
		{
			LMotion
				.Create(LeftDoor.localEulerAngles, _closedRotationLeft, 0.5f)
				.BindToLocalEulerAngles(LeftDoor)
				.AddTo(this);
			
			return LMotion
				.Create(RightDoor.localEulerAngles, _closedRotationRight, 0.5f)
				.BindToLocalEulerAngles(RightDoor)
				.ToUniTask(destroyCancellationToken);
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			foreach (var fx in _fxs)
			{
				fx.Stop();
			}
		}
	}
}