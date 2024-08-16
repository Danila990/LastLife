using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using LitMotion;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Equipment.Impl.Shoes
{
	public class WingsShoes : ShoesEntity
	{
		[SerializeField] private SkinnedMeshRenderer _renderer;
		[SerializeField, BoxGroup("Sound")] private AudioClip _clip;
		[SerializeField, BoxGroup("Sound")] private float _volume;
	
		private MotionHandle _handle;
		private IDisposable _stat;
		private PlayerCharacterAdapter _adapter;
		private AudioPlayer _audioPlayer;

		protected override void OnPutOnInternal()
		{
			base.OnPutOnInternal();
			if (Owner == null || Owner.Adapter is not PlayerCharacterAdapter adapter)
				return;
			
			_adapter = adapter;
			_adapter.MovementStateMachine.ReusableData.InAir.Subscribe(OnGroundedChanged).AddTo(gameObject);
			_adapter.Constraints = AdapterConstraint.Falling;
			_stat?.Dispose();
			_stat = _adapter.StatsProvider.Stats.IncreaseStats(StatType.JumpForce, CurrentItemArgs.JumpHeight);
		}

		protected override void OnTakeOffInternal()
		{
			base.OnTakeOffInternal();
			if(!_adapter)
				return;
			
			_adapter.Constraints = AdapterConstraint.None;
			_stat?.Dispose();
			_stat = null;
			StopSound();
		}

		protected override void OnTick()
		{
			if (_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
				_audioPlayer.SetPosition(MainTransform.position);
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			if(_adapter)
				_adapter.Constraints = AdapterConstraint.None;
			
			_stat?.Dispose();
			_handle.IsActiveCancel();
			StopSound();
		}

		private void OnGroundedChanged(bool inAir)
		{
			if (inAir)
			{
				PlayAnimation();
				PlaySound();
			}
			else
			{
				StopAnimation();
				StopSound();
			}
		}
		
		private void PlayAnimation()
		{
			_handle.IsActiveCancel();
			_handle = LMotion
				.Create(0, 100f, 0.1f)
				.WithLoops(-1, LoopType.Yoyo)
				.Bind(SetBlendWeight);
		}

		private void SetBlendWeight(float weight)
		{
			if(_renderer != null)
				_renderer.SetBlendShapeWeight(0, weight);
		}

		
		private void StopAnimation()
		{
			_handle.IsActiveCancel();
			_handle = LMotion
				.Create(_renderer.GetBlendShapeWeight(0), 0f, 0.1f)
				.Bind(SetBlendWeight);
		}

		private void OnDestroy()
		{
			_handle.IsActiveCancel();
			StopSound();
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
