using System;
using System.Collections.Generic;
using Core.Entity.Characters;
using Core.InputSystem;
using Core.Services;
using Db.Map;
using MessagePipe;
using VContainer.Unity;

namespace Core.Zones
{
	public class ZoneInteractionService : IStartable, IDisposable, ILateTickable
	{
		private readonly ISubscriber<PlayerContextChangedMessage> _sub;
		private readonly IPublisher<ZoneChangedMessage> _pub;
		private readonly IMapService _mapService;

		private CharacterContext _currentContext;
		private BoundsObject _currentZone;
		
		private IDisposable _disposable;
		
		
		public ZoneInteractionService(
			IMapService mapService,
			ISubscriber<PlayerContextChangedMessage> sub,
			IPublisher<ZoneChangedMessage> pub
			)
		{
			_mapService = mapService;
			_sub = sub;
			_pub = pub;
		}

		public void Start()
		{
			_disposable = _sub.Subscribe(OnContextChanged);
		}

		public void Dispose()
		{
			_disposable?.Dispose();
		}
		
		private void OnContextChanged(PlayerContextChangedMessage msg)
		{
			if (!msg.Created)
			{
				_currentContext = null;
				return;
			}
			_currentZone = null;
			_currentContext = msg.CharacterContext;
		}
		
		public void LateTick()
		{
			if(!_currentContext)
				return;

			var position = _currentContext.MainTransform.position;

			if (_currentZone && !_currentZone.InBounds(position))
			{
				_currentZone = null;
				_pub.Publish(new ZoneChangedMessage(0, _currentContext));
			}

			foreach (var zone in _mapService.MapObject.Zones)
			{
				if (zone.InBounds(position) && zone != _currentZone)
				{
					_currentZone = zone;
					_pub.Publish(new ZoneChangedMessage(zone.ZoneType, _currentContext));
				}
			}
		}
	}

	public struct ZoneChangedMessage
	{
		public readonly ZoneType ZoneType;
		public readonly CharacterContext Context;

		public ZoneChangedMessage(ZoneType zoneType, CharacterContext context)
		{
			ZoneType = zoneType;
			Context = context;
		}
	}
}
