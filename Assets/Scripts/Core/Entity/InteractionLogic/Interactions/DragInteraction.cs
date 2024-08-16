using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class DragInteraction : AbstractMonoInteraction
	{
		public Rigidbody TargetRigidbody;
		public OutlineHighLight OutlineHigh;
		private Vector3 _offset;
		private CollisionDetectionMode _detectionMode;
		private IDestroyListener _destroyListener;
		public Vector3 Offset => _offset;

		public override InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
		{
			return visiter.Accept(this, ref meta);
		}

		protected void Start()
		{
			InitOutline();
			InitRb();
		}
		
		protected virtual void InitRb()
		{
			if (TargetRigidbody) return;
			TargetRigidbody = GetComponent<Rigidbody>();
			_detectionMode = TargetRigidbody.collisionDetectionMode;
		}

		private void OnDestroy()
		{
			_destroyListener?.Release();
		}
		
		public void AttachInteractor(IDestroyListener destroyListener)
		{
			_destroyListener = destroyListener;
		}

		protected virtual void InitOutline()
		{
			OutlineHigh.Init();
		}

		public virtual void OnBeginDrag(Vector3 refPoint)
		{
			_offset = TargetRigidbody.position - refPoint;
			TargetRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		}

		public virtual void OnEndDrag()
		{
			_offset = default;
			_destroyListener = null;
			TargetRigidbody.collisionDetectionMode = _detectionMode;
		}

		public virtual void EnableOutline()
		{
			OutlineHigh.Enable();
		}

		public virtual void DisableOutline()
		{
			if (OutlineHigh)
			{
				OutlineHigh.Disable();
			}
		}

		public virtual void Drag(ref Vector3 targetPos, ref Quaternion targetRotation, ref float deltaTime)
		{
			var delta = targetPos - (TargetRigidbody.position - _offset);
			TargetRigidbody.angularVelocity = Vector3.zero;
			TargetRigidbody.rotation = Quaternion.RotateTowards(TargetRigidbody.rotation, targetRotation, 50f * deltaTime);
			TargetRigidbody.velocity = delta * 10;
#if UNITY_EDITOR
			Debug.DrawLine(targetPos, targetPos + Vector3.up, Color.red);
			Debug.DrawLine(TargetRigidbody.position, TargetRigidbody.position + delta, Color.magenta);
			Debug.DrawLine(TargetRigidbody.position, TargetRigidbody.position - _offset, Color.yellow);
#endif
		}

#if UNITY_EDITOR
		[Button]
		private void SetUpRef()
		{
			if (TryGetComponent<OutlineHighLight>(out var outline))
			{
				OutlineHigh = outline;
				return;
			}
			OutlineHigh = gameObject.AddComponent<OutlineHighLight>();
			OutlineHigh.OutlineWidth = 3;
			OutlineHigh.OutlineColor = new Color(1f, 0.64f, 0f);
		}
#endif
	}
}