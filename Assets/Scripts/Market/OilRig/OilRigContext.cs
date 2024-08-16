using System;
using Analytic;
using AnnulusGames.LucidTools.Audio;
using Core.Carry;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.InteractionLogic.Animation;
using Core.Entity.InteractionLogic.Interactions;
using Core.Factory;
using Core.Quests.Tips;
using Cysharp.Threading.Tasks;
using Installer;
using LitMotion;
using MiniGames;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer;

namespace Market.OilRig
{
	public class OilRigContext : EntityContext, IInjectableTag
	{
		[SerializeField] private InteractionMiniGameStarter _interactionMiniGameStarter;
		[SerializeField] private StaticCharacterAnimator _staticCharacterAnimator;
		[SerializeField] private GenericInteraction _genericInteraction;
		[SerializeField] private Transform _spawnPoint;
		[SerializeField] private string _spawnedObjectId;
		[SerializeField] private ConveyorObject _conveyor;
		[SerializeField] private AudioClip _refiningSound;
		[Inject] private readonly IObjectFactory _objectFactory;
		[Inject] private readonly IMiniGameService _miniGameService;
		[Inject] private readonly IAnalyticService _analyticService;
		[Inject] private readonly IQuestTipService _questTipService;
		private Animator _animation;
		private AudioPlayer _audioPlayer;
		private IDisposable _disposable;
		private IDisposable _winDisposable;
		private bool _inMiniGame;
		private float _currentWinPercent;
		private float _lastWinPercent;
		private MotionHandle _handle;
		public ConveyorObject ConveyorObject => _conveyor;


		private void OnEnable()
		{
			_interactionMiniGameStarter.OnMiniGameCompleted += OnMiniGameCompleted;
			_interactionMiniGameStarter.OnMiniGameStarted += OnMiniGameStarted;
			_animation = GetComponent<Animator>();
			_disposable = _conveyor.CanPlace.Subscribe(b => _genericInteraction.DontShow = !b);
		}

		private void Start()
		{
			_questTipService.AddTip(QuestTip);
		}
		
		private void OnDisable()
		{
			_interactionMiniGameStarter.OnMiniGameCompleted -= OnMiniGameCompleted;
			_interactionMiniGameStarter.OnMiniGameStarted -= OnMiniGameStarted;
			_handle.IsActiveCancel();
			_disposable?.Dispose();
		}

		private void OnMiniGameStarted(CharacterContext characterContext)
		{
			_inMiniGame = true;

			_staticCharacterAnimator.AttachCharacter(characterContext, characterContext.CurrentAdapter.transform);
			_animation.SetFloat(AHash.ActionMultiplier, 1f);
			_currentWinPercent = 0;
			_lastWinPercent = 0;
			_analyticService.SendEvent($"OilRig:Minigame:Rotate");
			if (_miniGameService.TryGetMiniGamePercentage(out var observable))
			{
				_winDisposable?.Dispose();
				_winDisposable = observable.Subscribe(OnUpdate);
				AnimateWhileSpin().Forget();
			}
		}

		private async UniTaskVoid AnimateWhileSpin()
		{
			while (_inMiniGame && !destroyCancellationToken.IsCancellationRequested)
			{
				if (_currentWinPercent > _lastWinPercent)
				{
					if (_animation.GetFloat(AHash.ActionMultiplier) == 0)
					{
						SmoothEnableAnim();
						PlaySound();
					}
				}
				else
				{
					if (Mathf.Approximately(_animation.GetFloat(AHash.ActionMultiplier), 1))
					{
						SmoothDisableAnim();
						StopSound();
					}
				}
				
				_lastWinPercent = _currentWinPercent;
				await UniTask.Delay(.25f.ToSec(), cancellationToken:destroyCancellationToken);
			}
			
			if (!destroyCancellationToken.IsCancellationRequested)
			{
				SmoothDisableAnim();
			}
		}

		private void PlaySound()
		{
			_audioPlayer?.Stop();
			_audioPlayer = LucidAudio
				.PlaySE(_refiningSound, 1)
				.SetPosition(transform.position)
				.SetSpatialBlend(1)
				.SetLoop();
		}

		private void StopSound()
		{
			if (_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
				_audioPlayer.Stop(0.1f);
		}
		
		private void SmoothEnableAnim()
		{
			_staticCharacterAnimator.PlayFPVRotate();
			_handle.IsActiveCancel();
			_handle = LMotion.Create(_animation.GetFloat(AHash.ActionMultiplier), 1f, 0.25f).Bind(SetFloat);
		}
		
		private void SmoothDisableAnim()
		{
			_handle.IsActiveCancel();
			_handle = LMotion.Create(_animation.GetFloat(AHash.ActionMultiplier), 0f, 0.25f).Bind(SetFloat);
		}

		private void SetFloat(float val)
		{
			_animation.SetFloat(AHash.ActionMultiplier, val);
			_staticCharacterAnimator.SetActionMult(val);
		}
		private void OnUpdate(float winPercent) => _currentWinPercent = winPercent;

		private void OnMiniGameCompleted(bool isWin)
		{
			_staticCharacterAnimator.StopFPVRotate();
			_inMiniGame = false;
			_winDisposable?.Dispose();
			if (_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
			{
				_audioPlayer.Stop(1);
			}
			_conveyor.RecalcCanPlace();
			_staticCharacterAnimator.Detach();

			if (isWin && _conveyor.CanPlace.Value)
			{
				var instance = (CarriedContext)_objectFactory.CreateObject(_spawnedObjectId, _spawnPoint.position, _spawnPoint.rotation);
				_analyticService.SendEvent($"OilRig:Minigame:EndRotate");
				_conveyor.Place(instance);
			}
			
			_conveyor.RecalcCanPlace();
		}
	}
}