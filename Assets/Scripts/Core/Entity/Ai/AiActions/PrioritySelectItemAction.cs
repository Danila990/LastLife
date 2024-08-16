using System.Collections.Generic;
using Core.Entity.Ai.AiItem;
using SharedUtils;

namespace Core.Entity.Ai.AiActions
{
	public class PrioritySelectItemAction : AbstractSelectItemAction
	{
		protected override void Select(IList<IAiItem> aiItems, out IAiItem selected)
		{
			aiItems.Shuffle();
			var maxPriority = float.NegativeInfinity;
			var currAttack = aiItems[0];
			var selectedTarget = SelectedTarget.value;
			foreach (var attack in aiItems)
			{
#if UNITY_EDITOR
				// if (_debug)
				// {
				// 	Debug.Log($"atk:{attack}, canSelect:{attack.CanBeSelected}, prioriry:{attack.Priority(ref target)}");
				// }
#endif
				var priority = attack.GetPriority(selectedTarget);
				if (//attack.CanBeSelected && 
				    maxPriority < priority)
				{
					currAttack = attack;
					maxPriority = priority;
				}
			}
			
			selected = currAttack;
		}
	}
}