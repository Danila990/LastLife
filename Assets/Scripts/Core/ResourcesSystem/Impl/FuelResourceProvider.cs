using System;
using Core.Fuel;
using Core.ResourcesSystem.Interfaces;

namespace Core.ResourcesSystem.Impl
{
	public class FuelResourceProvider : IResourceProvider
	{
		private readonly IFuelService _fuelService;
		public ResourceType ProviderType => ResourceType.Fuel;
		public FuelResourceProvider(IFuelService fuelService) => _fuelService = fuelService;
		
		public int GetCurrentResourceCount()
		{
			return _fuelService.TanksCount.Value;
		}
		
		public IObservable<int> GetResourceObservable()
		{
			return _fuelService.TanksCount;
		}
		public void AddResource(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				_fuelService.AddTank(new FuelTank { Fuel = 20 });
			}
		}
		
		public bool TrySpendResource(int amount)
		{
			if (_fuelService.TanksCount.Value < amount)
				return false;
			
			for (int i = 0; i < amount; i++)
			{
				_fuelService.GetTank();
			}
			return true;
		}
	}
}