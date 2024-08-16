using Core.AnimationRigging;
using NodeCanvas.Framework;
using UnityEngine;

namespace Core.Entity.Ai.AiActions
{
	public class AiAimStateAction : ActionTask
	{
		public BBParameter<EntityContext> Target;
		public BBParameter<float> AimSpeed = 4;
		public BBParameter<IRigProvider> RigProvider;

		private RigElementController _rig;
		private Transform _rigProviderAimTarget;

		protected override void OnExecute()
		{
 			_rig = RigProvider.value.Rigs["aim"];
			_rigProviderAimTarget = _rig.RigTarget;
			_rig.EnableRig();
		}
		
		protected override void OnUpdate()
		{
			if (!Target.value)
			{
				EndAction(false);
				return;
			}
			_rigProviderAimTarget.position = Vector3.Lerp(_rigProviderAimTarget.position,Target.value.LookAtTransform.position, Time.deltaTime * AimSpeed.value);
		}

		protected override void OnStop()
		{
			_rig.DisableRig();
		}
	}

}