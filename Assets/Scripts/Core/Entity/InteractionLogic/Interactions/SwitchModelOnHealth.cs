using System;
using Core.HealthSystem;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class SwitchModelOnDamage : MonoBehaviour
	{
		[SerializeField] private EntityContext _context;
		[SerializeField] private HealthSwitch[] _healthSwitches;
		private IHealth _targetHeath;
		private int _currentActiveIndex;
		
		private void Start()
		{
			_targetHeath = GetComponent<IHealthProvider>().GetHealth();
			_targetHeath.OnDamage.Subscribe(OnDamage).AddTo(destroyCancellationToken);
		}
		
		private void OnDamage(DamageArgs obj)
		{
			for (var i = 0; i < _healthSwitches.Length; i++)
			{
				ref var hs = ref _healthSwitches[i];
				if (hs.Enabled)
					continue;
				
				if (hs.HealthPercent >= _targetHeath.PercentHealth)
				{
					if (_context.AudioService.TryPlayQueueSound(_healthSwitches[_currentActiveIndex].Crack, _context.Uid.ToString(), _healthSwitches[_currentActiveIndex].Crack.length, out var player))
					{
						player
							.SetPosition(transform.position)
							.SetSpatialBlend(1f)
							.SetVolume(1f);
					}
					
					_healthSwitches[_currentActiveIndex].Model.SetActive(false);
					
					hs.Enabled = true;
					hs.Model.SetActive(true);
					_currentActiveIndex = i;
				}
			}
		}

		[Serializable]
		private struct HealthSwitch
		{
			[ReadOnly, ShowInInspector] public bool Enabled { get; set; }
			public float HealthPercent;
			public AudioClip Crack;
			public GameObject Model;
		}
	}
}