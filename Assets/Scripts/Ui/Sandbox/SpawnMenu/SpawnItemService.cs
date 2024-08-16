using System;
using Analytic;
using Core.Entity.InteractionLogic;
using Core.Factory;
using Core.Quests.Messages;
using Db.ObjectData;
using UniRx;
using UnityEngine;

namespace Ui.Sandbox.SpawnMenu
{
	public interface ISpawnItemService
	{
		IReactiveProperty<ObjectData> SelectedItem { get; }
		IObservable<Unit> ObjectSpawn { get; }
		void SelectDefault();
		void SelectItem(ObjectData itemObjectData);
		void Spawn();
	}
	
	public class SpawnItemService : IDisposable, ISpawnItemService
	{
		private readonly IRayCastService _rayCastService;
		private readonly IAdapterStrategyFactory _adapterStrategyFactory;
		private readonly ReactiveProperty<ObjectData> _selectedItem;
		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
		private readonly ReactiveCommand _onObjectSpawn = new();
		private readonly IObjectsData _objectsData;
		private readonly IAnalyticService _analyticService;
		private readonly IQuestMessageSender _questMessageSender;

		public IObservable<Unit>  ObjectSpawn => _onObjectSpawn;
		public IReactiveProperty<ObjectData> SelectedItem => _selectedItem;
		
		public SpawnItemService(
			IRayCastService rayCastService,
			IAdapterStrategyFactory adapterStrategyFactory,
			IObjectsData objectsData,
			IAnalyticService analyticService,
			IQuestMessageSender questMessageSender
			)
		{
			_objectsData = objectsData;
			_analyticService = analyticService;
			_rayCastService = rayCastService;
			_questMessageSender = questMessageSender;
			_adapterStrategyFactory = adapterStrategyFactory;
			_selectedItem = new ReactiveProperty<ObjectData>(objectsData.DefaultSelected.Model).AddTo(_compositeDisposable);
		}

		public void SelectDefault()
		{
			SelectItem(_objectsData.DefaultSelected.Model);
		}
		
		public void SelectItem(ObjectData itemObjectData)
		{
			_selectedItem.Value = itemObjectData;
		}
		
		public void Spawn()
		{
			if (_selectedItem.Value == null)
				return;
			var dir = _rayCastService.RayDir;
			dir.y = 0;
			var rot = Quaternion.FromToRotation(Vector3.forward, dir);
			_adapterStrategyFactory.CreateObject(_selectedItem.Value, _rayCastService.CurrentHitPoint, rot);
			_analyticService.SendEvent($"GraviGun:Spawn:{_selectedItem.Value.Id}");
			_onObjectSpawn?.Execute(); //TODO: Better solution?
			_questMessageSender.SendSpawnMessage(_selectedItem.Value);
		}

		public void Dispose()
		{
			_compositeDisposable?.Dispose();
			_onObjectSpawn?.Dispose();
		}
	}
}