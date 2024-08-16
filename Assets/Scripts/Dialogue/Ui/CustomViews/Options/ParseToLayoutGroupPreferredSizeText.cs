using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Ui.CustomViews.Options
{
	public class ParseToLayoutGroupPreferredSizeText : MonoBehaviour
	{
		public TextMeshProUGUI TextMeshProUGUI;
		public LayoutElement LayoutElement;

		private void OnValidate()
		{
			UpdatePreferredValues();
		}
		
		[Button]
		private void UpdatePreferredValues()
		{
			if (TextMeshProUGUI && LayoutElement)
			{
				var values = TextMeshProUGUI.GetPreferredValues();
				LayoutElement.preferredWidth = values.x;
				LayoutElement.preferredHeight = values.y;
			}
		}

		private void OnEnable()
		{
			TextMeshProUGUI.OnPreRenderText += Sub;	
		}

		private void OnDisable()
		{
			TextMeshProUGUI.OnPreRenderText -= Sub;	
		}

		private void Sub(TMP_TextInfo obj)
		{
			UpdatePreferredValues();
		}
	}
}