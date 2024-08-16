using System;

namespace Core.ResourcesSystem.Interfaces
{
	public interface IResourceProvider
	{
		ResourceType ProviderType { get; }
		int GetCurrentResourceCount();
		IObservable<int> GetResourceObservable();
		void AddResource(int amount);
		bool TrySpendResource(int amount);
	}
}