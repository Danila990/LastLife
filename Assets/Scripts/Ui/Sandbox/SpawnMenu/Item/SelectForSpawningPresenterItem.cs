using System;
using Db.ObjectData;
using Db.Panel;
using Ui.Widget;
using UniRx;

namespace Ui.Sandbox.SpawnMenu.Item
{
	public class SelectForSpawningPresenterItem : ElementPresenterItem
	{
		private readonly ISpawnItemService _spawnItemService;
		private readonly IDisposable _subDisposable;
		public override string Id => ElementData.ItemObjectDataId;
		private readonly OutlineButtonWidget _widget;
		private bool _isBlocked;

		public SelectForSpawningPresenterItem(
			ElementItemPanel elementData, 
			OutlineButtonWidget widget, 
			ObjectData objectData, 
			ISpawnItemService spawnItemService
			) : base(elementData, widget, objectData)
		{
			_widget = widget;
			_spawnItemService = spawnItemService;
		}
		
		public void Select()
		{
			_widget.SelectOutline();
		}

		public void Deselect()
		{
			_widget.DeselectOutline();
		}

		public override void SetBlocked(bool status)
		{
			_isBlocked = status;
			if (_isBlocked)
			{
				_widget.DeselectAlpha(alpha: 0.5f);
			}
			else
			{
				_widget.SelectAlpha();
			}
		}
		
		protected override void OnClickButton(Unit obj)
		{
			if (_isBlocked)
				return;
			
			_spawnItemService.SelectItem(ObjectData);
		}
	}
}