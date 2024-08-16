using System;
using System.Collections.Generic;
using Db.Map;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace Core.Zones
{
	public interface IMapZonesDescriber { }
	
	public abstract class MapZonesDescriber : IMapZonesDescriber, IDisposable, IStartable
	{
		private readonly ISubscriber<ZoneChangedMessage> _sub;
		protected readonly Dictionary<ZoneType, ZoneHandler> Handlers;

		private IDisposable _disposable;
		private readonly Queue<ZoneHandler> _activeHandlers;
		
		public MapZonesDescriber(ISubscriber<ZoneChangedMessage> sub)
		{
			_sub = sub;
			_activeHandlers = new Queue<ZoneHandler>();
			
			Handlers = new();
			CreateHandlers();
		}
		
		protected abstract void CreateHandlers();
		
		public void Start()
		{
			_disposable = _sub.Subscribe(OnZoneChanged);
		}
		
		public void Dispose()
		{
			_disposable?.Dispose();
		}

		
		private void OnZoneChanged(ZoneChangedMessage msg)
		{
			Clear();
			HandleZone(in msg);
		}

		private void Clear()
		{
			while (_activeHandlers.Count > 0)
				_activeHandlers.Dequeue().Reset();
		}

		private void HandleZone(in ZoneChangedMessage msg)
		{
			foreach (var kpv in Handlers)
			{
				if ((msg.ZoneType & kpv.Key) != 0)
				{
					kpv.Value.Handle(in msg);
					_activeHandlers.Enqueue(kpv.Value);
				}
			}
		}
	}

}
