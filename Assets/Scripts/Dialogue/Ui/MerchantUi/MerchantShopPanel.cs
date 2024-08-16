using TMPro;
using Ui.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue.Ui.MerchantUi
{
	public class MerchantShopPanel : MonoBehaviour
	{
		public Button BackButton;
		public AudioClip DealSound;
		
		public TextMeshProUGUI MessageTXT;
		public TextMeshProUGUI RightText;
		public MerchantItemCardWidget ItemCardPrefab;
		public MerchantItemCardWidget ModifiersCardPrefab;
		public Transform ItemsContainer;
		
		public NamedButtonWidget SubmitButton;
		public ResourceWidget FullPrice;
		public ResourceWidget CurrentResourceCount;
		public GridLayoutGroup GridLayout;
		public HorizontalLayoutGroup Footer;

		public MerchantItemCardWidget MerchantItemCardWidget() => Instantiate(ItemCardPrefab, ItemsContainer, false);
		public MerchantItemCardWidget ModifiersCardWidget() => Instantiate(ModifiersCardPrefab, ItemsContainer, false);
		
		private Vector2 _defaultCellSize;
		private float _defaultSpacing;
		public RectTransform Rt { get; private set; }
		private Vector2 _defaultSize;

		private void Awake()
		{
			Clear();
		}
		
		public void Init()
		{
			_defaultCellSize = GridLayout.cellSize;
			_defaultSpacing = Footer.spacing;
			Rt = transform as RectTransform;
			_defaultSize = Rt!.sizeDelta;
		}
		
		private void OnDisable()
		{
			gameObject.SetActive(false);
		}

		public void Clear()
		{
			foreach (Transform footerRoot in ItemsContainer)
			{
				Destroy(footerRoot.gameObject);	
			}
			CurrentResourceCount.gameObject.SetActive(true);
			MessageTXT.text = "";
			GridLayout.cellSize = _defaultCellSize;
			Footer.spacing = _defaultSpacing;
			Rt.sizeDelta = _defaultSize;
			SubmitButton.Text.text = "Buy";
			SubmitButton.SetWidthByText(50);
			FullPrice.ReLayout(ResourceWidget.CountTextPosition.Left);
			Footer.spacing = -20;
		}
		
		public void ReLayout()
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