using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Player.MovementFSM.Data.Collider
{
	[Serializable]
	public class CapsuleColliderData
	{
		[field:SerializeField] public CapsuleCollider Collider { get; private set; }
		[field:SerializeField] public Vector3 ColliderCenterInLocalSpace { get; private set; }
		[field:SerializeField] public Vector3 ColliderVerticalExtents { get; private set; }
		public void Initialize(GameObject gameObject)
		{
			if (Collider != null)
			{
				return;
			}
			
			Collider = gameObject.GetComponent<CapsuleCollider>();
			UpdateColliderData();
		}

		public void UpdateColliderData()
		{
			ColliderCenterInLocalSpace = Collider.center;
			ColliderVerticalExtents = new Vector3(0f, Collider.bounds.extents.y, 0f);
		}
	}

	[Serializable]
	public class DefaultColliderData
	{
		[field: SerializeField] public float Height { get; private set; } = 1.8f;
		[field: SerializeField] public float CenterY { get; private set; } = .9f;
		[field: SerializeField] public float Radius { get; private set; } = .2f;
	}

	[Serializable]
	public class SlopeData
	{
		[field: SerializeField] [Unit(Units.Percent)] [field:PropertyRange(0, 1f)] public float StepHeightPercentage { get; private set; } = .25f;
		[field: SerializeField] [field:PropertyRange(0, 3f)] public float FloatRayDistance { get; private set; } = 2f;
		[field: SerializeField] [field:PropertyRange(0, 50f)] public float StepReachForce { get; private set; } = 25f;
	}
	
	[Serializable]
	public class PlayerTriggerColliderData
	{
		[field: SerializeField] public BoxCollider GroundCheckCollider { get; private set; }

		public Vector3 GroundCheckColliderVerticalExtents { get; private set; }

		public void Initialize()
		{
			GroundCheckColliderVerticalExtents = GroundCheckCollider.bounds.extents;
		}
	}
}