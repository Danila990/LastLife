using System;
using Cheats.CheatPanel;
using Cheats.Impl;
using Cheats.Impl.Adv;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Extensions;
using VContainer.Unity;

namespace Cheats.Installer
{
	public class CheatInstaller : MonoInstaller
	{
		[SerializeField] private CheatPanelView _cheatPanelView;
		[SerializeField] private Button _cheatButton;
		
		public override void Install(IContainerBuilder builder)
		{
			builder.RegisterComponentInNewPrefab(_cheatPanelView, Lifetime.Singleton).DontDestroyOnLoad();
			builder.Register<CheatPanelController>(Lifetime.Singleton).WithParameter(_cheatButton).AsImplementedInterfaces();

			RegisterCommands(builder);
		}
		
		private void RegisterCommands(IContainerBuilder builder)
		{
			builder.Register<UnlockAllCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider>();
			builder.Register<SetViolenceStatusCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider>();
			builder.Register<CloseUiCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider>();
			builder.Register<SetFpsCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider>();
			builder.Register<StopAdvCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider>();
			builder.Register<SetBossTimerCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider>();
			builder.Register<SetImmortalityCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider, IDisposable>();
			builder.Register<ClearPrefsCheatCommand>(Lifetime.Singleton).As<ICheatCommandProvider>();
		}
	}
}