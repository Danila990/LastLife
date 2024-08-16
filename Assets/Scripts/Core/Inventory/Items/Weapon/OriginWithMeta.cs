using Cysharp.Threading.Tasks;
using DG.Tweening;
using SharedUtils;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public class OriginWithMeta : MonoBehaviour, IMetaOrigin
    {
        public Transform AnimatePart;
        public Transform Bullet;
        public async UniTask StartAnim()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            AnimatePart.DOLocalRotate(new Vector3(-28, 0, 0),0.3f);
            await UniTask.Delay(0.2f.ToSec(), cancellationToken: ct);
            Bullet.gameObject.SetActive(false);
        }

        public async UniTask EndAnim()
        {
            var ct = this.GetCancellationTokenOnDestroy();
            await UniTask.Delay(1f.ToSec(), cancellationToken: ct);
            AnimatePart.DOLocalRotate(new Vector3(-103, 0, 0),0.3f);
            await UniTask.Delay(0.5f.ToSec(), cancellationToken: ct);
            Bullet.gameObject.SetActive(true);
        }

        public Vector3 OriginPos => Bullet.position;
    }
}