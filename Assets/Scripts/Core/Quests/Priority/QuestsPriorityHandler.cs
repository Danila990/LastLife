using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using Utils;
using VContainer.Unity;

namespace Core.Quests.Priority
{
	public class QuestsPriorityHandler : IPostInitializable
	{

		private readonly IEnumerable<IQuestPriorityModifier> _modifiers;
		private readonly IQuestService _questService;
		private readonly CancellationToken _token;

		public QuestsPriorityHandler(IEnumerable<IQuestPriorityModifier> modifiers, IQuestService questService, InstallerCancellationToken installerCancellationToken)
		{
			_modifiers = modifiers;
			_questService = questService;
			_token = installerCancellationToken.Token;
		}

		public void PostInitialize()
		{
			foreach (var modifier in _modifiers)
			{
				modifier.OnPriorityChanged.Subscribe(OnPriorityChanged).AddTo(_token);
			}
		}

		private void OnPriorityChanged(ModifiedPriorityArgs args)
		{
			if (_questService.ActiveTree == null)
				return;
			
			_questService.ActiveTree.Value.ChangePriority(args);
		}
	}
}
