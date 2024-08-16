using System;
using Core.CameraSystem;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UnityEngine;
using Utils.Constants;

namespace Core.Entity.EntityAnimation
{
	public interface IPlayerAdapter{
		public ICameraService CameraService { get; }
		public void UseAnimationAction(Action entityAction);
	}
	
	public class PlayerAnimatorAdapter : CharacterAnimatorAdapter
	{
		[SerializeField] private FpvHandsAnimator _fpvHandsAnimator;
		
		private CharacterContext _characterContext;
		private IPlayerAdapter _playerCharacterAdapter;
		private bool IsTpvMode => _playerCharacterAdapter.CameraService.IsThirdPerson;
		
		public override void OnContextSet<T>(EntityAnimator entityAnimator, T entityContext)
		{
			base.OnContextSet(entityAnimator, entityContext);
			_playerCharacterAdapter = GetComponent<IPlayerAdapter>();
			if (entityContext is CharacterContext characterContext)
			{
				_characterContext = characterContext;
				_fpvHandsAnimator.OnContextChanged(characterContext, true);
			}
		}

		public override UniTask<bool> PlayAction(
			AnimationClip clip,
			string fpvAnimKey, 
			string fpvEventKey,
			float triggerTime, 
			float awaitingTime = 2,
			float speedMul = 1)
		{

			if (IsTpvMode)
			{
				return CharacterAnimator.PlayAction(clip, triggerTime, awaitingTime, speedMul);
			}
			else
			{
				return _fpvHandsAnimator.PlayAction(Animator.StringToHash(fpvAnimKey), fpvEventKey);
			}
		}
		
		public async override UniTask<bool> LegAttack(
			AnimationClip clip,
			string fpvAnimKey, 
			string fpvEventKey,
			float triggerTime,
			Action callback,
			float awaitingTime = 2,
			float speedMul = 1)
		{
			if (IsTpvMode)
			{
				_playerCharacterAdapter.UseAnimationAction(callback);
				await UniTask.Delay(clip.length.ToSec() * .6f, cancellationToken: destroyCancellationToken);
				return true;
			}
			else
			{
				var res = await _fpvHandsAnimator.PlayAction(Animator.StringToHash(fpvAnimKey), fpvEventKey);
				return res;
			}
		}

		public override void SetFloat(int hash, float value)
		{
			if (IsTpvMode)
			{		
				CharacterAnimator.Animator.SetFloat(hash, value);
			}
			else
			{
				_fpvHandsAnimator.ItemAnimationBehaviour?.SetFloat(hash, value);
			}
		}
		
		public override void Play(int hash, AnimationType type)
		{
			if (IsTpvMode && type.Equals(AnimationType.tpv))
			{
				CharacterAnimator.Animator.Play(hash);
			}
			else
			{
				_fpvHandsAnimator.FpvCam.FPVHands.HandAnimator.Play(hash);
				_fpvHandsAnimator.FpvCam.FPVHands.HandAnimator.Update(0);
			}
		}
	}
}