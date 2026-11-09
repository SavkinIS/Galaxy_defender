using Galaxy.Data.Infrastructure;

namespace Core.Boosts
{
    /// <summary>
    /// Purchase side of boosters. Lives globally so the lobby shop and the game buy
    /// with the same rules: price from the catalog, stock from the inventory.
    /// </summary>
    public interface IBoosterPurchaseService
    {
        int GetPrice(BoosterType type);

        int GetMaxCount(BoosterType type);

        int GetCount(BoosterType type);

        bool CanPurchase(BoosterType type);

        bool TryPurchase(BoosterType type);
    }
}
