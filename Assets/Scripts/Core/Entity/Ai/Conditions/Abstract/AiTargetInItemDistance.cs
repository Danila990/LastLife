using Core.Entity.Ai.AiItem;
using NodeCanvas.Framework;
using UnityEngine;

namespace Core.Entity.Ai.Conditions.Abstract
{
	public class AiTargetInItemDistance : AiTargetInDistance
	{
		public BBParameter<IAiItem> AiItem;
		public override float Distance => AiItem.value.UseRange;
		
		public override void OnDrawGizmosSelected() {
			if ( agent != null && AiItem is { isNoneOrNull: false }) {
				Gizmos.DrawWireSphere(agent.position, Distance);
			}
		}
	}
}