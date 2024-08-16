using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct LuckySpinQuestMessage
	{
		public readonly string InlineId;
		public readonly string CharacterId;

		public LuckySpinQuestMessage(string characterId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Take", "LuckySpin", characterId, FinalNodeIds.FINAL_NODE_ID);
			CharacterId = characterId;
		}
	}
}
