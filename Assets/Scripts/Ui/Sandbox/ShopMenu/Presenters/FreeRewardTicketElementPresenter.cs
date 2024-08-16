using Adv.Services.Interfaces;
using Core.Quests.Messages;
using Core.ResourcesSystem;
using Core.ResourcesSystem.Interfaces;
using Ui.Widget;

namespace Ui.Sandbox.ShopMenu.Presenters
{
	public class FreeRewardTicketElementPresenter
	{
		private readonly IAdvService _advService;
		private readonly IResourcesService _resourcesService;
		private readonly IQuestMessageSender _questMessageSender;

		public FreeRewardTicketElementPresenter(ShopButtonWidget shopButtonWidget, IAdvService advService, IResourcesService resourcesService, IQuestMessageSender questMessageSender)
		{
			_advService = advService;
			_resourcesService = resourcesService;
			_questMessageSender = questMessageSender;
			shopButtonWidget.Button.onClick.AddListener(OnClick);
		}
		
		private void OnClick()
		{
			_advService.RewardRequest(WatchForTicket, "TicketWatched");
		}
		
		private void WatchForTicket()
		{
			var meta = new ResourceEventMetaData(ResourceItemTypes.SHOP_ITEM_TYPE, ResourceItemIds.TICKET_WATCHED_ITEM_ID);
			_resourcesService.AddResource(ResourceType.Ticket, 1, meta);
			_questMessageSender.SendFreeTicketMessage();
		}
	}
}