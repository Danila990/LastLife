using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Inventory.Items.Weapon
{
    public class SteelFanOrigin : MonoBehaviour, IMetaOrigin
    {
        [SerializeField] private GameObject _fan;
        
        public async UniTask StartAnim()
        {
            _fan.SetActive(false);
            await UniTask.Delay(100);
        }

        public async UniTask EndAnim()
        {
            _fan.SetActive(true);
        }

        public Vector3 OriginPos => transform.position;
    }
}