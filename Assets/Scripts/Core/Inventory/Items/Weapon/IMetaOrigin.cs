using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public interface IMetaOrigin
    {
        public UniTask StartAnim();
        public UniTask EndAnim();
        public Vector3 OriginPos { get; }
    }
}