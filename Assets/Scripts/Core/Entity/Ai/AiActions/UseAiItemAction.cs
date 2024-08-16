using System;
using Core.Entity.Ai.AiItem;
using NodeCanvas.Framework;
using UnityEngine;

namespace Core.Entity.Ai.AiActions
{
	public class UseAiItemAction : ActionTask
	{
		public BBParameter<IAiItem> SelectedItem;
		public BBParameter<IAiTarget> AiTarget;
		private IAiItem _item;
		private IAiItemUseListener _itemUseListener;

		protected override string OnInit()
		{
			agent.TryGetComponent(out _itemUseListener);
			return null;
		}

		protected override void OnExecute()
		{
			_item = SelectedItem.value;
			if (_item is null)
			{
				EndAction(false);
				return;
			}
			_item.Use(AiTarget.value);
			_itemUseListener?.OnUse(_item, AiTarget.value);
		}

		protected override void OnUpdate() 
		{
			if (!_item.AiItemData.SelfEnd && elapsedTime >= _item.UseActionDuration || !_item.InUse) {
				EndAction();
				return;
			}
			var deltaTime = Time.deltaTime;
			_item.Tick(ref deltaTime);
			
			if(elapsedTime >= _item.UseActionDuration)
				EndAction(true);
		}

		protected override void OnStop(bool status)
		{
			_item?.EndUse(status);
			_itemUseListener?.OnEndUse(_item);
			_item = null;
			SelectedItem.value = null;
		}
	}
}