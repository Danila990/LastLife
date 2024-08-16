using Core.Actions;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Core.Entity.Ai.AiActions.Bird
{
	[Category("Ai")]
	public class AiMeleePunchAction : ActionTask<MeleePunchMonoAction>
	{
		public BBParameter<Vector3> HitOffset;
		
		protected override void OnExecute()
		{
			var hitPoint = agent.transform.position + agent.transform.rotation * HitOffset.value;
			agent.CastAttack(hitPoint);
			EndAction(true);
		}
	}
}