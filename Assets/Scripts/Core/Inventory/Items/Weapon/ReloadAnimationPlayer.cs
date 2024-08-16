using System;
using System.Threading;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using UnityEngine;
using Utils.Constants;

namespace Core.Inventory.Items.Weapon
{
	public class ReloadAnimationPlayer : IDisposable
	{
		private readonly CharacterContext _context;
		private readonly AudioClip _reloadSound;
		private readonly ItemAnimator _itemAnimator;
		private readonly ProjectileWeaponContext _weaponContext;
		private readonly IDisposable _reloading;
		private readonly float _defaultSpeed;
		private readonly float _processSpeed;
		private readonly CancellationToken _token;
		private readonly ReloadAnimationData _reloadAnimationData;

		private AudioPlayer _audioPlayer;

		public ReloadAnimationPlayer(CharacterContext context,
			ReloadAnimationData reloadAnimationData,
			WeaponItemAnimator weaponItemAnimator,
			ProjectileWeaponContext weaponContext,
			AudioClip reloadSound,
			float defaultSpeed,
			float processSpeed)
		{
			_reloadAnimationData = reloadAnimationData;
			_context = context;
			_token = context.destroyCancellationToken;
			_itemAnimator = weaponItemAnimator;
			_weaponContext = weaponContext;
			_reloading = weaponContext.Reloading.SkipLatestValueOnSubscribe().Subscribe(OnReload);
			_defaultSpeed = defaultSpeed;
			_processSpeed = processSpeed;
			_reloadSound = reloadSound;
		}

		public void OnTick()
		{
			if(!_context || _audioPlayer == null || _audioPlayer.state == AudioPlayer.State.Stop)
				return;
			
			_audioPlayer.SetPosition(_context.CurrentAdapter.transform.position);
		}
		
		private void OnReload(bool isReloading)
		{
			if (isReloading)
				PlayStartAndProcess().Forget();
			else
				PlayEnd();
		}
        
		private async UniTaskVoid PlayStartAndProcess()
		{
			await PlayStart();
			PlayProcess();
		}   
        
		private async UniTask PlayStart()
		{
			if (!_weaponContext.IsPlayerOwned)
			{
				await TpvStartReload();
				return;
			}
			
			if (_weaponContext.IsTpvMode)
			{
				await TpvStartReload();
				return;
			}
            
			await FpvStartReload();
		}
        
		private void PlayProcess()
		{
			if (_reloadSound)
			{
				_audioPlayer = LucidAudio
					.PlaySE(_reloadSound)
					.SetPosition(_context.CurrentAdapter.transform.position)
					.SetVolume(0.1f)
					.SetSpatialBlend(1f)
					.SetLink(_context);
			}
			
			if (!_weaponContext.IsPlayerOwned)
			{
				TpvReload();
				return;
			}
			
			if (_weaponContext.IsTpvMode)
			{
				TpvReload();
				return;
			}
            
			FpvReload();
		}

		private void PlayEnd()
		{
			StopSoundWithDelay().Forget();

			if (!_weaponContext.IsPlayerOwned)
			{
				TpvEndReload();
				return;
			}
			
			TpvEndReload();
			FpvEndReload();
		}

		private async UniTaskVoid StopSoundWithDelay()
		{
			await UniTask.Delay(0.2f.ToSec(), cancellationToken: _token);
			ForceStopSound();
		}

		private void ForceStopSound()
		{
			if (_audioPlayer != null)
			{
				_audioPlayer.Stop();
				_audioPlayer = null;
			}
		}
		
#region FPV
		private async UniTask FpvStartReload()
		{
			SetMultiplier(_processSpeed);
			await _itemAnimator.PlayAnimAsync(AHash.StartReload, "StartReload");
		}
		
		private void FpvReload()
		{
			_itemAnimator.PlayAnimAsync(AHash.Reload, "Reload").Forget();
		}
		
		private void FpvEndReload()
		{
			SetMultiplier(_defaultSpeed);
			_itemAnimator.PlayAnimAsync(AHash.EndReload, "EndReload").Forget();
		}
	#endregion
		
		#region TPV
		private async UniTask TpvStartReload()
		{
			SetMultiplier(_processSpeed);
			_context.CharacterAnimator.Animator.Play(AHash.StartReload);
			await UniTask.Delay(_reloadAnimationData.Start.length.ToSec(), cancellationToken: _token);
		}
		
		private void TpvReload()
		{
			_context.CharacterAnimator.Animator.SetBool(AHash.IsReloading, true);
		}
		
		private void TpvEndReload()
		{
			SetMultiplier(_defaultSpeed);
			_context.CharacterAnimator.Animator.SetBool(AHash.IsReloading, false);
		}
		#endregion
		
		private void SetMultiplier(float value)
		{
			if (!_weaponContext || !_context)
				return;

			_itemAnimator.SetFloat(AHash.ReloadMultiplier, value);
		}
		
		public void Dispose()
		{
			_reloading?.Dispose();
		}
	}
	
	
	[Serializable]
	public class ReloadAnimationData
	{
		public AnimationClip Start;
		public AnimationClip Process;
		public AnimationClip End;
	}
}
