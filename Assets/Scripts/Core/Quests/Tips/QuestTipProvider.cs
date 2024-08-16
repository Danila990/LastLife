using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Entity.Repository;
using Core.Quests.Tips.Impl;
using Core.Quests.Tips.Impl.Interfaces;
using Cysharp.Threading.Tasks;
using Db.Quests;
using MessagePipe;
using Ui.Sandbox.Quests.Views.Widgets;
using UniRx;
using UnityEngine;
using Utils;
using VContainer;
using VContainer.Unity;

namespace Core.Quests.Tips
{
	public class QuestTipProvider : IStartable, IDisposable
	{
		private readonly IMainQuestSelector _questSelector;
		private readonly QuestWidgetController _questWidgetController;
		private readonly IQuestTipData _questTipData;
		private readonly ITipObjectsData _tipObjectsData;
		private readonly IQuestTipService _tipService;
		private readonly IQuestService _questService;
		private readonly IObjectResolver _resolver;
		private readonly CancellationToken _token;
		private readonly Dictionary<string, QuestTipPool> _pools;

		private string _activeQuestInlineId;
		private QuestTipPool _activePool;
		
		public QuestTipProvider(
			IMainQuestSelector questSelector,
			QuestWidgetController questWidgetController,
			IQuestTipData questTipData,
			ITipObjectsData tipObjectsData,
			IQuestTipService tipService,
			IQuestService questService,
			IObjectResolver resolver,
			InstallerCancellationToken installerCancellationToken
			)
		{
			_pools = new();
			
			_questSelector = questSelector;
			_questWidgetController = questWidgetController;
			_questTipData = questTipData;
			_tipObjectsData = tipObjectsData;
			_tipService = tipService;
			_questService = questService;
			_resolver = resolver;
			_token = installerCancellationToken.Token;
		}
		
		public void Start()
		{
			Prewarm();
			_questWidgetController.QuestPresenter.OnMainQuestHided.Subscribe(_ => HideTips()).AddTo(_token);
			_questWidgetController.QuestPresenter.OnMainQuestDisplayed.Subscribe(OnMainQuestChanged).AddTo(_token);
			_tipService.OnTipAdded.Subscribe(OnTipAdded).AddTo(_token);
		}

		private void Prewarm()
		{
			var parent = new GameObject("--- TipObjectsHolder ---");
			foreach (var tipObjectData in _tipObjectsData.TipsData)
			{
				var pool = new QuestTipPool(tipObjectData.Prefab, parent.transform, _resolver);
				_pools[tipObjectData.Id] = pool;
				pool.Prewarm(tipObjectData.PrewarmCount);
			}
		}

		private void OnTipAdded(IQuestTip questTip)
		{
			if(questTip == null)
				return;
			
			ShowTip(questTip);
		} 
		
		private void OnMainQuestChanged(QuestData questData)
		{
			HideTips();

			if (questData == null || _activeQuestInlineId == questData.ImplementationData.InlineId)
				return;
			
			_activeQuestInlineId = questData.ImplementationData.InlineId;
			var tipObjectId = _questTipData.TipsData.FirstOrDefault(x => x.QuestInlineId == questData.ImplementationData.InlineId).TipObjectId;

			tipObjectId = string.IsNullOrEmpty(tipObjectId) ? _questTipData.FallbackId : tipObjectId;

			if (_pools.TryGetValue(tipObjectId, out var pool))
				ShowTips(questData, pool);
		}

		private void ShowTips(QuestData questData, QuestTipPool pool)
		{
			_activePool = pool;

			if (!_tipService.TipObjects.TryGetValue(questData.ImplementationData.InlineId, out var questTips))
				return;
			
			foreach (var questTip in questTips)
				ShowTip(questTip);
		}
		
		private void ShowTip(IQuestTip questTip)
		{
			if(questTip.Origin == null)
				return;
			
			if(!questTip.QuestInlineIds.Contains(_activeQuestInlineId))
				return;
			
			var instance = _activePool.Rent();
			InitializeTip(instance, questTip);
		}

		private void InitializeTip(QuestTipObject tipObject, IQuestTip questTip)
		{
			if (tipObject is ITrackedTip trackedTip)
			{
				trackedTip.SetTrackedTarget(questTip.Origin, questTip.Offset, questTip.Rotation);
			}
		}

		private void HideTips()
		{
			_activePool?.ReturnAll();
			_activePool = null;
		}
		
		public void Dispose()
		{
			foreach (var pool in _pools.Values)
			{
				pool.Dispose();
			}
		}
	}
}
