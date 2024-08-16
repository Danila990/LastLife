using Sirenix.OdinInspector;
using Ui.Resource;
using Ui.Sandbox;
using Ui.Sandbox.CharacterMenu;
using Ui.Sandbox.Conversation;
using Ui.Sandbox.EnemyUi;
using Ui.Sandbox.Experience;
using Ui.Sandbox.InventoryUi;
using Ui.Sandbox.JetPack;
using Ui.Sandbox.PlayerInput;
using Ui.Sandbox.Pointer;
using Ui.Sandbox.Quests.Views.Menu;
using Ui.Sandbox.Quests.Views.Widgets;
using Ui.Sandbox.ReloadUI;
using Ui.Sandbox.SelectMenuButtons;
using Ui.Sandbox.SettingsMenu;
using Ui.Sandbox.ShopMenu;
using Ui.Sandbox.SpawnMenu;
using Ui.Sandbox.WorldSpaceUI;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Extensions;
using VContainer.Unity;
using VContainerUi;
using VContainerUi.Model;

namespace Installer.Sandbox
{
	[CreateAssetMenu(menuName = SoNames.INSTALLERS + nameof(MarketUiInstaller), fileName = nameof(MarketUiInstaller))]
	public class MarketUiInstaller : ScriptableObjectInstaller
	{
		[TitleGroup("Canvas")]
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private Canvas _canvas;
		
		[TitleGroup("Views")]
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private PlayerInputView _playerInputView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private InventoryPreviewView _inventoryPreviewView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private SpawnMenuView _spawnMenuView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private SelectMenuButtonsView _selectMenuButtonsView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private CharacterMenuView _characterMenuView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private BossHealthUiView _bossHealthUiView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private WorldSpaceUiView _worldSpaceUiView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private ResourceView _resourceView;
		
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private ShopMenuView _shopMenuView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private ReloadingView _reloadingView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private ExperienceView _experienceView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private PointerView _pointerView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private SettingsMenuView _settingsMenuView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private JetPackView _jetPackView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private ConversationView _conversationView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private QuestsMenuView _questsMenuView;
		[SerializeField, AssetSelector(Paths = "Assets/Prefabs/Ui")] private WidgetQuestView _widgetQuestView;
		public override void Install(IContainerBuilder builder)
		{
			var canvas = Instantiate(_canvas);
			builder.RegisterInstance(canvas);

			ConfigureUiViews(builder, canvas);
			ConfigureWindows(builder);
			ConfigureWindowsController(builder);
		}

		private void ConfigureUiViews(IContainerBuilder builder, Canvas canvas)
		{
			//Input
			builder.RegisterUiView<PlayerInputController, PlayerInputView>(_playerInputView, canvas.transform);
			builder.RegisterUiView<WorldSpaceUiController, WorldSpaceUiView>(_worldSpaceUiView, canvas.transform);
			
			//Inventory
			builder.RegisterUiView<InventoryPreviewController, InventoryPreviewView>(_inventoryPreviewView, canvas.transform);
			builder.RegisterUiView<SelectMenuButtonsController, SelectMenuButtonsView>(_selectMenuButtonsView, canvas.transform);
			builder.RegisterUiView<ResourceUiController, ResourceView>(_resourceView, canvas.transform);
			
			//MenuPanels
			builder.RegisterUiView<SpawnMenuController, SpawnMenuView>(_spawnMenuView, canvas.transform);
			builder.RegisterUiView<CharacterMenuController, CharacterMenuView>(_characterMenuView, canvas.transform);
			builder.RegisterUiView<ShopMenuController, ShopMenuView>(_shopMenuView, canvas.transform);
			builder.RegisterUiView<SettingsMenuUiController, SettingsMenuView>(_settingsMenuView, canvas.transform);

			//Other
			builder.RegisterUiView<ReloadUIController, ReloadingView>(_reloadingView, canvas.transform);
			builder.RegisterUiView<ExperienceUiController, ExperienceView>(_experienceView, canvas.transform);
			builder.RegisterUiView<PointerController, PointerView>(_pointerView, canvas.transform);
			builder.RegisterUiView<JetPackController, JetPackView>(_jetPackView, canvas.transform);
			builder.RegisterUiView<ConversationController, ConversationView>(_conversationView, canvas.transform);

			//Quests
			builder.RegisterUiView<QuestsMenuController, QuestsMenuView>(_questsMenuView, canvas.transform);
			builder.RegisterUiView<QuestWidgetController, WidgetQuestView>(_widgetQuestView, canvas.transform);
		}

		private static void ConfigureWindows(IContainerBuilder builder)
		{
			builder.Register<MarketHudWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().As<SandboxMainWindow>();

			builder.Register<MarketSpawnMenuWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().As<SpawnMenuWindow>();		
			
			builder.Register<MarketQuestsMenuWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().As<QuestsMenuWindow>();
			
			builder.Register<MarketCharacterMenuWindow>(Lifetime.Singleton)
			 	.AsImplementedInterfaces().As<CharacterMenuWindow>();
			
			builder.Register<MarketShopMenuWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().As<ShopMenuWindow>();
						
			builder.Register<SettingsMenuWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().AsSelf();
			
			builder.Register<MarketIntermediateDeathWindow>(Lifetime.Singleton)
				.AsImplementedInterfaces().As<IntermediateDeathWindow>();
		}

		private void ConfigureWindowsController(IContainerBuilder builder)
		{
			builder.Register<WindowState>(Lifetime.Singleton)
				.AsImplementedInterfaces()
				.AsSelf();

			builder.RegisterEntryPoint<WindowsController>().WithParameter(UiScope.Local);
		}
	}
}