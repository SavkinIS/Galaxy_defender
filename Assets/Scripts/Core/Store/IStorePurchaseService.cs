using System;
using System.Collections.Generic;

namespace Core.Store
{
    public interface IStorePurchaseService
    {
        bool IsReady { get; }

        bool HasNoAds { get; }

        IReadOnlyList<StoreOfferSO> Offers { get; }

        event Action<StoreOfferSO> Purchased;

        bool TryGetOffer(string productId, out StoreOfferSO offer);

        bool IsOwned(StoreOfferSO offer);

        bool TryGetPrice(StoreOfferSO offer, out string price);

        void Purchase(StoreOfferSO offer, Action<bool> onComplete);
    }
}
