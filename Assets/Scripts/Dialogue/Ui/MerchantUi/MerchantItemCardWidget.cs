using TMPro;
using Ui.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Ui.MerchantUi
{
	public class MerchantItemCardWidget : MonoBehaviour
	{
		public Image Icon;
		public Image TransparentIcon;
		public Image Outline;
		public ResourceWidget Price;
		public AddRemoveWidget AddRemoveWidget;
		public TextMeshProUGUI Count;
		public TextMeshProUGUI Name;
		public GameObject BlockPanel;
		public CanvasGroup CanvasGroup;
	}
}