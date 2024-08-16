using System.Threading;
using Core.Quests.Tree;
using Cysharp.Threading.Tasks;
using Db.Quests;
using Dialogue.Services.Modules.MerchantShop;
using MessagePipe;
using UnityEngine;
using Utils;

namespace Core.Quests.Messages.Handler
{
	public class PurchaseMessageHandler : IQuestMessageHandler<MessageObjectLocalPurchase>
	{
		private readonly IQuestService _questService;
		private readonly ISubscriber<MessageObjectLocalPurchase> _purchaseSubscriber;
		private CancellationToken _token;


		public PurchaseMessageHandler(
			IQuestService questService,
			ISubscriber<MessageObjectLocalPurchase> purchaseSubscriber,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_questService = questService;
			_purchaseSubscriber = purchaseSubscriber;
			_token = installerCancellationToken.Token;
		}
		
		public void PostInitialize()
		{
			_purchaseSubscriber.Subscribe(OnPurchased).AddTo(_token);
		}
		
		private void OnPurchased(MessageObjectLocalPurchase msg)
			=> Handle(msg);

		public void Handle(MessageObjectLocalPurchase msg)
		{
			if (msg.BoughtItemId.EndsWith("BOOST"))
			{
				var inlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Buy", "Boost", FinalNodeIds.FINAL_NODE_ID);
				Handle(inlineId, msg.Quantity);
			}
		}

		private void Handle(string inlineId, int value)
		{
			foreach (var tree in _questService.Trees.Values)
			{
				var node = tree.GetFinalNode(inlineId);
				node?.SetValue(value);
			}
		}
		
		public void Dispose()
		{
		}
	}
}
