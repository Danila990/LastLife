using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using uPools;
using VFX;

namespace Db.VFXDataDto.Impl
{

    public class VFXContext : SerializedMonoBehaviour, IPooled<VFXContext>
    {
        [OdinSerialize]
        public List<IVfxComponent> Components;
        public IObjectPool<VFXContext> ToReturn { get; set; }
        public ParticleSystem ParticleSystem;
        public bool Clean;
        
        private void Start()
        {
            OnStart();
        }

        protected virtual void OnStart(){}

        public virtual void Release()
        {
            if (Components != null)
            {
                foreach (var component in Components)
                    component.OnRelease();
            }
            Stop();
        }
        
        public virtual void Attach(Vector3 pos, Vector3 normal, Transform parent)
        {
            var rot = Quaternion.LookRotation(normal);
            Attach(pos, rot, parent);
        }
        
        public virtual void Attach(Vector3 pos, Quaternion rot, Transform parent)
        {
            //ParticleSystem.transform.SetParent(parent);
            SetPosAndRotation(pos, rot);
        }

        public void SetPosAndRotation(Vector3 pos, Quaternion rot)
        {
            ParticleSystem.transform.SetPositionAndRotation(pos,rot);
        }
        
        public virtual void Play()
        {
            ParticleSystem.Play();
        }
        
        public virtual void Stop()
        {
            if (Clean)
            {
                ParticleSystem.Clear(true);
            }
            ParticleSystem.Stop();
        }
#if UNITY_EDITOR
        [OnInspectorInit]
        private void SetUpRefs()
        {
            if (!ParticleSystem)
            {
                ParticleSystem = GetComponent<ParticleSystem>();
                UnityEditor.EditorUtility.SetDirty(this);   
            }
        }
#endif

    }
}