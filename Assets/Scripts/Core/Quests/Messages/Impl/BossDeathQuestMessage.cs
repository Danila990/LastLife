using System;
using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct BossDeathQuestMessage 
	{
		public readonly string BossId;
		public readonly string InlineId;
		public BossDeathQuestMessage(string bossId)
		{
			BossId = bossId;
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Kill", "Boss", BossId, FinalNodeIds.FINAL_NODE_ID);
		}
	}

}
