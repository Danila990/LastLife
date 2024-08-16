using System.Threading;
using Core.Quests.Messages.Impl;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Utils;

namespace Core.Quests.Messages.Handler
{
	public class GenericMessageHandler : 
		IQuestMessageHandler<LuckySpinQuestMessage>,
		IQuestMessageHandler<SwitchLevelQuestMessage>,
		IQuestMessageHandler<TalkMerchantQuestMessage>,
		IQuestMessageHandler<ProduceQuestMessage>,
		IQuestMessageHandler<SpeedUpRefiningQuestMessage>,
		IQuestMessageHandler<ExchangeQuestMessage>,
		IQuestMessageHandler<CarryQuestMessage>
	{
		private readonly ISubscriber<LuckySpinQuestMessage> _luckySpinSubscriber;
		private readonly ISubscriber<SwitchLevelQuestMessage> _switchLevelSubscriber;
		private readonly ISubscriber<FreeTicketQuestMessage> _freeTicketSubscriber;
		private readonly ISubscriber<TalkMerchantQuestMessage> _talkMerchantSubscriber;
		private readonly ISubscriber<ProduceQuestMessage> _produceSubscriber;
		private readonly ISubscriber<SpeedUpRefiningQuestMessage> _speedUpRefiningSubscriber;
		private readonly ISubscriber<ExchangeQuestMessage> _exchangeSubscriber;
		private readonly ISubscriber<CarryQuestMessage> _carrySubscriber;
		private readonly IQuestService _questService;
		private readonly CancellationToken _token;
		
		public GenericMessageHandler(
			ISubscriber<LuckySpinQuestMessage> luckySpinSubscriber,
			ISubscriber<SwitchLevelQuestMessage> switchLevelSubscriber,
			ISubscriber<FreeTicketQuestMessage> freeTicketSubscriber,
			ISubscriber<TalkMerchantQuestMessage> talkMerchantSubscriber,
			ISubscriber<ProduceQuestMessage> produceSubscriber,
			ISubscriber<SpeedUpRefiningQuestMessage> speedUpRefiningSubscriber,
			ISubscriber<ExchangeQuestMessage> exchangeSubscriber,
			ISubscriber<CarryQuestMessage> carrySubscriber,
			IQuestService questService,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_luckySpinSubscriber = luckySpinSubscriber;
			_switchLevelSubscriber = switchLevelSubscriber;
			_freeTicketSubscriber = freeTicketSubscriber;
			_talkMerchantSubscriber = talkMerchantSubscriber;
			_produceSubscriber = produceSubscriber;
			_speedUpRefiningSubscriber = speedUpRefiningSubscriber;
			_exchangeSubscriber = exchangeSubscriber;
			_carrySubscriber = carrySubscriber;
			_questService = questService;
			_token = installerCancellationToken.Token;
		}

		public void PostInitialize()
		{
			_luckySpinSubscriber.Subscribe(Handle).AddTo(_token);
			_switchLevelSubscriber.Subscribe(Handle).AddTo(_token);
			_freeTicketSubscriber.Subscribe(Handle).AddTo(_token);
			_talkMerchantSubscriber.Subscribe(Handle).AddTo(_token);
			_produceSubscriber.Subscribe(Handle).AddTo(_token);
			_speedUpRefiningSubscriber.Subscribe(Handle).AddTo(_token);
			_exchangeSubscriber.Subscribe(Handle).AddTo(_token);
			_carrySubscriber.Subscribe(Handle).AddTo(_token);
		}
		
		
		public void Handle(CarryQuestMessage msg) => Handle(msg.InlineId);
		public void Handle(SwitchLevelQuestMessage msg)  => Handle(msg.InlineId);
		public void Handle(FreeTicketQuestMessage msg)  => Handle(msg.InlineId);
		public void Handle(LuckySpinQuestMessage msg) => Handle(msg.InlineId);
		public void Handle(TalkMerchantQuestMessage msg) => Handle(msg.InlineId);
		public void Handle(ProduceQuestMessage msg) => Handle(msg.InlineId, msg.Value);
		public void Handle(SpeedUpRefiningQuestMessage msg) => Handle(msg.InlineId);
		public void Handle(ExchangeQuestMessage msg)  => Handle(msg.InlineId);
		
		private void Handle(string inlineId, int value = 1)
		{
			foreach (var tree in _questService.Trees.Values)
			{
				var node = tree.GetFinalNode(inlineId);
				node?.SetValue(value);
			}
		}
		
		public void Dispose()
		{
		}
	}
}
