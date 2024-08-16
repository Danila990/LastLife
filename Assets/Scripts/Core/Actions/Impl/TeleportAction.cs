using System;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items.Weapon;
using Core.Map;
using Core.Services;
using SharedUtils;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using VContainer;
using Random = UnityEngine.Random;

namespace Core.Actions.Impl
{
	[CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(TeleportAction), fileName = nameof(TeleportAction))]
	public class TeleportAction : GenericEntityAction<WeaponContext>, IActionWithCooldown, IUnlockableAction
	{
		[SerializeField] private float _radius = 10f;
		[SerializeField] private float _delay = 2f;
		[SerializeField] private AudioClip _startClip;
		[SerializeField] private AudioClip _endClip;

		[Inject] private readonly IPlayerTeleportProvider _teleportProvider;
		
		private PlayerCharacterAdapter _adapter;

		private ReactiveCommand<float> _cooldown;
		public IObservable<float> OnCooldown => _cooldown;
		public IReactiveProperty<bool> IsUnlocked { get; set; }

		public override void Initialize()
		{
			_cooldown = new ReactiveCommand<float>();
		}

		public override void SetContext(EntityContext context)
		{
			base.SetContext(context);
			if (CurrentContext.Owner is CharacterContext { Adapter: PlayerCharacterAdapter adapter })
				_adapter = adapter;
		}

		public override void Dispose()
		{
			_cooldown?.Dispose();
		}

		public override void OnDeselect() { }
		
		public override void OnInput(bool state) { }
		
		public override void OnInputUp() { }
		
		public override void OnInputDown()
		{
			if(_adapter == null)
				return;
			
			Vector3 currentPosition = CurrentContext.Owner.MainTransform.position;

			var teleportPosition = _teleportProvider.GetTpPoint(currentPosition, _radius);
			
#if UNITY_EDITOR
			Debug.DrawLine(CurrentContext.Owner.MainTransform.position, currentPosition, Color.green, 2f);
			Util.DrawSphere(currentPosition, Quaternion.identity, 0.2f, Color.green, 2f);
#endif
			
			_adapter.Teleport(teleportPosition, _startClip, _endClip).Forget();
			_cooldown.Execute(_delay);
		}
		

	}

}
