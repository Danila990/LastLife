using System;
using System.Collections.Generic;
using Analytic;
using Shop.Models.Product;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using PurchaseEventArgs = UnityEngine.Purchasing.PurchaseEventArgs;

namespace Shop
{
	public class IAPManager : IDetailedStoreListener
	{
		private readonly IAnalyticService _analyticService;

		public enum State { PendingInitialize, Initializing, SuccessfullyInitialized, FailedToInitialize };

		public IStoreController StoreController => _storeController;
		
		private State _initializationState = State.PendingInitialize;
		public State InitializationState { get { return _initializationState; } }
		public bool IsInitialized { get { return _initializationState == State.SuccessfullyInitialized; } }

		public delegate void InitializationCallback( bool success );
		private InitializationCallback _onInitialized;
		public event InitializationCallback OnInitializedEvt
		{
			add
			{
				if( _initializationState is State.SuccessfullyInitialized or State.FailedToInitialize )
					value?.Invoke( _initializationState == State.SuccessfullyInitialized );
				else
					_onInitialized += value;
			}
			remove { _onInitialized -= value; }
		}

		public delegate void CompletedPurchaseCallback( Product product );
		public CompletedPurchaseCallback onPurchaseCompleted;

		public delegate void FailedPurchaseCallback( Product product, PurchaseFailureReason failureReason );
		public FailedPurchaseCallback onPurchaseFailed;

		public delegate void NativeIAPWindowClosedCallback();
		private NativeIAPWindowClosedCallback _onIAPWindowClosed;

		public delegate void NativeRestoreWindowClosedCallback( bool success );
		private NativeRestoreWindowClosedCallback _onRestoreWindowClosed;

		private IStoreController _storeController;
		private IExtensionProvider _storeExtensions;
		private CrossPlatformValidator _purchaseValidator;

		public IAPManager(IAnalyticService analyticService)
		{
			_analyticService = analyticService;
		}
		
		public void Initialize()
		{
			Initialize( null, false );
		}
		public void Initialize(IList<InAppProduct> products)
		{
			Initialize( products, false );
		}
		
		// public void Initialize( params ProductDefinition[] products )
		// {
		// 	Initialize( products, false );
		// }
		//
		// public void Initialize( IEnumerable<ProductDefinition> products )
		// {
		// 	Initialize( products, false );
		// }

		private void Initialize(IList<InAppProduct> products, bool initializeWithIAPCatalog )
		{
			if( _initializationState != State.PendingInitialize )
			{
				Debug.LogWarning( "IAP is already initializing!" );
				return;
			}

#if UNITY_EDITOR
			// Allows simulating failed IAP transactions in the Editor
			StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
			var builder = ConfigurationBuilder.Instance( StandardPurchasingModule.Instance() );
			if (initializeWithIAPCatalog)
			{
				IAPConfigurationHelper.PopulateConfigurationBuilder( ref builder, ProductCatalog.LoadDefaultCatalog() );
			}
			else if (products != null)
			{
				foreach (var product in products)
				{
					IDs ids = null;
					foreach (var specificId in product.SpecificIds)
					{
						ids ??= new IDs();
						ids.Add(specificId.SpecificId, specificId.StoresIds);
					}
					builder.AddProduct(product.InAppID, product.ProductType, ids);
				}
			}

			if( StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay )
				builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener( OnDeferredPurchase );

			_initializationState = State.Initializing;
			UnityPurchasing.Initialize( this, builder );
		}

		public void Purchase( string productID, NativeIAPWindowClosedCallback onIAPWindowClosed = null )
		{
			if( !IsInitialized )
			{
				Debug.LogWarning( "IAP isn't initialized yet, can't purchased items!" );
				onIAPWindowClosed?.Invoke();

				return;
			}

			_onIAPWindowClosed = onIAPWindowClosed;
			_storeController.InitiatePurchase( productID );
		}

		public void RestorePurchases( NativeRestoreWindowClosedCallback onRestoreWindowClosed = null )
		{
			if( !IsInitialized )
			{
				Debug.LogWarning( "IAP isn't initialized yet, can't restore purchases!" );
				onRestoreWindowClosed?.Invoke( false );

				return;
			}

			_onRestoreWindowClosed = onRestoreWindowClosed;

			switch( StandardPurchasingModule.Instance().appStore )
			{
				case AppStore.AppleAppStore: _storeExtensions.GetExtension<IAppleExtensions>().RestoreTransactions( OnNativeRestoreWindowClosed ); break;
				case AppStore.GooglePlay: _storeExtensions.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions( OnNativeRestoreWindowClosed ); break;
			}
		}

		public bool IsNonConsumablePurchased( string productID )
		{
			if( !IsInitialized )
			{
				Debug.LogWarning( "IAP isn't initialized yet, can't check previous purchases!" );
				return false;
			}

			if( string.IsNullOrEmpty( productID ) )
			{
				Debug.LogWarning( "Empty productID is passed!" );
				return false;
			}

			var product = _storeController.products.WithID( productID );
			if( product == null )
			{
				Debug.LogWarning( "IAP Product not found: " + productID );
				return false;
			}

			return product.hasReceipt && IsPurchaseValid( product );
		}

		public void OnInitialized( IStoreController storeController, IExtensionProvider storeExtensions )
		{
			_storeController = storeController;
			_storeExtensions = storeExtensions;

			if( StandardPurchasingModule.Instance().appStore == AppStore.AppleAppStore )
				storeExtensions.GetExtension<IAppleExtensions>().RegisterPurchaseDeferredListener( OnDeferredPurchase );
			// The CrossPlatform validator only supports Google Play and Apple App Store
			
// 			switch( StandardPurchasingModule.Instance().appStore )
// 			{
// 				case AppStore.GooglePlay:
// 				case AppStore.AppleAppStore:
// 				case AppStore.MacAppStore:
// 				{ 
// #if !UNITY_EDITOR 
// 				//byte[] appleTangleData = AppleStoreKitTestTangle.Data(); // While testing with StoreKit Testing
// 				byte[] appleTangleData = AppleTangle.Data();
// 				_purchaseValidator = new CrossPlatformValidator( GooglePlayTangle.Data(), appleTangleData, Application.identifier );
// #endif
// 					break;
// 				}
// 			}

			_initializationState = State.SuccessfullyInitialized;
			_onInitialized?.Invoke( true );
		}
		
		void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
		{
			Debug.LogWarning($"Purchase failed - Product: '{product.definition.id}'," +
			                 $" Purchase failure reason: {failureDescription.reason}," +
			                 $" Purchase failure details: {failureDescription.message}");
			OnNativeIAPWindowClosed();
		}

		void IStoreListener.OnInitializeFailed( InitializationFailureReason error )
		{
			Debug.LogWarning( "IAP initialization failed: " + error );

			_initializationState = State.FailedToInitialize;
			_onInitialized?.Invoke( false );
		}
		
		public void OnInitializeFailed(InitializationFailureReason error, string message)
		{
			Debug.LogError(message);
			Debug.LogError(error);
			_initializationState = State.FailedToInitialize;
			_onInitialized?.Invoke( false );
		}

		PurchaseProcessingResult IStoreListener.ProcessPurchase( PurchaseEventArgs purchaseEventArgs )
		{
			try
			{
				var product = purchaseEventArgs.purchasedProduct;
				if( IsPurchaseValid( product ) )
				{
					if( StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay && _storeExtensions.GetExtension<IGooglePlayStoreExtensions>().IsPurchasedProductDeferred( product ) )
					{
						// The purchase is deferred; therefore, we do not unlock the content or complete the transaction.
						// ProcessPurchase will be called again once the purchase is completed
						return PurchaseProcessingResult.Pending;
					}
					
					_analyticService.SendBusinessEvent(purchaseEventArgs);
					onPurchaseCompleted?.Invoke( product );
				}

				
				
				return PurchaseProcessingResult.Complete;
			}
			finally
			{
				OnNativeIAPWindowClosed();
			}
		}

		void IStoreListener.OnPurchaseFailed( Product product, PurchaseFailureReason failureReason )
		{
			Debug.LogWarning( $"IAP purchase failed for '{product.definition.id}': {failureReason}" );
			//GameAnalytics.NewDesignEvent($"IAP:{product.definition.id}:Fail");
			onPurchaseFailed?.Invoke( product, failureReason );
			OnNativeIAPWindowClosed();
		}

		private void OnDeferredPurchase( Product product )
		{
			Debug.Log( $"IAP purchase of {product.definition.id} is deferred" );
			OnNativeIAPWindowClosed();
		}

		private bool IsPurchaseValid( Product product )
		{
			if( _purchaseValidator != null )
			{
				try
				{
					_purchaseValidator.Validate( product.receipt );
				}
				catch( IAPSecurityException reason )
				{
					Debug.LogWarning( "Invalid IAP receipt: " + reason );
					return false;
				}
			}

			return true;
		}

		private void OnNativeIAPWindowClosed()
		{
			try
			{
				_onIAPWindowClosed?.Invoke();
				_onIAPWindowClosed = null;
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
		}

		private void OnNativeRestoreWindowClosed(bool success, string message)
		{
			Debug.Log( "IAP purchases restored: " + success + " " + message);

			try
			{
				_onRestoreWindowClosed?.Invoke( success );
				_onRestoreWindowClosed = null;
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
		}
	}
}