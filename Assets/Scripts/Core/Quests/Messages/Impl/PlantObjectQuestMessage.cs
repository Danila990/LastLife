using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct PlantObjectQuestMessage
	{
		public readonly string InlineId;
		public readonly string FactoryId;

		public PlantObjectQuestMessage(string factoryId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Plant", factoryId, FinalNodeIds.FINAL_NODE_ID);
			FactoryId = factoryId;
		}
	}
}
