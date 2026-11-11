using System;
using UnityEngine;

namespace Core.Store
{
    public sealed class EditorStorePlatform : IStorePlatform
    {
        public bool IsReady => true;

        public bool TryGetPrice(StoreOfferSO offer, out string price)
        {
            price = offer != null ? offer.FallbackPrice : string.Empty;
            return !string.IsNullOrEmpty(price);
        }

        public void Purchase(StoreOfferSO offer, Action<bool> onComplete)
        {
            if (offer == null || !offer.HasReward)
            {
                Log.Error("[Store] Simulated purchase rejected: offer is empty");
                onComplete?.Invoke(false);
                return;
            }

            Log.Info($"[Store] Simulated purchase: {offer.ProductId}");
            onComplete?.Invoke(true);
        }
    }
}
