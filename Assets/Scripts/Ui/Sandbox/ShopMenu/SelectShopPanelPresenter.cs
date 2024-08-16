using System;
using Db.Panel;
using Ui.Widget;
using UniRx;
using UnityEngine;

namespace Ui.Sandbox.ShopMenu
{
	public class SelectShopPanelPresenter : IDisposable
	{
		private readonly ShopMenuController _shopMenuController;
		private readonly NamedButtonWidget _widget;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		public IShopPanelData ShopPanelData { get; }
		public Transform PanelsContent { get; }

		public SelectShopPanelPresenter(
			ShopMenuController shopMenuController, 
			NamedButtonWidget widget, 
			IShopPanelData shopPanel, 
			Transform panelsContent)
		{
			_shopMenuController = shopMenuController;
			_widget = widget;
			ShopPanelData = shopPanel;
			PanelsContent = panelsContent;
			
			_widget.IconImg.sprite = shopPanel.PanelIcon;
			_widget.Text.text = shopPanel.PanelName;
			RecalcWidgetSize();
			
			_widget.Button
				.OnClickAsObservable()
				.Subscribe(OnClickPanelWidget).AddTo(_compositeDisposable);
		}

		private void RecalcWidgetSize()
		{
			var textSize = _widget.Text.GetPreferredValues();
			var iconPreferredSize = _widget.IconImg.rectTransform.sizeDelta.x;
			var rect = _widget.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(textSize.x + iconPreferredSize + 30, rect.sizeDelta.y);
		}
		
		private void OnClickPanelWidget(Unit obj)
		{
			_shopMenuController.SelectPanel(ShopPanelData.PanelId);
		}
		
		public void Hide()
		{
			_widget.DeselectAlpha();
			PanelsContent.gameObject.SetActive(false);
		}
		
		public void Select()
		{
			_widget.SelectAlpha();
			PanelsContent.gameObject.SetActive(true);
		}

		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}
	}
}