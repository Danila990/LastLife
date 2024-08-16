using System;
using Core.Actions;
using Core.Boosts.Impl;
using Core.Boosts.Inventory;
using Core.CameraSystem;
using Core.Effects;
using Core.Entity.EntityAnimation;
using Core.Equipment.Inventory;
using Core.HealthSystem;
using Core.InputSystem;
using Core.Inventory;
using Core.Player.MovementFSM;
using Core.Player.MovementFSM.Data;
using Core.Services.Input;
using Cysharp.Threading.Tasks;
using RootMotion.Dynamics;
using Sirenix.OdinInspector;
using Ui.Sandbox.PlayerInput;
using UniRx;
using UnityEngine;
using Utils;
using Utils.Constants;
using VContainer;

namespace Core.Entity.Characters.Adapters
{
    public class MechCharacterAdapter : BaseCharacterAdapter, IPlayerAdapter
    {
        [SerializeField] private CharacterEffector _characterEffector;
        [SerializeField] private PlayerAnimatorAdapter _playerAnimatorAdapter;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private CapsuleColliderUtility _capsuleColliderUtility;
        [SerializeField] private PlayerMovementFsmData _movementData;

        private IBoostProvider _boostProvider;
        private StatsProvider _statsProvider;
        private AllEquipment _allEquipment;
        private BoostsInventory _boostsInventory;
        private MechEntityContext _mechEntityContext;
        private IPlayerInputListener _inputListener;
        private MechMovementStateMachine _fsm;
        
        [Inject] private readonly IObjectResolver _resolver;
        [Inject] private readonly ICameraService _cameraService;
        [Inject] private readonly IInputService _inputService;
        [Inject] private readonly IInventoryService _inventoryService;
        [Inject] private readonly IPlayerInputProvider _inputProvider;
        [Inject] private readonly PlayerInputController _inputController;
        public override AnimatorAdapter AnimatorAdapter => _playerAnimatorAdapter;
        public override CharacterAnimatorAdapter CharacterAnimatorAdapter => _playerAnimatorAdapter;
        public override IBoostProvider BoostProvider => _boostProvider;
        public override StatsProvider StatsProvider => _statsProvider;
        public override BaseEntityEffector Effector => _characterEffector;
        public IPlayerInputListener InputListener => _inputListener;
        public Rigidbody Rigidbody => _rigidbody;
        public CapsuleColliderUtility CapsuleColliderUtility => _capsuleColliderUtility;
        public MechEntityContext MechEntityContext => _mechEntityContext;
        public ICameraService CameraService => _cameraService;

        [ShowInInspector] public PlayerInputDto InputDto { get; } = new();

        public Transform MainCameraTransform { get; private set; }
        public AdapterConstraint Constraints { get; set; }
        public override LifeEntity Entity => MechEntityContext;


        public void BindMainCameraTransform(Transform mainCameraTransform) => MainCameraTransform = mainCameraTransform;

        public void SetMechContext(MechEntityContext context)
        {
            _mechEntityContext = context;
            context.transform.SetParent(transform);
            context.SetAdapter(this);
            _playerAnimatorAdapter.OnContextSet(context.CharacterAnimator, context);
            _fsm.ContextChanged(context);
        }

        public override void SetEntityContext(CharacterContext characterContext)
        {
            characterContext.SetImmortal(true);
            characterContext.PuppetMaster.enabled = false;
            characterContext.MainTransform.SetParent(_mechEntityContext.SeatTransform);
            characterContext.MainTransform.SetLocalPositionAndRotation(Vector3.down*0.2398f, Quaternion.identity);;
            characterContext.Adapter.MainAdapterTransform.gameObject.SetActive(false);
            characterContext.Adapter.MainAdapterTransform.transform.position = Vector3.down * 1000;
            var charAdapter = (characterContext.Adapter as PlayerCharacterAdapter);
            var fsm = charAdapter.MovementStateMachine;
            fsm.ChangeState(fsm.IdlingState);
            _currentCharacterContext = characterContext;
        }

        private void ResetEntityContext()
        {
            _currentCharacterContext.SetImmortal(false);
            _currentCharacterContext.PuppetMaster.enabled = true;
            _currentCharacterContext.MainTransform.SetParent(_currentCharacterContext.Adapter.MainAdapterTransform);
            _currentCharacterContext.MainTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _currentCharacterContext.Adapter.MainAdapterTransform.position =
                _mechEntityContext.SeatTransform.position + _mechEntityContext.SeatTransform.forward*3f;
            _currentCharacterContext.Adapter.MainAdapterTransform.gameObject.SetActive(true);
            _currentCharacterContext = null;
            _fsm.ChangeState(_fsm.IdlingState);
        }
        
        public override void Init()
        {
            base.Init();
            _allEquipment = new AllEquipment();
            _boostsInventory = new BoostsInventory();
            _boostProvider = new MechBoostProvider();
            _statsProvider = new StatsProvider(_boostProvider, this);
            _fsm.ReusableData.StatsProvider = StatsProvider;
            _inputService.ObserveGetButtonDown("Leave_button").Subscribe(OnLeave).AddTo(this);
        }

        private void OnLeave(Unit _)
        {
            _inventoryService.SetInventory(_currentCharacterContext.Inventory);
            _cameraService.SetTrackedTarget(_currentCharacterContext);
            _inputProvider.UseListener(_currentCharacterContext.Adapter as IPlayerInputListener);
            _mechEntityContext.OnLeave();
            ResetEntityContext();
            DelayCall(InputRigType.PlayerInputRig).Forget();
        }
        
        public void OnSeat(CharacterContext context)
        {
            _inventoryService.SetInventory(_mechEntityContext.Inventory);
            _cameraService.SetTrackedTarget(_mechEntityContext);
            _cameraService.SetZoomStatus(false);
            _cameraService.SetThirdPerson();
            SetEntityContext(context);
            _inputProvider.UseListener(InputListener);
            DelayCall(InputRigType.MechInputRig).Forget();
        }
        
        private async UniTaskVoid DelayCall(InputRigType type)
        {
            var ct = this.GetCancellationTokenOnDestroy();
            await UniTask.DelayFrame(3, cancellationToken: ct);
            _inputController.SwitchInputRig(type);
        }
        
        private void Awake()
        {
            _fsm = new MechMovementStateMachine(this, _movementData);
            _inputListener = new InputProvider(_fsm.ReusableData, InputDto,this);
            _fsm.ChangeState(_fsm.IdlingState);
            _capsuleColliderUtility.Initialize(gameObject);
            _capsuleColliderUtility.CalculateCapsuleColliderDimensions();
        }

        private void FixedUpdate()
        {
            if(!_mechEntityContext) return;
            _fsm.PhysicsUpdate();
            var velNorm = transform.InverseTransformDirection(Rigidbody.velocity).normalized;
            _mechEntityContext.CharacterAnimator.Animator.SetFloat(AHash.PlayerVelocityX, velNorm.x);
            _mechEntityContext.CharacterAnimator.Animator.SetFloat(AHash.PlayerVelocityY, velNorm.z);
        }

        private void Update()
        {
            _fsm.HandleInput();
            _fsm.Update();
        }

        private void LateUpdate()
        {
            _fsm.LateTick();
        }
        
        public void UseAnimationAction(Action entityAction) 
            => _fsm.UseAnimationItem(entityAction);

    }
    
    public class InputProvider : IPlayerInputListener
    {
        private PlayerInputDto _inputDto;
        private readonly MechCharacterAdapter _currentContext;
        private PlayerStateReusableData _reusableData;
        public PlayerInputDto InputDto => _inputDto;

        public InputProvider(
            PlayerStateReusableData reusableData,
            PlayerInputDto inputDto,
            MechCharacterAdapter currentContext
            )
        {
            _reusableData = reusableData;
            _inputDto = inputDto;
            _currentContext = currentContext;
        }

        public void OnSprintDown()
        {
            _reusableData.OnSprintDown?.Invoke();
        }

        public void OnJumpDown()
        {
            _reusableData.OnJumpDown?.Invoke();
        }

        public void OnJumpUp()
        {
            _reusableData.OnJumpUp?.Invoke();
        }

        public void OnMoveDown()
        {
            
        }

        public void OnAimDown()
        {
        }

        public void OnAction(ActionKey action, InputKeyType inputKeyType)
        {
		
            if (!_currentContext.MechEntityContext.Inventory.SelectedItem || _currentContext.MechEntityContext.Health.IsDeath)
                return;
			
            switch (inputKeyType)
            {
                case InputKeyType.GetDown:
                    _currentContext.MechEntityContext.Inventory.SelectedItem.InputDown(action);
                    break;
                case InputKeyType.GetUp:
                    _currentContext.MechEntityContext.Inventory.SelectedItem.InputUp(action);
                    break;
            }
        }

        public void OnActionGet(ActionKey action, bool status)
        {
            if(_currentContext == null)
                return;
			
            if (!_currentContext.MechEntityContext.Inventory.SelectedItem || _currentContext.MechEntityContext.Health.IsDeath)
                return;
			
            _currentContext.MechEntityContext.Inventory.SelectedItem.OnInput(action, status);
        }
    }
}