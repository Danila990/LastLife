using Sirenix.OdinInspector;
using TMPro;
using Ui.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Ui.CustomViews
{
	public class BankMenuPanel : MonoBehaviour
	{
		public Button BackButton;
		public AudioClip DealSound;
		
		[BoxGroup("Header")] public TextMeshProUGUI PlayerNameTXT;
		[BoxGroup("Header")] public TextMeshProUGUI RightTxt;
		[BoxGroup("Header")] public Image ArrowImg;
		
		[BoxGroup("Header/Arrows"), HorizontalGroup("Header/Arrows/Group"), PreviewField] public Sprite BothArrow;
		[BoxGroup("Header/Arrows"), HorizontalGroup("Header/Arrows/Group"), PreviewField] public Sprite LeftArrow;
		
		[BoxGroup("Footer")] public Transform FooterRoot;
		[BoxGroup("Footer")] public TextMeshProUGUI MessageTXT;

		[BoxGroup("Prefabs")] public ResourceWidget ResourceWidget;
		[BoxGroup("Prefabs")] public AddRemoveWidget AddRemoveWidget;
		[BoxGroup("Prefabs")] public TextMeshProUGUI TextPrefab;
		[BoxGroup("Prefabs")] public NamedButtonWidget SimpleButtonWidget;

		[BoxGroup("Prefabs/Button"), PreviewField] public Sprite OrangeButtonSprite;
		public void Clear()
		{
			foreach (Transform footerRoot in FooterRoot)
			{
				Destroy(footerRoot.gameObject);	
			}
			ArrowImg.transform.localScale = new Vector3(1,1,1);
			MessageTXT.text = "";
		}
		
		public ResourceWidget CreateResourceWidget() => Instantiate(ResourceWidget, FooterRoot, false);
		public AddRemoveWidget CreateAddRemoveWidget() => Instantiate(AddRemoveWidget, FooterRoot, false);
		public TextMeshProUGUI CreateSimpleText() => Instantiate(TextPrefab, FooterRoot, false);
		public NamedButtonWidget CreateSimpleButtonWidget() => Instantiate(SimpleButtonWidget, FooterRoot, false);

		private void OnDisable()
		{
			gameObject.SetActive(false);
		}
		
		public void Relayout()
		{
			// Force re-layout
			var layouts = GetComponentsInChildren<LayoutGroup>();

			// Perform the first pass of re-layout. This will update the inner horizontal group's sizing, based on the text size.
			foreach (var layout in layouts)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
			}
            
			// Perform the second pass of re-layout. This will update the outer vertical group's positioning of the individual elements.
			foreach (var layout in layouts)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
			}
		}
	}
}