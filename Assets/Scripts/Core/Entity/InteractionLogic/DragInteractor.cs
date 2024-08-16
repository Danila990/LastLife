using System;
using Core.Entity.InteractionLogic.Interactions;
using UniRx;
using UnityEngine;

namespace Core.Entity.InteractionLogic
{
    public class DragInteractor : IInteractorVisiter, IDisposable, IDestroyListener
    {
        private readonly Transform _reference;
        private readonly IRayCastService _rayCastService;
        private float _currentDist;
        private DragInteraction _currentInteraction;
        private bool _inDrag;
        private Vector3 _targetPos;
        private ReactiveCommand<bool> _dragStatus = new();
        public bool InDrag => _inDrag;
        public IObservable<bool> DragStatus => _dragStatus;
        public DragInteraction CurrentInteraction => _currentInteraction;
        public Vector3 TargetPos => _targetPos;
        public float CurrentDist => _currentDist;


        public DragInteractor(
            Transform reference,
            IRayCastService rayCastService
            )
        {
            _reference = reference;
            _rayCastService = rayCastService;
        }

        public void Release()
        {
            if(!_inDrag) return;
            _inDrag = false;
            
            if (_currentInteraction)
            {
                _currentInteraction.OnEndDrag();
                _currentInteraction.DisableOutline();
            }
            
            _dragStatus.Execute(false);
        }

        public void SetCurrDist(float dist)
        {
            _currentDist = dist;
        }

        public void FixedUpdate(float delta)
        {
            if(!_inDrag) return;
            if (_currentInteraction is null || !_reference)
            {
               Release();
               return;
            }
            _targetPos = GetTargetPos();
            var targetRotation = Quaternion.FromToRotation(_currentInteraction.transform.up, Vector3.up) * _currentInteraction.transform.rotation;
            _currentInteraction.Drag(ref _targetPos, ref targetRotation, ref delta);
        }
        
        public void Update()
        {
            if(!_inDrag) return;
            if (_currentInteraction is null)
            {
                Release();
                return;
            }
            _targetPos = GetTargetPos();
        }

        private Vector3 GetTargetPos()
        {
            return _reference.position + _reference.forward * _currentDist;
        }

        public void Dispose()
        {
            _dragStatus?.Dispose();
        }
        
        public InteractionResultMeta Accept(DragInteraction interaction, ref InteractionCallMeta meta)
        {
            interaction.AttachInteractor(this);
            _currentInteraction = interaction;
            _inDrag = true;
            SetCurrDist(meta.Distance + _rayCastService.PlaneDist);
            interaction.OnBeginDrag(GetTargetPos());
            interaction.EnableOutline();
            _dragStatus.Execute(true);
            _targetPos = GetTargetPos();
            return StaticInteractionResultMeta.InteractedBlocked;
        }
        
        public InteractionResultMeta Accept(EnviromentProjectileInteraction environment,ref InteractionCallMeta meta) => StaticInteractionResultMeta.InteractedBlocked;
        public InteractionResultMeta Accept(EntityDamagable damagable,ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(EntityDestroyInteractable environment, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(GlobalCharacterDamageInteraction damage,ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(PlayerInputInteraction environment, ref InteractionCallMeta meta)=> StaticInteractionResultMeta.Default;
        public InteractionResultMeta Accept(EntityEffectable effectInteraction, ref InteractionCallMeta meta) => StaticInteractionResultMeta.Default;

    }
}