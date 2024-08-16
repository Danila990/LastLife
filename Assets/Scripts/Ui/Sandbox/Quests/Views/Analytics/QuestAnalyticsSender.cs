using System.Threading;
using Analytic;
using Core.Quests;
using Core.Quests.Tree;
using Cysharp.Threading.Tasks;
using Db.Quests;
using UniRx;
using Utils;
using VContainer.Unity;

namespace Ui.Sandbox.Quests.Views.Analytics
{
	public class QuestAnalyticsSender : IInitializable
	{
		private readonly IQuestService _questService;
		private readonly IAnalyticService _analyticService;
		private readonly CancellationToken _token;

		private readonly string _questEventFormat = "Quests:Complete:{0}:{1}";
		private readonly string _treeEventFormat = "Quests:CompleteAll:{0}";
		
		public QuestAnalyticsSender(
			IQuestService questService,
			InstallerCancellationToken installerCancellationToken,
			IAnalyticService analyticService
			)
		{
			_analyticService = analyticService;
			_token = installerCancellationToken.Token;
			_questService = questService;
		}

		public void Initialize()
		{
			_questService.OnTreeComplete.Subscribe(OnTreeComplete).AddTo(_token);
			_questService.OnQuestComplete.Subscribe(OnQuestComplete).AddTo(_token);
		}

		private void OnTreeComplete(QuestsTree tree)
		{
			var @event = string.Format(_treeEventFormat, tree.Id);
			_analyticService.SendEvent(@event);
		}
		
		private void OnQuestComplete(QuestData data)
		{
			var @event = string.Format(_questEventFormat, _questService.ActiveTree.Value.Id, data.ImplementationData.AnalyticId);
			_analyticService.SendEvent(@event);
		}

		
	}
}
