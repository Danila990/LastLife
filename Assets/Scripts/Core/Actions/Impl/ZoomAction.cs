using AnnulusGames.LucidTools.Audio;
using Core.CameraSystem;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Inventory.Items.Weapon;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;

namespace Core.Actions.Impl
{
    [CreateAssetMenu(menuName = SoNames.ACTION_DATA + nameof(ZoomAction), fileName = nameof(ZoomAction))]
    public class ZoomAction : GenericEntityAction<ProjectileWeaponContext>, IUnlockableAction
    {
        [SerializeField] private AudioClip _zoomClip;
        
        [Inject] private readonly ICameraService _cameraService;
        private CharacterContext _characterContext;
        private AimState _prevState;
        public IReactiveProperty<bool> IsUnlocked { get; set; }

        public override void OnDeselect()
        {
        }

        public override void OnInput(bool state)
        {
        }

        public override void OnInputUp()
        {
        }

        public override void OnInputDown()
        {
            
            if (_zoomClip)
            {
                LucidAudio
                    .PlaySE(_zoomClip)
                    .SetPosition(CurrentContext.GetOrigin().position)
                    .SetVolume(0.1f)
                    .SetSpatialBlend(1);
            }
            
            if (_characterContext.CurrentAdapter is not PlayerCharacterAdapter adapter) return;
            if (adapter.AimController.CurrState == AimState.Sniper)
            {
                adapter.SetAimState(_prevState);
                return;
            }
            _prevState = adapter.AimController.CurrState;
            adapter.SetAimState(AimState.Sniper);
        }
        
        public override void SetContext(EntityContext context)
        {
            base.SetContext(context);
            _characterContext = CurrentContext.Owner as CharacterContext;
        }
    }
}