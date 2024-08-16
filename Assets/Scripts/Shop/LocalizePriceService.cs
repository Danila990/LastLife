using System.Collections.Generic;
using Shop.Abstract;
using Shop.Models.Product;
using UniRx;
using VContainer.Unity;

namespace Shop
{
	public interface ILocalizePriceService
	{
		public IReadOnlyDictionary<string, ReactiveProperty<InAppPrice>> Prices { get; }
	}

	public class LocalizePriceService : IStartable, ILocalizePriceService
	{
		private readonly IInAppDatabase _inAppDatabase;
		private readonly IAPManager _iapManager;
		private readonly Dictionary<string, ReactiveProperty<InAppPrice>> _prices = new();

		public IReadOnlyDictionary<string, ReactiveProperty<InAppPrice>> Prices => _prices;

		public LocalizePriceService(
			IInAppDatabase inAppDatabase,
			IAPManager iapManager
		)
		{
			_inAppDatabase = inAppDatabase;
			_iapManager = iapManager;
		}

		public void Start()
		{
			var allProducts = _inAppDatabase.GetProducts();
			foreach (var product in allProducts)
			{
				_prices.Add(product.InAppID, new ReactiveProperty<InAppPrice>(product.Price));
			}

			_iapManager.OnInitializedEvt += OnInit;
		}

		private void OnInit(bool success)
		{
			if (!success) return;

			foreach (var product in _iapManager.StoreController.products.all)
			{
				var price = (float) product.metadata.localizedPrice;
				var iso = product.metadata.isoCurrencyCode;
				if (price <= 0.01) continue;
				_prices[product.definition.id].SetValueAndForceNotify(new InAppPrice(iso, price));
			}
		}
	}
}