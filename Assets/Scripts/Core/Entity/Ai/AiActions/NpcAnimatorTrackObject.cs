using Core.Entity.Ai.Npc;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Core.Entity.Ai.AiActions
{
	[Category("Ai")]
	public class NpcAnimatorTrackObject : ActionTask<NpcAnimator>
	{
		public BBParameter<Transform> ObjectToTrack;
		
		protected override void OnExecute()
		{
			if (ObjectToTrack.value is null)
			{
				EndAction(false);
				return;
			}
			
			agent.TrackObject(ObjectToTrack.value);
		}

		protected override void OnPause()
		{
			agent.StopTrack();
		}

		protected override void OnStop()
		{
			agent.StopTrack();
		}
	}
}