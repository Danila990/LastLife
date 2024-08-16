using UnityEngine;
using UnityEngine.UI;

namespace Ui.Widget
{
	public class OutlineButtonWidget : ButtonWidget
	{
		public Image Outline;
		public Color DefaultColor;
		public Color OutlineColor;

		public virtual void SelectOutline()
		{
			Outline.color = OutlineColor;
		}

		public virtual void DeselectOutline()
		{
			Outline.color = DefaultColor;
		}
	}
}