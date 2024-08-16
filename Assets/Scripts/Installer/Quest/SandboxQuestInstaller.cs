using Core.Quests;
using Core.Quests.Messages;
using Core.Quests.Messages.Handler;
using Core.Quests.Priority;
using Core.Quests.Save;
using Core.Quests.Tips;
using Sirenix.OdinInspector;
using Ui.Sandbox.Quests.Views.Analytics;
using UnityEngine;
using VContainer;
using VContainer.Extensions;

namespace Installer.Quest
{
	public class SandboxQuestInstaller : MonoInstaller
	{
		public override void Install(IContainerBuilder builder)
		{
			RegisterServices(builder);
		}

		private void RegisterServices(IContainerBuilder builder)
		{
			builder.Register<QuestActivationService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<QuestService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<MainQuestSelector>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<QuestTipService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<QuestTipProvider>(Lifetime.Singleton).AsImplementedInterfaces();

			//Save
			builder.Register<QuestSaveAdapter>(Lifetime.Singleton).AsImplementedInterfaces();
			
			//Priority
			builder.Register<BossPriorityModifier>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<QuestsPriorityHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			
			//Handlers
			builder.Register<BossMessageHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SupplyMessageHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SpawnMessageHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<UpgradeSkillMessageHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<GenericMessageHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<SwitchLevelMessageHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PurchaseMessageHandler>(Lifetime.Singleton).AsImplementedInterfaces();
						
			//Analytics
			builder.Register<QuestAnalyticsSender>(Lifetime.Singleton).AsImplementedInterfaces();
		}
	}

}
