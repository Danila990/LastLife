using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct TakeEquipmentQuestMessage
	{
		public readonly string InlineId;
		public readonly string EquipmentId;

		public TakeEquipmentQuestMessage(string equipmentId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Take", "Equipment", equipmentId, FinalNodeIds.FINAL_NODE_ID);
			EquipmentId = equipmentId;
		}
	}

}
