using Core.Quests.Messages.Impl;
using Installer.Sandbox;
using MessagePipe;
using VContainer;

namespace Installer.Quest
{
	public class QuestMessagePipeInstaller : MessagePipeInstaller
	{
		public override void Install(IContainerBuilder builder, MessagePipeOptions options)
		{
			RegisterMessages(builder, options);
		}

		private static void RegisterMessages(IContainerBuilder builder, MessagePipeOptions options)
		{
			builder.RegisterMessageBroker<BossDeathQuestMessage>(options);
			builder.RegisterMessageBroker<TakeSupplyQuestMessage>(options);
			builder.RegisterMessageBroker<TakeEquipmentQuestMessage>(options);
			builder.RegisterMessageBroker<SpawnNpcQuestMessage>(options);
			builder.RegisterMessageBroker<SpawnPropQuestMessage>(options);
			builder.RegisterMessageBroker<UpgradeSkillQuestMessage>(options);
			builder.RegisterMessageBroker<LuckySpinQuestMessage>(options);
			builder.RegisterMessageBroker<SwitchLevelQuestMessage>(options);
			builder.RegisterMessageBroker<SkipBossQuestMessage>(options);
			builder.RegisterMessageBroker<FreeTicketQuestMessage>(options);
			builder.RegisterMessageBroker<PlantObjectQuestMessage>(options);
			builder.RegisterMessageBroker<TalkMerchantQuestMessage>(options);
			builder.RegisterMessageBroker<CarryQuestMessage>(options);
			builder.RegisterMessageBroker<ProduceQuestMessage>(options);
			builder.RegisterMessageBroker<ExchangeQuestMessage>(options);
			builder.RegisterMessageBroker<SpeedUpRefiningQuestMessage>(options);
		}
	}
}
