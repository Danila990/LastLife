using System;
using Core.Quests.Tree.Node;
using Db.Quests;
using UniRx;

namespace Ui.Sandbox.Quests.Views.Single
{
	public class IntQuestPresenter : QuestPresenter<IntQuestView>
	{
		
		public IntQuestPresenter(
			IntQuestView view,
			QuestData questData,
			IReactiveTreeNode observable,
			CompositeDisposable disposable
		)
			: base(view, questData, observable, disposable)
		{
		}
		
		protected override void SetContentInternal()
		{
			View.CompletionInfo.text = string.Format(View.Format, QuestData.ImplementationData.FinalNode.InitialValue, QuestData.ImplementationData.FinalNode.Value);
		}
		protected override void OnQuestStateChanged(ReactiveIntNodeArgs args)
		{
			View.CompletionInfo.text = string.Format(View.Format, args.Value, args.Node.MaxValue);

		}
	}
}
