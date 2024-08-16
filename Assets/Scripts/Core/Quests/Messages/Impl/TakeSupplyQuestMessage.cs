using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct TakeSupplyQuestMessage
	{
		public readonly string InlineId;
		public readonly string SupplyId;
		public readonly int SupplyCount;

		public TakeSupplyQuestMessage(string supplyId, int supplyCount)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Take", "Supply", supplyId, FinalNodeIds.FINAL_NODE_ID);
			SupplyId = supplyId;
			SupplyCount = supplyCount;
		}
	}

}
