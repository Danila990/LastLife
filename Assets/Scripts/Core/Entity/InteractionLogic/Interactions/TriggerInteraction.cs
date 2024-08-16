
namespace Core.Entity.InteractionLogic.Interactions
{
	public class TriggerInteraction : AbstractMonoInteraction, IInteractableProvider
	{
		public override uint Uid => 9999;
        
		public override InteractionResultMeta Visit(IInteractorVisiter visiter,ref InteractionCallMeta meta)
		{
			return visiter.Accept(this, ref meta);
		}
	}
}
