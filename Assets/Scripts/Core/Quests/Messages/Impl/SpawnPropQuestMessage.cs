using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct SpawnPropQuestMessage
	{
		public readonly string InlineId;
		public readonly string FactoryId;

		public SpawnPropQuestMessage(string factoryId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Spawn", "Prop", factoryId, FinalNodeIds.FINAL_NODE_ID);
			FactoryId = factoryId;
		}
	}
}
