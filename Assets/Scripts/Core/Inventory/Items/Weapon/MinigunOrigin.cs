using Cysharp.Threading.Tasks;
using DG.Tweening;
using SharedUtils;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public class MinigunOrigin : MonoBehaviour, IMetaOrigin
    {
        [SerializeField] private Transform _minigunOrigin;
        [SerializeField] private float _rotationAngle = 90f;
        [SerializeField] private float _rotationDuration = 0.1f;

        public async UniTask StartAnim()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            _minigunOrigin.DOLocalRotate(new Vector3(0, 90, 0), _rotationDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear);
            await UniTask.Delay(_rotationDuration.ToSec(), cancellationToken: ct);
        }

        public async UniTask EndAnim()
        {

        }

        public Vector3 OriginPos => _minigunOrigin.position;

    }
}