using System.IO;
using System.Linq;
using Core.Services;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Utils
{
	public static class SceneUtil
	{
		private static string[] _sceneNames;
		
		public static string[] GetAllBuildScenes()
		{
			#if UNITY_EDITOR
			var count = SceneManager.sceneCountInBuildSettings;
			if (_sceneNames == null || _sceneNames.Length != count)
				_sceneNames = new string[count];
			var editorBuildSettingsScenes = EditorBuildSettings.scenes;

			for (int i = 0; i < count; i++)
			{
				var scenePath = editorBuildSettingsScenes[i].path;
				var scene = Path.GetFileNameWithoutExtension(scenePath);
				_sceneNames[i] = scene;
			}
			
			#endif
			return _sceneNames;
		}
		
		public static bool IsGameSceneExist(string sceneName)
		{
			if (string.IsNullOrEmpty(sceneName) || sceneName == SceneLoaderService.LAUNCH_SCENE)
				return false;

			GetAllBuildScenes();
			return _sceneNames.Any(scene => scene == sceneName);
		}
		
		public static bool IsSandboxSceneActive()
		{
			return SceneManager.GetActiveScene().name == SceneLoaderService.SANDBOX;
		}
		
	}
}
