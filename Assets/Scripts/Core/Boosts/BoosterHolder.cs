using System.Collections.Generic;
using Core;
using Core.Boosts;
using Core.Ship;
using Core.Waiting;
using Galaxy.Data.Infrastructure;
using Galaxy.Infrastructure.Boosters;
using UnityEngine;

public class BoosterHolder : MonoBehaviour
{
    [SerializeField] private List<BoosterView> _boosters = new List<BoosterView>();
    [SerializeField] private BoosterBuyPopup _buyPopup;

    private BoosterUsageController _controller;

    public BoosterUsageController Controller => _controller;

    public void Init(IShipsController ships, IWaitingZonesController waitingZones,
        EnemyMap enemyMap, GamePauseService pauseService,
        IBoosterInventoryService inventory, IBoosterPurchaseService purchase, BoosterCatalogSO catalog)
    {
        if (_buyPopup == null)
            _buyPopup = FindObjectOfType<BoosterBuyPopup>(true);

        var context = new BoosterContext(ships, waitingZones, enemyMap, pauseService, this);
        _controller = new BoosterUsageController(catalog, purchase, context, inventory);
        _controller.PurchaseRequested += OnPurchaseRequested;

        if (_boosters == null)
            return;

        for (var index = 0; index < _boosters.Count; index++)
            _boosters[index]?.Init(_controller);
    }

    private void OnDestroy()
    {
        if (_controller == null)
            return;

        _controller.PurchaseRequested -= OnPurchaseRequested;
        _controller.Dispose();
        _controller = null;
    }

    private void OnPurchaseRequested(BoosterType type)
    {
        if (_controller == null || _buyPopup == null)
            return;

        // The click reaches this point only when the purchase is possible, so the button is
        // enabled; TryPurchase re-checks everything anyway.
        _buyPopup.Show(_controller.GetPrice(type), true,
            () => _controller.TryPurchase(type));
    }
}
