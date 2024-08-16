using Adv.Services.Interfaces;
using Core.Entity.Characters;
using Core.Quests.Messages;
using Core.Quests.Messages.Impl;
using Db.ObjectData.Impl;
using GameSettings;
using MessagePipe;
using Ticket;
using Ui.Sandbox.WorldSpaceUI;
using UnityEngine;
using Utils.Constants;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
    public class ContainerItemSupplyInteraction : ItemSupplyInteraction
    {
        [SerializeField] protected int _quantity;

        public InventoryObjectDataSo ObjectToGive;
        public Animator Animator;
        
        [SerializeField] private Sprite _uiIcon;

        [Inject] private readonly IAdvService _advService;
        [Inject] private readonly ITicketService _ticketService;
        [Inject] private readonly ISettingsService _settingsService;
        [Inject] private readonly IQuestMessageSender _questMessageSender;

        private WorldSpaceSupplyBox _currentButton;
        private AdvWorldButtonPresenter _advPresenter;
        private TicketWorldButtonPresenter _ticketPresenter;

        protected override void OnStart()
        {
            _advPresenter = new AdvWorldButtonPresenter(_advService, Callback, "Take supply:" + ObjectToGive.ObjectData.Name);
            _ticketPresenter = new TicketWorldButtonPresenter(_ticketService, Callback,"Take supply:" + ObjectToGive.ObjectData.Name);
        }

        protected override void OnUpdate()
        {
            var isPlayerNearby = CurrPlayerSqrDist <= InteractDistance * InteractDistance;
            Animator.SetBool(AHash.Used, isPlayerNearby);
            
            if(!_currentButton) return;
            var cameraTransform = CameraService.CurrentBrain.OutputCamera.transform;
            var cameraDelta = cameraTransform.position - transform.position;
            CurrCamDist = cameraDelta.magnitude;
            _currentButton.transform.localScale = Vector3.one * 5 / Mathf.Max(1,CurrCamDist);
            _currentButton.gameObject.SetActive(isPlayerNearby);
        }

        protected override void OnPlayerExit()
        {
            //Animator.SetBool(AHash.Used,false);
            DisableUI();
        }
        
        protected override void OnPlayerEnter()
        {
            //Animator.SetBool(AHash.Used,true);
            DisableUI();
            ShowUI();
        }

        protected override void ShowUI()
        {
            _currentButton = WorldSpaceUIService.GetUI<WorldSpaceSupplyBox>(_worldButtonKey);
            _advPresenter.Attach(_currentButton.Button);
            _currentButton.Count.text = "x" + _quantity;
            /*if (_settingsService.GetValue<bool>(SettingsConsts.ALT_INTERACTION, GameSetting.ParameterType.Bool))
            {
                _currentButton.Count.text = $"TAKE x{_quantity}";
            }*/
            _currentButton.Icon_holder.sprite = _uiIcon;
            _ticketPresenter.Attach(_currentButton.TicketButton);
            _currentButton.Target = transform;
            _currentButton.Offset = Vector3.up * 1.5f;
        }
        
        protected override void DisableUI()
        {
            if (!_currentButton) 
                return;
            
            _currentButton.IsInactive = true;
            _advPresenter?.Dispose();
            _ticketPresenter?.Dispose();
            _currentButton = null;
        }

        protected override void OnDisposed()
        {
            _advPresenter?.Dispose();
            _ticketPresenter?.Dispose();
        }
        
        public override void Callback()
        {
            Use(PlayerSpawnService.PlayerCharacterAdapter.CurrentContext);
        }
        
        protected override void Select(CharacterContext characterContext)
        {
            var data = ObjectToGive.Model;
            characterContext.Inventory.AddItem(data, _quantity);
            characterContext.Inventory.TrySelectItem(data.Id);
            _questMessageSender.SendTakeSupplyMessage(ObjectToGive.Model.Id, _quantity);
        }
    }
}