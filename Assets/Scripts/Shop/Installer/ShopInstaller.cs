using Shop.Abstract;
using Shop.Impl;
using UnityEngine;
using VContainer;
using VContainer.Extensions;

namespace Shop.Installer
{
	public class ShopInstaller : MonoInstaller
	{
		[SerializeField] private InAppDatabase _inAppDatabase;
		[SerializeField] private ShopData _shopData;
		
		public override void Install(IContainerBuilder builder)
		{
			builder.Register<IAPManager>(Lifetime.Singleton).AsSelf();
			builder.Register<PurchaseService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<LocalizePriceService>(Lifetime.Singleton).AsImplementedInterfaces();
			
			builder.Register<ItemPurchaseService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<BundlePurchaseService>(Lifetime.Singleton).AsImplementedInterfaces();
			builder.Register<ShopStorage>(Lifetime.Singleton).AsImplementedInterfaces();

			builder.RegisterInstance(_inAppDatabase).As<IInAppDatabase>();
			builder.RegisterInstance(_shopData).As<IShopData>();
		}
	}
}