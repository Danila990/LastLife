using System;
using System.Threading;
using Core.Actions;
using Core.Boosts.Impl;
using Core.Boosts.Inventory;
using Core.CameraSystem;
using Core.Effects;
using Core.Entity.EntityAnimation;
using Core.Entity.EntityUpgrade.Experience;
using Core.Entity.InteractionLogic;
using Core.Entity.InteractionLogic.Interactions;
using Core.Equipment.Inventory;
using Core.FullScreenRenderer;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Inventory.Items;
using Core.Inventory.Items.Weapon;
using Core.Player.MovementFSM;
using Core.Player.MovementFSM.Data;
using Core.Services;
using Core.Services.Input;
using Cysharp.Threading.Tasks;
using Db.ObjectData.Impl;
using LitMotion;
using RootMotion.Dynamics;
using SharedUtils.PlayerPrefs;
using Sirenix.OdinInspector;
using Ui.Sandbox.InventoryUi;
using Ui.Sandbox.ReloadUI;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Characters.Adapters
{
	public class PlayerCharacterAdapter : BaseCharacterAdapter, IPlayerInputListener, IPlayerAdapter
	{
		[SerializeField] private PlayerMovementFsmData _movementData;
		[SerializeField] private CapsuleColliderUtility _capsuleColliderUtility;
		[SerializeField] private NavMeshObstacle _navMeshObstacle;
		[SerializeField] private InventoryObjectDataSo GravyGun;
		[SerializeField] private PlayerExperienceProvider _experienceProvider;
		[SerializeField] private PlayerAnimatorAdapter _playerAnimatorAdapter;
		
		[field:SerializeField] public FpvHandsAnimator FpvHandsAnimator { get; private set; }
		[SerializeField] private CharacterEffector _characterEffector;
			
		public Transform MainCameraTransform { get; private set; }
		public Rigidbody Rigidbody { get; private set; }

		[ShowInInspector] public PlayerInputDto InputDto { get; private set; } = new PlayerInputDto();

		public CapsuleColliderUtility CapsuleColliderUtility => _capsuleColliderUtility;
		
		private PlayerMovementStateMachine _movementFsm;
		private PlayerBoostProvider _boostProvider;
		private CharacterData _data;
		private bool _binded;
		private bool _isTeleporting;
		private AimState _aimState;
		private WeaponReloadHandler _reloadHandler;
		private AllEquipment _allEquipment;
		private StatsProvider _statsProvider;
		
		[Inject] private readonly IRayCastService _rayCastService;
		[Inject] private readonly ICameraService _cameraService;
		[Inject] private readonly IPlayerPrefsManager _playerPrefsManager;
		[Inject] private readonly IObjectResolver _resolver;
		[Inject] private readonly ReloadUIController _uiController;
		[Inject] private readonly InventoryPreviewController _inventoryPreview;
		[Inject] private readonly IPlayerStaticInteractionService _staticInteractionService;
		[Inject] private readonly IPlayerInputProvider _playerInputProvider;
		[Inject] private readonly IPassRendererProvider _passRendererProvider;
		[Inject] private readonly IFullScreenMaterialData _fullScreenMaterialData;
		
		private BoostsInventory _boostsInventory;
		
		private CancellationTokenSource _updateToken;

		public PlayerMovementStateMachine MovementStateMachine => _movementFsm;
		public ExperienceProvider ExperienceProvider => _experienceProvider;
		public AllEquipment AllEquipment => _allEquipment;
		public ICameraService CameraService => _cameraService;
		public BoostsInventory BoostsInventory => _boostsInventory;
		public AdapterConstraint Constraints { get; set; }
		public string ContextId { get; private set; }

		public override AnimatorAdapter AnimatorAdapter => _playerAnimatorAdapter;
		public override CharacterAnimatorAdapter CharacterAnimatorAdapter => _playerAnimatorAdapter;
		public override IBoostProvider BoostProvider => _boostProvider;
		public override StatsProvider StatsProvider => _statsProvider;
		public override BaseEntityEffector Effector => _characterEffector;
		

		public override void Init()
		{
			base.Init();
			
			_allEquipment = new AllEquipment();
			_boostsInventory = new BoostsInventory();
			_boostProvider = new PlayerBoostProvider(_resolver, this, _boostsInventory);
			_statsProvider = new StatsProvider(_boostProvider, this);
			
			_experienceProvider.Initialize(_playerPrefsManager, _resolver, this);
			_playerInputProvider.UseListener(this);
			MovementStateMachine.ReusableData.StatsProvider = StatsProvider;
			_resolver.Inject(_characterEffector);
			FpvHandsAnimator.Init(_cameraService, this);
		}

		private void OnDisable()
		{
			_allEquipment?.Dispose();
			_statsProvider?.Dispose();
		}

		public void SetContextId(string contextId)
		{
			ContextId = contextId;
		}

		public override void SetEntityContext(CharacterContext characterContext)
		{
			characterContext.SetEquipmentStorage(_allEquipment);
			base.SetEntityContext(characterContext);
			_data = characterContext.CharacterData;
			_navMeshObstacle.radius = _data.AgentRadius;
			characterContext.CharacterController.SetParent(transform);
			characterContext.CharacterController.localPosition = Vector3.zero;
			characterContext.PuppetMaster.targetRoot = transform;
			characterContext.PuppetMaster.solverIterationCount = AdapterData.SolverIteration;
			characterContext.BehaviourPuppet.knockOutDistance = 10;
			characterContext.PuppetMaster.mode = PuppetMaster.Mode.Kinematic;
			characterContext.name = "==PLAYER CONTEXT==";
			
			characterContext.Inventory.OnItemSelected.Subscribe(OnItemSelected).AddTo(this);
			characterContext.Inventory.OnItemDeselected.Subscribe(_ => OnItemDeselected()).AddTo(this);
			characterContext.LegsAnimator.Rigidbody = Rigidbody;
		
			_movementFsm.ContextChanged(characterContext);
			characterContext.CharacterAnimator.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			characterContext.Inventory.InsertItem(GravyGun.Model, 1);
			characterContext.Health.OnDeath.Subscribe(OnContextDeath).AddTo(characterContext);
			characterContext.Health.DiedFromBloodLoss = false;
			characterContext.LegsAnimator.DisableIfInvisible = null;
			_playerAnimatorAdapter.OnContextSet(characterContext.CharacterAnimator, characterContext);
				
			_reloadHandler?.Dispose();
			_reloadHandler = null;
			
			_reloadHandler = new WeaponReloadHandler(_uiController, _inventoryPreview);
			
			_binded = true;
			_movementFsm.ReusableData.IsDead = CurrentContext.Health.IsDeath;
			_movementFsm.ChangeState(_movementFsm.IdlingState);
			_experienceProvider.OnEntityChanged(CurrentContext);
			_characterEffector.SetContext(characterContext);
			_boostProvider.OnContextChanged(characterContext);
			OnContextSetup();
			CreateFixedUpdate();
			DelayedInit().Forget();
			
			if (!_cameraService.IsThirdPerson)
				_cameraService.SetFirstPerson();
		}

		private async UniTaskVoid DelayedInit()
		{
			await UniTask.DelayFrame(5, cancellationToken: destroyCancellationToken);
			CurrentContext.CharacterController.localRotation = Quaternion.identity;
			CurrentContext.PuppetMaster.mode = PuppetMaster.Mode.Active;
		}
		
		public void Attach(StaticWeaponInteraction staticWeaponInteraction)
		{
			CurrentContext.Inventory.UnSelect();
			_staticInteractionService.Attach(staticWeaponInteraction, CurrentContext.Health.OnDeath);
		}
		
		public void Detach()
		{
			if (CurrentContext)
			{
				_cameraService.SetTrackedTarget(CurrentContext);

				if (!CurrentContext.Health.IsDeath)
					CurrentContext.Inventory.TrySelectItem(0);
			}
			
			_playerInputProvider.UseListener(this);
		}
		
		public void SetAimState(AimState state)
		{
			if (_cameraService.IsThirdPerson)
			{
				_movementFsm.OnAimingChange(state != AimState.Default);
			}
			AimController.SetAimState(state);	
		}

		protected override void BeforeDestroy()
		{
			_binded = false;
			_movementFsm.DisposeCharacterContext(CurrentContext);
			SetAimState(AimState.Default);
			Destroy(CurrentContext.CharacterController.gameObject);
			Destroy(CurrentContext.gameObject);
		}
		
		private void OnItemSelected(ItemContext context)
		{
			if (context is ProjectileWeaponContext weapon)
			{
				_reloadHandler.SetWeapon(weapon, 1f);
			}
			else
			{
				_reloadHandler.Reset();
			}
		}
		
		private void OnItemDeselected()
		{
			_reloadHandler.Reset();
		}

		public void UseAnimationAction(Action entityAction) 
			=> _movementFsm.UseAnimationItem(entityAction);

		public void OnAimDown()
		{
			if (CurrentContext.Health.IsDeath)
				return;
			ToggleAim();
		}
		
		public void OnSprintDown()
		{
			_movementFsm.ReusableData.OnSprintDown?.Invoke();
		}
		
		public void OnJumpDown()
		{
			_movementFsm.ReusableData.OnJumpDown?.Invoke();
		}
		
		public void OnJumpUp()
		{
			_movementFsm.ReusableData.OnJumpUp?.Invoke();
		}
		
		public void OnMoveDown()
		{
			
		}
		
		public void OnActionGet(ActionKey actionKey, bool status)
		{
			if(CurrentContext == null)
				return;
			
			if (!CurrentContext.Inventory.SelectedItem || CurrentContext.Health.IsDeath)
				return;
			
			CurrentContext.Inventory.SelectedItem.OnInput(actionKey, status);
		}

		public void BindMainCameraTransform(Transform mainCameraTransform) => MainCameraTransform = mainCameraTransform;

		public void OnAction(ActionKey actionKey, InputKeyType inputKeyType)
		{
			if (CurrentContext.CarryInventory.HasContext && inputKeyType == InputKeyType.GetDown)
			{
				CurrentContext.CarryInventory.Remove();
				return;
			}
			
			if (!CurrentContext.Inventory.SelectedItem || CurrentContext.Health.IsDeath)
				return;
			
			switch (inputKeyType)
			{
				case InputKeyType.GetDown:
					CurrentContext.Inventory.SelectedItem.InputDown(actionKey);
					break;
				case InputKeyType.GetUp:
					CurrentContext.Inventory.SelectedItem.InputUp(actionKey);
					break;
			}
		}

		private void ToggleAim() => SetAimState(AimState.Default);
		
		private void OnViewChange(bool isThirdPerson)
		{
			_movementFsm.ReusableData.IsThirdPersonView = isThirdPerson;
			if(isThirdPerson)
			{
				_movementFsm.OnAimingChange(_aimState != AimState.Default);
				AimController.SetAimState(_aimState);
			}
			else
			{
				_aimState = AimController.CurrState;
				_movementFsm.OnAimingChange(true);
				AimController.SetAimState(AimState.Aim);	
			}
		}

		private void Awake()
		{
			Rigidbody = GetComponent<Rigidbody>();
			_movementFsm = new PlayerMovementStateMachine(this, _movementData);

			_capsuleColliderUtility.Initialize(gameObject);
			_capsuleColliderUtility.CalculateCapsuleColliderDimensions();
		}

		private void Start()
		{
			_cameraService.IsThirdPersonObservable.Subscribe(OnViewChange).AddTo(this);
		}


		private async UniTask DissolveAsync(float startValue, float endValue)
		{
			await LMotion
				.Create(startValue, endValue, 0.3f)
				.Bind(SetDissolve).ToUniTask(CurrentContext.destroyCancellationToken);
		}

		private void SetDissolve(float value)
		{
			if (!CurrentContext)
				return;
			foreach (var render in CurrentContext.BodyRenders)
			{
				foreach (var material in render.materials)
				{
					material.SetFloat(ShHash.DissolveSlider, value);
				}
			}
		}
		
		public async UniTaskVoid Teleport(Vector3 position, AudioClip start = null, AudioClip end = null)
		{
			if(_isTeleporting || !_movementFsm.State.CanExit)
				return;
			
			_isTeleporting = true;
			_movementFsm.ReusableData.TeleportPosition = position;
			var previousState = _movementFsm.State;
			var source = CurrentContext.Uid.ToString();
			var cooldown = 0.3f;
			if (CurrentContext.AudioService.TryPlayQueueSound(start, source, cooldown, out var startPlayer))
			{
				startPlayer
					.SetSpatialBlend(1f)
					.SetPosition(CurrentContext.MainTransform.position);
			}
			await DissolveAsync(0f, 1f);
			_movementFsm.ChangeState(_movementFsm.TeleportState);
			if (CurrentContext.AudioService.TryPlayQueueSound(end, source, cooldown, out var endPlayer))
			{
				endPlayer
					.SetSpatialBlend(1f)
					.SetPosition(CurrentContext.MainTransform.position);
			}
			await DissolveAsync(1f, 0f);
			
			_movementFsm.TeleportState.ResetConstraints();
			_movementFsm.ChangeState(previousState);
			SetAimState(AimController.CurrState);
			_isTeleporting = false;
		}
		
		public void Update()
		{
			if(!_binded) return;
			if (CurrentContext.Health.IsDeath)
				return;
			var target = CurrentContext.RigProvider.Rigs["aim"].RigTarget;
			target.position = 
				Vector3.Lerp(target.position, 
				_rayCastService.CurrentHitPoint, Time.deltaTime * 10);
			
			_movementFsm.HandleInput();
			_movementFsm.Update();
		}
		
		private void FixUpdate()
		{
			if(!_binded) return;
			if (CurrentContext.Health.IsDeath)
				return;
			_movementFsm.PhysicsUpdate();
			var velNorm = transform.InverseTransformDirection(Rigidbody.velocity).normalized;
			CurrentContext.CharacterAnimator.Animator.SetFloat(AHash.PlayerVelocityX, velNorm.x);
			CurrentContext.CharacterAnimator.Animator.SetFloat(AHash.PlayerVelocityY, velNorm.z);
		}
		
		public virtual void SetPosition(Vector3 position)
		{
			_isTeleporting = true;
			_movementFsm.ReusableData.TeleportPosition = position;
			_movementFsm.ChangeState(_movementFsm.TeleportState);
			_movementFsm.TeleportState.ResetConstraints();
			_movementFsm.ChangeState(_movementFsm.IdlingState);
			_isTeleporting = false;
		}
		
		public async UniTask ManualMoveTo(Transform t)
		{
			var previousState = _movementFsm.State;

			_movementFsm.ChangeState(_movementFsm.ManualWalk);
			_movementFsm.ManualWalk.SetConstraints();
			var startTime = Time.time;
			while (Vector3.Distance(transform.position, t.position) > 0.15f && startTime + 2f > Time.time)
			{
				var delta = t.position - transform.position;
				delta.Normalize();
				_movementFsm.ReusableData.MovementInput = new Vector2(delta.x, delta.z);
				await UniTask.NextFrame(destroyCancellationToken);
			}
			
			_movementFsm.ManualWalk.ResetConstraints();
			_movementFsm.ChangeState(previousState);
		}

		private void LateUpdate()
		{
			_movementFsm.LateTick();
		}

		private void OnTriggerEnter(Collider other) => _movementFsm.OnTriggerEnter(other);

		private void OnTriggerExit(Collider other) => _movementFsm.OnTriggerExit(other);

		private void CreateFixedUpdate()
		{
			if (_updateToken is { IsCancellationRequested: false } )
			{
				_updateToken.Cancel();
				_updateToken.Dispose();
			}
			_updateToken = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
			AsyncUpdate(PlayerLoopTiming.LastFixedUpdate, FixUpdate, _updateToken.Token).Forget();
		}
		
		private async UniTaskVoid AsyncUpdate(PlayerLoopTiming timing, Action action, CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				if(_binded)
					action();
				
				await UniTask.Yield(timing, token);
			}
		}
		
		private void OnContextDeath(DiedArgs obj)
		{
			_movementFsm.ReusableData.IsDead = true;
			if (CurrentContext.Inventory.SelectedItem)
			{
				CurrentContext.Inventory.UnSelect();
				_reloadHandler?.Dispose();
			}
		}

		protected override Vector3 GetVel() => Rigidbody.velocity;

#if UNITY_EDITOR
		[ShowInInspector, HideInEditorMode, DisplayAsString]
		public string CurrentState => _movementFsm?.State?.GetType().ToString();
#endif
		
	}

	[Flags]
	public enum AdapterConstraint
	{
		None = 1 << 0,
		Movement = 1 << 1,
		Rotation = 1 << 2,
		Jumping = 1 << 3,
		Falling = 1 << 4,
	}
}
