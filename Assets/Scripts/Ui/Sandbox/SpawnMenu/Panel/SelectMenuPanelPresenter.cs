using System;
using System.Collections.Generic;
using System.Linq;
using Core.Factory.Ui;
using Db.ObjectData;
using Db.Panel;
using Ui.Sandbox.SpawnMenu.Category;
using Ui.Sandbox.SpawnMenu.Item;
using Ui.Widget;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ui.Sandbox.SpawnMenu.Panel
{
	public class SelectMenuPanelPresenter : IDisposable
	{
		private readonly SpawnMenuController _spawnMenuController;
		private readonly NamedButtonWidget _selectPanelWidget;
		private readonly ItemsPanel _itemsPanel;
		private readonly PanelContent _panelsContent;
		private readonly Dictionary<string, CategoryPresenter> _categories = new Dictionary<string, CategoryPresenter>();
		private readonly Dictionary<string, ElementPresenterItem> _itemPresenters = new Dictionary<string, ElementPresenterItem>();
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private CategoryPresenter _selectedCategory;
		
		public ItemsPanel ItemsPanel => _itemsPanel;
		public IReadOnlyDictionary<string, ElementPresenterItem> ItemPresenters => _itemPresenters;
		public IReadOnlyDictionary<string, CategoryPresenter> CategoryPresenters => _categories;
		
		
		public SelectMenuPanelPresenter(
			SpawnMenuController spawnMenuController, 
			NamedButtonWidget selectPanelWidget, 
			ItemsPanel itemsPanel,
			PanelContent panelsContent)
		{
			_selectPanelWidget = selectPanelWidget;
			_itemsPanel = itemsPanel;
			_panelsContent = panelsContent;
			_spawnMenuController = spawnMenuController;
			
			_selectPanelWidget.IconImg.sprite = itemsPanel.PanelIcon;
			_selectPanelWidget.Text.text = itemsPanel.PanelName;
			
			_selectPanelWidget.Button
				.OnClickAsObservable()
				.Subscribe(OnClickPanelWidget).AddTo(_compositeDisposable);
		}	
				
		public void OnInitialized()
		{
			var category = _categories.Values.First();
			SelectCategory(category);
		}
		
		public void Hide()
		{
			_selectPanelWidget.DeselectAlpha();
			_panelsContent.gameObject.SetActive(false);
		}
		
		public void Select()
		{
			_selectPanelWidget.SelectAlpha();
			_panelsContent.gameObject.SetActive(true);
		}
		
		public ElementPresenterItem CreateElement(
			ElementItemPanel element, 
			ObjectData objectData, 
			OutlineButtonWidget viewSelectItemWidget,
			ISpawnMenuItemFactory spawnMenuItemFactory)
		{
			var categoriesHolder = _categories[element.CategoryId].CategoriesHolder;
			var selectElementWidget = Object.Instantiate(viewSelectItemWidget, categoriesHolder);
			var selectElementPresenter = spawnMenuItemFactory.Create(element, selectElementWidget, objectData).AddTo(_compositeDisposable);
			
			_itemPresenters.Add(selectElementPresenter.Id, selectElementPresenter);
			return selectElementPresenter;
		}

		public void AddCategory(ItemCategory categoryData, NamedButtonWidget categoryWidget, Transform categoryElementsHolder)
		{
			var presenter = new CategoryPresenter(this, categoryData, categoryWidget, categoryElementsHolder)
				.AddTo(_compositeDisposable);
			presenter.Deselect();
			
			_categories.Add(categoryData.CategoryId, presenter);
		}
		
		public void SelectCategory(CategoryPresenter category)
		{
			if (category == _selectedCategory)
				return;
			
			_selectedCategory?.Deselect();
			_selectedCategory = category;
			_selectedCategory.Select();
		}
		
		private void OnClickPanelWidget(Unit obj)
		{
			_spawnMenuController.SelectPanel(this);
		}
		
		public void Dispose()
		{
			_compositeDisposable.Dispose();	
		}
	}
}