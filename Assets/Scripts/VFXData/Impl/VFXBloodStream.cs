using Db.VFXDataDto.Impl;
using UnityEngine;

namespace VFXData.Impl
{
    public class VFXBloodStream : VFXContext
    {
        [SerializeField] private ParticleSystem _emissionSource;
        private Vector3 _initSize;
        
        protected override void OnStart()
        {
            _initSize = transform.localScale;
        }
        
        public override void Stop()
        {
            transform.localScale = _initSize;
            var emmision = _emissionSource.emission;
            emmision.enabled = false;
            emmision = ParticleSystem.emission;
            emmision.enabled = true;
            base.Stop();
        }

        public override void Play()
        {
            transform.localScale = _initSize;
            var emmision = _emissionSource.emission;
            emmision.enabled = true;
            emmision = ParticleSystem.emission;
            emmision.enabled = true;
            base.Play();
        }

        public override void Attach(Vector3 pos, Vector3 normal, Transform parent)
        {
            transform.SetParent(parent,true);
            base.Attach(pos + normal * 0.05f, normal, parent);
        }
    }
}