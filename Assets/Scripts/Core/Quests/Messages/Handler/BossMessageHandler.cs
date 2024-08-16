using System;
using System.Threading;
using Core.Quests.Messages.Impl;
using Core.Quests.Tree;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;
using Utils;

namespace Core.Quests.Messages
{

	public class BossMessageHandler : IQuestMessageHandler<BossDeathQuestMessage>, IQuestMessageHandler<SkipBossQuestMessage>
	{
		private readonly ISubscriber<BossDeathQuestMessage> _deathSubscriber;
		private readonly ISubscriber<SkipBossQuestMessage> _skipSubscriber;
		private readonly IQuestService _questService;
		private CancellationToken _token;


		public BossMessageHandler(
			ISubscriber<BossDeathQuestMessage> deathSubscriber,
			ISubscriber<SkipBossQuestMessage> skipSubscriber,
			InstallerCancellationToken installerCancellationToken,
			IQuestService questService
			)
		{
			_token = installerCancellationToken.Token;
			_deathSubscriber = deathSubscriber;
			_skipSubscriber = skipSubscriber;
			_questService = questService;
		}


		public void PostInitialize()
		{
			_deathSubscriber.Subscribe(Handle).AddTo(_token);
			_skipSubscriber.Subscribe(Handle).AddTo(_token);
		}
		
		public void Handle(SkipBossQuestMessage msg)
			=> Handle(msg.InlineId);
		
		public void Handle(BossDeathQuestMessage msg)
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
