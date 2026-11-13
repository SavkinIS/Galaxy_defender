#if Payments_yg
using System;
using YG;

namespace Core.Store
{
    /// <summary>
    /// Real billing through the Yandex Games purchases module.
    /// Compiled only when the plugin module is enabled
    /// (define "Payments_yg" appears after enabling it in InfoYG).
    ///
    /// TODO after enabling the module: verify the exact signatures of
    /// YG2.Purchase / YG2.onPurchaseSuccess / YG2.onPurchaseFailed and
    /// replace the price lookup with the platform catalogue price.
    /// </summary>
    public sealed class YandexStorePlatform : IStorePlatform, IDisposable
    {
        private Action<bool> _pending;

        public YandexStorePlatform()
        {
            YG2.onPurchaseSuccess += OnPurchaseSuccess;
            YG2.onPurchaseFailed += OnPurchaseFailed;
        }

        public bool IsReady => YG2.isSDKEnabled;

        public bool TryGetPrice(StoreOfferSO offer, out string price)
        {
            price = offer != null ? offer.FallbackPrice : string.Empty;
            return !string.IsNullOrEmpty(price);
        }

        public void Purchase(StoreOfferSO offer, Action<bool> onComplete)
        {
            if (offer == null || !offer.HasReward)
            {
                onComplete?.Invoke(false);
                return;
            }

            if (!IsReady)
            {
                Log.Error("[Store] Purchase rejected: platform is not ready");
                onComplete?.Invoke(false);
                return;
            }

            if (_pending != null)
            {
                Log.Error("[Store] Purchase rejected: another purchase is in progress");
                onComplete?.Invoke(false);
                return;
            }

            _pending = onComplete;
            YG2.PurchaseByID(offer.ProductId);
        }

        public void Dispose()
        {
            YG2.onPurchaseSuccess -= OnPurchaseSuccess;
            YG2.onPurchaseFailed -= OnPurchaseFailed;
            _pending = null;
        }

        private void OnPurchaseSuccess(string productId)
        {
            var callback = _pending;
            _pending = null;

            if (callback == null)
            {
                Log.Info($"[Store] Purchase '{productId}' confirmed without a pending request");
                return;
            }

            callback.Invoke(true);
        }

        private void OnPurchaseFailed(string productId)
        {
            var callback = _pending;
            _pending = null;
            callback?.Invoke(false);
        }
    }
}
#endif
