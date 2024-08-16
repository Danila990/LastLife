using System.Threading;
using Core.Quests.Messages.Impl;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Utils;

namespace Core.Quests.Messages.Handler
{
	public class UpgradeSkillMessageHandler : IQuestMessageHandler<UpgradeSkillQuestMessage>
	{
		private readonly ISubscriber<UpgradeSkillQuestMessage> _upgradeSkillSubscriber;
		private readonly IQuestService _questService;
		private CancellationToken _token;


		public UpgradeSkillMessageHandler(
			ISubscriber<UpgradeSkillQuestMessage> upgradeSkillSubscriber,
			IQuestService questService,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_upgradeSkillSubscriber = upgradeSkillSubscriber;
			_questService = questService;
			_token = installerCancellationToken.Token;
		}
		
		public void PostInitialize()
		{
			_upgradeSkillSubscriber.Subscribe(Handle).AddTo(_token);
		}
		

		public void Handle(UpgradeSkillQuestMessage msg)
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
