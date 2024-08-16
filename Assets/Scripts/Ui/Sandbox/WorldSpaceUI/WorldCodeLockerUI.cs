using System.Linq;
using TMPro;
using Ui.Widget;
using UnityEngine.UI;

namespace Ui.Sandbox.WorldSpaceUI
{
	public class WorldCodeLockerUI : WorldSpaceUI
	{
		public TextMeshProUGUI[] InputFields;
		public string ResultStr => InputFields.Aggregate<TextMeshProUGUI, string>("", (s, ugui) => s += ugui.text);
		public ButtonWidget ActivateButton;
		public Image ColoredImage;
		/*public ButtonWidget TicketButton;
		public ButtonWidget AdvButton;*/
	}
}
