using System;
using Core.InputSystem;
using Core.Player.MovementFSM.Data;
using UnityEngine;
using VContainer;

namespace VFX
{
	public class EmitOnJump : MonoBehaviour, IVfxComponent
	{
		[SerializeField] private ParticleSystem _particleSystem;
		[SerializeField] private int _emitCount;
		private PlayerStateReusableData _reusableData;
		
		
		[Inject]
		private void Construct(IPlayerSpawnService spawnService)
		{
			_reusableData = spawnService.PlayerCharacterAdapter.MovementStateMachine.ReusableData;
		}

		private void Update()
		{
			if (_reusableData == null)
				return;
			
			if(_reusableData.InJump)
				ChangeRateOverLifeTime(_emitCount);
			else
				ChangeRateOverLifeTime(0f);
		}

		private void ChangeRateOverLifeTime(float mult)
		{
			var emission = _particleSystem.emission;
			emission.enabled = mult > 0;
			emission.rateOverDistanceMultiplier = mult;
		}
		
		public void OnRelease()
		{
			_reusableData = null;
		}
	}

	public interface IVfxComponent
	{
		public void OnRelease();
	}
}
