using System.Threading;
using Core.Quests.Messages.Impl;
using Core.Services;
using Cysharp.Threading.Tasks;
using UniRx;
using Utils;

namespace Core.Quests.Messages.Handler
{
	public class SwitchLevelMessageHandler : IQuestMessageHandler<SwitchLevelQuestMessage>
	{
		private readonly IQuestService _questService;
		private readonly ISceneLoaderService _sceneLoader;
		private CancellationToken _token;


		public SwitchLevelMessageHandler(
			IQuestService questService,
			ISceneLoaderService sceneLoader,
			InstallerCancellationToken installerCancellationToken
		)
		{
			_questService = questService;
			_sceneLoader = sceneLoader;
			_token = installerCancellationToken.Token;
		}
		
		public void PostInitialize()
		{
			_sceneLoader.BeforeSceneChange.Subscribe(OnBeforeSceneChange).AddTo(_token);
		}
		
		private void OnBeforeSceneChange(SceneChangeEventData eventData)
			=> Handle(new SwitchLevelQuestMessage(eventData.LoadedScene));

		public void Handle(SwitchLevelQuestMessage msg)
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
