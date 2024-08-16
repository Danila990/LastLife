using System.Threading;
using Core.CameraSystem;
using Core.InputSystem;
using CustEditor.Attributes;
using Cysharp.Threading.Tasks;
using Db.Map;
using Installer;
using Sirenix.OdinInspector;
using Ui.Sandbox.WorldSpaceUI;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
    public abstract class PlayerInputInteraction : AbstractMonoInteraction, IInjectableTag
    {
        [Inject] protected readonly IPlayerSpawnService PlayerSpawnService;
        [Inject] protected readonly ICameraService CameraService;
        [Inject] protected readonly IWorldSpaceUIService WorldSpaceUIService;

        [SerializeField, BoxGroup("PlayerInputInteraction")] protected bool _awaitPlayer;
        [SerializeField, BoxGroup("PlayerInputInteraction"), WorldUiNames] protected string _worldButtonKey;
        [SerializeField, Optional, BoxGroup("PlayerInputInteraction")] protected Transform _targetTransform;
        
        public float TimeToUse = 1f;
        [HideIf("@Bounds != null")]
        public float InteractDistance = 3f;
        public SimpleBoundsObject Bounds;

        private bool _prevStatus;
        protected float CurrCamDist;
        protected float CurrPlayerSqrDist;
        private CancellationTokenSource _cts;
        
        public void Start()
        {
            if (!_targetTransform)
                _targetTransform = transform;
            
            if (_awaitPlayer)
            {
                AwaitPlayer(destroyCancellationToken).Forget();
            }
            
            OnStart();
        }
        
        
        public void Enable()
        {
            if (_awaitPlayer)
                return;

            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
                AwaitPlayer(_cts.Token).Forget();
            }
        }
		
        public void Disable()
        {
            if (_awaitPlayer)
                return;
            
            switch (_cts)
            {
                case null:
                    return;
                
                case { IsCancellationRequested: false }:
                    _cts.Cancel();
                    break;
            }

            _cts?.Dispose();
            _cts = null;
            
            UpdateStatus(false);
        }

        
        private void Update()
        {
            OnUpdate();
        }
        
        protected virtual void OnDestroy()
        {
            OnDisposed();
        }

        protected virtual void OnDisable()
        {
            if(_cts is { IsCancellationRequested: false })
                _cts.Cancel();
            
            _cts?.Dispose();
        }
        
        protected virtual void OnStart() { }
        
        protected virtual void OnUpdate() { }
        
        protected virtual void OnPlayerExit() { }

        protected virtual void OnPlayerEnter() { }
        
        protected virtual void OnDisposed() {}

        
        public virtual void Use(EntityContext user) { }
        
        public virtual void Callback() { }
        
        public override InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
        {
            return visiter.Accept(this, ref meta);
        }
        
        protected async UniTaskVoid AwaitPlayer(CancellationToken token)
        {
            await UniTask.Delay(2500, delayTiming:PlayerLoopTiming.FixedUpdate, cancellationToken: token);
            while (!token.IsCancellationRequested)
            {
                if (!gameObject.activeInHierarchy)
                {
                    UpdateStatus(false);
                    await UniTask.Delay(2500, delayTiming:PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                    continue;
                }
                
                if (PlayerSpawnService is null)
                {
                    await UniTask.Delay(1000, delayTiming:PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                    continue;
                }

                await UniTask.Delay(Random.Range(100, 250), delayTiming:PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                if (!PlayerSpawnService.PlayerCharacterAdapter.CurrentContext)
                {
                    await UniTask.Delay(1000, delayTiming:PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                    continue;
                }

                var status = Bounds ? CalculateByBounds() : CalculateByDistance();
                status &= AdditionalCondition();

                UpdateStatus(status);
            }
        }

        protected virtual bool AdditionalCondition() => true;

        private bool CalculateByBounds()
        {
            return Bounds.InBounds(PlayerSpawnService.PlayerCharacterAdapter.CurrentContext.MainTransform.position);
        }
        private bool CalculateByDistance()
        {
            var pos = PlayerSpawnService.PlayerCharacterAdapter.CurrentContext.LookAtTransform.position;
            var cameraTransform = CameraService.CurrentBrain.OutputCamera.transform;
            var delta = pos - _targetTransform.position;
            var cameraDelta = cameraTransform.position - _targetTransform.position;
            CurrCamDist = cameraDelta.magnitude;
            CurrPlayerSqrDist = delta.sqrMagnitude;
            var dot = Vector3.Dot(cameraDelta, cameraTransform.forward);
            var status = CurrPlayerSqrDist <= InteractDistance * InteractDistance;
            status &= dot < 0;
            if (status)
            {
                var castStatus = !Physics.Linecast(_targetTransform.position, cameraTransform.position, LayerMasks.Environment, QueryTriggerInteraction.Ignore);
                status = castStatus;
            }
            return status;
        }
        
        protected virtual void UpdateStatus(bool status)
        {
            if (_prevStatus == status)
                return;
            
            _prevStatus = status;
            
            if (status)
            {
                OnPlayerEnter();
                return;
            }
            OnPlayerExit();
        }
    }

}