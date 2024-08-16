using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace Analytic
{
	public class AnalyticService : IAnalyticService
	{
		private readonly IEnumerable<IAnalyticAdapter> _analyticAdapters;
		private readonly List<IProgressionAnalyticAdapter> _progressionAnalytics = new List<IProgressionAnalyticAdapter>(3);
		private readonly List<IBusinessAnalyticAdapter> _businessAnalyticAdapters = new List<IBusinessAnalyticAdapter>(3);
		private readonly List<IAdvertisingAnalyticAdapter> _advertisingAnalytic = new List<IAdvertisingAnalyticAdapter>(1);
		private readonly List<IResourceAnalyticAdapter> _resourceAnalytic = new List<IResourceAnalyticAdapter>(1);
		private readonly Dictionary<Type, IAnalyticAdapter> _adapters = new();

		public AnalyticService(IEnumerable<IAnalyticAdapter> analyticAdapters)
		{
			_analyticAdapters = analyticAdapters;
			
			foreach (var adapter in _analyticAdapters)
			{
				if (adapter is IProgressionAnalyticAdapter progressionAnalyticAdapter)
				{
					_progressionAnalytics.Add(progressionAnalyticAdapter);
				}
				if (adapter is IBusinessAnalyticAdapter businessAnalyticAdapter)
				{
					_businessAnalyticAdapters.Add(businessAnalyticAdapter);
				}
				if (adapter is IAdvertisingAnalyticAdapter advertisingAnalyticAdapter)
				{
					_advertisingAnalytic.Add(advertisingAnalyticAdapter);
				}
				if (adapter is IResourceAnalyticAdapter res)
				{
					_resourceAnalytic.Add(res);
				}
			}
		}
		
		public void SendEvent(string args)
		{
			foreach (var sender in _analyticAdapters)
			{
				sender.Send(args);
			}
		}
		
		public void SendEvent(string args, float value)
		{
			foreach (var sender in _analyticAdapters)
			{
				sender.Send(args, value);
			}
		}
		
		public void SendResourceEvent(ResourceEventType eventType, string currency, float amount, string itemType, string itemId)
		{
			foreach (var sender in _resourceAnalytic)
			{
				sender.SendResourceEvent(eventType, currency, amount, itemType, itemId);
			}
		}

		public void SendADEvent(in ADEventArgs args)
		{
			foreach (var sender in _advertisingAnalytic)
			{
				sender.SendADEvent(in args);
			}
		}

		public void SendProgression(ProgressionEventStatus status, string progression01)
		{
			foreach (var sender in _progressionAnalytics)
			{
				sender.SendProgression(status, progression01);
			}
		}

		public void SendProgression(ProgressionEventStatus status, string progression01, string progression02)
		{
			foreach (var sender in _progressionAnalytics)
			{
				sender.SendProgression(status, progression01, progression02);
			}
		}

		public void SendBusinessEvent(PurchaseEventArgs args)
		{
			foreach (var sender in _businessAnalyticAdapters)
			{
				sender.SendBusinessEvent(args);
			}
		}
		private void AddAdapter<T>(T adapter) where T : IAnalyticAdapter
		{
			_adapters.Add(adapter.GetType(), adapter);
		}

		private void AddAdapter<T, TAsType>(T adapter)
			where T : IAnalyticAdapter
			where TAsType : IAnalyticAdapter
		{
			_adapters.Add(typeof(TAsType), adapter);
		}

		public bool TryGetAdapter<T>(out T adapter)
			where T : class, IAnalyticAdapter
		{
			var type = typeof(T);
			if (_adapters.TryGetValue(type, out var a))
			{
				adapter = a as T;
				return true;
			}

			adapter = null;
			return false;
		}
	}
}