using System;
using Analytic;
using Core.Entity.EntityUpgrade;
using Core.Entity.EntityUpgrade.Experience;
using Core.Entity.EntityUpgrade.Upgrades.Abs;
using Core.Entity.Head;
using Core.InputSystem;
using Core.Quests.Messages;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using MessagePipe;
using SharedUtils;
using SharedUtils.PlayerPrefs;
using TMPro;
using UniRx;
using UnityEngine;
using Utils;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Core.Services.Experience
{
	public interface IExperienceService
	{
		IReactiveProperty<float> CurrentExperience { get; }
		IReactiveProperty<int> CurrentLevel { get; }
		IObservable<ExperienceLvlEvent> OnExperienceChanged { get; }
		IReactiveProperty<int> AvailablePoints { get; }
		bool UpgradeIsAvailableFor(CharacterUpgrades characterUpgrades);
		float GetExperienceToLevel(int level);
		bool IsMaxLevel();
		void SpendPointToUpgrade(EntityUpgradeParameters entityUpgrade, CharacterUpgrades upgrade);
		void AddPoints(CharacterUpgrades upgrade, int amount);
	}
	
	[Serializable]
	public class ExperienceData
	{
		public TMP_Text ExperienceText;
	}
	
	public class ExperienceService : IStartable, IDisposable, IExperienceService
	{
		private readonly ISubscriber<PlayerContextChangedMessage> _playerContextSub;
		private readonly IPlayerSpawnService _playerSpawnService;
		private readonly ISubscriber<ExperienceMessage> _experienceMessage;
		private readonly IAnalyticService _analyticService;
		private readonly ExperienceData _experienceData;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly IQuestMessageSender _questMessageSender;

		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		
		private readonly FloatReactiveProperty _currentExperience = new FloatReactiveProperty();
		private readonly IntReactiveProperty _currentLevel = new IntReactiveProperty();
		private readonly IntReactiveProperty _availableUpgradePoints = new IntReactiveProperty();
		private readonly ReactiveCommand<ExperienceLvlEvent> _experienceCommand = new ReactiveCommand<ExperienceLvlEvent>();
		
		public IReactiveProperty<float> CurrentExperience => _currentExperience;
		public IReactiveProperty<int> CurrentLevel => _currentLevel;
		public IReactiveProperty<int> AvailablePoints => _availableUpgradePoints;
		public IObservable<ExperienceLvlEvent> OnExperienceChanged => _experienceCommand;

		public float GetExperienceToLevel(int level)
			=> 1000;
		
		private ExperienceProvider _experienceProvider;
		public ExperienceService(
			IPlayerSpawnService playerSpawnService,
			ISubscriber<PlayerContextChangedMessage> playerContextSub,
			ISubscriber<ExperienceMessage> experienceMessage,
			IAnalyticService analyticService,
			ExperienceData experienceData,
			IPlayerPrefsManager playerPrefsManager,
			IQuestMessageSender questMessageSender
			)
		{
			_playerSpawnService = playerSpawnService;
			_playerContextSub = playerContextSub;
			_experienceMessage = experienceMessage;
			_analyticService = analyticService;
			_experienceData = experienceData;
			_playerPrefsManager = playerPrefsManager;
			_questMessageSender = questMessageSender;
		}
		
		public void Start()
		{
			_experienceProvider = _playerSpawnService.PlayerCharacterAdapter.ExperienceProvider;
			
			_playerContextSub.Subscribe(OnPlayerContextChanged).AddTo(_compositeDisposable);
			_experienceMessage.Subscribe(OnExperienceEarned).AddTo(_compositeDisposable);
		}
		
		public bool UpgradeIsAvailableFor(CharacterUpgrades characterUpgrades)
		{
			return characterUpgrades.GetUpgradePoints(_playerPrefsManager) > 0;
		}
		
		public void SpendPointToUpgrade(EntityUpgradeParameters entityUpgrade, CharacterUpgrades upgrade)
		{
			var points = upgrade.GetUpgradePoints(_playerPrefsManager);
			points -= 1;
			var currentUpLevel = upgrade.GetEntityUpgradeLevel(entityUpgrade, _playerPrefsManager);
				
			upgrade.SetUpgradePoints(_playerPrefsManager, points);
			upgrade.SetEntityUpgradeLevel(entityUpgrade, currentUpLevel + 1, _playerPrefsManager);
			_experienceProvider.OnEntityUpgraded(upgrade);
			_availableUpgradePoints.Value = _experienceProvider.AvailablePoints;
			_questMessageSender.SendUpgradeSkillMessage(null);
		}
		
		public void AddPoints(CharacterUpgrades upgrade, int amount)
		{
			var points = upgrade.GetUpgradePoints(_playerPrefsManager);
			points += amount;
			upgrade.SetUpgradePoints(_playerPrefsManager, points);
			_availableUpgradePoints.Value = _experienceProvider.AvailablePoints;
		}

		private void OnPlayerContextChanged(PlayerContextChangedMessage msg)
		{
			if (!_experienceProvider.IsAvailable)
			{
				_experienceCommand.Execute(new ExperienceLvlEvent(false));
				return;
			}
			if (msg.Created)
			{
				var lastLevel = _experienceProvider.CurrentLevel;
				var lastExp = _experienceProvider.CurrentExperience;
				lastExp = Mathf.Clamp(lastExp, 0, GetExperienceToLevel(lastLevel) - 1);
				_availableUpgradePoints.Value = _experienceProvider.AvailablePoints;
				_currentLevel.Value = lastLevel;
				_currentExperience.Value = lastExp;
				_experienceCommand.Execute(new ExperienceLvlEvent(_currentLevel.Value, _currentExperience.Value, GetExperienceToLevel(lastLevel), true));
			}
		}

		private void OnExpChanged(float newExp, bool skipAnim = false)
		{
			var expToLevelUp = GetExperienceToLevel(_currentLevel.Value);
			
			while (newExp >= expToLevelUp && _currentLevel.Value < _experienceProvider.MaxLevel)
			{
				_experienceProvider.AddPoint(1);
				_currentLevel.Value++;
				_analyticService.SendEvent($"Experience:LevelUp:{_experienceProvider.UniqueId}:LVL_{_currentLevel.Value}");
				newExp -= expToLevelUp;

				if (skipAnim)
					continue;
				_experienceCommand.Execute(new ExperienceLvlEvent(_currentLevel.Value, expToLevelUp, expToLevelUp, false));
			}

			if (_currentLevel.Value >= _experienceProvider.MaxLevel)
				newExp = 0;
			
			_availableUpgradePoints.Value = _experienceProvider.AvailablePoints;
			_currentExperience.Value = newExp;
			_experienceCommand.Execute(new ExperienceLvlEvent(_currentLevel.Value, _currentExperience.Value, expToLevelUp, skipAnim));
			_experienceProvider.CurrentExperience = _currentExperience.Value;
			_experienceProvider.CurrentLevel = _currentLevel.Value;
		}

		public void AddExperience(ExperienceMessage experience)
		{
			CreateEpxText(experience).Forget();
			if (!_experienceProvider.IsAvailable || _currentLevel.Value >= _experienceProvider.MaxLevel)
				return;
			
			_currentExperience.Value += experience.ExperienceCount;
			OnExpChanged(_currentExperience.Value, experience.SkipAnim);
		}
		
		private void OnExperienceEarned(ExperienceMessage earned)
		{
			if (earned.IsForce)
			{
				AddExperience(earned);
				return;
			}
			
			if (earned.SourceFrom is HeadContext)
			{
				AddExperience(earned);
				return;
			}
			
			if (ReferenceEquals(earned.TargetToAdd, _playerSpawnService.PlayerCharacterAdapter.CurrentContext))
			{
				AddExperience(earned);
				return;
			}
		}

		public bool IsMaxLevel()
		{
			return _playerSpawnService.PlayerCharacterAdapter.ExperienceProvider.MaxLevel
			       <= _playerSpawnService.PlayerCharacterAdapter.ExperienceProvider.CurrentLevel;
		}


		private async UniTaskVoid CreateEpxText(ExperienceMessage earnedPosition)
		{
			if (earnedPosition.Position != default)
			{
				var spawnPos = earnedPosition.Position;
				var text = Object.Instantiate(_experienceData.ExperienceText, spawnPos, Quaternion.identity);
				text.text = $"+{earnedPosition.ExperienceCount}XP";
				text.transform.localScale = earnedPosition.Scale;
				var delta = _playerSpawnService.PlayerCharacterAdapter.transform.position - spawnPos;
				delta.y = 0;
				text.transform.forward = -delta.normalized;
				
				var textColorAlpha = text.color;
				textColorAlpha.a = 0;

				var token = text.destroyCancellationToken;
				
#pragma warning disable CS4014
				LMotion
					.Create(text.color, textColorAlpha, 1.45f)
					.BindToColor(text).ToUniTask(token);

#pragma warning disable CS4014
				LMotion
					.Create(spawnPos, spawnPos + Vector3.up * 1.5f, 1.5f)
					.BindToPosition(text.transform).ToUniTask(token);

				await UniTask.Delay(1.6f.ToSec(), cancellationToken: token);
				
				
				if (text)
				{
					Object.Destroy(text.gameObject);
				}
			}
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
			_currentExperience?.Dispose();
			_availableUpgradePoints?.Dispose();
			_currentLevel?.Dispose();
		}
	}
	
	public readonly struct ExperienceLvlEvent
	{
		public readonly float ExpToLevelUp;
		public readonly int NewLevel;
		public readonly float Experience;

		public readonly bool Force;
		public readonly bool IsAvailable;

		public override string ToString()
		{
			return $"Experience to level up: {ExpToLevelUp} \nNew level: {NewLevel}\nExperience: {Experience}";
		}

		public ExperienceLvlEvent(int newLevel, float experience, float expToLevelUp, bool force)
		{
			ExpToLevelUp = expToLevelUp;
			Force = force;
			NewLevel = newLevel;
			Experience = experience;
			IsAvailable = true;
		}
		
		public ExperienceLvlEvent(bool available)
		{
			ExpToLevelUp = 0;
			NewLevel = 0;
			Experience = 0;
			Force = false;
			IsAvailable = available;
		}
	}
}