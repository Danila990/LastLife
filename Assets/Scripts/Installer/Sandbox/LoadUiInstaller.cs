using Sirenix.OdinInspector;
using Ui.Sandbox.LoadScreen;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Extensions;
using VContainer.Unity;
using VContainerUi;
using VContainerUi.Model;

namespace Installer.Sandbox
{
	[CreateAssetMenu(menuName = SoNames.INSTALLERS + nameof(LoadUiInstaller), fileName = nameof(LoadUiInstaller))]
	public class LoadUiInstaller : ScriptableObjectInstaller
	{
		[TitleGroup("Canvas")]
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private Canvas _canvas;
		
		[TitleGroup("Views")]
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private LoadScreenView _loadView;
		
		public override void Install(IContainerBuilder builder)
		{
			var canvas = Instantiate(_canvas);
			builder.RegisterInstance(canvas);
			DontDestroyOnLoad(canvas);
			
			ConfigureUiViews(builder, canvas);
			ConfigureWindows(builder);
			ConfigureWindowsController(builder);
		}

		private void ConfigureUiViews(IContainerBuilder builder, Canvas canvas)
		{
			builder.RegisterUiView<LoadScreenController, LoadScreenView>(_loadView, canvas.transform);
		}

		private static void ConfigureWindows(IContainerBuilder builder)
		{
			builder.Register<LoadScreenWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
		}

		private void ConfigureWindowsController(IContainerBuilder builder)
		{
			builder.Register<WindowState>(Lifetime.Singleton)
				.AsImplementedInterfaces()
				.AsSelf();

			builder.RegisterEntryPoint<WindowsController>().WithParameter(UiScope.Project);
		}
	}
}
