using Adv.Messages;
using Core.Entity.Characters;
using Core.Entity.Repository;
using Core.InputSystem;
using Core.Loot;
using Core.Services.Experience;
using Core.Zones;
using Dialogue.Services.Modules.MerchantShop;
using GameStateMachine.States.Impl.Project;
using MessagePipe;
using Shop.Messages;
using UnityEngine;
using VContainer;
using VContainer.Extensions;
using VContainerUi;
using VContainerUi.Services.Impl;

namespace Installer.Sandbox
{
	public class GeneralMessagePipeInstaller : MonoInstaller
	{
		[SerializeField] private MessagePipeInstaller[] _additionalInstallers;
		
		public override void Install(IContainerBuilder builder)
		{
			ConfigureMessagePipe(builder);
		}
		
		private void ConfigureMessagePipe(IContainerBuilder builder)
		{
			builder.Register<UiMessagesReceiverService>(Lifetime.Singleton)
				.AsImplementedInterfaces();

			builder.Register<UiMessagesPublisherService>(Lifetime.Singleton)
				.AsImplementedInterfaces();

			var options = builder.RegisterMessagePipe();
			RegisterMessages(builder, options);
			builder.RegisterUiSignals(options);

			builder.RegisterBuildCallback(c
				=> GlobalMessagePipe.SetProvider(c.AsServiceProvider()));

			foreach (var installer in _additionalInstallers)
				installer.Install(builder, options);
		}

		private static void RegisterMessages(IContainerBuilder builder, MessagePipeOptions options)
		{
			builder.RegisterMessageBroker<SceneLoadedMessage>(options);
			builder.RegisterMessageBroker<MessageEntityDeath>(options);
			builder.RegisterMessageBroker<MessageDamageEvent>(options);
			builder.RegisterMessageBroker<RepositoryEvent>(options);
			builder.RegisterMessageBroker<PurchaseMessage>(options);
			builder.RegisterMessageBroker<PlayerContextChangedMessage>(options);
			builder.RegisterMessageBroker<PlayerContextDeathMessage>(options);
			builder.RegisterMessageBroker<PlayerContextDamageMessage>(options);
			builder.RegisterMessageBroker<MessageShowAdReady>(options);
			builder.RegisterMessageBroker<MessageHideAd>(options);
			builder.RegisterMessageBroker<MessageShowAd>(options);
			builder.RegisterMessageBroker<ShowShopMenu>(options);
			builder.RegisterMessageBroker<ExperienceMessage>(options);
			builder.RegisterMessageBroker<LootMessage>(options);
			builder.RegisterMessageBroker<ZoneChangedMessage>(options);
			builder.RegisterMessageBroker<MessageObjectLocalPurchase>(options);
			builder.RegisterMessageBroker<MessageShowAdWindow>(options);
		}
	}

}
