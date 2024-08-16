using System.Threading;
using Core.Quests.Messages.Impl;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Utils;

namespace Core.Quests.Messages.Handler
{
	public class SpawnMessageHandler : IQuestMessageHandler<SpawnNpcQuestMessage>, IQuestMessageHandler<SpawnPropQuestMessage>
	{
		private readonly ISubscriber<SpawnNpcQuestMessage> _spawnNpcSubscriber;
		private readonly ISubscriber<SpawnPropQuestMessage> _spawnPropSubscriber;
		private readonly IQuestService _questService;
		private CancellationToken _token;


		public SpawnMessageHandler(
			ISubscriber<SpawnNpcQuestMessage> spawnNpcSubscriber,
			ISubscriber<SpawnPropQuestMessage> spawnPropSubscriber,
			IQuestService questService,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_spawnNpcSubscriber = spawnNpcSubscriber;
			_spawnPropSubscriber = spawnPropSubscriber;
			_questService = questService;
			_token = installerCancellationToken.Token;
		}
		
		public void PostInitialize()
		{
			_spawnNpcSubscriber.Subscribe(Handle).AddTo(_token);
			_spawnPropSubscriber.Subscribe(Handle).AddTo(_token);
		}
		
		
		public void Handle(SpawnNpcQuestMessage msg)
			=> Handle(msg.InlineId);
		
		public void Handle(SpawnPropQuestMessage msg)
			=> Handle(msg.InlineId);

		private void Handle(string inlineId)
		{
			foreach (var tree in _questService.Trees.Values)
			{
				var node = tree.GetFinalNode(inlineId);
				node?.SetValue(1);
			}
		}
		
		public void Dispose()
		{
		}
	}

}
