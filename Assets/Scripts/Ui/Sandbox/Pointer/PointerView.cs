using System;
using System.Collections.Generic;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Head;
using Core.HealthSystem;
using Core.InputSystem;
using MessagePipe;
using SharedUtils;
using UniRx;
using UnityEngine;
using VContainer.Unity;
using VContainerUi.Abstraction;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.Pointer
{
    public class PointerView : UiView
    {
        public PointerIcon Prefab;
        public Transform Container;
    }

    public class PointerController :  UiController<PointerView>, ILateTickable, IDisposable
    {
        private readonly ISubscriber<PlayerContextChangedMessage> _subscriber;
        private readonly ICameraService _cameraService;
        private readonly Dictionary<LifeEntity, (PointerIcon Icon, IDisposable Sub)> _pointers = new Dictionary<LifeEntity, (PointerIcon, IDisposable)>();
        private readonly Stack<LifeEntity> _toRemove = new Stack<LifeEntity>();
        private readonly IDisposable _disposable;

        private CharacterContext _currentContext;
        
        public PointerController(ISubscriber<PlayerContextChangedMessage> subscriber, ICameraService cameraService)
        {
            _subscriber = subscriber;
            _cameraService = cameraService;

            _disposable = _subscriber.Subscribe(OnContextChanged);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
        
        private void OnContextChanged(PlayerContextChangedMessage msg)
        {
            _currentContext = msg.CharacterContext;
        }
        
        public void Add(LifeEntity entity)
        {
            if(_pointers.ContainsKey(entity))
                return;
            
            var newPointer = Object.Instantiate(View.Prefab, View.Container);
            newPointer.OnCreate();
            _pointers.Add(entity, (newPointer, entity.Health.OnDeath.Subscribe(_ => OnEntityDeath(entity))));
        }
    
        public void Remove(LifeEntity entity)
        {
            if (_pointers.TryGetValue(entity, out var item))
            {
                item.Sub?.Dispose();
                Object.Destroy(_pointers[entity].Icon.gameObject);
                _pointers.Remove(entity);
            }
        }

        public void LateTick()
        {
            var camera = _cameraService.CurrentBrain.OutputCamera;
            if(!camera || _pointers.Count == 0 || !_currentContext)
                return;
            
            // Left, Right, Down, Up
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);

            foreach (var kpv in _pointers)
            {
                var context = kpv.Key;
                var pointerIcon = kpv.Value.Icon;

                var playerPosition = _currentContext.MainTransform.position + Vector3.up;
                
                if(context.MeshProvider == null || context.MeshProvider.MainRenderer == null)
                {
                    _toRemove.Push(context);
                    continue;
                }
                
                var targetPosition = context.MeshProvider.MainRenderer.bounds.center;

                if (!_cameraService.IsThirdPerson)
                {
                    playerPosition += camera.transform.forward * 2;
                }
                
                var toTarget = targetPosition - playerPosition;
                var ray = new Ray(playerPosition, toTarget);
                
                var rayMinDistance = Mathf.Infinity;
                var index = 0;

                for (int p = 0; p < 4; p++)
                {
                    if (!planes[p].Raycast(ray, out float distance))
                        continue;

                    if (distance < rayMinDistance)
                    {
                        rayMinDistance = distance;
                        index = p;
                    }
                }

                rayMinDistance = Mathf.Clamp(rayMinDistance, 0, toTarget.magnitude);
                
                var worldPosition = ray.GetPoint(rayMinDistance);
                var position = camera.WorldToScreenPoint(worldPosition);
                var rotation = GetIconRotation(index);

                if (toTarget.magnitude > rayMinDistance)
                    pointerIcon.Show();
                else
                    pointerIcon.Hide();

                pointerIcon.SetIconPosition(position, rotation);
            }

            while (_toRemove.Count > 0)
                Remove(_toRemove.Pop());
        }

        private void OnEntityDeath(LifeEntity entity)
        {
            Remove(entity);
        }
        
        private Quaternion GetIconRotation(int planeIndex)
        {
            if (planeIndex == 0)
                return Quaternion.Euler(0f, 0f, -180f);
            
            if (planeIndex == 1)
                return Quaternion.Euler(0f, 0f, 0);
            
            if (planeIndex == 2)
                return Quaternion.Euler(0f, 0f, -90);
            
            if (planeIndex == 3) 
                return Quaternion.Euler(0f, 0f, 90);
            
            return Quaternion.identity;
        }
    }
}