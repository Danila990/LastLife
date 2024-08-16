using System;
using System.Collections.Generic;
using Core.Boosts.EndHandlers;
using Core.Boosts.Impl;
using Core.InputSystem;
using Core.Services;
using MessagePipe;
using SharedUtils.PlayerPrefs.Impl;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Boosts.Inventory
{
	public class BoostInventoryRuntimeSaver : IInitializable, IDisposable
	{
		private const string PREFS_KEY = "applied_boosts";

		private readonly IPlayerSpawnService _spawnService;
		private readonly InMemoryPlayerPrefsManager _inMemoryPrefs;
		private readonly ISceneLoaderService _sceneLoaderService;
		private readonly ISubscriber<PlayerContextChangedMessage> _subscriber;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		
		private IDisposable _characterSpawnDisposable;

		public BoostInventoryRuntimeSaver(
			IPlayerSpawnService spawnService,
			InMemoryPlayerPrefsManager inMemoryPrefs,
			ISceneLoaderService sceneLoaderService,
			ISubscriber<PlayerContextChangedMessage> subscriber
			)
		{
			_spawnService = spawnService;
			_inMemoryPrefs = inMemoryPrefs;
			_sceneLoaderService = sceneLoaderService;
			_subscriber = subscriber;
		}

		public void Initialize()
		{
			_sceneLoaderService.BeforeSceneChange.Subscribe(_ => OnBeforeSceneChange()).AddTo(_compositeDisposable);
			_characterSpawnDisposable = _subscriber.Subscribe(msg => OnContextChanged(msg.Created));
		}

		private void OnBeforeSceneChange()
		{
			var activeBoosts = _spawnService.PlayerCharacterAdapter.BoostProvider.ActiveBoosts;
			if (activeBoosts.Count > 0)
			{
				var boostsData = new List<AppliedBoostData>();
				foreach (var boost in activeBoosts.Values)
				{
					var handler = boost.GetEndHandler<TemporaryEndBoostHandler>();
					if(handler == null)
						continue;
					
					boostsData.Add(new AppliedBoostData(boost.Boost.BoostArgs, handler.Timer.RemainingTime.Value.TotalSeconds));
				}
				_inMemoryPrefs.SetValue(PREFS_KEY, boostsData);
			}
		}

		private void OnContextChanged(bool isCreated)
		{
			if (isCreated)
			{
				TryRestoreAppliedBoosts();
				_characterSpawnDisposable?.Dispose();
			}
		}
		
		private void TryRestoreAppliedBoosts()
		{
			if(_inMemoryPrefs.HasKey(PREFS_KEY))
			{
				var appliedBoosts = _inMemoryPrefs.GetValue<List<AppliedBoostData>>(PREFS_KEY);

				foreach (var savedBoost in appliedBoosts)
				{
					Debug.Log($"Applied {savedBoost.BoostArgs} {savedBoost.Duration}");
					var boostArgs = savedBoost.BoostArgs;
					boostArgs.Duration = savedBoost.Duration;
					
					_spawnService.PlayerCharacterAdapter.BoostProvider.ApplyBoostForce(in boostArgs);
				}
				
				_inMemoryPrefs.DeleteKey(PREFS_KEY);
			}
		}

		[Serializable]
		private readonly struct AppliedBoostData
		{
			public readonly BoostArgs BoostArgs;
			public readonly float Duration;

			public AppliedBoostData(in BoostArgs boostArgs, double duration)
			{
				Duration = (float)duration;
				BoostArgs = boostArgs;
			}
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
			_characterSpawnDisposable?.Dispose();
		}
	}
}
