using System;
using System.Collections.Generic;
using System.Threading;
using Analytic;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using Core.Factory;
using Core.HealthSystem;
using Core.Pause;
using Core.Quests;
using Core.Quests.Messages;
using Core.Quests.Save;
using Core.Services.BossLoop;
using Cysharp.Threading.Tasks;
using GameSettings;
using LitMotion;
using LitMotion.Extensions;
using NodeCanvas.Framework;
using SharedUtils;
using SharedUtils.PlayerPrefs;
using SharedUtils.PlayerPrefs.Impl;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer.Unity;

namespace Core.Services
{
	public interface IBossSpawnService
	{
		IReactiveProperty<float> CurrentSpanTimer { get; }
		IReactiveProperty<HeadContext> CurrentBoss { get; }
		public IReactiveProperty<bool> HasBossInRoom { get; }
		public void SetTimerTime(int time);
	}
	
	public class BossSpawnService : IPostStartable, IDisposable, IBossSpawnService, IPostInitializable
	{
		private readonly IAdapterStrategyFactory _adapterStrategyFactory;
		private readonly IPauseService _pauseService;
		private readonly IBossLoopData _bossLoopData;
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly InstallerCancellationToken _installerCancellationToken;
		private readonly IAudioService _audioService;
		private readonly IMapService _mapService;
		private readonly ISettingsService _settingsService;
		private readonly IAnalyticService _analyticService;
		private readonly ISceneLoaderService _sceneLoader;
		private readonly InMemoryPlayerPrefsManager _inMemoryPrefs;
		private readonly IQuestMessageSender _questMessageSender;
		private readonly IQuestSaveAdapter _questSaveAdapter;

		private readonly FloatReactiveProperty _currentDelayTime = new FloatReactiveProperty();
		private readonly ReactiveProperty<HeadContext> _currentBoss = new ReactiveProperty<HeadContext>();
		private readonly CompositeMotionHandle _compositeMotion = new CompositeMotionHandle();
		private readonly List<BossSpawnData> _bossLoop = new List<BossSpawnData>();
		private BoolReactiveProperty _hasBossInRoom;
		private int _currentBossIndex;

		private AiHeadAdapter _boss;
		private IDisposable _disposable;
		private IDisposable _timerObservable;
		private IDisposable _settingsDisposable;
		private float _bossSpawnTime;
		private RuntimeBossData _cache;
		
		private const string IN_BOSS_LOOP = "InBossLoop";
		private const string LAST_KILLED_BOSS = "LastKilledBoss_new";
		private const string LIFE_SCOPE_PREFS_KEY = "life_scope_boss_data";

		public IReactiveProperty<float> CurrentSpanTimer => _currentDelayTime;
		public IReactiveProperty<HeadContext> CurrentBoss => _currentBoss;
		public IReactiveProperty<bool> HasBossInRoom => _hasBossInRoom;
		
		public BossSpawnService(
			IAdapterStrategyFactory adapterStrategyFactory,
			IPauseService pauseService,
			IBossLoopData bossLoopData,
			IPlayerPrefsManager playerPrefsManager, 
			InstallerCancellationToken installerCancellationToken,
			IAudioService audioService,
			IMapService mapService,
			ISettingsService settingsService,
			IAnalyticService analyticService,
			ISceneLoaderService sceneLoader,
			InMemoryPlayerPrefsManager inMemoryPrefs,
			IQuestMessageSender questMessageSender,
			IQuestSaveAdapter questSaveAdapter
			)
		{
			_hasBossInRoom = new();
			
			_adapterStrategyFactory = adapterStrategyFactory;
			_pauseService = pauseService;
			_bossLoopData = bossLoopData;
			_playerPrefsManager = playerPrefsManager;
			_installerCancellationToken = installerCancellationToken;
			_audioService = audioService;
			_mapService = mapService;
			_settingsService = settingsService;
			_analyticService = analyticService;
			_sceneLoader = sceneLoader;
			_inMemoryPrefs = inMemoryPrefs;
			_questMessageSender = questMessageSender;
			_questSaveAdapter = questSaveAdapter;
		}
		
		public void PostStart()
		{
			PrepareBossLoop();
			_currentBossIndex = _playerPrefsManager.GetValue(LAST_KILLED_BOSS,0);
			StartBossTimer();
		}
		
		public void PostInitialize()
		{
			_disposable = _sceneLoader.BeforeSceneChange.Subscribe(_ => OnBeforeSceneChange());
		}

		private void OnBeforeSceneChange()
		{
			_inMemoryPrefs.SetValue(LIFE_SCOPE_PREFS_KEY, GetBossData());
		}

		private RuntimeBossData GetBossData()
		{
			var haveBoss = _currentBoss.Value != null;
			return !haveBoss ? null : new RuntimeBossData(true, _currentBoss.Value.Health.CurrentHealth);
		}
		
		private void PrepareBossLoop()
		{
			_settingsDisposable = _settingsService.OnValueByKeyChanged.Where(s => s == SettingsConsts.BOSS_SEQUENCE).Subscribe(OnSettingsChanged);
			var bossSequence = _settingsService.GetValue<string>(SettingsConsts.BOSS_SEQUENCE, GameSetting.ParameterType.String);
			SetupBossLoop(bossSequence);
		}
		
		private void SetupBossLoop(string bossSequence)
		{
			_bossLoop.Clear();
			var settingNames = bossSequence.Split(',');
			foreach (var bossSpawnData in _bossLoopData.SpawnData)
			{
				var bossIndex = Array.IndexOf(settingNames, bossSpawnData.BossId);
				
				if (bossSpawnData.IsNew && 
				    _playerPrefsManager.GetValue<bool>(IN_BOSS_LOOP, false) && 
				    !_playerPrefsManager.GetValue<bool>(BossKilledPrefKey(bossSpawnData.BossId), false))
				{
					_bossLoop.Insert(0, bossSpawnData);
					continue;
				}
				
				if (bossIndex >= 0 && _bossLoop.Count > bossIndex)
				{
					_bossLoop.Insert(bossIndex, bossSpawnData);
				}
				else
				{
					_bossLoop.Add(bossSpawnData);
				}
			}
		}

		private void OnSettingsChanged(string key)
		{
			var boss = _settingsService.GetValue<string>(SettingsConsts.BOSS_SEQUENCE, GameSetting.ParameterType.String);
			SetupBossLoop(boss);
		}

		[Button]
		private void StartBossTimer()
		{
			var spawnData = GetNextBoss();
			if(spawnData.DependedOnQuest && !_questSaveAdapter.IsCompleteTree(spawnData.TreeId))
				return;
			
			var bossRoomProvider = _mapService.MapObject.BossRoomProvider;
			_boss = _adapterStrategyFactory.CreateAiAdapter<AiHeadAdapter>(spawnData.BossId, bossRoomProvider.BossSpawnPoint.position, bossRoomProvider.BossSpawnPoint.rotation);
			_boss.Entity.Health.OnDeath.SubscribeWithState2(_boss, spawnData, OnBossDied).AddTo(_boss);
			if(_boss.Entity.Health is Health health)
				health.SetImmortal(true);
			
			_hasBossInRoom.Value = true;
			_boss.GetComponent<Blackboard>().SetVariableValue("InBossRoom", true);
			_cache = _inMemoryPrefs.GetValue<RuntimeBossData>(LIFE_SCOPE_PREFS_KEY);
			_inMemoryPrefs.DeleteKey(LIFE_SCOPE_PREFS_KEY);
			_currentDelayTime.Value = _cache is { IsSpawned: true } ? 0 : spawnData.BossDelaySpawn;
			StartTimer();
		}

		private BossSpawnData GetNextBoss()
		{
			if (_currentBossIndex >= _bossLoop.Count)
			{
				_playerPrefsManager.SetValue(IN_BOSS_LOOP, true);
				_currentBossIndex = 0;
			}
			
			return _bossLoop[_currentBossIndex++];
		}

		private void StartTimer()
		{
			if (_currentDelayTime.Value <= 0)
			{
				SpawnBoss(_boss);
				return;
			}
			
			_timerObservable?.Dispose();
			_timerObservable = Observable
				.Interval(1f.ToSec())
				.TakeWhile(_ => !_pauseService.IsPaused)
				.Repeat()
				.SubscribeWithState(_boss, OnTick);
		}

		private void OnTick(long obj, AiHeadAdapter aiHeadAdapter)
		{
			if (_currentDelayTime.Value > 0)
			{
				_currentDelayTime.Value -= 1f;
			}
			else
			{
				SpawnBoss(aiHeadAdapter);	
			}
		}
		
		public void SetTimerTime(int time)
		{
			_currentDelayTime.Value = time;
		}
		
		[Button]
		private void SpawnBoss(AiHeadAdapter aiHeadAdapter)
		{
			_bossSpawnTime = Time.realtimeSinceStartup;
			_timerObservable?.Dispose();

			_hasBossInRoom.Value = false;
			aiHeadAdapter.OnBossRoomOpened();
			_currentBoss.Value = aiHeadAdapter.CurrentContext;
			_compositeMotion?.Clear();
			
			if(_boss.Entity.Health is Health health)
				health.SetImmortal(false);
			if(_cache != null)
				_currentBoss.Value.Health.SetCurrentHealth(_cache.Health);

			_cache = null;
			var bossRoomProvider = _mapService.MapObject.BossRoomProvider;
			var sound = bossRoomProvider.DoorOpening;

			
			if (_audioService.TryPlayQueueSound(sound, "DoorOpening", 0.1f, out var player))
			{
				player
					.SetVolume(1f)
					.SetSpatialBlend(1)
					.SetPosition(bossRoomProvider.BossLeftDoor.position);
			}
			
			LMotion
				.Create(bossRoomProvider.BossLeftDoor.localPosition.x, bossRoomProvider.LOpenOffsetX, sound.length)
				.BindToLocalPositionX(bossRoomProvider.BossLeftDoor)
				.AddTo(_compositeMotion);
			
			LMotion
				.Create(bossRoomProvider.BossRightDoor.localPosition.x, bossRoomProvider.ROpenOffsetX, sound.length)
				.BindToLocalPositionX(bossRoomProvider.BossRightDoor)
				.AddTo(_compositeMotion);
		}
		
		private void OnBossDied(DiedArgs obj, AiHeadAdapter aiHeadAdapter, BossSpawnData bossSpawnData)
		{
		
			_questMessageSender.SendBossDeathMessage(bossSpawnData.BossId);
			if (!string.IsNullOrEmpty(bossSpawnData.BossId))
			{
				_analyticService.SendEvent($"BossEvents:KillTime:{bossSpawnData.BossId}", Time.realtimeSinceStartup - _bossSpawnTime);
				_playerPrefsManager.SetValue<bool>(BossKilledPrefKey(bossSpawnData.BossId), true);
			}
			
			_compositeMotion.Clear();
			var bossRoomProvider = _mapService.MapObject.BossRoomProvider;
			
			LMotion
				.Create(bossRoomProvider.BossLeftDoor.localPosition.x, bossRoomProvider.LCloseOffsetX, 1f)
				.BindToLocalPositionX(bossRoomProvider.BossLeftDoor).AddTo(_compositeMotion);
			
			LMotion
				.Create(bossRoomProvider.BossRightDoor.localPosition.x, bossRoomProvider.RCloseOffsetX, 1f)
				.BindToLocalPositionX(bossRoomProvider.BossRightDoor).AddTo(_compositeMotion);
			
			DeleteBossTask(_installerCancellationToken.Token).Forget();
		}

		private async UniTaskVoid DeleteBossTask(CancellationToken token)
		{
			_playerPrefsManager.SetValue<int>(LAST_KILLED_BOSS, _currentBossIndex);
			_mapService.MapObject.BossRoomProvider.DeathBounds.DeathAsync();

			await UniTask.Delay(6f.ToSec(), cancellationToken: token);
			
			_currentBoss.Value = null;
			_boss = null;
			StartBossTimer();
		}

		public void ManualSkipBoss()
		{
			if (_boss != null)
			{
				_boss.DestroyBoss();
				_currentBoss.Value = null;
				_boss = null;
			}
			StartBossTimer();
		}

		public void Dispose()
		{
			_compositeMotion?.Cancel();
			_timerObservable?.Dispose();
			_settingsDisposable?.Dispose();
			_currentDelayTime?.Dispose();
			_currentBoss?.Dispose();
			_disposable?.Dispose();
			_hasBossInRoom?.Dispose();
		}
		
		private static string BossKilledPrefKey(string bossId) => bossId + "_KILLED";

		[Serializable]
		private class RuntimeBossData
		{
			public bool IsSpawned;
			public float Health;

			public RuntimeBossData(bool isSpawned, float health)
			{
				IsSpawned = isSpawned;
				Health = health;
			}
		}
	}
}