using System;
using Core.Entity.Head;
using Core.HealthSystem;
using Core.Services;
using LitMotion;
using LitMotion.Extensions;
using SharedUtils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using VContainer.Unity;
using VContainerUi.Abstraction;

namespace Ui.Sandbox.EnemyUi
{
	public class BossHealthUiView : UiView
	{
		public TextMeshProUGUI CountDownText;
		public Image HealthBar;
		public Image HealthBarColored;
		public Color InactiveColor;
		public Color ActiveColor;
	}

	public class BossHealthUiController : UiController<BossHealthUiView>, IStartable
	{
		private readonly IBossSpawnService _bossSpawnService;
		private MotionHandle _healthBarHandle;
		private IDisposable _currentSub;

		public BossHealthUiController(IBossSpawnService bossSpawnService)
		{
			_bossSpawnService = bossSpawnService;
		}
		
		public void Start()
		{
			_bossSpawnService.CurrentSpanTimer.Subscribe(OnTimerTick).AddTo(View);
			_bossSpawnService.CurrentBoss.Subscribe(BossSpawned).AddTo(View);
		}
		
		private void BossSpawned(HeadContext context)
		{
			_currentSub?.Dispose();

			if (context)
			{
				View.HealthBar.fillAmount = 1f;
				View.CountDownText.gameObject.SetActive(false);
				SetActive();
				_currentSub = context.Health.OnDamage.SubscribeWithState(context.Health, OnHealthChanged);
			}
			else
			{
				View.HealthBar.fillAmount = 1f;
				View.CountDownText.gameObject.SetActive(true);
			}

			if (!_bossSpawnService.HasBossInRoom.Value && context == null)
				SetInactive();

		}
		
		private void OnHealthChanged(DamageArgs obj, ILifeEntityHealth lifeEntityHealth)
		{
			_healthBarHandle.IsActiveCancel();
			
			_healthBarHandle = LMotion
				.Create(View.HealthBar.fillAmount, lifeEntityHealth.PercentHealth, 0.25f)
				.BindToFillAmount(View.HealthBar);
		}

		private void OnTimerTick(float currentTime)
		{
			View.CountDownText.text = "BOSS FIGHT - " + currentTime.ToSec().ToString(@"mm\:ss");
			SetActive();
		}

		private void SetInactive()
		{
			View.HealthBarColored.color = View.InactiveColor;
			View.CountDownText.text = "COMPLETE ALL TASKS";
		}
		
		private void SetActive()
		{
			if(View.HealthBarColored.color == View.ActiveColor)
				return;
			
			View.HealthBarColored.color = View.ActiveColor;
			
		}
	}
}