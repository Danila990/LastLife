using System;
using Core.Quests.Tree.Node;
using Db.Quests;
using UniRx;

namespace Ui.Sandbox.Quests.Views.Single
{
	public class BoolQuestPresenter : QuestPresenter<BoolQuestView>
	{
		
		public BoolQuestPresenter(
			BoolQuestView view,
			QuestData questData,
			IReactiveTreeNode observable,
			CompositeDisposable disposable
		)
			: base(view, questData, observable, disposable)
		{
		}
		
		protected override void SetContentInternal()
		{
			//temp
			View.CheckImage.gameObject.SetActive(false);
		}
		protected override void OnQuestStateChanged(ReactiveIntNodeArgs args)
		{
			View.CheckImage.gameObject.SetActive(args.Value >= args.Node.MaxValue);
		}
	}
}
