using System;
using Core.Boosts;
using Core.Currency;
using Core.Store;
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
        private readonly ISoftCurrencyService _currency;
        private readonly IStorePurchaseService _store;

        public ShopPresenter(ShopScreen screen,
            LobbyStateMachine lobbyStateMachine,
            BoosterCatalogSO catalog,
            ISoftCurrencyService currency,
            IStorePurchaseService store)
        {
            _screen = screen;
            _lobbyStateMachine = lobbyStateMachine;
            _catalog = catalog;
            _currency = currency;
            _store = store;
        }

        public void Initialize()
        {
            _screen.Initialize();
            _screen.BackRequested += ToMainMenu;

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
            _currency.BalanceChanged -= OnBalanceChanged;
            _store.Purchased -= OnStorePurchased;
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

        private void OnBalanceChanged(int balance) => Refresh();

        private void OnStorePurchased(StoreOfferSO offer) => Refresh();

        private void Refresh()
        {
            RefreshOfferCards();
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
