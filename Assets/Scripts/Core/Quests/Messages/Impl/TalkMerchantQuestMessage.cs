using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct TalkMerchantQuestMessage
	{
		public readonly string InlineId;
		public readonly string MerchantId;

		public TalkMerchantQuestMessage(string merchantId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Talk", merchantId, FinalNodeIds.FINAL_NODE_ID);
			MerchantId = merchantId;
		}
	}
	
	public readonly struct ProduceQuestMessage
	{
		public readonly string InlineId;
		public readonly string ProducedObjId;
		public readonly int Value;

		public ProduceQuestMessage(string producedObjId, int value)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Produce", producedObjId, FinalNodeIds.FINAL_NODE_ID);
			ProducedObjId = producedObjId;
			Value = value;
		}
	}
	
	public readonly struct ExchangeQuestMessage
	{
		public readonly string InlineId;
		public readonly string FromId;
		public readonly string ForId;

		public ExchangeQuestMessage(string fromId, string forId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Exchange", fromId, forId, FinalNodeIds.FINAL_NODE_ID);
			FromId = fromId;
			ForId = forId;
		}
	}
	
	public readonly struct SpeedUpRefiningQuestMessage
	{
		public readonly string InlineId;
		public readonly string ProducedObjId;

		public SpeedUpRefiningQuestMessage(string producedObjId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "SpeedUp", "Refiner", producedObjId, FinalNodeIds.FINAL_NODE_ID);
			ProducedObjId = producedObjId;
		}
	}
	
	public readonly struct CarryQuestMessage
	{
		public readonly string InlineId;
		public readonly string CarriedObjId;

		public CarryQuestMessage(string carriedObjId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Take", carriedObjId, FinalNodeIds.FINAL_NODE_ID);
			CarriedObjId = carriedObjId;
		}
	}
}
