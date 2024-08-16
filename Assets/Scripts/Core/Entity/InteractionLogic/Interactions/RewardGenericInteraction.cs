using Adv.Services.Interfaces;
using Core.Entity.Characters;
using Ticket;
using Ui.Sandbox.WorldSpaceUI;
using UnityEngine;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class RewardGenericInteraction : GenericInteraction
	{
		[SerializeField] private string _meta;
		
		
		private AdvWorldButtonPresenter _advWorldButtonPresenter;
		private TicketWorldButtonPresenter _ticketWorldButtonPresenter;

		[Inject]
		private void Construct(IAdvService advService, ITicketService ticketService)
		{
			_advWorldButtonPresenter = new AdvWorldButtonPresenter(advService, Callback, _meta);
			_ticketWorldButtonPresenter = new TicketWorldButtonPresenter(ticketService, Callback, _meta);
		}
		
		public void SetMeta(string meta)
		{
			_meta = meta;
		}

		protected override void ShowUI()
		{
			CurrentUI = WorldSpaceUIService.GetUI<WorldSpaceSupplyBox>(_worldButtonKey);
			_advWorldButtonPresenter.Attach(CurrentUI.Button);
			_ticketWorldButtonPresenter.Attach(CurrentUI.TicketButton);
			CurrentUI.Target = _targetTransform;
			CurrentUI.Offset = Offset;
		}
        
		protected override void DisableUI()
		{
			if (!CurrentUI)
				return;
            
			_advWorldButtonPresenter?.Dispose();
			_ticketWorldButtonPresenter?.Dispose();
			CurrentUI.IsInactive = true;
			CurrentUI = null;
		}

		protected override void Select(CharacterContext characterContext)
		{
			OnUsed?.Execute(characterContext);
		}
	}

}
