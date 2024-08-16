using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Core.Factory
{
    public class RigController : MonoBehaviour
    {
        public Transform AimTarget;
        [SerializeField] private Rig[] _rigs;

        private float[] _baseWeight;

        private void Awake()
        {
            _baseWeight = new float[_rigs.Length];
            for (var i = 0; i < _rigs.Length; i++)
            {
                _baseWeight[i] = _rigs[i].weight;
            }
        }

        public void EnableRig()
        {
            AwaitEndFrame().Forget();
        }

        private async UniTaskVoid AwaitEndFrame()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            while (Math.Abs(_rigs[0].weight - _baseWeight[0]) > 0.01f)
            {
                await UniTask.WaitForEndOfFrame(this, ct);
                for (var i = 0; i < _rigs.Length; i++)
                {
                    _rigs[i].weight = _baseWeight[i];
                }
            }
        }
        
        private async UniTaskVoid AwaitEndFrameDisable()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            while (_rigs[0].weight != 0)
            {
                await UniTask.WaitForEndOfFrame(this, ct);
                foreach (var t in _rigs)
                {
                    t.weight = 0;
                }
            }
        }
        
        public void DisableRig()
        {
            AwaitEndFrameDisable().Forget();
        }

    }
}