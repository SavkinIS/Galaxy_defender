using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Ad;

namespace Core
{
    public class ADController : MonoBehaviour
    {
        public static ADController Instance { get; private set; }

        public bool IsShowing => _isShowing;

        private readonly Queue<AdRequest> _requests = new Queue<AdRequest>();
        private readonly Dictionary<AdRequestType, AdOfferPopup> _popups = new Dictionary<AdRequestType, AdOfferPopup>();
        private bool _isShowing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShowRewardedAd(AdRequestType type, Action onGranted, Action onDeclined = null)
        {
            ShowRewardedAd(AdRequests.Create(type, onGranted, onDeclined));
        }

        public void ShowRewardedAd(AdRequest request)
        {
            if (request == null)
                return;

            _requests.Enqueue(request);

            if (!_isShowing)
                ShowNext();
        }

        private void ShowNext()
        {
            if (_requests.Count == 0)
            {
                _isShowing = false;
                return;
            }

            _isShowing = true;
            AdRequest request = _requests.Dequeue();
            AdOfferPopup popup = ResolvePopup(request);

            if (popup != null)
            {
                popup.Present(request.Message,
                    () => Resolve(request, popup, true),
                    () => Resolve(request, popup, false));
                return;
            }

            if (request.RequiresPopup)
                request.OnDeclined?.Invoke();
            else
                request.OnGranted?.Invoke();

            ShowNext();
        }

        private AdOfferPopup ResolvePopup(AdRequest request)
        {
            AdOfferPopup popup = request.Popup != null ? request.Popup : FindPopup(request.Type);
            return popup != null && popup.IsConfigured ? popup : null;
        }

        private AdOfferPopup FindPopup(AdRequestType type)
        {
            if (_popups.TryGetValue(type, out AdOfferPopup cached) && cached != null)
                return cached;

            AdOfferPopup[] popups = FindObjectsOfType<AdOfferPopup>(true);
            foreach (AdOfferPopup popup in popups)
            {
                if (popup != null)
                    _popups[popup.Type] = popup;
            }

            return _popups.TryGetValue(type, out AdOfferPopup found) ? found : null;
        }

        private void Resolve(AdRequest request, AdOfferPopup popup, bool granted)
        {
            popup.Dismiss();

            if (granted)
                request.OnGranted?.Invoke();
            else
                request.OnDeclined?.Invoke();

            ShowNext();
        }
    }
}
