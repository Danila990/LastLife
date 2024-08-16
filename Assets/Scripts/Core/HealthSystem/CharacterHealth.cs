using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Entity;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.HealthSystem
{
	[Serializable]
	public class CharacterHealth : Health, ILifeEntityHealth
	{
		[SerializeField] private bool _dieFromBloodLoss;
		[SerializeField] private float _maxBloodLevel;
		[ValueDropdown("@Utils.Constants.VFXConsts.GetKeys()")]
		public string BloodLossParticleKey;
		private float _currentBloodLevel;
		private LifeEntity _lifeEntity;
		private List<IHealth> _proxyHealth = new();
		
		public bool DiedFromBloodLoss {
			get => _dieFromBloodLoss;
			set => _dieFromBloodLoss = value; 
		}
		
		public float CurrentBloodLevel => _currentBloodLevel;
		
		public void SetContext(LifeEntity characterContext)
		{
			_lifeEntity = characterContext;
		}

		public override void DoDamage(ref DamageArgs args)
		{
			if (CurrentHealth <= args.Damage && _proxyHealth.Count > 0)
			{
				_proxyHealth[0].ForceDeath();
				_proxyHealth.RemoveAt(0);
				return;
			}
			
			base.DoDamage(ref args);
		}

		protected override void OnInit()
		{
			_currentBloodLevel = _maxBloodLevel;
		}

		protected override void OnDiedEvent(ref DamageArgs args)
		{
			_onDeath.Execute(new DiedArgs(_lifeEntity, args.DamageSource,args.MetaDamageSource));
		}
		
		public void SpendBlood(float amount)
		{
			_currentBloodLevel -= amount;
			
			if(!_dieFromBloodLoss) 
				return;
			
			if(_currentBloodLevel > 0) 
				return;
			
			SetCurrentHealth(0);
		}

		public void StartBloodLoss(ref DamageArgs args, Vector3 pos, Vector3 normal, Transform attach)
		{
			if(args.BloodLossAmount <= 0 || args.BloodLossTime <= 0) 
				return;
			
			if(_currentBloodLevel < 0) 
				return;
			
			BloodLoss(args, pos, normal, attach, _lifeEntity.destroyCancellationToken).Forget();
		}
		
		public void AddHealth(float amount)
		{
			CurrentHealth += amount;
			ClampHealth();
			_onDamage.Execute(default);
		}
		
		public void AddHealthWithoutClamp(float value)
		{
			CurrentHealth += value;
			_onDamage.Execute(default);
		}
		
		public void ClampHealth()
		{
			CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
		}
		
		public void AddHealthPercent(float percent)
		{
			AddHealth(MaxHealth * percent);
		}

		public void SetProxyHealth(IHealth proxyHealth)
		{
			if(proxyHealth == null || proxyHealth.IsDeath)
				return;

			_proxyHealth.Add(proxyHealth);
		}
		
		public void RemoveProxyHealth(IHealth proxyHealth)
		{
			_proxyHealth.Remove(proxyHealth);;
		}
		
		private async UniTaskVoid BloodLoss(
			DamageArgs args,
			Vector3 pos, 
			Vector3 normal, 
			Transform attach, 
			CancellationToken token)
		{
			if (!_lifeEntity.VFXFactory.TryGetParticle(BloodLossParticleKey, out var context)) 
				return;
			
			context.Attach(pos,normal,attach);
			context.Play();
			var time = args.BloodLossTime;
			while (time > 0)
			{
				time -= Time.deltaTime;
				await UniTask.NextFrame(PlayerLoopTiming.Update, token);
				SpendBlood(args.BloodLossAmount * Time.deltaTime);
				if(CurrentBloodLevel <= 0) 
					break;
			}
			_lifeEntity.VFXFactory.Release(BloodLossParticleKey,context);
		}
		
	}
}