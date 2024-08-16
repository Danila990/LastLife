using System;
using AnnulusGames.LucidTools.Audio;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Factory;
using Core.Inventory.Items.Weapon;
using Core.Services;
using SharedUtils;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ShieldAction), fileName = nameof(ShieldAction))]
	public class ShieldAction : GenericEntityAction<WeaponContext>, IActionWithCooldown, IUnlockableAction
	{
		[ValueDropdown("@Core.Factory.DataObjects.FactoryData.ByType(EntityType.Object)")]
		[SerializeField] private string _shieldId;
		[SerializeField] private float _delay = 7f;
		[SerializeField] private float _duration = 7f;
		[SerializeField] private float _blinkDuration = 2f;
		[SerializeField] private AudioClip _showSound;
		[SerializeField] private AudioClip _hideSound;

		[SerializeField] private Vector3 _offsetTpv;
		[SerializeField] private Vector3 _offsetFpv;

		private PlayerCharacterAdapter _adapter;
		private ShieldEntityContext _shield;
		private CompositeDisposable _shieldObserving;
		private IDisposable _contextChanging;
		private IDisposable _inventoryItemChanging;

		private ReactiveCommand<float> _cooldown;
		public IObservable<float> OnCooldown => _cooldown;
		public IReactiveProperty<bool> IsUnlocked { get; set; }

		[Inject] private readonly ISpawnPointProvider _spawnPointProvider;
		[Inject] private readonly IObjectResolver _resolver;
		[Inject] private readonly ICameraService _cameraService;
		[Inject] private readonly IObjectFactory _factory;

		public override void Initialize()
		{
			_cooldown = new ReactiveCommand<float>();
		}
		
		public override void SetContext(EntityContext context)
		{
			base.SetContext(context);
			if (CurrentContext.Owner is CharacterContext { Adapter: PlayerCharacterAdapter adapter })
			{
				_adapter = adapter;
				_contextChanging?.Dispose();
				_contextChanging = _adapter.ContextChanged.Subscribe(OnContextChanged);
			}
		}

		private void OnContextChanged(CharacterContext context)
		{
			_shieldObserving?.Dispose();
			_contextChanging?.Dispose();
			_inventoryItemChanging?.Dispose();
		}

		public override void Dispose()
		{
			_cooldown?.Dispose();
			_shieldObserving?.Dispose();
			_contextChanging?.Dispose();
			_inventoryItemChanging?.Dispose();
			_shieldObserving = null;
		}

		public override void OnDeselect()
		{
			_shieldObserving?.Dispose();
			_shieldObserving = null;
			HideShield();
		}
		
		public override void OnInput(bool state) { }
		
		public override void OnInputUp() { }
		
		public override void OnInputDown()
		{
			if(_shieldObserving != null)
				return;
			
			_shieldObserving?.Dispose();
			_shieldObserving = new CompositeDisposable();
			ShowShield();
			_cooldown.Execute(_delay + _duration);
			
			_inventoryItemChanging?.Dispose();
			_inventoryItemChanging = _adapter.CurrentContext.Inventory.OnItemSelected.Subscribe(_ =>
			{
				HideShield();
			});
			
			Observable.Timer((_duration - _blinkDuration).ToSec())
				.TakeUntilDestroy(_shield)
				.Subscribe(_ =>
				{
					_shield.PlayBlinking(_blinkDuration);
				})
				.AddTo(_shieldObserving);
			
			Observable.Timer(_duration.ToSec())
				.TakeUntilDestroy(_shield)
				.Finally(() =>
				{
					_shieldObserving?.Dispose();
					_shieldObserving = null;
				})
				.Subscribe(l => HideShield())
				.AddTo(_shieldObserving);
			
			_shield.SetCalc(Calc);
		}


		private (Vector3 Pos, Vector3 Rot) Calc()
		{
			Vector3 targetPosition;
			Vector3 targetRotation;
			if (_cameraService.IsThirdPerson)
			{
				targetPosition = _adapter.transform.position + _adapter.transform.forward + _offsetTpv;
				targetRotation = _adapter.transform.eulerAngles;
			}
			else
			{
				var camera = _cameraService.CurrentBrain.OutputCamera.transform;
				var position = camera.position + camera.forward;
				position.y = _adapter.transform.position.y;
				targetPosition = position + _offsetFpv;
				var eulerAngels = camera.eulerAngles;
				eulerAngels.x = 0;
				eulerAngels.z = 0;
				targetRotation = eulerAngels;
			}

			return (targetPosition, targetRotation);
		}
		
		private void ShowShield()
		{
			var context = _factory.CreateObject(_shieldId);
			if (context is ShieldEntityContext shield)
			{
				_shield = shield;
				PlaySound(_showSound);
				return;
			}
			
			Debug.LogError($"There is no shield under the id ({_shieldId})");
		}

		private void PlaySound(AudioClip clip)
		{
			if(!clip)
				return;

			LucidAudio.PlaySE(clip)
				.SetPosition(Calc().Pos)
				.SetSpatialBlend(1f)
				.SetVolume(1f);
		}
		
		private void HideShield()
		{
			_inventoryItemChanging?.Dispose();
			if(_shield == null)
				return;
			
			PlaySound(_hideSound);
			_shield.SetCalc(null);
			Destroy(_shield.gameObject);
		}
	}
}
