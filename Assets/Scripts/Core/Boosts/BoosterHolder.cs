using System.Collections.Generic;
using Core;
using Core.Boosts;
using Core.Currency;
using Core.Ship;
using Core.Waiting;
using UnityEngine;

public class BoosterHolder : MonoBehaviour
{
    [SerializeField] private List<BoosterView> _boosters = new List<BoosterView>();
    [SerializeField] private List<BoosterConfigSO> _configs = new List<BoosterConfigSO>();
    [SerializeField] private BoosterBuyPopup _buyPopup;

    private BoostersController _controller;

    public BoostersController Controller => _controller;

    public void Init(ISoftCurrencyService currency, IShipsController ships,
        IWaitingZonesController waitingZones, EnemyMap enemyMap, GamePauseService pauseService)
    {
        if (_buyPopup == null)
            _buyPopup = FindObjectOfType<BoosterBuyPopup>(true);

        var context = new BoosterContext(ships, waitingZones, enemyMap, pauseService, this);
        _controller = new BoostersController(_configs, currency, context);
        _controller.PurchaseRequested += OnPurchaseRequested;

        if (_boosters == null)
            return;

        for (var index = 0; index < _boosters.Count; index++)
            _boosters[index]?.Init(_controller);
    }

    private void OnDestroy()
    {
        if (_controller != null)
            _controller.PurchaseRequested -= OnPurchaseRequested;
    }

    private void OnPurchaseRequested(BoosterType type)
    {
        if (_controller == null || _buyPopup == null)
            return;

        // The click reaches this point only when the balance covers the price, so the button is
        // enabled; TryPurchase re-checks the balance anyway.
        _buyPopup.Show(_controller.GetPrice(type), true,
            () => _controller.TryPurchase(type));
    }
}
