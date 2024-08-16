using UnityEngine;
using UnityEngine.UI;

namespace Ui.Widget
{
	public class HighlightableImage : MonoBehaviour
	{
		public Image Ico;
		public Image Outline;
		public Color DefaultColor;
		public Color HighlightColor;

		public void Highlight(bool status)
		{
			Outline.enabled = status;
			Outline.color = status ? HighlightColor : DefaultColor;
		}
	}
}