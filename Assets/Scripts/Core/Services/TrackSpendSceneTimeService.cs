using System;
using Analytic;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Core.Services
{
	public class TrackSpendSceneTimeService : IInitializable, IDisposable
	{
		private readonly ISceneLoaderService _sceneLoaderService;
		private readonly IAnalyticService _analyticService;
		private IDisposable _disposable;
		private float _timeWhenSceneLoad;

		public TrackSpendSceneTimeService(
			ISceneLoaderService sceneLoaderService,
			IAnalyticService analyticService)
		{
			_sceneLoaderService = sceneLoaderService;
			_analyticService = analyticService;
		}
		
		public void Initialize()
		{
			_disposable = _sceneLoaderService.BeforeSceneChange.Subscribe(BeforeSceneChanged);
			_timeWhenSceneLoad = Time.realtimeSinceStartup;
		}
		
		private void BeforeSceneChanged(SceneChangeEventData data)
		{
			_analyticService.SendEvent($"SceneTimeSpend:{data.SceneFrom}", Time.realtimeSinceStartup - _timeWhenSceneLoad);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}
	}
}