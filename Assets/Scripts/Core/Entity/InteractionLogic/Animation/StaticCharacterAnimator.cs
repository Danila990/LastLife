using Core.AnimationRigging;
using Core.Entity.Characters;
using Core.Entity.EntityAnimation;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Utils.Constants;

namespace Core.Entity.InteractionLogic.Animation
{
	public class StaticCharacterAnimator : MonoBehaviour
	{
		[SerializeField] private bool _rotateRoot;
		[SerializeField, VerticalGroup("Root")] private Transform _rootRotationTarget;
		[SerializeField, VerticalGroup("Root")] private Transform _movePoint;
		[SerializeField, VerticalGroup("Root")] private float _moveDuration = 1f;
		
		[SerializeField] private RigAnimatorTarget _lHandTarget;
		[SerializeField] private RigAnimatorTarget _rHandTarget;

		private bool _isConnected;
		
		private RigElementController _secondGrip;
		private RigElementController _firstGrip;
		private MotionHandle _rootHandle;
		private MotionHandle _rotationHandle;
		private CharacterAnimatorAdapter _animatorAdapter;

		public void AttachCharacter(CharacterContext characterContext, Transform root)
		{
			_secondGrip = characterContext.RigProvider.Rigs["firstGrip"];
			_firstGrip = characterContext.RigProvider.Rigs["secondGrip"];
			_secondGrip.EnableRig();
			_firstGrip.EnableRig();
			_animatorAdapter = characterContext.CurrentAdapter.CharacterAnimatorAdapter;
			MoveRoot(root);
			UpdateAsync(root).Forget();
		}
		
		public void Detach()
		{
			_secondGrip?.DisableRig();
			_firstGrip?.DisableRig();
			_rootHandle.IsActiveCancel();
			_rotationHandle.IsActiveCancel();
			
			_secondGrip = null;
			_firstGrip = null;
			_isConnected = false;
			_animatorAdapter = null;
		}

		public void PlayFPVRotate()
		{
			if(!_isConnected) return;
			_animatorAdapter.Play(AHash.Rotate,AnimationType.fpv);
		}

		public void StopFPVRotate()
		{
			if(!_isConnected) return;
			_animatorAdapter.Play(AHash.IdleParameterHash,AnimationType.fpv);
		}

		public void SetActionMult(float mult)
		{
			if(!_isConnected) return;
			_animatorAdapter.SetFloat(AHash.ActionMultiplier,mult);
		}
		
		private async UniTaskVoid UpdateAsync(Transform root)
		{
			if (_isConnected)
				return;
			_isConnected = true;

			while (_isConnected && !destroyCancellationToken.IsCancellationRequested)
			{
				if (_secondGrip != null)
				{
					_secondGrip.RigTarget.position = _rHandTarget.Position;
					_secondGrip.RigData.Rig.weight = _rHandTarget.Weight;
				}

				if (_firstGrip != null)
				{
					_firstGrip.RigTarget.position = _lHandTarget.Position;
					_firstGrip.RigData.Rig.weight = _lHandTarget.Weight;
				}
				
				if (_rotateRoot)
				{
					var forward = _rootRotationTarget.forward;
					forward.y = 0;
					root.rotation = Quaternion.LookRotation(forward);
				}

				await UniTask.NextFrame(destroyCancellationToken);
			}
		}

		private void OnDisable()
		{
			_rootHandle.IsActiveCancel();
			_rotationHandle.IsActiveCancel();
		}

		private void MoveRoot(Transform root)
		{
			_rootHandle.IsActiveCancel();
			_rotationHandle.IsActiveCancel();
			
			_rootHandle = LMotion
				.Create(root.position, _movePoint.position, _moveDuration)
				.WithScheduler(MotionScheduler.FixedUpdate)
				.BindToPosition(root);
			
			_rotationHandle = LMotion
				.Create(root.rotation, Quaternion.LookRotation(_rootRotationTarget.forward), _moveDuration)
				.WithScheduler(MotionScheduler.FixedUpdate)
				.BindToRotation(root);
		}
	}

}