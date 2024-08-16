using Ui.Sandbox.SelectMenu;
using Ui.Sandbox.SpawnMenu.Panel;
using Ui.Widget;
using UnityEngine;

namespace Ui.Sandbox.SpawnMenu
{
	public class SpawnMenuView : SelectMenuMainView
	{
		public NamedButtonWidget SelectPanelButtonWidget;
		public Transform SelectPanelButtonsHolder;

		public PanelContent PanelContentPrefab;
		public Transform ElementsContentHolder;
		public Transform ElementsForCategory;

		public OutlineButtonWidget SelectItemWidget;
		public OutlineButtonWidget SelectItemWidgetForNPC;
		public NamedButtonWidget CategoriesButtonWidget;
	}
}