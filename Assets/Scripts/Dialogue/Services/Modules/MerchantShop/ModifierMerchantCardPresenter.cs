using System;
using Core.Services;
using Db.MerchantData;
using Dialogue.Ui.MerchantUi;
using GameSettings;
using NodeCanvas.Tasks.Conditions;
using Ui.Widget;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class ModifierMerchantCardPresenter : IDisposable
	{
		private readonly MerchantShopItemData _itemModel;
		private readonly MerchantItemCardWidget _widget;
		private readonly ISettingsService _settingsService;
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly ReactiveCommand<ModifierMerchantCardPresenter> _onClicked = new ();
		private IDisposable _disposable;

		public int Price => _itemModel.Price;
		public bool IsConsumable => Data.StoreItemType == StoreItemType.Consumable;

		public IObservable<ModifierMerchantCardPresenter> Clicked => _onClicked;
		public MerchantShopItemData Data => _itemModel;
		public bool IsActive { get; private set; }

		public ModifierMerchantCardPresenter(
			MerchantShopItemData itemModel,
			MerchantItemCardWidget widget, 
			ISettingsService settingsService, 
			IItemStorage itemStorage,
			IItemUnlockService itemUnlockService)
		{
			_itemModel = itemModel;
			_widget = widget;
			_settingsService = settingsService;
			_itemStorage = itemStorage;
			_itemUnlockService = itemUnlockService;
		}

		public void Show(IntReactiveProperty currentResourceCount)
		{
			_disposable = _widget.Icon.OnPointerClickAsObservable().Subscribe(OnClickImage);
			_widget.AddRemoveWidget.gameObject.SetActive(false);

			if (Data.TransparentIcon)
			{
				_widget.TransparentIcon.sprite = _itemModel.Ico;
				_widget.TransparentIcon.gameObject.SetActive(true);
			}
			else
			{
				_widget.TransparentIcon.transform.parent.gameObject.SetActive(false);
				_widget.Icon.sprite = _itemModel.Ico;
			}

			if (Data.DrawPrice)
			{
				_widget.Price.SetCount(Price);
				_widget.Price.SetResource(_itemModel.ResourceType);
				_widget.Price.ReLayout(ResourceWidget.CountTextPosition.Left);
				_widget.Price.gameObject.SetActive(true);
			}
			else
			{
				_widget.Price.gameObject.SetActive(false);
			}
			
			
			_widget.Name.text = _itemModel.Name;
			_widget.Outline.enabled = false;
			IsActive = true;

			
		}
		
		public bool IsAvailable()
		{
			if (IsConsumable)
				return true;

			if (!string.IsNullOrEmpty(Data.UnlockKey) &&
			    _itemUnlockService.IsUnlocked(Data))
			{
				return false;
			}


			if (_itemStorage.All.TryGetValue(Data.ObjectDataIdToBuy, out var objectData))
			{
				return !_itemUnlockService.IsUnlocked(objectData);
			}
			else
			{
				Debug.LogError($"Item {Data.ObjectDataIdToBuy} not found");
				return true;
			}
		}

		private void OnClickImage(PointerEventData obj)
		{
			_onClicked.Execute(this);
		}
		
		public void Select()
		{
			_widget.Outline.color = new Color(0, 0.7686f, 1);
			_widget.Outline.enabled = true;
		}
		
		public void Deselect()
		{
			_widget.Outline.enabled = false;
		}

		public void Dispose()
		{
			IsActive = false;
			_onClicked?.Dispose();
			_disposable?.Dispose();
		}
		
		public void Disable()
		{
			Dispose();
			var gray = new Color(0.65f, 0.65f, 0.65f);
			_widget.TransparentIcon.color = gray;
			_widget.Icon.color = gray;
			_widget.Name.faceColor = gray;
			_widget.CanvasGroup.alpha = .9f;
			_widget.Price.gameObject.SetActive(false);
		}
	}
}