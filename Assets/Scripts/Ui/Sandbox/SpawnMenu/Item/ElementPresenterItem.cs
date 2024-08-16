using System;
using Db.ObjectData;
using Db.Panel;
using Ui.Widget;
using UniRx;

namespace Ui.Sandbox.SpawnMenu.Item
{
	public abstract class ElementPresenterItem : IDisposable
	{
		public ObjectData ObjectData { get; }
		protected readonly ElementItemPanel ElementData;
		protected readonly ButtonWidget Widget;
		private readonly IDisposable _clickDisposable;
		
		public abstract string Id { get; }
		
		public ElementPresenterItem(ElementItemPanel elementData, ButtonWidget widget, ObjectData objectData)
		{
			ObjectData = objectData;
			ElementData = elementData;
			Widget = widget;
			Widget.IconImg.sprite = objectData.Ico;
			
			_clickDisposable = Widget.Button.OnClickAsObservable().Subscribe(OnClickButton);
		}
		
		public void SetUnlockStatus(bool status)
		{
			Widget.gameObject.SetActive(status);
		}
		
		public abstract void SetBlocked(bool status);
		
		protected abstract void OnClickButton(Unit obj);
		
		public void Dispose()
		{
			_clickDisposable?.Dispose();
			OnDispose();
		}
		protected virtual void OnDispose() { }
	}
}