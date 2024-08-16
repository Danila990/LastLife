using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.Widget
{
	public class ShopButtonWidget : ButtonWidget
	{
		public TextMeshProUGUI ItemAdditionalTXT;
		public TextMeshProUGUI ItemCountTXT;
		
		public TextMeshProUGUI PriceTXT;
		public Image PriceImage;

		private Sequence _sequence;
		public GameObject NewHolder;

		public void UsePrimeImage()
		{
			PriceTXT.gameObject.SetActive(false);
			PriceImage.gameObject.SetActive(true);
		}
		
		public void Highlight()
		{
			_sequence?.Kill(true);
			_sequence = DOTween.Sequence();
			_sequence
				.SetUpdate(true)
				.Append(transform.DOScale(1.2f, 0.2f))
				.Append(transform.DOScale(1f, 0.2f));
		}

		protected override void OnDisableInternal()
		{
			_sequence?.Kill(true);
		}
	}
}