using System.Collections;
using System.Linq;
using Core.Services;
using Cysharp.Threading.Tasks;
using GameSettings;
using NUnit.Framework;
using SharedUtils;
using Tests.UITest;
using Ui.Sandbox.Conversation;
using Ui.Sandbox.SettingsMenu;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainerUi.Messages;
using VContainerUi.Services;

namespace Tests.Common
{
	public class SettingsPanelTests : UiTest
	{
		private const int AFTER_TEST_DELAY = 150;
		
		[UnityTest]
		public IEnumerator QualitySettingTest([ValueSource(nameof(SceneNames))] string sceneName) => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitScene(sceneName, CancellationToken);
			await UniTask.Delay(100, cancellationToken: CancellationToken);
			var settingsService = scope.Container.Resolve<ISettingsService>();
			
			var enumerable = Enumerable.Range(0, settingsService.QualityPreset.Presets.Length);
			foreach (var index in enumerable)
			{
				settingsService.QualityPreset.SelectPreset(index);
				await UniTask.Delay(AFTER_TEST_DELAY, cancellationToken: CancellationToken);
			}
		});
		
		[UnityTest]
		public IEnumerator RestartSettingsTest([ValueSource(nameof(SceneNames))] string sceneName) => UniTask.ToCoroutine(async () =>
		{
#if RELEASE_BRANCH 
			return;
#endif
			var scope = await InitScene(sceneName, CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var sceneLoaderService = scope.Container.Resolve<ISceneLoaderService>();
			var msger = scope.Container.Resolve<IUiMessagesPublisherService>();
			msger.OpenWindowPublisher.OpenWindow<SettingsMenuWindow>();
			var controller = scope.Container.Resolve<SettingsMenuUiController>();
			var conversationView = scope.Container.Resolve<ConversationView>();
			await AssertUiViewActive<SettingsMenuView>(scope.Container.Resolve<Canvas>());
			
			await UniTask.Delay(0.2f.ToSec(), cancellationToken:CancellationToken);
			await Press(controller.RestartSettingsPresenter.RestartButton.gameObject);
			UniTask.Action(async () =>
			{
				await UniTask.Delay(0.25f.ToSec(), cancellationToken: CancellationToken);
				await Press(conversationView.Submit.gameObject);
			}).Invoke();
			
			await sceneLoaderService.AfterSceneChange.ToUniTask(true, cancellationToken:CancellationToken).Timeout(2f.ToSec());
			
			await UniTask.Delay(AFTER_TEST_DELAY, cancellationToken:CancellationToken);
		});
		
		[UnityTest]
		public IEnumerator ClearNPCSettingsTest([ValueSource(nameof(SceneNames))] string sceneName) => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitScene(sceneName, CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var msger = scope.Container.Resolve<IUiMessagesPublisherService>();
			msger.OpenWindowPublisher.OpenWindow<SettingsMenuWindow>();
			var controller = scope.Container.Resolve<SettingsMenuUiController>();
			await AssertUiViewActive<SettingsMenuView>(scope.Container.Resolve<Canvas>());
			
			await UniTask.Delay(0.2f.ToSec(), cancellationToken:CancellationToken);
			await Press(controller.ClearSettingsPresenter.ClearNpc.gameObject);
			await UniTask.Delay(AFTER_TEST_DELAY, cancellationToken:CancellationToken);
		});
		
		[UnityTest]
		public IEnumerator ClearPropsSettingsTest([ValueSource(nameof(SceneNames))] string sceneName) => UniTask.ToCoroutine(async () =>
		{
			var scope = await InitScene(sceneName, CancellationToken);
			await UniTask.Delay(1000, cancellationToken: CancellationToken);
			var msger = scope.Container.Resolve<IUiMessagesPublisherService>();
			msger.OpenWindowPublisher.OpenWindow<SettingsMenuWindow>();
			var controller = scope.Container.Resolve<SettingsMenuUiController>();
			await AssertUiViewActive<SettingsMenuView>(scope.Container.Resolve<Canvas>());

			await UniTask.Delay(0.2f.ToSec(), cancellationToken:CancellationToken);
			await Press(controller.ClearSettingsPresenter.ClearProps.gameObject);
			await UniTask.Delay(AFTER_TEST_DELAY, cancellationToken:CancellationToken);
		});

		public static IEnumerable SceneNames()
		{
			yield return SceneLoaderService.SANDBOX;
			yield return SceneLoaderService.MARKET;
		}
	}
}