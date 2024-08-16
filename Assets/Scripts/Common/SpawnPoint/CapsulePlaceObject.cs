using Core.Entity.Ai.Merchant;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Common.SpawnPoint
{
	public class CapsulePlaceObject : MerchantPlaceObject
	{
		public Transform Door;
		
		public Vector3 Opened;
		public Vector3 Closed;
		public Ease OpenedEase;
		public Ease ClosedEase;
		
		public Transform TpTargetForMerchant;
		public Transform MoveTargetForMerchant;
		public Transform MoveTargetForPlayer;
		public Transform ExitPoint;
		public ParticleSystem WorkingEffect;
		public ParticleSystem EndWorkEffect;
		
		private MotionHandle _handle;

		public override void ConnectedContext(MerchantEntityContext context)
		{
			context.Blackboard.SetVariableValue("CapsulePoint", MoveTargetForMerchant.gameObject);
		}

		private void OnDisable()
		{
			_handle.IsActiveCancel();
		}
		
		[Button]
		public void OpenDoor()
		{
			_handle.IsActiveCancel();
			_handle = LMotion
				.Create(Door.localPosition, Opened, 1.5f)
				.WithEase(OpenedEase)
				.BindToLocalPosition(Door);
		}

		public void OpenDoorForce()
		{
			Door.localPosition = Opened;
		}
		

		[Button]
		public void CloseDoor(float delay)
		{
			_handle.IsActiveCancel();
			_handle = LMotion
				.Create(Door.localPosition, Closed, 1.5f)
				.WithDelay(delay)
				.WithEase(ClosedEase)
				.BindToLocalPosition(Door);
		}
	}
}