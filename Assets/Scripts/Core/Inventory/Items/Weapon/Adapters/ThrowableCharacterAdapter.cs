using System;
using Core.Entity.Characters;
using Core.Entity.EntityAnimation;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using UnityEngine;

namespace Core.Inventory.Items.Weapon.Adapters
{
	public class ThrowableCharacterAdapter : ThrowableAdapter
	{
		private readonly CharacterContext _characterContext;
		private readonly CharacterAnimatorAdapter _currentAnimatorAdapter;

		private bool _isThrowing;
		private IDisposable _timer;

		public ThrowableCharacterAdapter(ThrowableWeaponContext context) : base(context)
		{
			_characterContext = context.Owner as CharacterContext;
			Debug.Assert(_characterContext);
			_currentAnimatorAdapter = _characterContext.CurrentAdapter.CharacterAnimatorAdapter;
		}
		
		public override void Throw()
		{
			ThrowAsync().Forget();
		}

		private async UniTaskVoid ThrowAsync()
		{
			if(_isThrowing) 
				return;
            
			if(Context.HasQuantity && Context.CurrentQuantity.Value <= 0)
				return;
			
			InAttackTimer();
			_isThrowing = true;
			
			var res = await _currentAnimatorAdapter.PlayAction(
				Context.ThrowAnimation,
				Context.GetTossAnim(), 
				Context.GetTossEventKey(),
				Context.AnimTriggerTime, 
				Context.AttackDelay,
				2f);
			
			_isThrowing = false;
			
			if(!res) 
				return;
            
			Context.ThrowObject();
		}
		
		public override Vector3 GetSpawnOriginPos()
		{
			var pos = Context.CameraService.IsThirdPerson ? Context.ItemAnimator.RuntimeModel.transform.position : Context.CameraService.CurrentBrain.transform.position;
			return pos + Context.Owner.MainTransform.forward;
		}
		
		public override Vector3 GetTargetPosition()
		{
			var ray = Context.RayCastService.CurrentHitPoint - Context.RayCastService.RayPosition;
			var normal = Context.RayCastService.CurrentNormal;
            
			normal.x = 0;
			normal.z = 0;
			normal.y = Mathf.Min(0, normal.y * 1.5f);
			
			return Vector3.ClampMagnitude(ray,25) + Context.RayCastService.RayPosition + normal;
		}
		
		private void InAttackTimer()
		{
			InAttack = true;
			_timer?.Dispose();
			_timer = Observable
				.Timer(1.5f.ToSec())
				.Subscribe(_ => InAttack = false);
		}

		public override void Dispose()
		{
			_timer?.Dispose();
		}
	}
}