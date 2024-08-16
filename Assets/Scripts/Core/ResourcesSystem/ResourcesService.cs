using System;
using System.Collections.Generic;
using Analytic;
using Core.Fuel;
using Core.ResourcesSystem.Impl;
using Core.ResourcesSystem.Interfaces;
using SharedUtils.PlayerPrefs;
using Sirenix.OdinInspector;
using Ticket;
using UniRx;
using VContainer.Unity;

namespace Core.ResourcesSystem
{
	public class ResourcesService : IResourcesService, IInitializable, IDisposable
	{
		private readonly IPlayerPrefsManager _playerPrefsManager;
		private readonly IAnalyticService _analyticService;
		private readonly TicketResourceProvider _ticketResourceProvider;
		private readonly FuelResourceProvider _fuelResourceProvider;

		private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

		[ShowInInspector]
		private readonly Dictionary<ResourceType, IResourceProvider> _resourceProviders = new Dictionary<ResourceType, IResourceProvider>();

		public ResourcesService(
			IPlayerPrefsManager playerPrefsManager,
			ITicketService ticketService, 
			IFuelService fuelService, 
			IAnalyticService analyticService)
		{
			_playerPrefsManager = playerPrefsManager;
			_analyticService = analyticService;

			_ticketResourceProvider = new TicketResourceProvider(ticketService);
			_fuelResourceProvider = new FuelResourceProvider(fuelService);
		}
		
		public void Initialize()
		{
			InitializeSimpleResource(ResourceType.Oil);
			InitializeSimpleResource(ResourceType.GoldTicket);
			InitializeSimpleResource(ResourceType.GoldTicketInBank);
			_resourceProviders.Add(ResourceType.Ticket, _ticketResourceProvider);
			_resourceProviders.Add(ResourceType.Fuel, _fuelResourceProvider);
		}

		private void InitializeSimpleResource(ResourceType resourceType)
		{
			var resource = new SimpleGenericResourceProvider(resourceType, _playerPrefsManager).AddTo(_compositeDisposable);
			_resourceProviders.Add(resourceType, resource);
		}
		
		public int GetCurrentResourceCount(ResourceType resourceType)
		{
			if (!_resourceProviders.TryGetValue(resourceType, out var resourceProvider))
				throw new KeyNotFoundException("Resource provider not found for resource type: " + resourceType);
			
			return resourceProvider.GetCurrentResourceCount();
		}
		
		public IObservable<int> GetResourceObservable(ResourceType resourceType)
		{
			if (!_resourceProviders.TryGetValue(resourceType, out var resourceProvider))
				throw new KeyNotFoundException("Resource provider not found for resource type: " + resourceType);
			
			return resourceProvider.GetResourceObservable();
		}
		
		public void AddResource(ResourceType resourceType, int amount, ResourceEventMetaData resourceEventMetaData)
		{
			if (!_resourceProviders.TryGetValue(resourceType, out var resourceProvider))
				throw new KeyNotFoundException("Resource provider not found for resource type: " + resourceType);
			
			_analyticService.SendResourceEvent(
				ResourceEventType.Add, 
				ResourceToString(resourceType),
				amount, 
				resourceEventMetaData.ItemType, 
				resourceEventMetaData.ConcreteItemId);
			
			resourceProvider.AddResource(amount);
		}
		
		public bool TrySpendResource(ResourceType resourceType, int amount, ResourceEventMetaData resourceEventMetaData)
		{
			if (!_resourceProviders.TryGetValue(resourceType, out var resourceProvider))
				throw new KeyNotFoundException("Resource provider not found for resource type: " + resourceType);
			
			var canSpend = resourceProvider.TrySpendResource(amount);
			
			if (canSpend)
			{
				_analyticService.SendResourceEvent(
					ResourceEventType.Remove, 
					ResourceToString(resourceType),
					amount, 
					resourceEventMetaData.ItemType, 
					resourceEventMetaData.ConcreteItemId);
			}
			
			return canSpend;
		}
		
		public void Dispose()
		{
			_compositeDisposable?.Dispose();
		}

		public string ResourceToString(ResourceType resourceType)
		{
			return resourceType switch
			{
				ResourceType.Oil => "Oil",
				ResourceType.Fuel => "Fuel",
				ResourceType.Ticket => "Ticket",
				ResourceType.GoldTicket => "GoldTicket",
				ResourceType.GoldTicketInBank => "GoldTicketInBank",
				_ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null)
			};
		}
	}
}