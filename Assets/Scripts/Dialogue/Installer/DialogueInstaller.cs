using System.Collections.Generic;
using Db.MerchantData;
using Dialogue.Services;
using Dialogue.Services.Modules.CameraModule;
using Dialogue.Services.Modules.MerchantShop;
using Dialogue.Services.Modules.ResourcesModule;
using Dialogue.Ui;
using Dialogue.Ui.CustomViews;
using Dialogue.Ui.MerchantUi;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Extensions;
using VContainer.Unity;
using VContainerUi;
using Yarn.Unity;

namespace Dialogue.Installer
{
	public class DialogueInstaller : MonoInstaller
	{
		[SerializeField] private DialogueRunner _dialogueRunner;
		[SerializeField] private YarnProject _yarnProject;
		[SerializeField] private MerchantItemCollectionData[] _merchantItemCollections;
		
		[TitleGroup("Ui")]
		[SerializeField] private DialogueUiView _dialogueUiView;
		[SerializeField] private BankMenuPanel _bankMenuPanel;
		[SerializeField] private MerchantShopPanel _merchantShopPanel;
		[SerializeField] private DialogueViewBase[] _dialogueViewBases;
		
		public override void Install(IContainerBuilder builder)
		{
			builder.RegisterComponentInNewPrefab(_dialogueRunner, Lifetime.Singleton);
			builder.RegisterInstance(_dialogueViewBases);
			builder.RegisterInstance(_yarnProject);
			builder.RegisterInstance(_merchantItemCollections).As<IReadOnlyList<MerchantItemCollectionData>>();

			builder.Register<DialogueService>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<CameraDialogueModule>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ResourcesDialogueModule>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<MerchantShopModule>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<PlayerPrefsModule>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BoostLocalPurchaseHandler>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<LocalItemPurchaseService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<LocalAbilityPurchase>(Lifetime.Singleton).AsImplementedInterfaces();
			
			RegisterUi(builder);
			RegisterWindow(builder);
		}
		
		private void RegisterUi(IContainerBuilder builder)
		{
			builder.RegisterUiViewUnderRegisteredCanvas<DialogueUiController, DialogueUiView>(_dialogueUiView);
			
			builder.Register(resolver =>
			{
				var uiView = resolver.Resolve<DialogueUiView>();
				var bankMenuPanel = Instantiate(_bankMenuPanel, uiView.transform);
				bankMenuPanel.gameObject.SetActive(false);
				return bankMenuPanel;
			}, Lifetime.Singleton);
			
			builder.Register(resolver =>
			{
				var uiView = resolver.Resolve<DialogueUiView>();
				var merchantShopPanel = Instantiate(_merchantShopPanel, uiView.transform);
				merchantShopPanel.Init();
				merchantShopPanel.gameObject.SetActive(false);
				return merchantShopPanel;
			}, Lifetime.Singleton);
		}

		private void RegisterWindow(IContainerBuilder builder)
		{
			builder.Register<DialogueWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
		}
	}
}