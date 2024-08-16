using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePipe;
using Shop.Abstract;
using Shop.Messages;
using UnityEngine;
using UnityEngine.Purchasing;
using VContainer.Unity;

namespace Shop
{
	public interface IPurchaseService
	{
		void PurchaseItem(IPurchasable purchasable);
		void RegisterCallBack(string inAppId, IPurchaseCallbackHandler callbackHandler);
	}
	
	public class PurchaseService : IInitializable, IPurchaseService
	{
		private readonly IAPManager _iapManager;
		private readonly IPublisher<PurchaseMessage> _publisher;
		private readonly IInAppDatabase _inAppDatabase;
		private readonly Dictionary<string, List<IPurchaseCallbackHandler>> _shopItems = new Dictionary<string, List<IPurchaseCallbackHandler>>();
		
		public PurchaseService(
			IAPManager iapManager,
			IPublisher<PurchaseMessage> publisher,
			IInAppDatabase inAppDatabase)
		{
			_iapManager = iapManager;
			_publisher = publisher;
			_inAppDatabase = inAppDatabase;
		}
		
		public void Initialize()
		{
			_iapManager.onPurchaseCompleted += OnPurchaseCompleted;
		}
		
		private void Purchased(string definitionID, [CanBeNull] List<IPurchaseCallbackHandler> callbackHandlers)
		{
			var prod = _inAppDatabase.GetProductById(definitionID);
			_publisher.Publish(new PurchaseMessage(prod));
			if (callbackHandlers != null)
			{
				foreach (var callbackHandler in callbackHandlers)
				{
					callbackHandler.OnPurchased(definitionID);
				}
			}
		}
		
		public void RegisterCallBack(string inAppId, IPurchaseCallbackHandler callbackHandler)
		{
			if (_shopItems.TryGetValue(inAppId, out var callbackHandlers))
			{
				callbackHandlers.Add(callbackHandler);
			}
			else
			{
				var listeners = new List<IPurchaseCallbackHandler>(3)
				{
					callbackHandler
				};
				_shopItems[inAppId] = listeners;
			}
		}
		
		private void OnPurchaseCompleted(Product product)
		{
			_shopItems.TryGetValue(product.definition.id, out var callbackHandlers);
			Purchased(product.definition.id, callbackHandlers);

			// if ()
			// {
			// }
			// else
			// {
			// 	Debug.LogError($"Item :{product.definition.id} doesnt registered");
			// }
		}
		

		public void PurchaseItem(IPurchasable purchasable)
		{
			if (purchasable is null)
			{
				Debug.LogError("purchasable is null");
				return;
			}
			
			_iapManager.Purchase(purchasable.InAppId, OnIAPWindowClosed);
		}


		private void OnIAPWindowClosed()
		{
			
		}
	}
	
	public interface IPurchasable
	{
		public string InAppId { get; }
	}

	public interface IPurchaseCallbackHandler
	{
		void OnPurchased(string item);
	}
}