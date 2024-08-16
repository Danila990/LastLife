using Core.AnimationRigging;
using Core.Entity.Ai.AiItem;
using NodeCanvas.Framework;
using UnityEngine;

namespace Core.Entity.Ai.AiActions
{
	public class AiAtAiTargetAimStateAction : ActionTask<Transform>
	{
		public BBParameter<IAiTarget> Target;
		public BBParameter<float> AimSpeed = 4;
		public BBParameter<IRigProvider> RigProvider;
		public BBParameter<IAiItem> Item;
		public bool WithoutItem;
		public bool RotatePlayer;
		public string GenericRigName;
		
		private RigElementController _rig;
		private Transform _rigProviderAimTarget;
		private IAiItem _item;

		protected override void OnExecute()
		{
			_item = Item.value;
			if (!WithoutItem)
			{
				if (_item is null)
				{
					EndAction(false);
					return;
				}
				if (!_item.AiItemData.UseRig)
				{
					EndAction(true);
					return;
				}
			}
			
			_rig = RigProvider.value.Rigs[_item != null ? _item.AiItemData.RigName : GenericRigName];
			_rigProviderAimTarget = _rig.RigTarget;
			_rig.EnableRig();
		}
		
		protected override void OnUpdate()
		{
			if (!Target.value.IsActive)
			{
				EndAction(false);
				return;
			}
			
			if (_item != null && (elapsedTime >= _item.UseActionDuration || !_item.InUse)) {
				EndAction();
				return;
			}

			var valueLookAtPoint = Target.value.LookAtPoint;
			if (RotatePlayer)
			{
				var delta = (valueLookAtPoint - agent.position).normalized;
				delta.y = 0;
				agent.rotation = Quaternion.Lerp(agent.rotation, Quaternion.LookRotation(delta), Time.deltaTime * AimSpeed.value);
			}
			
			_rigProviderAimTarget.position = Vector3.Lerp(_rigProviderAimTarget.position,valueLookAtPoint, Time.deltaTime * AimSpeed.value * 1.5f);
		}

		protected override void OnStop()
		{
			_rig?.DisableRig();
		}
	}
}