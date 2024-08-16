using TMPro;
using UnityEngine;

namespace Ui.Widget
{
	public class NamedButtonWidget : ButtonWidget
	{
		public TextMeshProUGUI Text;

		public void SetWidthByText(float margin = 20)
		{
			var rectTransform = (RectTransform)transform;
			var width = Text.preferredWidth;
			var delta = rectTransform.sizeDelta;
			delta.x = width + margin;
			rectTransform.sizeDelta = delta;
		}
	}
}