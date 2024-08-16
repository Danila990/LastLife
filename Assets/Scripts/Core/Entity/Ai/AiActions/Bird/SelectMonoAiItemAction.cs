using Core.Entity.Ai.AiItem;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Core.Entity.Ai.AiActions.Bird
{
	public class SelectMonoAiItemAction : ActionTask
	{
		[RequiredField] public BBParameter<MonoItemProvider> MonoItemProvider;
		[RequiredField] public BBParameter<string> ItemName;

		public BBParameter<IAiItem> SelectItem;

		protected override void OnExecute()
		{
			var item = MonoItemProvider.value.GetItemById(ItemName.value);
			if (item is null)
			{
				EndAction(false);
				return;
			}
			
			SelectItem.value = item;
			EndAction();
		}
	}
}