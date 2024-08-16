using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Entity.Repository;
using Core.Quests.Tips.Impl.Interfaces;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using Utils;
using VContainer.Unity;

namespace Core.Quests.Tips
{

	public interface IQuestTipService
	{

		public IReactiveCommand<IQuestTip> OnTipAdded { get; }
		public IReadOnlyDictionary<string, List<IQuestTip>> TipObjects { get; }
		public void AddTip(IQuestTip questTip, string inlineId);
		public void RemoveTip(IQuestTip questTip, string inlineId);
		
		public void AddTip(IQuestTip questTip);
		public void RemoveTip(IQuestTip questTip);
	}
	
	public class QuestTipService : IQuestTipService, IInitializable, IDisposable
	{
		private readonly Dictionary<string, List<IQuestTip>> _tips;
		private readonly ISubscriber<RepositoryEvent> _subscriber;
		private readonly CancellationToken _token;
		private readonly ReactiveCommand<IQuestTip> _onTipAdded;
		public IReadOnlyDictionary<string, List<IQuestTip>> TipObjects => _tips;
		public IReactiveCommand<IQuestTip> OnTipAdded => _onTipAdded;

		public QuestTipService(ISubscriber<RepositoryEvent> subscriber, InstallerCancellationToken installerCancellationToken)
		{
			_tips = new();
			_onTipAdded = new ();
				
			_subscriber = subscriber;
			_token = installerCancellationToken.Token;
		}
		public void Initialize()
		{
			_subscriber.Subscribe(OnRepositoryEvent).AddTo(_token);
		}

		private void OnRepositoryEvent(RepositoryEvent repEvent)
		{
			if(repEvent.EntityContext.QuestTip == null)
				return;

			var questTip = repEvent.EntityContext.QuestTip;
			if (!questTip.Origin || questTip.QuestInlineIds == null || questTip.QuestInlineIds.Count == 0)
				return;
			
			switch (repEvent.EvtType)
			{
				case RepositoryEvent.EventType.Add:
					OnRepositoryAddEvent(questTip);
					break;
				case RepositoryEvent.EventType.Remove:
					OnRepositoryRemoveEvent(questTip);
					break;
			}
		}

		private void OnRepositoryAddEvent(IQuestTip questTip)
		{
			if (questTip.QuestInlineIds.Count == 1)
			{
				AddTip(questTip, questTip.QuestInlineIds.First());
				return;
			}
			
			AddTip(questTip);
		}

		private void OnRepositoryRemoveEvent(IQuestTip questTip)
		{
			if (questTip.QuestInlineIds.Count == 1)
			{
				RemoveTip(questTip, questTip.QuestInlineIds.First());
				return;
			}

			RemoveTip(questTip);
		}

		public void AddTip(IQuestTip questTip, string inlineId)
		{
			if (string.IsNullOrEmpty(inlineId))
				return;
			
			if (!_tips.ContainsKey(inlineId))
				_tips[inlineId] = new();

			_tips[inlineId].Add(questTip);
			_onTipAdded.Execute(questTip);
		}
		
		public void RemoveTip(IQuestTip questTip, string inlineId)
		{
			if (string.IsNullOrEmpty(inlineId))
				return;
			
			if (_tips.ContainsKey(inlineId))
				_tips[inlineId].Remove(questTip);
		}
		
		public void AddTip(IQuestTip questTip)
		{
			foreach (var inlineId in questTip.QuestInlineIds)
				AddTip(questTip, inlineId);
		}
		public void RemoveTip(IQuestTip questTip)
		{
			foreach (var inlineId in questTip.QuestInlineIds)
				RemoveTip(questTip, inlineId);
		}

		public void Dispose()
		{
			_tips?.Clear();
		}
	}
}
