using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct SkipBossQuestMessage
	{
		public readonly string InlineId;

		public SkipBossQuestMessage(object _ = null)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Skip", "Boss", FinalNodeIds.FINAL_NODE_ID);
		}
	}
}
