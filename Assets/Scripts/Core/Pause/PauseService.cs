using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Core.Pause
{
	public struct PauseArgs
	{
		public bool? IsAdv;
		public bool? IsBackground;
	}

	public interface IPauseService
	{
		public bool IsPaused { get; }
		public IReadOnlyReactiveProperty<bool> IsPauseObservable { get; }
		
		public bool TrySubscribe(IPauseObserver observer, out IDisposable subscription);
		public IDisposable Subscribe(IPauseObserver observer);
		public void SetPause(ref PauseArgs args);
	}

	public interface IPauseObserver
	{
		public void OnPause(bool isPaused, in PauseArgs args);
	}

	public class PauseService : IPauseService, IDisposable
	{
		private readonly List<IPauseObserver> _subscribers = new List<IPauseObserver>();
		private readonly List<Subscription> _subscriptions = new List<Subscription>();
		private PauseArgs _pauseArgs = new PauseArgs
		{
			IsAdv = false,
			IsBackground = false,
		};


		private readonly BoolReactiveProperty _isPauseReactive = new BoolReactiveProperty();
		[ShowInInspector]
		public bool IsPaused => _pauseArgs.IsAdv.Value || _pauseArgs.IsBackground.Value;
		public IReadOnlyReactiveProperty<bool> IsPauseObservable => _isPauseReactive;
		

		private class Subscription : IDisposable
		{
			private readonly Action<IPauseObserver, Subscription> _unsubscribe;
			private readonly IPauseObserver _subscriber;

			public Subscription(IPauseObserver subscriber, Action<IPauseObserver, Subscription> unsubscribe)
			{
				_unsubscribe = unsubscribe;
				_subscriber = subscriber;
			}

			public void Dispose() => _unsubscribe(_subscriber, this);
		}

		public bool TrySubscribe(IPauseObserver observer, out IDisposable subscription)
		{
			subscription = null;

			if (_subscribers.Contains(observer))
				return false;
			
			subscription = Subscribe(observer);

			return true;
		}
		
		public IDisposable Subscribe(IPauseObserver observer)
		{
			if (_subscribers.Contains(observer))
				return null;
			
			var subscription = new Subscription(observer, Unsubscribe);

			_subscribers.Add(observer);
			_subscriptions.Add(subscription);

			return subscription;
		}

		public void SetPause(ref PauseArgs args)
		{
			if (args.IsAdv.HasValue)
				_pauseArgs.IsAdv = args.IsAdv;

			if (args.IsBackground.HasValue)
				_pauseArgs.IsBackground = args.IsBackground;
			
			// TODO:
			// Remove timeScale and AudioListener changing
			Time.timeScale = IsPaused ? 0 : 1;
			AudioListener.pause = IsPaused;

			_isPauseReactive.Value = IsPaused;
			Notify();
		}

		public void Dispose()
		{
			foreach (var subscription in _subscriptions)
				subscription?.Dispose();
			
			_isPauseReactive.Dispose();
		}

		private void Notify()
		{
			foreach (var subscriber in _subscribers)
				subscriber.OnPause(IsPaused, in _pauseArgs);
		}

		private void Unsubscribe(IPauseObserver subscriber, Subscription subscription)
		{
			_subscribers.Remove(subscriber);
			_subscriptions.Remove(subscription);
		}
	}
}
