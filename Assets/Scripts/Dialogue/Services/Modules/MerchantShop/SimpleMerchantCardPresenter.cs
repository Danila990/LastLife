using System;
using Core.Services;
using Db.MerchantData;
using Dialogue.Ui.MerchantUi;
using GameSettings;
using Ui.Widget;
using UniRx;
using UnityEngine;
using Utils.Constants;

namespace Dialogue.Services.Modules.MerchantShop
{
	public class SimpleMerchantCardPresenter : IDisposable
	{
		private readonly MerchantItemCardWidget _widget;
		public MerchantItemCardWidget Widget => _widget;
		private readonly IItemStorage _itemStorage;
		private readonly IItemUnlockService _unlockService;
		private IDisposable _disposable;
		private int Price => Data.Price;
		public bool IsConsumable { get; set; }
		public IntReactiveProperty CountInCart { get; private set; } = new IntReactiveProperty();
		public MerchantShopItemData Data { get; }

		public SimpleMerchantCardPresenter(
			MerchantShopItemData data,
			MerchantItemCardWidget widget,
			IItemStorage itemStorage,
			IItemUnlockService unlockService
			)
		{
			Data = data;
			_widget = widget;
			_itemStorage = itemStorage;
			_unlockService = unlockService;
			IsConsumable = Data.StoreItemType == StoreItemType.Consumable;
		}

		public void Show(IObservable<int> currentResourceCount)
		{
			
			_widget.AddRemoveWidget.AddBTN.onClick.AddListener(OnClickAdd);
			_widget.AddRemoveWidget.RemoveBTN.onClick.AddListener(OnClickRemove);
			
			_widget.Icon.sprite = Data.Ico;
			_widget.Price.SetCount(Price);
			_widget.Price.SetResource(Data.ResourceType);
			_widget.Price.ReLayout(ResourceWidget.CountTextPosition.Left);
			_widget.Name.text = Data.Name;

			_disposable = currentResourceCount.Subscribe(OnResourceCountChanged);
		}
		
		private void OnResourceCountChanged(int count)
		{
			RefreshUi(count);
		}

		private void OnClickRemove()
		{
			CountInCart.Value -= 1;
		}

		private void OnClickAdd()
		{
			CountInCart.Value += 1;
		}
		
		public int GetPriceByCount() => Price * CountInCart.Value;

		public void RefreshUi(int resourceCount)
		{
			if (IsConsumable)
			{
				_widget.AddRemoveWidget.AddBTN.interactable = resourceCount >= Price;
			}
			else
			{
				_widget.AddRemoveWidget.AddBTN.interactable = resourceCount >= Price && CountInCart.Value < 1;
			}
			_widget.AddRemoveWidget.RemoveBTN.interactable = CountInCart.Value > 0;
			_widget.Count.text = CountInCart.ToString();
			
			if (IsAvailable())
			{
				_widget.BlockPanel.gameObject.SetActive(false);
				_widget.AddRemoveWidget.gameObject.SetActive(true);
			}
			else
			{
				_disposable?.Dispose();
				_widget.AddRemoveWidget.RemoveBTN.onClick.RemoveAllListeners();
				_widget.AddRemoveWidget.AddBTN.onClick.RemoveAllListeners();
				_widget.BlockPanel.gameObject.SetActive(true);
				_widget.AddRemoveWidget.gameObject.SetActive(false);
				_widget.Icon.color = new Color(0.5f,0.5f,0.5f);
				CountInCart.Value = 0;
			}
		}

		public bool IsAvailable()
		{
			if (IsConsumable)
				return true;

			if (!string.IsNullOrEmpty(Data.UnlockKey) &&
			    _unlockService.IsUnlocked(Data))
			{
				return false;
			}


			if (_itemStorage.All.TryGetValue(Data.ObjectDataIdToBuy, out var objectData))
			{
				return !_unlockService.IsUnlocked(objectData);
			}
			else
			{
				Debug.LogError($"Item {Data.ObjectDataIdToBuy} not found");
				return true;
			}
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
			CountInCart?.Dispose();
		}
	}
}