using Core.ResourcesSystem;
using Db;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Widget
{

	public class ResourceWidget : MonoBehaviour
	{
		public TextMeshProUGUI CountText;
		public Image ResourceIcon;
		public float TextMargin = 20f;
		
		public ResourceIconData ResourceIconData;
		public ResourceType ResourceType { get; set; }

		[Button]
		public void SetResource(ResourceType resourceType)
		{
			ResourceType = resourceType;
			ResourceIcon.sprite = ResourceIconData.GetIcon(resourceType);
		}
		
		public void SetCount(int value)
		{
			CountText.text = value.ToString();
		}
		
		[Button]
		public void ReLayout(CountTextPosition textPosition)
		{
			Vector4 countTextMargin;
			switch (textPosition)
			{
				case CountTextPosition.Left:
					CountText.alignment = TextAlignmentOptions.Right;
					countTextMargin = CountText.margin;
					countTextMargin.x = TextMargin;
					countTextMargin.z = TextMargin;
					CountText.margin = countTextMargin;
					CountText.transform.SetAsFirstSibling();
					break;
				case CountTextPosition.Right:
					CountText.alignment = TextAlignmentOptions.Left;
					countTextMargin = CountText.margin;
					countTextMargin.x = TextMargin;
					countTextMargin.z = TextMargin;
					CountText.margin = countTextMargin;
					CountText.transform.SetAsLastSibling();
					break;
			}
		}
		
		public enum CountTextPosition
		{
			Left,
			Right
		}
	}
}