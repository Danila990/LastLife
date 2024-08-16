using Core.Quests.Messages.Impl;
using Db.ObjectData;
using MessagePipe;

namespace Core.Quests.Messages
{
	public interface IQuestMessageSender
	{
		public void SendBossDeathMessage(string bossId);
		public void SendTakeSupplyMessage(string supplyId, int quantity);
		public void SendTakeEquipmentMessage(string supplyId);
		public void SendSpawnMessage(ObjectData data);
		public void SendUpgradeSkillMessage(string skillId);
		public void SendLuckySpinMessage(string characterId);
		public void SendPlaceObjectMessage(string factoryId);
		public void SendSkipBossMessage();
		public void SendFreeTicketMessage();
		public void SendTalkMerchantMessage(string merchantId);
		public void SendProduceMessage(string producedObjId, int value);
		public void SendExchangeMessage(string fromId, string forId);
		public void SendSpeedUpRefiningMessage(string producedObjId);
		public void SendCarryMessage(string carriedObjId);

	}
	
	public class QuestMessageSender : IQuestMessageSender
	{
		#region Fields
		
		private readonly IPublisher<BossDeathQuestMessage> _bossDeathPublisher;
		private readonly IPublisher<TakeSupplyQuestMessage> _takeSupplyPublisher;
		private readonly IPublisher<TakeEquipmentQuestMessage> _takeEquipmentPublisher;
		private readonly IPublisher<SpawnNpcQuestMessage> _spawnNpsPublisher;
		private readonly IPublisher<SpawnPropQuestMessage> _spawnPropPublisher;
		private readonly IPublisher<UpgradeSkillQuestMessage> _upgradeSkillPublisher;
		private readonly IPublisher<LuckySpinQuestMessage> _luckySpinPublisher;
		private readonly IPublisher<SkipBossQuestMessage> _skipBossPublisher;
		private readonly IPublisher<FreeTicketQuestMessage> _freeTicketPublisher;
		private readonly IPublisher<PlantObjectQuestMessage> _placeObjectPublisher;
		private readonly IPublisher<TalkMerchantQuestMessage> _talkMerchantPublisher;
		private readonly IPublisher<ProduceQuestMessage> _producePublisher;
		private readonly IPublisher<ExchangeQuestMessage> _exchangePublisher;
		private readonly IPublisher<SpeedUpRefiningQuestMessage> _speedUpRefiningPublisher;
		private readonly IPublisher<CarryQuestMessage> _carryPublisher;
		
		#endregion

		#region Ctor
		
		public QuestMessageSender(
			IPublisher<BossDeathQuestMessage> bossDeathPublisher,
			IPublisher<TakeSupplyQuestMessage> takeSupplyPublisher,
			IPublisher<TakeEquipmentQuestMessage> takeEquipmentPublisher,
			IPublisher<SpawnNpcQuestMessage> spawnNpsPublisher,
			IPublisher<SpawnPropQuestMessage> spawnPropPublisher,
			IPublisher<UpgradeSkillQuestMessage> upgradeSkillPublisher,
			IPublisher<LuckySpinQuestMessage> luckySpinPublisher,
			IPublisher<SkipBossQuestMessage> skipBossPublisher,
			IPublisher<FreeTicketQuestMessage> freeTicketPublisher,
			IPublisher<TalkMerchantQuestMessage> talkMerchantPublisher,
			IPublisher<PlantObjectQuestMessage> placeObjectPublisher,
			IPublisher<ProduceQuestMessage> producePublisher,
			IPublisher<ExchangeQuestMessage> exchangePublisher,
			IPublisher<SpeedUpRefiningQuestMessage> speedUpRefiningPublisher,
			IPublisher<CarryQuestMessage> carryPublisher
			)
		{
			_bossDeathPublisher = bossDeathPublisher;
			_takeSupplyPublisher = takeSupplyPublisher;
			_takeEquipmentPublisher = takeEquipmentPublisher;
			_spawnNpsPublisher = spawnNpsPublisher;
			_spawnPropPublisher = spawnPropPublisher;
			_upgradeSkillPublisher = upgradeSkillPublisher;
			_luckySpinPublisher = luckySpinPublisher;
			_skipBossPublisher = skipBossPublisher;
			_freeTicketPublisher = freeTicketPublisher;
			_placeObjectPublisher = placeObjectPublisher;
			_talkMerchantPublisher = talkMerchantPublisher;
			_producePublisher = producePublisher;
			_exchangePublisher = exchangePublisher;
			_speedUpRefiningPublisher = speedUpRefiningPublisher;
			_carryPublisher = carryPublisher;
		}
		
		#endregion

		public void SendBossDeathMessage(string bossId)
			=> _bossDeathPublisher.Publish(new BossDeathQuestMessage(bossId));

		public void SendTakeSupplyMessage(string supplyId, int quantity)
		 => _takeSupplyPublisher.Publish(new TakeSupplyQuestMessage(supplyId, quantity));
		
		public void SendPlaceObjectMessage(string factoryId)
		 => _placeObjectPublisher.Publish(new PlantObjectQuestMessage(factoryId));

		public void SendTakeEquipmentMessage(string supplyId)
			=> _takeEquipmentPublisher.Publish(new TakeEquipmentQuestMessage(supplyId));

		public void SendSpawnMessage(ObjectData data)
		{
			if (data is CharacterObjectData)
			{
				SendSpawnNpcMessage(null);
				return;
			}

			SendSpawnPropMessage(data.Id);
		}
		
		private void SendSpawnPropMessage(string id)
			=> _spawnPropPublisher.Publish(new SpawnPropQuestMessage(id));
		
		private void SendSpawnNpcMessage(string id)
			=> _spawnNpsPublisher.Publish(new SpawnNpcQuestMessage(id));
		
		public void SendUpgradeSkillMessage(string skillId)
			=> _upgradeSkillPublisher.Publish(new UpgradeSkillQuestMessage(skillId));
		
		public void SendLuckySpinMessage(string characterId)
			=> _luckySpinPublisher.Publish(new LuckySpinQuestMessage(characterId));
		
		public void SendSkipBossMessage()
			=> _skipBossPublisher.Publish(new SkipBossQuestMessage(null));
		
		public void SendFreeTicketMessage()
			=> _freeTicketPublisher.Publish(new FreeTicketQuestMessage(null));
		
		public void SendTalkMerchantMessage(string merchantId)
			=> _talkMerchantPublisher.Publish(new TalkMerchantQuestMessage(merchantId));

		public void SendProduceMessage(string producedObjId, int value)
			=> _producePublisher.Publish(new ProduceQuestMessage(producedObjId, value));

		public void SendExchangeMessage(string fromId, string forId)
			=> _exchangePublisher.Publish(new ExchangeQuestMessage(fromId, forId));
		
		public void SendSpeedUpRefiningMessage(string producedObjId)
			=> _speedUpRefiningPublisher.Publish(new SpeedUpRefiningQuestMessage(producedObjId));
		
		public void SendCarryMessage(string carriedObjId)
			=> _carryPublisher.Publish(new CarryQuestMessage(carriedObjId));
	}

}
