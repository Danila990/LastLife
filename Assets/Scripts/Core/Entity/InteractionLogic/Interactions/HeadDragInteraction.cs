using Core.Entity.Head;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class HeadDragInteraction : DragInteraction
	{
		[SerializeField] private HeadContext _context;
		private IRagdollManager _rm;
		protected IRagdollManager RagdollManager => _rm ??= _context.GetRagdollManager();
		
		protected override void InitRb()
		{
			TargetRigidbody = _context.CurrentAdapter.GetComponent<Rigidbody>();
		}
		
		public override void OnBeginDrag(Vector3 refPoint)
		{
			base.OnBeginDrag(refPoint);
			RagdollManager.SetState(RagdollState.Drag);
		}

		public override void OnEndDrag()
		{
			base.OnEndDrag();
			RagdollManager.SetState(RagdollState.Ragdoll);
		}
	}
}