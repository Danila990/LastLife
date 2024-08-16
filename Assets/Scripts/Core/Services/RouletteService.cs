using System;
using Adv.Messages;
using Adv.Services.Interfaces;
using Core.Entity.InteractionLogic.Interactions;
using Core.Quests.Messages;
using GameSettings;
using LitMotion;
using LitMotion.Extensions;
using MessagePipe;
using SharedUtils.PlayerPrefs.Impl;
using Ticket;
using Ui.Sandbox.CharacterMenu;
using Ui.Sandbox.CharacterMenu.Roulette;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer.Unity;

namespace Core.Services
{
	public class RouletteService : IStartable, IFixedTickable, IDisposable
	{
		private readonly CharacterMenuView _characterMenuView;
		private readonly CharacterMenuController _characterMenuController;
		private readonly IAdvService _advService;
		private readonly IPublisher<ShowShopMenu> _publisher;
		private readonly ITicketService _ticketService;
		private readonly ISettingsService _settingsService;
		private readonly IQuestMessageSender _questMessageSender;
		private readonly InMemoryPlayerPrefsManager _inMemoryPlayerPrefsManager;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private RouletteController _rouletteController;
		private TicketWorldButtonPresenter _spinTicket;
		private TicketWorldButtonPresenter _takeTicket;
		private MotionHandle _rouletteHandle;
		private MotionHandle _rotateImg;
		private IDisposable _settingsDisposable;

		public RouletteService(
			CharacterMenuView characterMenuView,
			CharacterMenuController characterMenuController,
			IAdvService advService,
			IPublisher<ShowShopMenu> publisher,
			ITicketService ticketService,
			ISettingsService settingsService,
			IQuestMessageSender questMessageSender,
			InMemoryPlayerPrefsManager inMemoryPlayerPrefsManager
		)
		{
			_characterMenuView = characterMenuView;
			_characterMenuController = characterMenuController;
			_advService = advService;
			_publisher = publisher;
			_ticketService = ticketService;
			_settingsService = settingsService;
			_questMessageSender = questMessageSender;
			_inMemoryPlayerPrefsManager = inMemoryPlayerPrefsManager;
		}
		
		public void Start()
		{
			_rouletteController = new RouletteController(_characterMenuView.RouletteData).AddTo(_compositeDisposable);
			_rouletteController.Init();
			
			_settingsDisposable = _settingsService.OnValueByKeyChanged.Where(s => s == SettingsConsts.IN_PLAYER_WINDOW_IAP).Subscribe(OnAbTest);
			OnAbTest(_settingsService.GetValue<string>(SettingsConsts.IN_PLAYER_WINDOW_IAP, GameSetting.ParameterType.String));
			
			_characterMenuController.IsShown.Subscribe(OnRouletteShown).AddTo(_compositeDisposable);
			_rouletteController.OnButtonState.Subscribe(OnButtonState).AddTo(_compositeDisposable);
			_characterMenuView.RouletteData.HideOrShowButton.OnClickAsObservable().Subscribe(OnClickShowOrHideRoulette).AddTo(_compositeDisposable);
			
			_characterMenuView.RouletteData.SpinButton.OnClickAsObservable().Subscribe(SpinSub).AddTo(_compositeDisposable);
			_characterMenuView.RouletteData.TakeButton.OnClickAsObservable().Subscribe(SelectCharacterSub).AddTo(_compositeDisposable);
			
			_spinTicket = new TicketWorldButtonPresenter(_ticketService, Spin, "Spin").AddTo(_compositeDisposable);
			_spinTicket.Attach(_characterMenuView.RouletteData.SpinButtonTicket);
			
			_takeTicket = new TicketWorldButtonPresenter(_ticketService, SelectCharacter, "Take").AddTo(_compositeDisposable);
			_takeTicket.Attach(_characterMenuView.RouletteData.TakeButtonTicket);
			
			_characterMenuView.RouletteData.OpenShop.OnClickAsObservable().Subscribe(OpenShop).AddTo(_compositeDisposable);
		}
		
		private void OnAbTest(string obj)
		{
			if (obj is "A")
			{
				_rouletteController.RouletteIsOpened = false;
				ShowOrHideRoulette(true);
			}
			else
			{
				_rouletteController.RouletteIsOpened = true;
				ShowOrHideRoulette(false);
			}
		}

		private void OnClickShowOrHideRoulette(Unit obj)
		{
			ShowOrHideRoulette(!_rouletteController.RouletteIsOpened);
		}
		
		private void ShowOrHideRoulette(bool isOpened)
		{
			if(isOpened == _rouletteController.RouletteIsOpened)
				return;
			
			var rouletteHolder = _characterMenuView.RouletteData.RouletteHolder;
			var wheelUpDownImg = _characterMenuView.RouletteData.RouletteUpDownImg;
			_rouletteHandle.IsActiveCancel();
			_rotateImg.IsActiveCancel();

			const float animDuration = .3f;
			if (isOpened)
			{
				_rouletteHandle = LMotion
					.Create(rouletteHolder.anchoredPosition.y, 485, animDuration)
					.WithEase(Ease.OutQuad)
					.BindToAnchoredPositionY(rouletteHolder);
				
				_rotateImg = LMotion
					.Create(wheelUpDownImg.localEulerAngles.z, 0, animDuration)
					.WithEase(Ease.OutQuad)
					.BindToLocalEulerAnglesZ(wheelUpDownImg);
			}
			else
			{
				_rouletteHandle = LMotion
					.Create(rouletteHolder.anchoredPosition.y, 100, animDuration)
					.WithEase(Ease.InQuad)
					.BindToAnchoredPositionY(rouletteHolder);
				
				_rotateImg = LMotion
					.Create(wheelUpDownImg.localEulerAngles.z, 180, animDuration)
					.WithEase(Ease.InQuad)
					.BindToLocalEulerAnglesZ(wheelUpDownImg);
			}
			_rouletteController.RouletteIsOpened = isOpened;
		}

		private void OpenShop(Unit obj)
		{
			_publisher.Publish(new ShowShopMenu("shooters", _rouletteController.LastDrop?.Id));
		}
		
		private void OnRouletteShown(bool status)
		{
			if (status)
			{
				_rouletteController.Refresh();
				if (_rouletteController.IsAllItemsUnlocked)
				{
					_characterMenuView.RouletteData.RouletteHolder.gameObject.SetActive(false);
					var size = _characterMenuView.RouletteData.ScrollView.sizeDelta;
					Debug.Log(size);
					Debug.Log(((RectTransform)_characterMenuView.RouletteData.ScrollView.parent).sizeDelta);
					var parentSize = ((RectTransform)_characterMenuView.RouletteData.ScrollView.parent).sizeDelta.y - size.y;
					size.y = 1500;
					_characterMenuView.RouletteData.ScrollView.sizeDelta = size;
				}
				else if (_rouletteController.LastDrop == null)
				{
					_characterMenuView.RouletteData.TakeButton.gameObject.SetActive(false);
					_characterMenuView.RouletteData.OpenShop.gameObject.SetActive(false);
					SetButtonPos(_characterMenuView.RouletteData.SpinButton,new Vector2(0,0));
				}
			}
		}

		private void OnButtonState(bool isActive)
		{
			_characterMenuView.RouletteData.SpinButton.gameObject.SetActive(isActive);
			_characterMenuView.RouletteData.TakeButton.gameObject.SetActive(isActive);
			_characterMenuView.RouletteData.OpenShop.gameObject.SetActive(isActive);
			SetButtonPos(_characterMenuView.RouletteData.SpinButton,new Vector2(-350,0));
			SetButtonPos(_characterMenuView.RouletteData.TakeButton,new Vector2(200,0));
		}

		private void SetButtonPos(Component btn, Vector2 pos)
		{
			((RectTransform)btn.transform).anchoredPosition = pos;
		}

		private void SelectCharacterSub(Unit obj)
		{
			_advService.RewardRequest(SelectCharacter, "Roulette:SelectCharacter");
		}
		
		private void SelectCharacter()
		{
			if(_characterMenuController is  null || _rouletteController is null)
				return;

			var id = _rouletteController.ClaimLastDrop().Id;
			_inMemoryPlayerPrefsManager.SetValue(Consts.SPIN_CHARACTER_KEY,id);
			_questMessageSender.SendLuckySpinMessage(null);
			_characterMenuController.PlayAndClose(id);
		}

		private void SpinSub(Unit obj)
		{
			_advService.RewardRequest(Spin, "Roulette:Spin");
		}
		
		private void Spin()
		{
			_rouletteController.OnSpinStart();
			_rouletteController.Spin(20);
		}

		public void FixedTick()
		{
			_rouletteController.UpdateSpeed(Time.fixedUnscaledDeltaTime);	
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
			_spinTicket?.Dispose();
			_takeTicket?.Dispose();
		}
	}
}