using System;

namespace Core.Store
{
    public interface IStorePlatform
    {
        bool IsReady { get; }
        bool TryGetPrice(StoreOfferSO offer, out string price);
        void Purchase(StoreOfferSO offer, Action<bool> onComplete);
    }
}
