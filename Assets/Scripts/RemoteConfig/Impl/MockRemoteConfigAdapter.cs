using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SharedUtils;
using UniRx;
using VContainer.Unity;

namespace RemoteConfig.Impl
{
	public class MockRemoteConfigAdapter : IRemoteConfigAdapter, IAsyncStartable, IDisposable
	{
		private readonly ReactiveCommand<bool> _reactiveCommand = new ReactiveCommand<bool>();

		private readonly Dictionary<string, string> _config = new Dictionary<string, string>()
		{
			
		};
		public IObservable<bool> OnConfigUpdated => _reactiveCommand;
		
		public async UniTask StartAsync(CancellationToken cancellation)
		{
			await UniTask.Delay(0.1f.ToSec(), cancellationToken: cancellation);
		}

		public string GetValue(string key, string defaultValue)
		{
			return _config.GetValueOrDefault(key, defaultValue);
		}
		
		public void Dispose()
		{
			_reactiveCommand?.Dispose();
		}
	}

}