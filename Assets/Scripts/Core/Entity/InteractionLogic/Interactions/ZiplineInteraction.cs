using System;
using System.Linq;
using AnnulusGames.LucidTools.Audio;
using Core.AnimationRigging;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.EntityAnimation;
using Core.HealthSystem;
using Core.Inventory.Items;
using Cysharp.Threading.Tasks;
using RootMotion.Dynamics;
using StateMachine;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class ZiplineInteraction : PlayerInputInteraction, ICallbackListener
	{
		[SerializeField] private Transform _from;
		[SerializeField] private Transform _to;
		[SerializeField] private float _duration = 1f;
		[SerializeField] private AudioClip _pickUpSound;
		
		private WorldSpaceButton _currentUI;
		private WorldButtonPresenter _defaultWorldButton;
		
		private IDisposable _disposable;
		private bool _disable;
		private AudioPlayer _audioPlayer;
		public bool IsPlayerMoved { get; private set; }
		
		protected override void OnStart()
		{
			_defaultWorldButton = new WorldButtonPresenter(Callback);
		}
		
		public override void Callback()
		{
			DisableUI();
			Use(PlayerSpawnService.PlayerCharacterAdapter.CurrentContext);
		}
		
		public override void Use(EntityContext user)
		{
			if (user is CharacterContext { Adapter: PlayerCharacterAdapter adapter })
			{
				PlaySound();
				MovePlayer(adapter);
				DisableUI();
				
				_disposable?.Dispose();
				_disposable = adapter.Entity.Health.OnDeath.Subscribe(OnUserDeath);
			}
		}

		protected override void OnPlayerExit()
		{
			DisableUI();
		}

		protected override void OnPlayerEnter()
		{
			DisableUI();
			ShowUI();
		}
		
		public override InteractionResultMeta Visit(IInteractorVisiter visiter,ref InteractionCallMeta meta)
		{
			return visiter.Accept(this, ref meta);
		}

		private void OnUserDeath(DiedArgs _)
		{
			_disable = true;
			_disposable?.Dispose();
			_disposable = null;
		}
		
		protected void PlaySound()
		{
			if (_pickUpSound)
			{
				_audioPlayer = LucidAudio
					.PlaySE(_pickUpSound)
					.SetPosition(transform.position)
					.SetPitch(0.7f)
					.SetSpatialBlend(1);
			}
		}

		private void MovePlayer(PlayerCharacterAdapter adapter)
		{
			IsPlayerMoved = true;
			var mode = adapter.CurrentContext.PuppetMaster.mode;
			adapter.CurrentContext.PuppetMaster.mode = PuppetMaster.Mode.Disabled;
			var firstGrip = adapter.CurrentContext.RigProvider.Rigs["firstGrip"];
			var secondGrip = adapter.CurrentContext.RigProvider.Rigs["secondGrip"];
			var weapon = adapter.CurrentContext.Inventory.InventoryItems.First().ItemContext;
			adapter.CurrentContext.Inventory.SelectItem(weapon);
			firstGrip.EnableRig();
			secondGrip.EnableRig();
			
			firstGrip.RigTarget.position = adapter.transform.position + Vector3.up * 2;
			secondGrip.RigTarget.position = adapter.transform.position + Vector3.up * 2;
			adapter.transform.forward = _from.forward;
			
			
			
			CameraService.CanSwitchCamera = false;
			
			var prevState = adapter.MovementStateMachine.State;
			adapter.MovementStateMachine.ChangeState(adapter.MovementStateMachine.ZiplineState);
			_disable = false;
			Detach(adapter, firstGrip, secondGrip, mode, prevState, weapon).Forget();
		}

		protected override void OnDisposed()
		{
			base.OnDisposed();
			if (CameraService != null)
			{
				CameraService.CanSwitchCamera = true;
			}
		}

		private async UniTaskVoid Detach(PlayerCharacterAdapter adapter, RigElementController firstGrip, RigElementController secondGrip, PuppetMaster.Mode mode, IState prevState, ItemContext weapon)
		{
			var ct = adapter.GetCancellationTokenOnDestroy();
			var speed = (_to.position - _from.position).magnitude / _duration;
			var pos = _from.position;
			await UniTask.NextFrame(ct);
			
			adapter.CharacterAnimatorAdapter.Play(AHash.Zipline,AnimationType.fpv);
			
			while ((pos - _to.position).magnitude > 0.1f && !_disable)
			{
				await UniTask.NextFrame(ct);
				pos = Vector3.MoveTowards(pos, _to.position, speed * Time.deltaTime);
				adapter.transform.position = pos;
				_audioPlayer.SetPositionSafe(pos);
			}
			
			CameraService.CanSwitchCamera = true;
			
			await UniTask.NextFrame(ct);
			adapter.MovementStateMachine.ZiplineState.ResetConstraints();
			adapter.MovementStateMachine.ChangeState(prevState);
			adapter.SetAimState(adapter.AimController.CurrState);
			firstGrip.DisableRig();
			secondGrip.DisableRig();
			adapter.transform.position = _to.position;
			adapter.CurrentContext.transform.position = _to.position;
			adapter.CurrentContext.PuppetMaster.Teleport(_to.position,Quaternion.identity,true);
			await UniTask.DelayFrame(3, cancellationToken: ct);
			adapter.CurrentContext.PuppetMaster.mode = mode;
			
			while (!ct.IsCancellationRequested)
			{
				adapter.transform.position = _to.position;
				adapter.CurrentContext.transform.position = _to.position;
				adapter.CurrentContext.PuppetMaster.Teleport(_to.position,Quaternion.identity,true);
				await UniTask.WaitForFixedUpdate(ct);
				if((adapter.transform.position-_to.position).sqrMagnitude<2) break;
			}
			IsPlayerMoved = false;
		}

		private void ShowUI()
		{
			_currentUI = WorldSpaceUIService.GetUI<WorldSpaceButton>(_worldButtonKey);
			_defaultWorldButton.Attach(_currentUI.Button);
			_currentUI.Target = transform;
			_currentUI.Offset = Vector3.up * 0.5f;
		}
        
		private void DisableUI()
		{
			if (!_currentUI)
				return;
            
			_defaultWorldButton.Dispose();
			_currentUI.IsInactive = true;
			_currentUI = null;
		}
	}
}
