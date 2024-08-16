using Core.Entity.Characters;
using Core.Services;
using GameStateMachine.States.Impl.Project;
using SharedUtils.PlayerPrefs.Impl;
using Ui.Sandbox.SceneLoad;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Core.Entity.InteractionLogic.Interactions
{
	public class LoadLevelInteraction : MonoBehaviour
	{
		public RewardGenericInteraction GenericInteraction;
		private InMemoryPlayerPrefsManager _inMemoryPrefs;
		private ISceneLoaderService _sceneLoaderService;

		public SceneData[] SceneLoadVariants;
		private SceneData _sceneToLoad;
		private ProjectStateMachine _pFsm;
		private InMemoryPlayerPrefsManager InMemoryPlayerPrefsManager => _inMemoryPrefs ??= GenericInteraction.ObjectResolver.Resolve<InMemoryPlayerPrefsManager>();
		private ISceneLoaderService SceneLoaderService => _sceneLoaderService ??= GenericInteraction.ObjectResolver.Resolve<ISceneLoaderService>();
		private ProjectStateMachine ProjectStateMachine => _pFsm ??= GenericInteraction.ObjectResolver.Resolve<ProjectStateMachine>();
		
		private void Awake()
		{
			var sceneName = SceneManager.GetActiveScene().name;
			foreach (var viewSceneLoadVariant in SceneLoadVariants)
			{
				if (sceneName == viewSceneLoadVariant.SceneId)
					continue;
				_sceneToLoad = viewSceneLoadVariant;
			}
			
			GenericInteraction.SetMeta($"Train:{_sceneToLoad.SceneId}");
			GenericInteraction.UseCustomText = true;
			GenericInteraction.CustomText = _sceneToLoad.SceneTextName;
		}
		
		private void Start()
		{
			GenericInteraction.Used.TakeUntilDisable(this).Subscribe(OnUsed);
		}
		
		private void OnUsed(CharacterContext obj)
		{
			Load();
		}
		
		public void Load()
		{
			Load(_sceneToLoad.SceneId);
		}
		
		private void Load(string sceneName)
		{
			InMemoryPlayerPrefsManager.SetValue("MetroUsed", true);
			ProjectStateMachine.ChangeStateAsync<ProjectLoadSceneState, string>(sceneName);
		}
	}
}