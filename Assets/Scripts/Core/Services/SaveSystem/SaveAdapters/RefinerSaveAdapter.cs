using System;
using System.Collections.Generic;
using Core.Carry;
using Core.Factory;
using Core.ResourcesSystem;
using Core.Timer;
using Cysharp.Threading.Tasks;
using Market.OilRig;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.SaveSystem.SaveAdapters
{
	public class RefinerSaveAdapter : IAutoLoadAdapter
	{
		private readonly IRefineProvider _refineProvider;
		private readonly ITimerProvider _timerProvider;
		private readonly IObjectFactory _objectFactory;

		public bool CanSave => true;
		public string SaveKey => "Refiner";
		
		public RefinerSaveAdapter(IRefineProvider refineProvider, ITimerProvider timerProvider, IObjectFactory objectFactory)
		{
			_refineProvider = refineProvider;
			_timerProvider = timerProvider;
			_objectFactory = objectFactory;
		}
		
		public string CreateSave()
		{
			if (!_refineProvider.GetResourceType(ResourceType.Oil, out var list))
				return string.Empty;
			
			var pool = ListPool<RefinerSaveData>.Get();
			foreach (var refinerFactoryContext in list)
			{
				var refinerSave = new RefinerSaveData(refinerFactoryContext.RefinerObjects.Count, refinerFactoryContext.Duration);
				for (var index = 0; index < refinerFactoryContext.RefinerObjects.Count; index++)
				{
					var refiner = refinerFactoryContext.RefinerObjects[index];
					var remainingTimeValue = refiner.Timer is not null ? refiner.Timer.RemainingTime.Value : default(TimeSpan);
					var totalTimeValue = refiner.Timer?.TotalTime ?? default(TimeSpan);
					
					var x = new RefinerObjectSaveData(totalTimeValue, remainingTimeValue, refiner.InProcess);
					refinerSave.RefinerObjects[index] = x;
				}
				pool.Add(refinerSave);
			}
			var result = "";
			
			try
			{
				result = JsonConvert.SerializeObject(pool);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(RefinerSaveAdapter)}]" + e.Message);
			}
			
			ListPool<RefinerSaveData>.Release(pool);
            
			return result;
		}		
		
		public void LoadSave(string value)
		{
			LoadAsync(value).Forget();
		}

		private async UniTaskVoid LoadAsync(string value)
		{
			await UniTask.DelayFrame(1);
			List<RefinerSaveData> saveData = null;
			
			try
			{
				saveData = JsonConvert.DeserializeObject<List<RefinerSaveData>>(value);
			}
			catch (Exception e)
			{
				Debug.LogError($"[{nameof(RefinerSaveAdapter)}]" + e.Message);
			}
			
			if (saveData is null)
			{
				return;
			}
			
			if (!_refineProvider.GetResourceType(ResourceType.Oil, out var type))
				return;
			
			for (var index = 0; index < type.Count; index++)
			{
				var refinerFactoryContext = type[index];
				var refinerSave = saveData[index];
				for (var i = 0; i < refinerSave.RefinerObjects.Length; i++)
				{
					if (!refinerSave.RefinerObjects[i].RefinerInProcess)
						continue;
					refinerFactoryContext.LaunchRefiner(
						refinerFactoryContext.RefinerObjects[i],
						(FuelBarrelContext)_objectFactory.CreateObject("FuelBarrel", Vector3.zero),
						_timerProvider.AddOrGetTimer(
							refinerFactoryContext.RefinerObjects[i].Id, 
							refinerSave.RefinerObjects[i].TotalTime,
							refinerSave.RefinerObjects[i].RemainingTime
						));
				}
			}
		}
		
		[Serializable]
		private struct RefinerSaveData
		{
			public float Duration { get; }
			public RefinerObjectSaveData[] RefinerObjects;
			
			public RefinerSaveData(int refinerObjectsCount, float duration)
			{
				Duration = duration;
				RefinerObjects = new RefinerObjectSaveData[refinerObjectsCount];
			}
		}
		
		[Serializable]
		private struct RefinerObjectSaveData
		{
			public TimeSpan RemainingTime { get; }
			public TimeSpan TotalTime { get; }
			public bool RefinerInProcess { get; }
			
			public RefinerObjectSaveData(TimeSpan totalTime, TimeSpan remainingTime, bool refinerInProcess)
			{
				RemainingTime = remainingTime;
				TotalTime = totalTime;
				RefinerInProcess = refinerInProcess;
			}
		}
	}
}