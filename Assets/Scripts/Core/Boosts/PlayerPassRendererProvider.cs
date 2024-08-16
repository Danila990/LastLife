using System;
using System.Threading;
using Core.Boosts.Impl;
using Core.FullScreenRenderer;
using Core.InputSystem;
using Core.PlayerDeath;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using UnityEngine;
using Utils;
using VContainer.Unity;

namespace Core.Boosts
{
	public class PlayerPassRendererProvider : IPostInitializable, IDisposable
	{
		private const string LOW_HP = "LowHp";
		
		private readonly IPassRendererProvider _passRendererProvider;
		private readonly IPlayerSpawnService _spawnService;
		private readonly IFullScreenMaterialData _fullScreenMaterialData;
		private readonly ISubscriber<PlayerContextDeathMessage> _deathSub;
		private readonly ISubscriber<PlayerContextDamageMessage> _damageSub;
		private readonly ISubscriber<PlayerContextChangedMessage> _changedContextSub;
		private readonly CancellationToken _token;

		private FullScreenMaterial _currentFullScreenMaterial;
		private bool isDead;

		public PlayerPassRendererProvider(
			IPassRendererProvider passRendererProvider,
			IPlayerSpawnService spawnService,
			IFullScreenMaterialData fullScreenMaterialData,
			InstallerCancellationToken installerCancellationToken,
			ISubscriber<PlayerContextDeathMessage> deathSub,
			ISubscriber<PlayerContextDamageMessage> damageSub,
			ISubscriber<PlayerContextChangedMessage> changedContextSub
			)
		{
			_passRendererProvider = passRendererProvider;
			_spawnService = spawnService;
			_fullScreenMaterialData = fullScreenMaterialData;
			_deathSub = deathSub;
			_damageSub = damageSub;
			_changedContextSub = changedContextSub;
			_token = installerCancellationToken.Token;
		}

		public void PostInitialize()
		{
			_spawnService.PlayerCharacterAdapter.BoostProvider.OnBoostApplied.Subscribe(OnBoostApplied).AddTo(_token);
			_deathSub.Subscribe(_ => OnDeath()).AddTo(_token);
			_damageSub.Subscribe(OnDamage).AddTo(_token);
			_changedContextSub.Subscribe(msg => OnContextChanged(msg.Created)).AddTo(_token);
		}

		private void OnContextChanged(bool isCreated)
		{
			if (isCreated && isDead)
			{
				PlayFadeOut(LOW_HP);
				isDead = false;
			}
		}
		
		private void OnDamage(PlayerContextDamageMessage msg)
		{
			if(msg.DamageArgs.Damage <= 0)
				return;
				
			PlayFade(LOW_HP);
		}

		private void OnDeath()
		{
			PlayFadeIn(LOW_HP, false);
			isDead = true;
		}
		
		private void OnBoostApplied(AppliedBoostArgs boostArgs)
		{
			if(!boostArgs.Suppressed)
				PlayFade(boostArgs.BoostArgs.Type);
		}

		private void PlayFade(string key, bool resetMaterialOnEnd = true)
		{
			if (TrySetMaterialToPassRenderer(key))
				_currentFullScreenMaterial.Animation.Fade(resetMaterialOnEnd ? _passRendererProvider.ResetMaterial : null);

		}
		
		private void PlayFadeIn(string key, bool resetMaterialOnEnd = true)
		{
			if (TrySetMaterialToPassRenderer(key))
				_currentFullScreenMaterial.Animation.FadeIn(resetMaterialOnEnd ? _passRendererProvider.ResetMaterial : null);
		}
		
		private void PlayFadeOut(string key, bool resetMaterialOnEnd = true)
		{
			if (TrySetMaterialToPassRenderer(key))
				_currentFullScreenMaterial.Animation.FadeOut(resetMaterialOnEnd ? _passRendererProvider.ResetMaterial : null);
		}


		private bool TrySetMaterialToPassRenderer(string key)
		{
			_currentFullScreenMaterial?.Dispose();	
			
			if (_fullScreenMaterialData.TryGetMaterialData(key, out var data))
			{
				var matInstance = UnityEngine.Object.Instantiate(data.MaterialPrefab);
				_currentFullScreenMaterial = data;
				
				_currentFullScreenMaterial.Animation.SetCancellationToken(_token);
				_currentFullScreenMaterial.Animation.SetMaterialInstance(matInstance);
				_passRendererProvider.SetMaterial(matInstance);
			}

			return data != null;
		}
		
		
		public void Dispose()
		{
			_currentFullScreenMaterial?.Dispose();
		}
		
	}
}
