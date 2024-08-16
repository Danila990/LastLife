using Ui.Sandbox.CharacterMenu;
using UnityEngine;

namespace Ui.Widget
{
	public class ButtonWidgetWithContent : ButtonWidget
	{
		public OutlineButtonWidget Prefab;
		public GameObject Container;
		public int PoolSize;
		public ButtonOutlineWidgetPool Pool;

		public void Prewarm()
		{
			Pool = new ButtonOutlineWidgetPool(Prefab, Container.transform);
			Pool.Prewarm(PoolSize);
		}
	}

}
