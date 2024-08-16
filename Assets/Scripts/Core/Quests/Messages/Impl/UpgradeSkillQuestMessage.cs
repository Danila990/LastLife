using Core.Quests.Tree;
using Db.Quests;

namespace Core.Quests.Messages.Impl
{
	public readonly struct UpgradeSkillQuestMessage
	{
		public readonly string InlineId;
		public readonly string SkillId;

		public UpgradeSkillQuestMessage(string skillId)
		{
			InlineId = TreeUtils.CreateInlineId(QuestsTree.ROOT_ID, "Upgrade", "Skill", skillId, FinalNodeIds.FINAL_NODE_ID);
			SkillId = skillId;
		}
	}

}
