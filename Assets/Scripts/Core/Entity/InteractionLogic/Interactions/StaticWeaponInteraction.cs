using Adv.Services.Interfaces;
using AnnulusGames.LucidTools.Audio;
using Core.AnimationRigging;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using GameSettings;
using Sirenix.OdinInspector;
using Ticket;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class StaticWeaponInteraction : PlayerInputInteraction
    {
        [TitleGroup("Meta")] 
        [SerializeField] private string _metaData;
        [SerializeField] private int _quantity;
        [SerializeField, PreviewField] private Sprite _uiIcon;

        [PropertySpace(15)]
        [SerializeField] private AudioClip _pickUpSound;
        [SerializeField] protected Transform _uiOrigin;
        [SerializeField] protected StaticItemContext _item;
        [SerializeField] private RigData _rigData;
        [SerializeField] private Transform _rigTarget;
        
        
        [Inject] protected readonly IObjectResolver ObjectResolver;
        [Inject] private readonly IAdvService _advService;
        [Inject] private readonly ITicketService _ticketService;
        [Inject] private readonly ISettingsService _settingsService;

        protected WorldSpaceButton CurrentUI;
        private PlayerCharacterAdapter _attachedAdapter;
        private RigElementController _rigController;
        private TicketWorldButtonPresenter _ticketPresenter;
        private AdvWorldButtonPresenter _advPresenter;

        public StaticItemContext StaticItemContext => _item;
        protected Transform RigTarget => _rigTarget;
        protected PlayerCharacterAdapter AttachedAdapter => _attachedAdapter;

        protected override void OnStart()
        {
            _rigController = new RigElementController(_rigData, _rigTarget);
            if (_awaitPlayer)
            {
                AwaitPlayer(destroyCancellationToken).Forget();
            }
            _ticketPresenter = new TicketWorldButtonPresenter(_ticketService, RefillCallback,"Interaction:" + _metaData);
            _advPresenter = new AdvWorldButtonPresenter(_advService, RefillCallback, "Interaction:" + _metaData);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _rigController?.Dispose();
        }

        protected virtual void OnUpdateOnActive(){}
        protected override void OnUpdate()
        {
            if (!_attachedAdapter)
                return;
            _item.TickOnActive();
            OnUpdateOnActive();
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

        protected virtual void ShowUI()
        {
            CurrentUI = WorldSpaceUIService.GetUI<WorldSpaceButton>(_worldButtonKey);
            _advPresenter.Attach(CurrentUI.Button);
            var supplyButton = CurrentUI as WorldSpaceSupplyBox;
            if (supplyButton != null)
            {
                supplyButton.Count.text = "x" + _quantity;
                supplyButton.Icon_holder.sprite = _uiIcon;
            }
            /*if (_settingsService.GetValue<bool>(SettingsConsts.ALT_INTERACTION, GameSetting.ParameterType.Bool))
            {
                supplyButton.Count.text = $"TAKE x{_quantity}";
            }*/
            _ticketPresenter.Attach(CurrentUI.TicketButton);
            CurrentUI.Target = _uiOrigin;
            CurrentUI.Offset = Vector3.zero;
        }
        
        private void DisableUI()
        {
            if (!CurrentUI)
                return;
            
            _advPresenter.Dispose();
            _ticketPresenter.Dispose();
            
            CurrentUI.IsInactive = true;
            CurrentUI = null;
            OnDisableUI();
        }

        protected virtual void OnDisableUI() { }

        public override void Use(EntityContext user)
        {
            if (user is IControllableEntity { Adapter: PlayerCharacterAdapter playerCharacterAdapter })
            {
                AttachTo(playerCharacterAdapter);
                PlaySound();
            }
        }
        
        private void AttachTo(PlayerCharacterAdapter playerCharacterAdapter)
        {
            _attachedAdapter = playerCharacterAdapter;
            StaticItemContext.Attach(playerCharacterAdapter);
            playerCharacterAdapter.ContextChanged.TakeUntilDestroy(_attachedAdapter).Subscribe(ContextChanged);
            playerCharacterAdapter.Attach(this);
            playerCharacterAdapter.AimController.SetAimState(AimState.Default);
            OnAttached();
        }
        
        private void ContextChanged(CharacterContext obj)
        {
            Detach();
        }

        protected virtual void OnAttached() {}

        public void Detach()
        {
            if (_attachedAdapter)
                _attachedAdapter.Detach();
            
            OnDetached();
            _attachedAdapter = null;
        }
        
        protected virtual void OnDetached() {}

        protected void PlaySound()
        {
            if (_pickUpSound)
            {
                LucidAudio
                    .PlaySE(_pickUpSound)
                    .SetPosition(transform.position)
                    .SetSpatialBlend(1);
            }
        }

        public override void Callback()
        {
            DisableUI();
            Use(PlayerSpawnService.PlayerCharacterAdapter.CurrentContext);
        }

        public void RefillCallback()
        {
            _item.Refill(_quantity);
            Callback();
        }
    }
}