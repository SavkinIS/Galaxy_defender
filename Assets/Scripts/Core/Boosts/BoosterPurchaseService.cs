using Core.Currency;
using Galaxy.Data.Infrastructure;
using Galaxy.Infrastructure.Boosters;

namespace Core.Boosts
{
    public sealed class BoosterPurchaseService : IBoosterPurchaseService
    {
        private readonly BoosterCatalogSO _catalog;
        private readonly IBoosterInventoryService _inventory;
        private readonly ISoftCurrencyService _currency;

        public BoosterPurchaseService(BoosterCatalogSO catalog,
            IBoosterInventoryService inventory,
            ISoftCurrencyService currency)
        {
            _catalog = catalog;
            _inventory = inventory;
            _currency = currency;
        }

        public int GetPrice(BoosterType type)
        {
            return TryGetConfig(type, out var config) ? config.Price : 0;
        }

        public int GetMaxCount(BoosterType type)
        {
            return TryGetConfig(type, out var config) ? config.MaxCount : 0;
        }

        public int GetCount(BoosterType type)
        {
            return _inventory.Get(type);
        }

        public bool CanPurchase(BoosterType type)
        {
            if (!TryGetConfig(type, out var config))
                return false;

            if (_inventory.Get(type) >= config.MaxCount)
                return false;

            return _currency != null && _currency.Balance >= config.Price;
        }

        public bool TryPurchase(BoosterType type)
        {
            if (!CanPurchase(type))
                return false;

            if (!TryGetConfig(type, out var config))
                return false;

            if (!_currency.TrySpend(config.Price))
                return false;

            _inventory.Add(type, 1);
            return true;
        }

        private bool TryGetConfig(BoosterType type, out BoosterConfigSO config)
        {
            config = null;
            return _catalog != null && _catalog.TryGet(type, out config);
        }
    }
}
