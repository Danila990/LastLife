using System.Collections.Generic;
using Core.Entity.Ai.AiItem;
using NodeCanvas.Framework;

namespace Core.Entity.Ai.AiActions
{
	public abstract class AbstractSelectItemAction : ActionTask
	{
		public BBParameter<List<IAiItem>> AiItems;
		public BBParameter<IAiItem> SelectedAiItem;
		public BBParameter<IAiTarget> SelectedTarget;
		public BBParameter<float> ItemDistance;
			
		protected override void OnExecute()
		{
			var items = AiItems.value;
			if (items == null || items.Count == 0)
			{
				EndAction(false);
				return;
			}
			
			Select(items, out var selected);
			if (selected != null)
			{
				SelectedAiItem.SetValue(selected);
				//Logger.Log(SelectedAiItem.value.ToString(), context: agent);
				ItemDistance.value = selected.UseRange;
				EndAction(true);
			}
			else
			{
				EndAction(false);
			}
		}

		protected abstract void Select(IList<IAiItem> aiItems, out IAiItem selected);
	}
}