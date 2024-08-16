using System;
using Core.Player.MovementFSM.Data.Collider;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils
{
	[Serializable]
	public class CapsuleColliderUtility
	{
		[field:SerializeField] public CapsuleColliderData CapsuleColliderData { get; private set; }
		[field:SerializeField] public DefaultColliderData DefaultColliderData { get; private set; }
		[field:SerializeField] public PlayerTriggerColliderData TriggerColliderData { get; private set; }
		[field:SerializeField] public SlopeData SlopeData { get; private set; }

		public void Initialize(GameObject gameObject)
		{
			CapsuleColliderData.Initialize(gameObject);
		}
		
		[OnInspectorGUI]
		public void CalculateCapsuleColliderDimensions()
		{
			SetColRadius(DefaultColliderData.Radius);
			SetColHeight(DefaultColliderData.Height * (1f - SlopeData.StepHeightPercentage));

			SetColCenter();

			var halfCol = CapsuleColliderData.Collider.height / 2f;
			if (halfCol < CapsuleColliderData.Collider.radius)
			{
				SetColRadius(halfCol);
			}

			CapsuleColliderData.UpdateColliderData();
		}
		
		private void SetColCenter()
		{
			var delta = DefaultColliderData.Height - CapsuleColliderData.Collider.height;
			CapsuleColliderData.Collider.center = new Vector3(0f, DefaultColliderData.CenterY + (delta / 2f), 0);
		}
		
		private void SetColHeight(float value)
		{
			CapsuleColliderData.Collider.height = value;
		}
		
		private void SetColRadius(float value)
		{
			CapsuleColliderData.Collider.radius = value;
		}
	}
}