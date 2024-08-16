using System;
using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Core.Equipment.Data;
using Core.Inventory;
using Core.Player.MovementFSM;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Equipment.Impl.JetPack
{
	public class JetPackContext : EquipmentEntityContext
	{
		[SerializeField] [TabGroup("Args")] private SerializedJetPackArgs _args;
		[SerializeField] [TabGroup("Fx")] private ParticleSystem[] _flameFx;
		[SerializeField] [TabGroup("Fx")] private float _flyFxSpeed = 8f;
		[SerializeField] [TabGroup("Fx")] private float _defaultFlyFxSpeed = 1f;
		[SerializeField] [TabGroup("Sfx")] private float _volume = 1f;
		[SerializeField] [TabGroup("Sound")] private AudioClip _flySound;
		[SerializeField] [BoxGroup("Sound")] public AudioClip _pourSound;

		private AudioPlayer _audioPlayer;
		private AudioPlayer _pourAudioPlayer;
		private JetPackArgs _jetPackArgs;
		private JetPackItemArgs _currentArgs;
		private SimpleTimerAction _fuelConsumption;
		private PlayerCharacterAdapter _adapter;
		private PlayerMovementStateMachine _fsm;

		public ref JetPackArgs Args => ref _jetPackArgs;
		public JetPackState State { get; private set; }

		protected override void OnCreated(IObjectResolver resolver)
		{
			base.OnCreated(resolver);
			_jetPackArgs = _args.GetArgs();
			_fuelConsumption = new SimpleTimerAction(_args.ConsumptionInterval);
			_fuelConsumption.SetAction(OnFuelConsumed);
		}

		protected override void OnTakeOffInternal()
		{
			_audioPlayer.IsActiveStop();
			_pourAudioPlayer.IsActiveStop();
		}

		public override void OnDestroyed(IEntityRepository entityRepository)
		{
			base.OnDestroyed(entityRepository);
			_audioPlayer.IsActiveStop();
			_pourAudioPlayer.IsActiveStop();
		}


		public override void ChangeCurrentArgs(in IEquipmentArgs args)
		{
			if (args is JetPackItemArgs jetPackItemArgs)
			{
				_currentArgs = jetPackItemArgs;
				_jetPackArgs.CurrentFuel.Value = _currentArgs.Fuel;
			}
		}
		public override IEquipmentArgs GetItemArgs()
		{
			return _currentArgs;
		}
		
		protected override void OnPutOnInternal()
		{
			if (Owner != null && Owner.Adapter is PlayerCharacterAdapter playerAdapter)
				SetAdapter(playerAdapter);
		}

		protected override void PlaceCosmetic()
		{
			if (Inventory.TryGetOrigin(PartType, out var data))
			{
				var placementData = data.GetOffset(_currentArgs.FactoryId);

				MainTransform.SetParent(placementData.Origin);
				MainTransform.localPosition = placementData.Origin.localPosition + placementData.Offset;
				MainTransform.localEulerAngles = placementData.Rotation;
			}
		}

		private void SetAdapter(PlayerCharacterAdapter adapter)
		{
			_adapter = adapter;
			_fsm = adapter.MovementStateMachine;
			OnLaunchInternal();
		}
		public void PlayPourSound()
		{
			_pourAudioPlayer.IsActiveStop();
			_pourAudioPlayer = LucidAudio
				.PlaySE(_pourSound)
				.SetVolume(0.3f);
		}
		public void AddFuel(float value)
		{
			var newValue = Mathf.Clamp(_jetPackArgs.CurrentFuel.Value + value, 0, _jetPackArgs.MaxFuel);
			_jetPackArgs.CurrentFuel.Value = newValue;
			_currentArgs.Fuel = newValue;
			PlayPourSound();
		}
		
		public virtual bool Fly(float delta, Vector3 preferredDirection)
		{
			if (_adapter == null || !IsEquipped || IsDestroyed)
				return false;

			var flyForce = new Vector3(1f, _args.FlySpeed, 1f) * delta;

			flyForce.x *= preferredDirection.x;
			flyForce.z *= preferredDirection.z;

			_fsm.Player.Rigidbody.AddForce(flyForce, ForceMode.VelocityChange);
			var clampedVelocity = _fsm.Player.Rigidbody.velocity;
			clampedVelocity.y = Mathf.Clamp(0, 2f, clampedVelocity.y);
			clampedVelocity.x = Mathf.Lerp(_fsm.Player.Rigidbody.velocity.x, 0, 2f * Time.fixedDeltaTime);
			clampedVelocity.z = Mathf.Lerp(_fsm.Player.Rigidbody.velocity.z, 0, 2f * Time.fixedDeltaTime);
			_fsm.Player.Rigidbody.velocity = clampedVelocity;

			OnFly(delta);
			return _jetPackArgs.CurrentFuel.Value > 0;
		}

#region Callbacks

		public void OnLaunch()
		{
			TryChangeState(JetPackState.Launched, OnLaunchInternal);
		}

		private void OnFly(float deltaTime)
		{
			if(!IsEquipped || IsDestroyed)
				return;
				
			if (_audioPlayer is { state: AudioPlayer.State.Playing } && MainTransform)
				_audioPlayer.SetPosition(MainTransform.position);

			_fuelConsumption?.Tick(ref deltaTime);

			TryChangeState(JetPackState.Process, OnFlyInternal);
		}

		public void OnStop()
		{
			TryChangeState(JetPackState.Stopped, OnStopInternal);
		}

		private void TryChangeState(JetPackState state, Action onChange)
		{
			if (State == state)
				return;

			onChange();

			State = state;
		}

		private void OnFuelConsumed()
		{
			_currentArgs.Fuel -= _jetPackArgs.FuelСonsumption;
			_jetPackArgs.CurrentFuel.Value -= _jetPackArgs.FuelСonsumption;
			OnFuelConsumedInternal();
		}
		
#endregion

#region VirtualCallbacks

		protected virtual void OnLaunchInternal()
		{
			foreach (var fx in _flameFx)
			{
				var main = fx.main;
				main.startSpeedMultiplier = _defaultFlyFxSpeed;
				fx.Play();
			}
			_fuelConsumption.CanUse(true);
		}
		protected virtual void OnFlyInternal()
		{
			_audioPlayer?.Stop();
			_audioPlayer = LucidAudio
				.PlaySE(_flySound)
				.SetVolume(_volume)
				.SetSpatialBlend(1f)
				.SetAutoStop(false)
				.SetLoop();

			foreach (var fx in _flameFx)
			{
				var main = fx.main;
				main.startSpeedMultiplier = _flyFxSpeed;
			}
		}
		protected virtual void OnStopInternal()
		{
			if(_audioPlayer != null && _audioPlayer.state != AudioPlayer.State.Stop)
				_audioPlayer.Stop(0.3f);
			_audioPlayer = null;
			
			_fuelConsumption.CanUse(false);

			foreach (var fx in _flameFx)
			{
				var main = fx.main;
				main.startSpeedMultiplier = _defaultFlyFxSpeed;
			}
		}

		protected virtual void OnFuelConsumedInternal() { }

		public virtual void OnDeath()
		{
			if (_audioPlayer is { state: AudioPlayer.State.Playing })
				_audioPlayer.Stop();
		}

#endregion
	}

	public enum JetPackState
	{
		Launched,
		Process,
		Stopped
	}

	public readonly struct JetPackArgs
	{
		public readonly float MoveSpeedModifier;
		public readonly float FlySpeed;
		public readonly FloatReactiveProperty CurrentFuel;
		public readonly float MaxFuel;
		public readonly float FuelСonsumption;
		public readonly float ConsumptionInterval;

		public JetPackArgs(ref SerializedJetPackArgs args)
		{
			MoveSpeedModifier = args.MoveSpeedModifier;
			FlySpeed = args.FlySpeed;
			CurrentFuel = new FloatReactiveProperty(args.Fuel);
			MaxFuel = args.Fuel;
			FuelСonsumption = args.FuelСonsumption;
			ConsumptionInterval = args.ConsumptionInterval;
		}
	}

	[Serializable]
	public struct SerializedJetPackArgs
	{
		public float MoveSpeedModifier;
		public float FlySpeed;
		public float Fuel;
		public float FuelСonsumption;
		public float ConsumptionInterval;

		public JetPackArgs GetArgs()
		{
			return new JetPackArgs(ref this);
		}
	}
}
