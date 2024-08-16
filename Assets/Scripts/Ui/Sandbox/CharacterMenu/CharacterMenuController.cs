using System;
using System.Collections.Generic;
using System.Threading;
using Adv.Services.Interfaces;
using Analytic;
using Core.Entity.EntityUpgrade;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Render;
using Core.Services;
using Core.Services.Experience;
using Cysharp.Threading.Tasks;
using Db.ObjectData;
using GameSettings;
using MessagePipe;
using SharedUtils;
using SharedUtils.PlayerPrefs;
using SharedUtils.PlayerPrefs.Impl;
using Shop;
using Shop.Abstract;
using Sirenix.Utilities;
using Ticket;
using TMPro;
using Ui.Sandbox.SelectMenu;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer.Unity;
using VContainerUi.Messages;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.CharacterMenu
{
	public class CharacterMenuController : SelectMenuController<CharacterMenuView>, IInitializable, IStartable, IDisposable, IPostTickable
	{
		private readonly IItemStorage _itemStorage;
		private readonly IPlayerSpawnService _spawnService;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly ISubscriber<PlayerContextChangedMessage> _contextChangedEvent;

		private readonly IPublisher<MessageOpenWindow> _openWin;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IExperienceService _experienceService;
		private readonly ICharacterUpgradesData _upgradesData;
		private readonly IRendererFactory _rendererFactory;
		private readonly IAnalyticService _analyticService;
		private readonly ISettingsService _settingsService;
		private readonly IShopStorage _shopStorage;
		private readonly IPurchaseService _purchaseService;
		private readonly ITicketService _ticketService;
		private readonly IAdvService _advService;
		private readonly ILocalizePriceService _localizePriceService;
		private readonly InMemoryPlayerPrefsManager _inMemoryPlayerPrefsManager;
		private readonly Dictionary<string, CharacterSelectPresenter> _selectControllers = new Dictionary<string, CharacterSelectPresenter>();
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private readonly BoolReactiveProperty _isShown;
		private const string SELECTED_CHARACTER_KEY = "SELECTED_CHARACTER_KEY";
		private const string DEFAULT_CHARACTER = "pistolman";
		
		private EquipmentMenuController _equipmentMenuController;
		private MainCharacterPreviewController _previewController;
		public ReactiveProperty<CharacterObjectData> SelectedCharacter { get; set; } = new ReactiveProperty<CharacterObjectData>();
		public CharacterObjectData ActiveCharacter { get; set; }
		public CharacterSelectPresenter SelectedCharacterPresenter { get; set; }
		public IReactiveProperty<bool> IsShown => _isShown;
		
		private bool _isAlive;
		private CharacterPanelSwitchController _screensController;
		private SkillsScreenController _skillsScreenController;
		private bool _inWindowIAPVersion = false;
		
		
		public CharacterMenuController(
			IMenuPanelService menuPanelService, 
			IItemStorage itemStorage,
			IPlayerSpawnService spawnService,
			IPlayerPrefsManager playerPrefsManager,
			ISubscriber<PlayerContextChangedMessage> contextChangedEvent,
			IPublisher<MessageOpenWindow> openWin,
			IItemUnlockService itemUnlockService,
			IExperienceService experienceService,
			ICharacterUpgradesData upgradesData,
			IRendererFactory rendererFactory,
			IAnalyticService analyticService,
			ISettingsService settingsService,
			IShopStorage shopStorage,
			IPurchaseService purchaseService,
			ITicketService ticketService,
			IAdvService advService,
			ILocalizePriceService localizePriceService,
			InMemoryPlayerPrefsManager inMemoryPlayerPrefsManager
			) : base(menuPanelService)
		{
			_itemStorage = itemStorage;
			_spawnService = spawnService;
			_playerPrefsManager = playerPrefsManager;
			_contextChangedEvent = contextChangedEvent;

			_openWin = openWin;
			_itemUnlockService = itemUnlockService;
			_experienceService = experienceService;
			_upgradesData = upgradesData;
			_rendererFactory = rendererFactory;
			_analyticService = analyticService;
			_settingsService = settingsService;
			_shopStorage = shopStorage;
			_purchaseService = purchaseService;
			_ticketService = ticketService;
			_advService = advService;
			_localizePriceService = localizePriceService;
			_inMemoryPlayerPrefsManager = inMemoryPlayerPrefsManager;
			_isShown = new BoolReactiveProperty(false);
		}
		
		public void Initialize()
		{
			SelectedCharacter.Value = GetSavedChar();
		}

		public CharacterObjectData GetSavedChar()
		{
			var selectedName = _playerPrefsManager.GetValue(SELECTED_CHARACTER_KEY, DEFAULT_CHARACTER);
			var inMem = _inMemoryPlayerPrefsManager.GetValue(Consts.SPIN_CHARACTER_KEY, "");
			if (!string.IsNullOrEmpty(inMem))
			{
				selectedName = inMem;
			}

			return _itemStorage.Characters.TryGetValue(selectedName, out var characterObjectData) ? characterObjectData : null;
		}
		
		public void Start()
		{
			_settingsService.OnValueByKeyChanged.Where(s => s == SettingsConsts.IN_PLAYER_WINDOW_IAP).Subscribe(OnAbTest).AddTo(View);
			_settingsService.OnValueByKeyChanged.Where(s => s == SettingsConsts.ADDITIONAL_ADV_FEATURES).Subscribe(OnAdvFeatureTest).AddTo(View);
			
			InitControllers();
			CreateCharItems();
			Subscribes();
			SelectDefaultCharacter();
			
			OnAbTest(_settingsService.GetValue<string>(SettingsConsts.IN_PLAYER_WINDOW_IAP, GameSetting.ParameterType.String));
			OnAdvFeatureTest(_settingsService.GetValue<string>(SettingsConsts.ADDITIONAL_ADV_FEATURES, GameSetting.ParameterType.String));
		}
		
		private void OnAdvFeatureTest(string getValue)
		{
			_skillsScreenController.SetIsActiveFeatures(getValue is not "A");
		}
		
		private void OnAbTest(string getValue)
		{
			_inWindowIAPVersion = getValue is not "A";
			foreach (var item in _selectControllers.Values)
			{
				item.OnConfigUpdated(_inWindowIAPVersion);
			}
		}
		

		public void PostTick()
		{
			_previewController?.OnTick();
		}

#region Initialize
		private void SelectDefaultCharacter()
		{
			var selectedName = _playerPrefsManager.GetValue(SELECTED_CHARACTER_KEY, DEFAULT_CHARACTER);
			if (_selectControllers.TryGetValue(selectedName, out var value))
				ActiveCharacter = value.ObjectData;
			SelectCharacter(selectedName);
		}

		private void Subscribes()
		{
			_itemUnlockService.OnItemUnlock.Subscribe(OnItemUnlock).AddTo(_compositeDisposable);
			_contextChangedEvent.Subscribe(OnContextChanged).AddTo(_compositeDisposable);
			_isShown.AddTo(_compositeDisposable);
			_experienceService.CurrentLevel.Subscribe(OnLevelChanged).AddTo(_compositeDisposable);
		}
		
		private void CreateCharItems()
		{
			foreach (var characterObject in _itemStorage.Characters.Values)
				CreateNewItem(characterObject);
		}
		
		private void InitControllers()
		{
			_screensController = new CharacterPanelSwitchController(View.CharacterScreen, View.SkillsScreen).AddTo(_compositeDisposable);
			_skillsScreenController = new SkillsScreenController(
					View.SkillsScreenData,
					_upgradesData, 
					_playerPrefsManager, 
					_experienceService, 
					SelectedCharacter,
					_itemUnlockService,
					_advService,
					_ticketService)
				.AddTo(_compositeDisposable);
			
			_equipmentMenuController = new EquipmentMenuController(View.EquipmentMenuData, _itemStorage).AddTo(_compositeDisposable);
			_previewController = new MainCharacterPreviewController(View.MainCharacterPreview, _rendererFactory).AddTo(_compositeDisposable);
		}

#endregion
		
#region Public
		public void SelectSkillsScreen()
			=> _screensController.SelectSkillsScreen();

		public void PlayAndClose(string objectId)
		{
			CreatePlayer(_selectControllers[objectId].ObjectData);
		}
		
		public void SelectCharacter(CharacterObjectData characterObject)
		{
			SelectedCharacterPresenter?.Deselect();
			SelectedCharacterPresenter = _selectControllers[characterObject.Id];
			SelectedCharacterPresenter.Select();
			

			View.CharNameTXT.text = string.Format(View.CharNameFormat, characterObject.Name, characterObject.AdditionalName);
			SelectedCharacter.Value = characterObject;


			if (SelectedCharacterPresenter.IsUnlocked)
			{
				_playerPrefsManager.SetValue(SELECTED_CHARACTER_KEY, characterObject.Id);

				if (_upgradesData.TryGetUpgrade(characterObject.PlayerId, out var upgrade))
				{
					var level = upgrade.GetLevel(_playerPrefsManager);
					var uiData = _upgradesData.GetUI(level);
					SetFullPreviewLevel(uiData, level);
				}
				SetPreviewLevelStatus(upgrade != null);
			}
			else
			{
				SetPreviewLevelStatus(false);
			}
			
			
			if(_isShown.Value)
				UpdateEquipment();
			
			UpdateChooseBtnState();
			ChangeEquipmentState();
		}
#endregion
		
#region Subs
		private void OnItemUnlock(ObjectData obj)
		{
			_previewController.OnItemUnlock(obj.Id);
			if (_selectControllers.TryGetValue(obj.Id, out var presenterItem))
			{
				presenterItem.SetUnlockStatus(true);
				UpdateChooseBtnState();
				_skillsScreenController.RefreshUi();
				
				if (_upgradesData.TryGetUpgrade(presenterItem.ObjectData.PlayerId, out var upgrade))
				{
					var level = upgrade.GetLevel(_playerPrefsManager);
					var uiData = _upgradesData.GetUI(level);
					presenterItem.SetLevel(level, uiData);
				}
				else
				{
					presenterItem.HideLevel();
				}
				SelectCharacter(presenterItem.ObjectData);
			}
		}
		
		private void OnContextChanged(PlayerContextChangedMessage msg)
		{
			if (msg.Created)
			{
				MenuPanelService.SetBlockSelectingStatus(false);
				_equipmentMenuController.SetInventoryRef(msg.CharacterContext.EquipmentInventory);
				_isAlive = true;
				msg.CharacterContext.Health.OnDeath
					.SubscribeWithState(
						View.destroyCancellationToken,
						OnCharDeath)
					.AddTo(msg.CharacterContext);
			}
		}

		private void OnCharDeath(DiedArgs args, CancellationToken ct)
		{
			_inMemoryPlayerPrefsManager.SetValue(Consts.SPIN_CHARACTER_KEY,"");
			OnDeadShowPanel(ct).Forget();
			_analyticService.SendEvent($"Player:Died:{args.GetDiedMeta()}");
		}
		
		private void OnLevelChanged(int level)
		{
			if(ActiveCharacter == null)
				return;
			
			var uiData = _upgradesData.GetUI(level);
			_selectControllers[ActiveCharacter.Id].SetLevel(level, uiData);
			
			if(ActiveCharacter != SelectedCharacter.Value)
				return;
			
			SetFullPreviewLevel(uiData, level);
			SetPreviewLevelStatus(uiData != null);
		}
		
		private void OnClickPlay(Unit obj)
		{
			if (SelectedCharacter == null)
				return;

			CreatePlayer(SelectedCharacter.Value);
		}
		
		private void OnClickBuy(Unit obj)
		{
			if (SelectedCharacter == null || !_shopStorage.CharacterShopItems.TryGetValue(SelectedCharacter.Value.Id, out var shopItem))
				return;

			_purchaseService.PurchaseItem(shopItem);
		}
		
#if UNITY_INCLUDE_TESTS
		public void ClickPlayForTest()
		{
			CreatePlayer(ActiveCharacter);
		}
#endif
		
		private void CreateNewItem(CharacterObjectData characterObject)
		{
			var charSelectButton = Object.Instantiate(View.SelectCharacterPrefab, View.Characters4SelectHolder);
			var characterSelectController = new CharacterSelectPresenter(
				this,
				charSelectButton, 
				characterObject,
				View.LockSprite
			).AddTo(_compositeDisposable);
			
			if (_upgradesData.TryGetUpgrade(characterObject.PlayerId, out var upgrade))
			{
				var level = upgrade.GetLevel(_playerPrefsManager);
				var uiData = _upgradesData.GetUI(level);
				characterSelectController.SetLevel(level, uiData);
			}
			else
			{
				characterSelectController.HideLevel();
			}

			characterSelectController.SetUnlockStatus(_itemUnlockService.IsUnlocked(characterObject));
			characterSelectController.OnConfigUpdated(_inWindowIAPVersion);
			_selectControllers.Add(characterObject.Id, characterSelectController);
		}
#endregion
		
#region Overides
		public override void OnHide()
		{
			base.OnHide();
			_equipmentMenuController.Hide();
			_previewController.Hide();
			_isShown.Value = false;
		}

		public override void OnShow()
		{
			base.OnShow();
			ChangeEquipmentState();
			_isShown.Value = true;
			View.HideButton.gameObject.SetActive(_isAlive);
			View.PlayBTN.OnClickAsObservable().Subscribe(OnClickPlay).AddTo(ShowOnlyDisposable);
			View.BuyBTN.OnClickAsObservable().Subscribe(OnClickBuy).AddTo(ShowOnlyDisposable);
			
			_screensController.SelectCharacterScreen();
			UpdateChooseBtnState();
			UpdateEquipment();
		}

		private void UpdateEquipment()
		{
			var all = _spawnService.PlayerCharacterAdapter.AllEquipment;
			
			_previewController.Display(SelectedCharacter.Value.Id);
			if (ActiveCharacter == SelectedCharacter.Value && _spawnService.PlayerCharacterAdapter.CurrentContext)
			{
				var active = _spawnService.PlayerCharacterAdapter.CurrentContext.EquipmentInventory.Controller.ActiveEquipment;
				_previewController.ActiveRenderer.EquipmentRenderer.DisplayFromRuntime(all, active);
			}
			else
			{
				_previewController.ActiveRenderer.EquipmentRenderer.DisplayFromSave(all, SelectedCharacter.Value.PlayerId);
			}
		}
		
		private void UpdateChooseBtnState()
		{
			if (SelectedCharacterPresenter.IsUnlocked)
			{
				View.PlayBTN.gameObject.SetActive(true);
				View.BuyBTN.gameObject.SetActive(false);
				if (_spawnService.PlayerCharacterAdapter.CurrentContext is null)
				{
					View.PlayBTN.interactable = true;
					return;
				}
				var playerIsDeath = _spawnService.PlayerCharacterAdapter.CurrentContext.Health.IsDeath;
				var selectedPlayerNotEqualPlaying = SelectedCharacter.Value?.PlayerId != ActiveCharacter?.PlayerId;
			
				View.PlayBTN.interactable = 
					playerIsDeath ||
					selectedPlayerNotEqualPlaying;
				View.BuyBTNTxt.fontSize = 42;
			}
			else
			{
				View.PlayBTN.gameObject.SetActive(false);
				View.BuyBTN.gameObject.SetActive(true);
				View.BuyBTNTxt.text = _localizePriceService.Prices[_shopStorage.CharacterShopItems[SelectedCharacterPresenter.ObjectData.Id].InAppId].Value.ToString();
				View.BuyBTNTxt.fontSize = 32;
			}
		}

#endregion

		private async UniTaskVoid OnDeadShowPanel(CancellationToken token)
		{
			MenuPanelService.CloseMenu();
			_openWin.OpenWindow<IntermediateDeathWindow>();
			_isAlive = false;

			await UniTask.Delay(2.5f.ToSec(), cancellationToken: token);
			
			MenuPanelService.SelectMenu(nameof(CharacterMenuWindow));
			MenuPanelService.SetBlockSelectingStatus(true);
		}

		private void ChangeEquipmentState()
		{
			if (ActiveCharacter == null || SelectedCharacter == null || !_isAlive)
			{
				_equipmentMenuController.Hide();
				return;
			}

			if (SelectedCharacter.Value == ActiveCharacter)
			{
				_equipmentMenuController.Show();
			}
			else
			{
				_equipmentMenuController.Hide();
				_equipmentMenuController.HideContent();
			}
		}

		private void CreatePlayer(CharacterObjectData characterObject)
		{
			ActiveCharacter = characterObject;
			_spawnService.CreatePlayerContext(characterObject);
			MenuPanelService.CloseMenu();
		}

		private void SelectCharacter(string id)
		{
			if (_selectControllers.TryGetValue(id, out var value))
			{
				SelectCharacter(value.ObjectData);
			}
		}
		
		private void SetFullPreviewLevel(CharacterUpgradeUIData uiData, int level)
		{
			View.SelectedCharacterLvlImage.sprite = uiData.Sprite;
			View.SelectedCharacterLvlNumber.text = level.ToString();
			View.SelectedCharacterLvlNumber.color = uiData.TextColor;
		}

		private void SetPreviewLevelStatus(bool status)
		{
			if (_inWindowIAPVersion && status == false)
			{
				View.SelectedCharacterLvlImage.sprite = View.LockSprite;
				View.SelectedCharacterLvlImage.enabled = true;
				View.SelectedCharacterLvlNumber.enabled = false;
				View.SelectedCharacterLvlTXT.enabled = false;
			}
			else
			{
				if (View.SelectedCharacterLvlImage.enabled != status)
					View.SelectedCharacterLvlImage.enabled = status;
			
				if (View.SelectedCharacterLvlNumber.enabled != status)
					View.SelectedCharacterLvlNumber.enabled = status;
				
				View.SelectedCharacterLvlTXT.enabled = status;
			}
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
			SelectedCharacterPresenter?.Dispose();
			SelectedCharacter?.Dispose();
		}
	}
}