using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Widget
{
	public class OutlineButtonWidgetWithNumber : OutlineButtonWidget
	{
		public TextMeshProUGUI Number;
		public Image NumberBack;
		public Image AdditionalOutline;
		public GameObject NewHolder;
		
		public override void DeselectOutline()
		{
			AdditionalOutline.enabled = false;
		}

		public override void SelectOutline()
		{
			AdditionalOutline.enabled = true;
		}
	}
}
