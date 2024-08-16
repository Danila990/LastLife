using LitMotion;
using LitMotion.Extensions;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Utils;

namespace Core.Entity.Ai.AiActions
{
	[Category("Ai")]
	public class LookAtAiTarget : ActionTask<Transform>
	{
		public BBParameter<IAiTarget> lookTarget;
		public bool repeat = false;
		public float timeToRotate;
		private MotionHandle _handle;

		protected override string info {
			get { return "LookAt " + lookTarget; }
		}

		protected override void OnExecute() { DoLook(); }
		protected override void OnUpdate()
		{
			if (!repeat)
				return;
			DoLook();
		}

		void DoLook() {
			if (lookTarget.value is null)
			{
				EndAction(false);
				return;
			}
			
			var lookPos = lookTarget.value.LookAtPoint;
			lookPos.y = agent.position.y;
			if (repeat)
			{
				agent.LookAt(lookPos);
			}
			else
			{
				var delta = lookPos - agent.position;
				_handle.IsActiveCancel();
				_handle = LMotion
					.Create(agent.rotation, Quaternion.LookRotation(delta.normalized), timeToRotate)
					.WithOnComplete(() => EndAction(true))
					.BindToRotation(agent);
			}
		}
		
		protected override void OnStop()
		{
			_handle.IsActiveCancel();
		}
	}
}