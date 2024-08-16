using System;
using System.Collections.Generic;
using Adv.Services.Interfaces;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.EntityUpgrade;
using Core.Entity.EntityUpgrade.Upgrades.Abs;
using Core.Services;
using Core.Services.Experience;
using Db.ObjectData;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils.PlayerPrefs;
using Ticket;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui.Sandbox.CharacterMenu
{
	[Serializable]
	public class SkillsScreenData
	{
		public UpgradeUiItem UpgradeUiItemPrefab;
		public Transform UpgradeUiItemParent;
		public TextMeshProUGUI PointToUse;
		public Image FillUpgradeImage;
		public Image PointSubstrate;
		public float AdditionalCharWidth;
		public AudioClip UpgradeSound;

		public GameObject Buttons;
		public Button AdvButton;
		public Button TicketButton;
	}
	
	public class SkillsScreenController : IDisposable
	{
		private readonly SkillsScreenData _skillsScreenData;
		private readonly ICharacterUpgradesData _upgradesData;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly IExperienceService _experienceService;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly IAdvService _advService;
		private readonly ITicketService _ticketService;
		private readonly UpgradeUiItemPool _upPool;
		private readonly List<SkillsUpgradeItemData> _rentedItems = new List<SkillsUpgradeItemData>();
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private CharacterUpgrades _currentCharacterUpgrades;
		private float _sumUpgrade;
		private float _maxSumUpgrade;
		private float _defaultSubstrateWidth;
		private AudioPlayer _audioPlayer;
		private readonly ReactiveProperty<CharacterObjectData> _selectedCharacter;
		private bool _featureStatus;


		public SkillsScreenController(
			SkillsScreenData skillsScreenData,
			ICharacterUpgradesData upgradesData,
			IPlayerPrefsManager playerPrefsManager,
			IExperienceService experienceService, 
			ReactiveProperty<CharacterObjectData> selectedCharacter,
			IItemUnlockService itemUnlockService,
			IAdvService advService,
			ITicketService ticketService
			)
		{
			_selectedCharacter = selectedCharacter;
			_skillsScreenData = skillsScreenData;
			_upgradesData = upgradesData;
			_playerPrefsManager = playerPrefsManager;
			_experienceService = experienceService;
			_itemUnlockService = itemUnlockService;
			_advService = advService;
			_ticketService = ticketService;
			_upPool = new UpgradeUiItemPool(skillsScreenData.UpgradeUiItemPrefab, skillsScreenData.UpgradeUiItemParent);
			_upPool.Prewarm(3);
			selectedCharacter.Subscribe(OnSelectedCharacterChanged).AddTo(_compositeDisposable);
			_experienceService.AvailablePoints.SkipLatestValueOnSubscribe().Subscribe(OnAvailablePointsChanged).AddTo(_compositeDisposable);
		}

		public void SetIsActiveFeatures(bool status)
		{
			_skillsScreenData.TicketButton.onClick.RemoveAllListeners();
			_skillsScreenData.AdvButton.onClick.RemoveAllListeners();
			_featureStatus = status;
			if (status)
			{
				_skillsScreenData.Buttons.SetActive(true);
				_skillsScreenData.TicketButton.onClick.AddListener(OnClickTicket);
				_skillsScreenData.AdvButton.onClick.AddListener(OnClickAdv);
			}
			else
			{
				_skillsScreenData.Buttons.SetActive(false);
			}
		}
		
		private void OnClickAdv()
		{
			if (_selectedCharacter.Value != null)
			{
				_advService.RewardRequest(AddSkillPoint, $"AddSkillPoint:{_selectedCharacter.Value}");
			}
		}

		private void OnClickTicket()
		{
			if (_selectedCharacter.Value != null)
			{
				_ticketService.TryUseTicket(AddSkillPoint, $"AddSkillPoint:{_selectedCharacter.Value}");
			}
		}

		public void AddSkillPoint()
		{
			if (!_currentCharacterUpgrades)
				return;
			
			_experienceService.AddPoints(_currentCharacterUpgrades, 1);
			RefreshUi();
		}

		private void OnSelectedCharacterChanged(CharacterObjectData obj)
		{
			Show(obj);
		}

		private void OnAvailablePointsChanged(int obj)
		{
			RefreshUi();
		}

		public void Show(CharacterObjectData characterObject)
		{
			if (_defaultSubstrateWidth == 0)
				_defaultSubstrateWidth = _skillsScreenData.PointSubstrate.rectTransform.rect.width;
			
			Clear();
			if (!_upgradesData.TryGetUpgrade(characterObject.PlayerId, out var upgrade))
				return;
			
			_currentCharacterUpgrades = upgrade;
			var points = upgrade.GetUpgradePoints(_playerPrefsManager);
			SetSkillPoints(points);
			_sumUpgrade = 0;
			_maxSumUpgrade = 0;
			var isUnlocked = _itemUnlockService.IsUnlocked(characterObject);
			var counter = 0;
			foreach (var entityUpgrade in upgrade.GetEntityUpgrades())
			{
				var newItem = _upPool.Rent();
				var skillUpgrade = new SkillsUpgradeItemData(newItem, entityUpgrade, upgrade, characterObject);
				var currentUpgradeLevel = upgrade.GetEntityUpgradeLevel(entityUpgrade, _playerPrefsManager);
				
				_sumUpgrade += currentUpgradeLevel;
				_maxSumUpgrade += entityUpgrade.MaxLevel;
				
				newItem.UpgradeName.text = entityUpgrade.Name;
				newItem.MarkDotsUpgraded(currentUpgradeLevel);
				newItem.transform.SetSiblingIndex(counter);
				newItem.SetIsAvailable(UpgradeIsAvailable(upgrade, entityUpgrade));

				if (isUnlocked)
				{
					var disposable = newItem.UpgradeButton.Button.OnClickAsObservable().SubscribeWithState2(entityUpgrade, upgrade, OnClickUpgrade);
					newItem.AttachDisposable(disposable);
				}
				else
				{
					newItem.UpgradeButton.Button.gameObject.SetActive(false);
				}
				
				
				_rentedItems.Add(skillUpgrade);
				counter++;
			}
			
			if (_featureStatus)
			{
				var enable = !Mathf.Approximately(_maxSumUpgrade / (_sumUpgrade + points), 1);
				_skillsScreenData.Buttons.SetActive(enable && isUnlocked);
			}
			_skillsScreenData.FillUpgradeImage.fillAmount = _sumUpgrade / _maxSumUpgrade;
		}

		public void RefreshUi()
		{
			if (_rentedItems.Count <= 0)
				return;
			
			_sumUpgrade = 0;
			_maxSumUpgrade = 0;
			
			foreach (var item in _rentedItems)
			{
				if (!_itemUnlockService.IsUnlocked(item.CharacterObject))
				{
					item.UiItem.UpgradeButton.gameObject.SetActive(false);
					continue;
				}
				
				item.UiItem.UpgradeButton.gameObject.SetActive(true);
				var currentUpgradeLevel = item.Upgrade.GetEntityUpgradeLevel(item.EntityUpgrade, _playerPrefsManager);
				item.UiItem.MarkDotsUpgraded(currentUpgradeLevel);
				item.UiItem.SetIsAvailable(UpgradeIsAvailable(item.Upgrade, item.EntityUpgrade));
				
				_sumUpgrade += currentUpgradeLevel;
				_maxSumUpgrade += item.EntityUpgrade.MaxLevel;
			}
			if (_featureStatus)
			{
				_skillsScreenData.Buttons.SetActive(!Mathf.Approximately(_maxSumUpgrade / (_sumUpgrade + _experienceService.AvailablePoints.Value), 1));
			}
			
			SetSkillPoints(_rentedItems[0].Upgrade.GetUpgradePoints(_playerPrefsManager));
			_skillsScreenData.FillUpgradeImage.fillAmount = _sumUpgrade / _maxSumUpgrade;
		}
		
		private void OnClickUpgrade(Unit arg1, EntityUpgradeParameters entityUpgrade, CharacterUpgrades upgrade)
		{
			if (UpgradeIsAvailable(upgrade, entityUpgrade))
			{
				_experienceService.SpendPointToUpgrade(entityUpgrade, upgrade);
				_audioPlayer.IsActiveStop();
				_audioPlayer = LucidAudio
					.PlaySE(_skillsScreenData.UpgradeSound)
					.SetVolume(0.3f);
			}
			RefreshUi();
		}
		
		private bool UpgradeIsAvailable(CharacterUpgrades upgrade, EntityUpgradeParameters entityUpgrade)
		{
			return _experienceService.UpgradeIsAvailableFor(upgrade) && 
			       !upgrade.EntityUpgradeIsMaxLevel(entityUpgrade, _playerPrefsManager);
		}

		private void Clear()
		{
			_currentCharacterUpgrades = null;
			_skillsScreenData.FillUpgradeImage.fillAmount = 0;
			foreach (var upgradeUiItem in _rentedItems)
				_upPool.Return(upgradeUiItem.UiItem);
			
			_rentedItems.Clear();
		}

		public void Dispose()
		{
			_upPool.Dispose();
			_compositeDisposable.Dispose();
			_audioPlayer.IsActiveStop();
		}

		private void SetSkillPoints(int value)
		{
			var strNumber = value.ToString(); 
			_skillsScreenData.PointToUse.text = "Points:  " + strNumber;
			_skillsScreenData.PointSubstrate.enabled = value != 0;
			
			if(!_skillsScreenData.PointSubstrate.enabled)
				return;
			
			var sizeDelta = _skillsScreenData.PointSubstrate.rectTransform.sizeDelta;
			sizeDelta.x = _defaultSubstrateWidth + strNumber.Length * _skillsScreenData.AdditionalCharWidth;
			_skillsScreenData.PointSubstrate.rectTransform.sizeDelta = sizeDelta;
		}
		
		private readonly struct SkillsUpgradeItemData
		{
			public UpgradeUiItem UiItem { get; }
			public EntityUpgradeParameters EntityUpgrade { get; }
			public CharacterUpgrades Upgrade { get; }
			public CharacterObjectData CharacterObject  { get;  }
			
			public SkillsUpgradeItemData(
				UpgradeUiItem uiItem,
				EntityUpgradeParameters entityUpgrade,
				CharacterUpgrades upgrade, 
				CharacterObjectData characterObject)
			{
				UiItem = uiItem;
				EntityUpgrade = entityUpgrade;
				Upgrade = upgrade;
				CharacterObject = characterObject;
			}
		}
	}
}