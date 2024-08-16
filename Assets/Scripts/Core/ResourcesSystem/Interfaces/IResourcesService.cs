using System;

namespace Core.ResourcesSystem.Interfaces
{
	public interface IResourcesService
	{
		int GetCurrentResourceCount(ResourceType resourceType);
		IObservable<int> GetResourceObservable(ResourceType resourceType);
		void AddResource(ResourceType resourceType, int amount, ResourceEventMetaData metaData);
		bool TrySpendResource(ResourceType resourceType, int amount, ResourceEventMetaData metaData);
	}

	public static class ResourceItemTypes
	{
		public const string MERCHANT_ITEM_TYPE = "Merchant";
		public const string SHOP_ITEM_TYPE = "Shop";
		public const string OTHERS = "Others";
	}
	
	public static class ResourceItemIds
	{
		public const string BANKER_ITEM_ID = "Banker";
		public const string REPAIRMAN_ITEM_ID = "RepairMan";
		public const string SUGAR_BABY_ITEM_ID = "SugarBaby";
		public const string TICKET_WATCHED_ITEM_ID = "TicketWatched";
	}

	public readonly struct ResourceEventMetaData
	{
		/// <summary>
		/// Type of Item
		/// <example>Merchant</example>
		/// </summary>
		public readonly string ItemType;
		/// <summary>
		/// MerchantId
		/// <example>Banker</example>
		/// </summary>
		public readonly string ConcreteItemId;
		
		public ResourceEventMetaData(string itemType, string concreteItemId)
		{
			ItemType = itemType;
			ConcreteItemId = concreteItemId;
		}
	} 
}