using AnnulusGames.LucidTools.Audio;
using Core.Entity.Characters;
using Ui.Sandbox.WorldSpaceUI;
using UnityEngine;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class ItemSupplyInteraction : PlayerInputInteraction, ICallbackListener
    {
        [SerializeField] private AudioClip _pickUpSound;
        [SerializeField] protected Vector3 Offset;
        [SerializeField] protected bool DisableUiOnUse = true;

        private WorldButtonPresenter _defaultWorldButton;
        public WorldSpaceButton CurrentUI { get; protected set; }
        public bool UsePlayerSpaceOffset;
        
        protected override void OnStart()
        {
            _defaultWorldButton = new WorldButtonPresenter(Callback);
        }

        protected override void OnPlayerExit()
        {
            DisableUI();
        }

        protected override void OnPlayerEnter()
        {
            DisableUI();
            ShowUI();
        }

        protected override void OnUpdate()
        {
            if (UsePlayerSpaceOffset && CurrentUI)
            {
                CurrentUI.Offset = GetOffset();
            }
        }
        
        private Vector3 GetOffset()
        {
            if (UsePlayerSpaceOffset)
            {
                var delta = _targetTransform.position - CameraService.CurrentBrain.transform.position;
                var rotation = Quaternion.LookRotation(delta.normalized);
                return rotation * Offset;
            }
            else
            {
                return Offset;
            }
        }

        protected virtual void ShowUI()
        {
            CurrentUI = WorldSpaceUIService.GetUI<WorldSpaceButton>(_worldButtonKey);
            _defaultWorldButton.Attach(CurrentUI.Button);
            CurrentUI.Target = _targetTransform;
            CurrentUI.Offset = GetOffset();
        }
        
        protected virtual void DisableUI()
        {
            _defaultWorldButton?.Dispose();
            if (!CurrentUI)
                return;
            
            CurrentUI.IsInactive = true;
            CurrentUI = null;
        }


        public override InteractionResultMeta Visit(IInteractorVisiter visiter, ref InteractionCallMeta meta)
        {
            return visiter.Accept(this, ref meta);
        }
        
        public override void Use(EntityContext user)
        {
            if (user is CharacterContext characterContext)
            {
                Select(characterContext);
                PlaySound();
            }
        }

        protected void PlaySound()
        {
            if (_pickUpSound)
            {
                LucidAudio
                    .PlaySE(_pickUpSound)
                    .SetVolume(0.5f);
            }
        }

        protected virtual void Select(CharacterContext characterContext) { }

        public override void Callback()
        {
            if(DisableUiOnUse)
                DisableUI();
            
            Use(PlayerSpawnService.PlayerCharacterAdapter.CurrentContext);
        }
    }

}