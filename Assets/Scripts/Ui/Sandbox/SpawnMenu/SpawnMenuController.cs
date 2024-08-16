using System;
using System.Collections.Generic;
using System.Linq;
using Core.Factory.Ui;
using Core.Services;
using Db.ObjectData;
using Db.Panel;
using Ui.Sandbox.SelectMenu;
using Ui.Sandbox.SpawnMenu.Item;
using Ui.Sandbox.SpawnMenu.Panel;
using UniRx;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.SpawnMenu
{
	public class SpawnMenuController : SelectMenuController<SpawnMenuView>, IStartable
	{
		private readonly ISpawnItemService _spawnItemService;
		private readonly IPanelsData _panelsData;
		private readonly IItemStorage _itemStorage;
		private readonly ISpawnMenuItemFactory _spawnMenuItemFactory;
		private readonly IItemUnlockService _itemUnlockService;
		private readonly Dictionary<string, SelectMenuPanelPresenter> _panelPresenters = new Dictionary<string, SelectMenuPanelPresenter>();
		private readonly Dictionary<string, ElementPresenterItem> _allElements = new Dictionary<string, ElementPresenterItem>();
		private SelectMenuPanelPresenter _selectedPanel;
		private SelectForSpawningPresenterItem _selectedElement;

		public SpawnMenuController(
			IMenuPanelService menuPanelService,
			ISpawnItemService spawnItemService,
			IPanelsData panelsData,
			IItemStorage itemStorage,
			ISpawnMenuItemFactory spawnMenuItemFactory,
			IItemUnlockService itemUnlockService) 
			: base(menuPanelService)
		{
			_spawnItemService = spawnItemService;
			_panelsData = panelsData;
			_itemStorage = itemStorage;
			_spawnMenuItemFactory = spawnMenuItemFactory;
			_itemUnlockService = itemUnlockService;
		}

		public void Start()
		{
			InitializePanels();
			SelectPanel(_panelPresenters.Values.First());
			_itemUnlockService.OnItemUnlock.Subscribe(OnItemUnlock).AddTo(View);
			_spawnItemService.SelectedItem.Subscribe(OnSelectedItemChange).AddTo(View);
		}
		
		private void OnItemUnlock(ObjectData obj)
		{
			if (_allElements.TryGetValue(obj.Id, out var presenterItem))
			{
				presenterItem.SetUnlockStatus(true);
			}	
		}

		private void OnSelectedItemChange(ObjectData obj)
		{
			if (obj == null || _selectedElement?.Id == obj.Id)
				return;
			
			_selectedElement?.Deselect();
			if (_allElements[obj.Id] is SelectForSpawningPresenterItem presenter)
			{
				_selectedElement = presenter;
				_selectedElement.Select();
			}
			else
			{
				throw new ArgumentException(obj.Id);
			}
		}

		private void InitializePanels()
		{
			foreach (var panelsDataPanel in _panelsData.Panels)
			{
				var widget = Object.Instantiate(View.SelectPanelButtonWidget, View.SelectPanelButtonsHolder);
				var panelsContent = Object.Instantiate(View.PanelContentPrefab, View.ElementsContentHolder);
				var panelPresenter = new SelectMenuPanelPresenter(this, widget, panelsDataPanel, panelsContent).AddTo(View);
				_panelPresenters.Add(panelPresenter.ItemsPanel.PanelId, panelPresenter);

				foreach (var categoryData in panelsDataPanel.CategoriesData)
				{
					var categoryWidget = Object.Instantiate(View.CategoriesButtonWidget, panelsContent.CategoriesHolder);
					var categoryElementsHolder = Object.Instantiate(View.ElementsForCategory, panelsContent.ElementsHolder);
					panelPresenter.AddCategory(categoryData, categoryWidget, categoryElementsHolder);
				}
				
				foreach (var element in panelsDataPanel.ElementItems)
				{
					var objectData = _itemStorage.All[element.ItemObjectDataId];
					var elementPresenterItem = panelPresenter.CreateElement(element, objectData, objectData is CharacterObjectData ? View.SelectItemWidgetForNPC : View.SelectItemWidget, _spawnMenuItemFactory);
					elementPresenterItem.SetUnlockStatus(_itemUnlockService.IsUnlocked(objectData));

					_allElements.Add(elementPresenterItem.Id, elementPresenterItem);
				}
				panelPresenter.OnInitialized();
				panelPresenter.Hide();
			}
		}

		public void SelectPanel(SelectMenuPanelPresenter itemsPanel)
		{
			if (_selectedPanel == itemsPanel)
				return;
			
			_selectedPanel?.Hide();
			_selectedPanel = itemsPanel;
			_selectedPanel.Select();
		}
		
		public void SelectPanel(string id)
		{
			if (_selectedPanel?.ItemsPanel.PanelId == id)
				return;
			
			_selectedPanel?.Hide();
			_selectedPanel = _panelPresenters[id];
			_selectedPanel.Select();
		}
	}
}