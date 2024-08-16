using NodeCanvas.Framework;

namespace Core.Entity.Ai.AiActions
{
	public class AiClearTargetAction : ActionTask
	{
		public BBParameter<IAiTarget> AiTarget;
		public Faction TargetFaction;
		public bool DontClearTarget;
		
		protected override void OnExecute()
		{
			if (DontClearTarget && AiTarget.value is { HasEntity: true } && AiTarget.value.Faction == TargetFaction)
			{
				EndAction(false);
				return;
			}
			if (AiTarget.value != null)
			{
				AiTarget.SetValue(null);
			}
			EndAction(false);
		}
	}

}