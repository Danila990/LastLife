using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

namespace Core.Equipment.Impl.Hat
{
	public class PropellerHatEquipmentEntity : HatEquipmentEntity
	{
		
		[SerializeField, BoxGroup("Params")] private float _fallLimit;
		[SerializeField, BoxGroup("Visual")] private Transform _propellerObject;
		[SerializeField, BoxGroup("Visual")] private float _turnoverDuration;
		[SerializeField, BoxGroup("Sound")] private AudioClip _clip;
		[SerializeField, BoxGroup("Sound")] private float _volume;

		private AudioPlayer _audioPlayer;
		private PlayerCharacterAdapter _adapter;
		private IDisposable _stat;
		private MotionHandle _handle;

		protected override void OnPutOnInternal()
		{
			base.OnPutOnInternal();
			if(Owner == null)
				return;
			
			if(Owner.Adapter is not PlayerCharacterAdapter adapter)
				return;
			
			_adapter = adapter;
		}

		protected override void OnTakeOffInternal()
		{
			base.OnTakeOffInternal();
			_stat?.Dispose();
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_stat?.Dispose();
			StopRotate();
			StopSound();
		}

		protected override void OnTick()
		{
			if (_adapter.Rigidbody.velocity.y < 0 && _adapter.MovementStateMachine.ReusableData.InAir.Value)
			{
				ApplyEffect();
				if (_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
					_audioPlayer.SetPosition(MainTransform.position);
			}
			else
			{
				RemoveEffect();
			}
		}

		private void ApplyEffect()
		{
			if(_stat != null)
				return;

			StartRotate();
			PlaySound();
			_stat = _adapter.StatsProvider.Stats.IncreaseStats(StatType.FallSpeedLimit, _fallLimit);
		}

		private void RemoveEffect()
		{
			if(_stat == null)
				return;

			StopRotate();
			StopSound();
			_stat?.Dispose();
			_stat = null;
		}

		private void StartRotate()
		{
			_handle.IsActiveCancel();

			var eulerAngles = _propellerObject.localEulerAngles;
			var startRotation = eulerAngles;
			var endRotation = eulerAngles + Vector3.up * 360f;
			_handle = LMotion
				.Create(startRotation, endRotation, _turnoverDuration)
				.WithLoops(-1, LoopType.Incremental)
				.BindToLocalEulerAngles(_propellerObject);

		}

		private void StopRotate()
		{
			_handle.IsActiveCancel();
		}

		private void PlaySound()
		{
			StopSound();
			_audioPlayer = LucidAudio
				.PlaySE(_clip)
				.SetPosition(MainTransform.position)
				.SetSpatialBlend(1f)
				.SetLoop()
				.SetVolume(_volume);
		}

		private void StopSound()
		{
			if (_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
				_audioPlayer.Stop(0.1f);
		}
	}

}
