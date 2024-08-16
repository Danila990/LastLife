using System;
using Core.Quests.Tree.Node;
using Db.Quests;
using UniRx;
using UnityEngine;

namespace Ui.Sandbox.Quests.Views.Single
{

	public interface IQuestPresenter<out TView> : IDisposable where TView : QuestView
	{
		public TView View { get; }
	}
	
	public abstract class QuestPresenter<TView> : IQuestPresenter<TView> where TView : QuestView
	{
		private readonly TView _view;
		protected readonly QuestData QuestData;
		private readonly IDisposable _subscription;
		public TView View => _view;
		
		public QuestPresenter(TView view, QuestData questData, IReactiveTreeNode treeNode, CompositeDisposable disposable)
		{
			_view = view;
			QuestData = questData;
			SetContent();

			_subscription = treeNode.Subscribe(OnQuestStateChanged).AddTo(disposable);
		}

		private void SetContent()
		{
			_view.Description.text = QuestData.MainData.Description;
			_view.Icon.sprite = QuestData.MainData.Icon;
			_view.IconShadow.sprite = QuestData.MainData.Icon;
			if (QuestData.MainData.DisplayData.OverrideForList)
			{
				var overridenW = QuestData.MainData.DisplayData.OverridenListWidth;
				var iconD = _view.Icon.rectTransform.sizeDelta;
				
				_view.Icon.rectTransform.sizeDelta = new(overridenW, iconD.y);
				_view.IconShadow.rectTransform.sizeDelta = new(overridenW, iconD.y);
			}	
			
			var rotation = QuestData.MainData.DisplayData.Rotation;
			_view.Icon.rectTransform.eulerAngles = rotation;
			_view.IconShadow.rectTransform.eulerAngles = rotation;
			
			SetContentInternal();
		}

		protected abstract void SetContentInternal();
		protected abstract void OnQuestStateChanged(ReactiveIntNodeArgs args);

		public void Dispose()
		{
			_subscription?.Dispose();
		}

	}
}
