using System.Collections.Generic;
using System.Linq;
using Core.Entity.Ai.AiItem;

namespace Core.Entity.Ai.AiActions
{
	public class FirstSelectItemAction : AbstractSelectItemAction
	{
		protected override void Select(IList<IAiItem> aiItems, out IAiItem selected)
		{
			selected = aiItems.FirstOrDefault();
		}
	}
}