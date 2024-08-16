using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct SwitchLevelQuestMessage
	{
		public readonly string InlineId;
		public readonly string LevelId;

		public SwitchLevelQuestMessage(string levelId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Goto", levelId, FinalNodeIds.FINAL_NODE_ID);
			LevelId = levelId;
		}
	}

}
