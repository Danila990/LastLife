using Core.Entity.Characters.Adapters;

namespace Core.Entity.Head
{
	public class SpiderAiHeadAdapter : AiHeadAdapter
	{
		new public SpiderHeadContext CurrentContext { get; set; }
		
		public override void SetEntityContext(HeadContext context)
		{
			base.SetEntityContext(context);
			CurrentContext = (SpiderHeadContext)context;
		}
	}
}