using System;
using Core.Boosts;
using Core.Currency;
using Core.Store;
using Galaxy.Data.Infrastructure;
using Galaxy.Infrastructure.Boosters;
using Galaxy.Lobby.Routing;
using Galaxy.Lobby.Screens;
using VContainer.Unity;

namespace Galaxy.Lobby.Presenters
{
    public class ShopPresenter : IInitializable, IDisposable
    {
        private readonly ShopScreen _screen;
        private readonly LobbyStateMachine _lobbyStateMachine;
        private readonly BoosterCatalogSO _catalog;
        private readonly IBoosterPurchaseService _purchase;
        private readonly IBoosterInventoryService _inventory;
        private readonly ISoftCurrencyService _currency;
        private readonly IStorePurchaseService _store;

        public ShopPresenter(ShopScreen screen,
            LobbyStateMachine lobbyStateMachine,
            BoosterCatalogSO catalog,
            IBoosterPurchaseService purchase,
            IBoosterInventoryService inventory,
            ISoftCurrencyService currency,
            IStorePurchaseService store)
        {
            _screen = screen;
            _lobbyStateMachine = lobbyStateMachine;
            _catalog = catalog;
            _purchase = purchase;
            _inventory = inventory;
            _currency = currency;
            _store = store;
        }

        public void Initialize()
        {
            _screen.Initialize();
            _screen.BackRequested += ToMainMenu;

            _inventory.Changed += OnInventoryChanged;
            _currency.BalanceChanged += OnBalanceChanged;
            _store.Purchased += OnStorePurchased;

            Refresh();
        }

        public void Activate()
        {
            _screen.Show();
            Refresh();
        }

        public void Deactivate()
        {
            _screen.Hide();
        }

        public void Dispose()
        {
            _screen.BackRequested -= ToMainMenu;
            _inventory.Changed -= OnInventoryChanged;
            _currency.BalanceChanged -= OnBalanceChanged;
            _store.Purchased -= OnStorePurchased;
        }

        private void OnBuyRequested(BoosterType type)
        {
            if (_purchase.TryPurchase(type))
                Refresh();
        }

        private void OnOfferRequested(StoreOfferSO offer)
        {
            if (offer == null)
                return;

            _store.Purchase(offer, success =>
            {
                if (success)
                    Refresh();
            });
        }

        private void OnInventoryChanged(BoosterType type, int count) => Refresh();

        private void OnBalanceChanged(int balance) => Refresh();

        private void OnStorePurchased(StoreOfferSO offer) => Refresh();

        private void Refresh()
        {
            RefreshBoosterCards();
            RefreshOfferCards();
        }

        private void RefreshBoosterCards()
        {
            var items = _screen.Items;

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];
                if (item == null)
                    continue;

                if (!_catalog.TryGet(item.Type, out var config))
                {
                    item.Bind(null, 0, 0, false, OnBuyRequested);
                    continue;
                }

                item.Bind(config,
                    _purchase.GetCount(config.Type),
                    _purchase.GetMaxCount(config.Type),
                    _purchase.CanPurchase(config.Type),
                    OnBuyRequested);
            }
        }

        private void RefreshOfferCards()
        {
            var cards = _screen.Offers;

            for (var index = 0; index < cards.Count; index++)
            {
                var card = cards[index];
                if (card == null)
                    continue;

                if (!_store.TryGetOffer(card.ProductId, out var offer))
                {
                    card.Bind(null, string.Empty, false, false, _catalog, OnOfferRequested);
                    continue;
                }

                _store.TryGetPrice(offer, out var price);

                bool owned = _store.IsOwned(offer);
                bool canBuy = _store.IsReady && !owned;

                card.Bind(offer, price, owned, canBuy, _catalog, OnOfferRequested);
            }
        }

        private void ToMainMenu() => _lobbyStateMachine.ChangeState(LobbyStateId.MainMenu);
    }
}
