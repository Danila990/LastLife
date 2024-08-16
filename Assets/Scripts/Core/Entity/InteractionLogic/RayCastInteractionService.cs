using System;
using System.Buffers;
using Core.CameraSystem;
using UniRx;
using UnityEngine;
using Utils;
using VContainer.Unity;
using CameraType = Core.CameraSystem.CameraType;

namespace Core.Entity.InteractionLogic
{
    public interface IRayCastService
    { 
        ref RaycastHit CurrentHit { get; }
        ref RaycastHit SphereHitCash { get; }
        ref Vector3 CurrentHitPoint { get; }
        public ref Vector3 RayPosition { get; }
        public ref Vector3 RayDir { get; }
        float PlaneDist { get; }
        Vector3 CurrentNormal { get; set; }
        void RayInteract(IInteractorVisiter visiter);
        void SphereCastRayInteract(float radius, bool onlyUniq, IInteractorVisiter visiter,  float distance, uint selfUid);
    }

    public class RayCastInteractionService : IRayCastService, IStartable, ILateTickable, IDisposable
    {
        private const float RAY_LENGTH = 100f;
        private readonly ICameraService _cameraService;
        private readonly CompositeDisposable _compositeDisposable = new();
        private Transform _cameraTransform;
        private RaycastHit _currentHit;
        private RaycastHit _sphereHitCash;
        private Vector3 _hitPoint;
        private Vector3 _rayPosition;
        private Vector3 _rayDir;
        private bool _cancel;
        
        public ref RaycastHit CurrentHit => ref _currentHit;
        public ref RaycastHit SphereHitCash => ref _sphereHitCash;
       
        public ref Vector3 CurrentHitPoint => ref _hitPoint;
        public ref Vector3 RayPosition => ref _rayPosition;
        public ref Vector3 RayDir => ref _rayDir;
        public float PlaneDist { get; private set; } = 1;
        public Vector3 CurrentNormal { get; set; }

        public RayCastInteractionService(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }
        
        public void Start()
        {
            _cameraTransform = _cameraService.CurrentBrain.OutputCamera.transform;
        }

        public void LateTick()
        {
            UpdateRay();
            if (_cameraService.CurrentCameraController.CameraTargetEntity is null || !_cameraService.CurrentCameraController.CameraTargetEntity.TargetIsActive)
                return;
            
            UpdatePlaneTpv();
            UpdatePlaneFpv();
        }
        
        private void UpdatePlaneFpv()
        {
            if (_cameraService.IsThirdPerson)
                return;
            
            PlaneDist = 0.5f;
        }

        private void UpdatePlaneTpv()
        {
            if (!_cameraService.IsThirdPerson)
                return;
            
            var delta = _cameraService.CurrentCameraController.CameraTargetEntity.CameraTargetRoot.position - _cameraTransform.position;
            var dist = Vector3.Dot(delta, _cameraTransform.forward);
            PlaneDist = dist + 0.5f;
        }

        private void UpdateRay()
        {
            if (_cameraService.CurrentCameraController.CameraType == CameraType.Cutscene)
                return;
            
            var forward = _cameraTransform.forward;
            _rayPosition = _cameraTransform.position + forward * PlaneDist;
            _rayDir = forward;
            //Debug.DrawLine(_rayPosition,_rayPosition + forward * RAY_LENGTH, Color.blue);
           
            var ray = new Ray(_rayPosition, forward);

            if (Physics.Raycast(ray, out _currentHit, RAY_LENGTH, LayerMasks.InteractionMask))
            {
                _hitPoint = _currentHit.point;
                CurrentNormal = _currentHit.normal;
                //Debug.DrawLine(_rayPosition, _hitPoint, Color.red);
            }
            else
            {
                _hitPoint = _cameraTransform.position + _cameraTransform.forward * RAY_LENGTH;
                CurrentNormal = Vector3.up;
                _currentHit = new RaycastHit();
            }
        }
        
        public void RayInteract(IInteractorVisiter visiter)
        {
            InternalRayInteract(ref CurrentHit, visiter);
        }

        public void SphereCastRayInteract(float radius, bool onlyUniq, IInteractorVisiter visiter,float distance,uint selfUid = 0)
        {
            var forward = _cameraTransform.forward;
            var rayPosition = _cameraTransform.position + forward * PlaneDist;
            var ray = new Ray(rayPosition, forward);
            var pool = ArrayPool<RaycastHit>.Shared.Rent(20);
            VisiterUtils.SphereCastVisit(selfUid, ray, radius, ref pool, distance, visiter);
            ArrayPool<RaycastHit>.Shared.Return(pool);
        }

        private static void InternalRayInteract(ref RaycastHit hit, IInteractorVisiter visiter)
        {
            if (hit.transform)
                VisiterUtils.RayVisit(hit, visiter);
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }

        public void SetRayHit(RaycastHit hit)
        {
            SphereHitCash = hit;
        }
    }
}