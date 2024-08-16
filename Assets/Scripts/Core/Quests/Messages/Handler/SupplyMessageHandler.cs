using System.Threading;
using Core.Quests.Messages.Impl;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Utils;

namespace Core.Quests.Messages.Handler
{
	public class SupplyMessageHandler : IQuestMessageHandler<TakeSupplyQuestMessage>, IQuestMessageHandler<TakeEquipmentQuestMessage>, IQuestMessageHandler<PlantObjectQuestMessage>
	{
		private readonly ISubscriber<TakeSupplyQuestMessage> _supplySubscriber;
		private readonly ISubscriber<TakeEquipmentQuestMessage> _equipmentSubscriber;
		private readonly ISubscriber<PlantObjectQuestMessage> _placeObjectSubscriber;
		private readonly IQuestService _questService;
		private CancellationToken _token;

		public SupplyMessageHandler(
			ISubscriber<TakeSupplyQuestMessage> supplySubscriber,
			ISubscriber<TakeEquipmentQuestMessage> equipmentSubscriber,
			ISubscriber<PlantObjectQuestMessage> placeObjectSubscriber,
			IQuestService questService,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_supplySubscriber = supplySubscriber;
			_equipmentSubscriber = equipmentSubscriber;
			_placeObjectSubscriber = placeObjectSubscriber;
			_questService = questService;
			_token = installerCancellationToken.Token;
		}

		public void PostInitialize()
		{
			_supplySubscriber.Subscribe(Handle).AddTo(_token);
			_equipmentSubscriber.Subscribe(Handle).AddTo(_token);
			_placeObjectSubscriber.Subscribe(Handle).AddTo(_token);
		}
		
		public void Handle(TakeSupplyQuestMessage msg)
		{
			foreach (var tree in _questService.Trees.Values)
			{
				var node = tree.GetFinalNode(msg.InlineId);
				node?.SetValue(msg.SupplyCount);
			}
		}
		
		public void Handle(TakeEquipmentQuestMessage msg)
		{
			foreach (var tree in _questService.Trees.Values)
			{
				var node = tree.GetFinalNode(msg.InlineId);
				node?.SetValue(1);
			}
		}

		public void Handle(PlantObjectQuestMessage msg)
		{
			foreach (var tree in _questService.Trees.Values)
			{
				var node = tree.GetFinalNode(msg.InlineId);
				node?.SetValue(1);
			}
		}

		public void Dispose()
		{
		}
	}

}
