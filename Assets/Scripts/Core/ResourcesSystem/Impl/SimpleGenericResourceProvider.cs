using System;
using Core.ResourcesSystem.Interfaces;
using SharedUtils.PlayerPrefs;
using Sirenix.OdinInspector;
using UniRx;

namespace Core.ResourcesSystem.Impl
{

	public class SimpleGenericResourceProvider : IResourceProvider, IDisposable
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		[ShowInInspector]
		private readonly IntReactiveProperty _currentResourceCount;
		public ResourceType ProviderType { get; }

		public SimpleGenericResourceProvider(ResourceType providerType, IPlayerPrefsManager playerPrefsManager)
		{
			_playerPrefsManager = playerPrefsManager;
			ProviderType = providerType;
			_currentResourceCount = new IntReactiveProperty(_playerPrefsManager.GetValue<int>(GetPrefsKey(), 0));
		}

		public int GetCurrentResourceCount() => _currentResourceCount.Value;
		
		public IObservable<int> GetResourceObservable() => _currentResourceCount;
		
		[Button]
		public void AddResource(int amount)
		{
			_currentResourceCount.Value += amount;
			SaveResourceCount();
		}
		
		[Button]
		public bool TrySpendResource(int amount)
		{
			if (_currentResourceCount.Value >= amount)
			{
				_currentResourceCount.Value -= amount;
				SaveResourceCount();
				return true;
			}
			return false;
		}
		
		private string GetPrefsKey()
			=> $"{ProviderType.ToString()}_ResourceCount";
		
		[Button]
		private void SaveResourceCount()
			=> _playerPrefsManager.SetValue(GetPrefsKey(), GetCurrentResourceCount());
		
		public void Dispose()
		{
			_currentResourceCount?.Dispose();
		}
	}
}