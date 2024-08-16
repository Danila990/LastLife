using System;
using Db.Panel;
using Ui.Sandbox.SpawnMenu.Panel;
using Ui.Widget;
using UniRx;
using UnityEngine;

namespace Ui.Sandbox.SpawnMenu.Category
{
	public class CategoryPresenter : IDisposable
	{
		private readonly SelectMenuPanelPresenter _selectMenuPanelPresenter;
		private readonly ItemCategory _categoryData;
		private readonly NamedButtonWidget _widget;
		private readonly Transform _categoryElementsHolder;
		private readonly IDisposable _disposable;
		
		public Transform CategoriesHolder => _categoryElementsHolder;
		
		public CategoryPresenter(
			SelectMenuPanelPresenter selectMenuPanelPresenter,
			ItemCategory categoryData,
			NamedButtonWidget widget, 
			Transform categoryElementsHolder)
		{
			_selectMenuPanelPresenter = selectMenuPanelPresenter;
			_categoryData = categoryData;
			_widget = widget;
			_categoryElementsHolder = categoryElementsHolder;

			widget.Text.text = categoryData.CategoryId;
			widget.IconImg.sprite = categoryData.Ico;
			_disposable = widget.Button
				.OnClickAsObservable()
				.Subscribe(OnClickCategory);
		}
		
		private void OnClickCategory(Unit obj)
		{
			_selectMenuPanelPresenter.SelectCategory(this);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
		
		public void Select()
		{
			_widget.SelectAlpha();
			_categoryElementsHolder.gameObject.SetActive(true);
		}
		
		public void Deselect()
		{
			_widget.DeselectAlpha();
			_categoryElementsHolder.gameObject.SetActive(false);
		}
	}
}