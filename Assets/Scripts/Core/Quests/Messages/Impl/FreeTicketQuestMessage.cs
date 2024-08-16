using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct FreeTicketQuestMessage
	{
		public readonly string InlineId;

		public FreeTicketQuestMessage(object _ = null)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Take", "FreeTicket", FinalNodeIds.FINAL_NODE_ID);
		}
	}

}
