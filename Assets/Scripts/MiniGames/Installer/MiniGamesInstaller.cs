using MiniGames.Impl.SpinnerMiniGame;
using MiniGames.Impl.SpinnerMiniGame.View;
using MiniGames.Ui;
using UnityEngine;
using VContainer;
using VContainer.Extensions;
using VContainerUi;

namespace MiniGames.Installer
{
	public class MiniGamesInstaller : MonoInstaller
	{
		[SerializeField] private MiniGameUiView _miniGameUiView;
		[SerializeField] private SpinnerMiniGameView _spinnerMiniGameView;
		
		public override void Install(IContainerBuilder builder)
		{
			builder.Register<MiniGameService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SpinnerMiniGameController>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.RegisterInstance(_spinnerMiniGameView);
			RegisterUi(builder);
			RegisterWindow(builder);
		}
		
		private void RegisterUi(IContainerBuilder builder)
		{
			builder.RegisterUiViewUnderRegisteredCanvas<MiniGameUiController, MiniGameUiView>(_miniGameUiView);
		}

		private void RegisterWindow(IContainerBuilder builder)
		{
			builder.Register<MiniGameWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
		}
	}
}