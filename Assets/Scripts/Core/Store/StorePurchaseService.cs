using System;
using System.Collections.Generic;
using Core.Currency;
using Galaxy.Infrastructure.Boosters;

namespace Core.Store
{
    public sealed class StorePurchaseService : IStorePurchaseService
    {
        private readonly StoreCatalogSO _catalog;
        private readonly IStorePlatform _platform;
        private readonly ISaveService _saveService;
        private readonly ISoftCurrencyService _currency;
        private readonly IBoosterInventoryService _inventory;

        public StorePurchaseService(StoreCatalogSO catalog,
            IStorePlatform platform,
            ISaveService saveService,
            ISoftCurrencyService currency,
            IBoosterInventoryService inventory)
        {
            _catalog = catalog;
            _platform = platform;
            _saveService = saveService;
            _currency = currency;
            _inventory = inventory;
        }

        public event Action<StoreOfferSO> Purchased;

        public bool IsReady => _platform.IsReady;

        public bool HasNoAds => _saveService.GetSaveData().NoAds;

        public IReadOnlyList<StoreOfferSO> Offers => _catalog.Offers;

        public bool TryGetOffer(string productId, out StoreOfferSO offer)
        {
            return _catalog.TryGetByProductId(productId, out offer);
        }

        public bool IsOwned(StoreOfferSO offer)
        {
            if (offer == null)
                return false;

            if (offer.GrantNoAds && _saveService.GetSaveData().NoAds)
                return true;

            if (offer.Repeatable)
                return false;

            return _saveService.GetSaveData().PurchasedProducts.Contains(offer.ProductId);
        }

        public bool TryGetPrice(StoreOfferSO offer, out string price)
        {
            if (_platform.TryGetPrice(offer, out price) && !string.IsNullOrEmpty(price))
                return true;

            price = offer != null ? offer.FallbackPrice : string.Empty;
            return !string.IsNullOrEmpty(price);
        }

        public void Purchase(StoreOfferSO offer, Action<bool> onComplete)
        {
            if (offer == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            if (IsOwned(offer))
            {
                Log.Info($"[Store] '{offer.ProductId}' is already owned");
                onComplete?.Invoke(false);
                return;
            }

            _platform.Purchase(offer, success =>
            {
                if (success)
                    Grant(offer);

                onComplete?.Invoke(success);
            });
        }

        private void Grant(StoreOfferSO offer)
        {
            var data = _saveService.GetSaveData();

            if (offer.SoftAmount > 0)
                _currency.Add(offer.SoftAmount);

            for (var index = 0; index < offer.Boosters.Count; index++)
            {
                var reward = offer.Boosters[index];
                if (reward.Count > 0)
                    _inventory.Add(reward.Type, reward.Count);
            }

            if (offer.GrantNoAds)
                data.NoAds = true;

            if (!offer.Repeatable && !data.PurchasedProducts.Contains(offer.ProductId))
                data.PurchasedProducts.Add(offer.ProductId);

            _saveService.Save();

            Log.Info($"[Store] Granted '{offer.ProductId}': soft +{offer.SoftAmount}, " +
                     $"boosters {offer.Boosters.Count}, noAds {offer.GrantNoAds}");

            Purchased?.Invoke(offer);
        }
    }
}
