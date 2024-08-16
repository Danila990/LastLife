using System;
using AnnulusGames.LucidTools.Audio;
using UniRx;
using UnityEngine;
using Utils;

namespace Core.Inventory.Items.Weapon
{
    public interface ISnapListener
    {
        void OnSnap(SnapObject snapObject);
    }
    
    public class SnapObject : MonoBehaviour
    {
        [SerializeField] private Rigidbody _targetRigidbody;
        [SerializeField] private AudioClip _detachSound;
        
        private bool _attached;
        private bool _allowAttach = true;
        private Vector3 _targetPos;
        private Quaternion _targetRot;
        private int _startLayer;

        private ReactiveCommand _onAttached;
        public IObservable<Unit> OnAttached => _onAttached??= new ReactiveCommand();
        
        private void Start()
            => _startLayer = _targetRigidbody.gameObject.layer;

        private void OnDestroy()
            => _onAttached?.Dispose();

        protected void Update()
        {
            if(!_attached) return;
            transform.SetLocalPositionAndRotation(_targetPos,_targetRot);
        }

        public void Detach()
        {
            _allowAttach = false;
            _targetRigidbody.gameObject.layer = _startLayer;
            _targetRigidbody.isKinematic = false;
            if (_detachSound && _attached)
            {
                LucidAudio.PlaySE(_detachSound).SetPosition(transform.position).SetSpatialBlend(1f);
            }
            _attached = false;
        }

        public void AllowAttach()
        {
            _allowAttach = true;
            _targetRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if(!_allowAttach) return;
            if(_attached) return;
            if(!Layers.ContainsLayer(LayerMasks.C4Layers,collision.collider.gameObject.layer)) return;
            _attached = true;
            _onAttached?.Execute();
            _targetRigidbody.isKinematic = true;
            gameObject.layer = Layers.RayCastOnlyLayer;

            if (collision.collider.TryGetComponent(out ISnapListener snapListener))
            {
                snapListener.OnSnap(this);
            }
            
            transform.SetParent(collision.collider.transform);
            transform.SetPositionAndRotation(collision.contacts[0].point,Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal));
            _targetPos = transform.localPosition;
            _targetRot = transform.localRotation;
        }
    }
}