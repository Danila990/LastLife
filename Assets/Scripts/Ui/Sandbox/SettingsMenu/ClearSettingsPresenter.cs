using System;
using System.Linq;
using Adv.Services.Interfaces;
using Core.Entity;
using Core.Entity.Characters;
using Core.Entity.Characters.Adapters;
using Core.Entity.Repository;
using Dialogue.Ui.CustomViews.Options;
using Ticket;
using TMPro;
using Ui.Sandbox.WorldSpaceUI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.SettingsMenu
{
    public class ClearSettingsPresenter : IDisposable
    {
        public WorldSpaceSupplyBox ClearNpc { get; private set; }
        public WorldSpaceSupplyBox ClearProps { get; private set; }
        
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        [Inject] private readonly IEntityRepository _entityRepository;
        [Inject] private readonly IAdvService _advService;
        [Inject] private readonly ITicketService _ticketService;
        
        public ClearSettingsPresenter(SettingsParameterWidget widget, WorldSpaceSupplyBox buttonWidgetPrefab, Sprite viewClearIcon, Sprite viewCharacterIcon)
        {
            widget.SettingsNameTxt.text = "Clear Settings";
            widget.ButtonsHolder.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 185f);
            widget.HorizontalLayoutGroup.spacing = 50;
            widget.SettingsNameTxt.enabled = false;
            ClearNpc = Object.Instantiate(buttonWidgetPrefab, widget.ButtonsHolder);
            ClearProps = Object.Instantiate(buttonWidgetPrefab, widget.ButtonsHolder);

            ClearNpc.Count.text = "Clear Npc";
            ClearNpc.Count.fontSize = 36f;
            ClearNpc.Icon_holder.sprite = viewCharacterIcon;
            ClearNpc.Icon_holder.gameObject.SetActive(true);
            ClearNpc.Count.GetComponent<ParseToLayoutGroupPreferredSizeText>().enabled = false;
            ClearNpc.Icon_holder.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 120f);
            ClearNpc.Icon_holder.GetComponent<LayoutElement>().preferredWidth = 75;
            ClearNpc.Count.GetComponent<LayoutElement>().preferredWidth = 181;
            
            
            ClearProps.Count.text = "Clear Props";
            ClearProps.Count.fontSize = 36f;
            ClearProps.Icon_holder.sprite = viewClearIcon;
            ClearProps.Icon_holder.gameObject.SetActive(true);
            ClearProps.Icon_holder.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 120f);
            ClearProps.Icon_holder.GetComponent<LayoutElement>().preferredWidth = 75;

            
            ClearNpc.Button.Button
                .OnClickAsObservable()
                .SubscribeWithState2<Unit, Action, string>(OnClickClearNpc, "ClearNpc", OnClickAdv)
                .AddTo(_compositeDisposable);
            
            ClearNpc.TicketButton.Button
                .OnClickAsObservable()
                .SubscribeWithState2<Unit, Action, string>(OnClickClearNpc, "ClearNpc", OnClickTicket)
                .AddTo(_compositeDisposable);
            
            ClearProps.Button.Button
                .OnClickAsObservable()
                .SubscribeWithState2<Unit, Action, string>(OnClickClearProps, "ClearProps", OnClickAdv)
                .AddTo(_compositeDisposable);
            
            ClearProps.TicketButton.Button
                .OnClickAsObservable()
                .SubscribeWithState2<Unit, Action, string>(OnClickClearProps, "ClearProps", OnClickTicket)
                .AddTo(_compositeDisposable);
        }

        private void OnClickAdv(Unit _, Action action, string meta)
        {
            _advService.RewardRequest(action, meta);
        }
        
        private void OnClickTicket(Unit _, Action action, string meta)
        {
            _ticketService.TryUseTicket(action, meta);
        }

        private void OnClickClearProps()
        {
            var entityContexts = _entityRepository.EntityContext.Where(context => context is PhysicEntityContext).ToArray();
            foreach (var entityContext in entityContexts)
            {
                entityContext.OnDestroyed(_entityRepository);
                Object.Destroy(entityContext.gameObject);
            }
        }

        private void OnClickClearNpc()
        {
            var entityContexts = _entityRepository.EntityContext.Where(context => context is CharacterContext { Adapter: not PlayerCharacterAdapter }).ToArray();
            foreach (var entityContext in entityContexts)
            {
                entityContext.OnDestroyed(_entityRepository);
                Object.Destroy(entityContext.gameObject);
            }
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}