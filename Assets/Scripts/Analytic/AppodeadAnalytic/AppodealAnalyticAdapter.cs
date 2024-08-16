using System;
using System.Collections.Generic;
using System.Globalization;
#if RELEASE_BRANCH
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
#endif
using UnityEngine;
using UnityEngine.Purchasing;

namespace Analytic.AppodeadAnalytic
{
#if RELEASE_BRANCH
	public class AppodealAnalyticAdapter : IBusinessAnalyticAdapter, IInAppPurchaseValidationListener
	{
		private const string FAKE_RECEIPT_DATA = "ThisIsFakeReceiptData";
		
		public void Send(string args)
		{
		}

		public void SendBusinessEvent(PurchaseEventArgs args)
		{
			var product = args.purchasedProduct;
			var currency = product.metadata.isoCurrencyCode;
			var price = product.metadata.localizedPrice.ToString(CultureInfo.InvariantCulture);
			var additionalParams = new Dictionary<string, string> { };
			Debug.Log("purchased product: " + product.definition.id);
			
			
			if(product.hasReceipt) {
				
#if UNITY_ANDROID
				Receipt receipt = JsonUtility.FromJson<Receipt>(product.receipt);
				if (string.IsNullOrEmpty(receipt.Payload) || receipt.Payload == FAKE_RECEIPT_DATA)
				{
					Debug.LogWarning("Payload is invalid: " + receipt.Payload);
					return;
				}

				PayloadAndroid payloadAndroid = JsonUtility.FromJson<PayloadAndroid>(receipt.Payload);
				
				var purchase = new PlayStoreInAppPurchase
						.Builder(GetPurchaseType(product.definition))
						.WithPublicKey("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyH/ZfZq65+2z28/FVWzVklrya6IUdHtKM40VDtOpekx8y8uT3GCU2bSr1trGM/3xgqxoVcNnNsQT5jNCBHPc2Pe0QtZHeP5HxYPr5xYiCo5iJ//qPKxAKESwqqw95I/J+WerX/5FXHqnzM7caKYMdb0j6HbrHrAe90GYE5C1x0ZWz8pn2m8hH0GNdY3dA3T5hyAgvMg5Ln9zKlyecL3HgI8+4i6q8xx75PRQkjl8HPReWzFN2seooKwfY8KnRcdQwGC89mWob+mPUBTdqSmgz7IE/LaOkPgCLlL/PttEOUroGmo6NZV6xdZqxyCeXZhefUBKJF9KSek9Ze4e0Pi1gwIDAQAB")
						.WithSignature(payloadAndroid.Signature)
						.WithPurchaseData(payloadAndroid.Json)
						.WithPrice(price.ToString(CultureInfo.InvariantCulture))
						.WithCurrency(currency)
						.WithPurchaseTimestamp(DateTime.Now.Ticks)
						.WithAdditionalParameters(additionalParams)
						.Build();

				Appodeal.ValidatePlayStoreInAppPurchase(purchase, this);
#elif UNITY_IPHONE
            	var applePurchase = new AppStoreInAppPurchase
						.Builder(GetApplePurchaseType(product.definition))
						.WithCurrency(currency)
						.WithPrice(price.ToString(CultureInfo.InvariantCulture))
						.WithProductId(product.definition.id)
						.WithTransactionId(product.transactionID)
						.WithAdditionalParameters(additionalParams)
						.Build();

				Appodeal.ValidateAppStoreInAppPurchase(applePurchase, this);
#endif
			}
			else
			{
				
				Debug.Log("has no receipt");
			}
		}

		private static PlayStorePurchaseType GetPurchaseType(ProductDefinition product)
		{
			return product.type switch
			{
				ProductType.Consumable => PlayStorePurchaseType.InApp,
				ProductType.NonConsumable => PlayStorePurchaseType.InApp,
				ProductType.Subscription => PlayStorePurchaseType.Subs,
				_ => PlayStorePurchaseType.InApp,
			};
		}
		
		private static AppStorePurchaseType GetApplePurchaseType(ProductDefinition product)
		{
			return product.type switch
			{
				ProductType.Consumable => AppStorePurchaseType.Consumable,
				ProductType.NonConsumable => AppStorePurchaseType.NonConsumable,
				ProductType.Subscription => AppStorePurchaseType.NonRenewingSubscription,
			};
		}
		
		public void OnInAppPurchaseValidationSucceeded(string json)
		{
			Debug.Log("In-App Purchase Validation Succeeded\n" + json);
		}
		
		public void OnInAppPurchaseValidationFailed(string json)
		{
			Debug.LogWarning("In-App Purchase Validation Failed\n" + json);
		}
	}
#endif

}

