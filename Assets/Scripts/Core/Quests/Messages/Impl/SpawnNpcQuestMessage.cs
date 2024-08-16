using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct SpawnNpcQuestMessage
	{
		public readonly string InlineId;
		public readonly string FactoryId;

		public SpawnNpcQuestMessage(string factoryId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Spawn", "Npc", factoryId, FinalNodeIds.FINAL_NODE_ID);
			FactoryId = factoryId;
		}
	}

}
