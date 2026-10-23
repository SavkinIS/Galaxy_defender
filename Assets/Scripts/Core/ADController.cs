using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Ad;

namespace Core
{
    public class ADController : MonoBehaviour
    {
        private sealed class AdRequest
        {
            public AdOfferPopup Popup;
            public string Message;
            public Action Granted;
            public Action Declined;
        }

        private const string DefaultMessage = "Посмотреть рекламу?";

        public static ADController Instance { get; private set; }

        public bool IsShowing => _isShowing;

        private readonly Queue<AdRequest> _requests = new Queue<AdRequest>();
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

        public void ShowRewardedAd(Action onRewardGranted, Action onDeclined = null)
        {
            ShowRewardedAd(null, DefaultMessage, onRewardGranted, onDeclined);
        }

        public void ShowRewardedAd(string message, Action onRewardGranted, Action onDeclined = null)
        {
            ShowRewardedAd(null, message, onRewardGranted, onDeclined);
        }

        public void ShowRewardedAd(AdOfferPopup popup, string message, Action onRewardGranted,
            Action onDeclined = null)
        {
            _requests.Enqueue(new AdRequest
            {
                Popup = popup,
                Message = message,
                Granted = onRewardGranted,
                Declined = onDeclined
            });

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
            var request = _requests.Dequeue();
            if (request.Popup != null && request.Popup.IsConfigured)
            {
                request.Popup.Present(request.Message,
                    () => Resolve(request, true),
                    () => Resolve(request, false));
                return;
            }

            request.Declined?.Invoke();
            ShowNext();
        }

        private void Resolve(AdRequest request, bool granted)
        {
            if (request.Popup != null && request.Popup.IsConfigured)
                request.Popup.Dismiss();

            if (granted)
                request.Granted?.Invoke();
            else
                request.Declined?.Invoke();

            ShowNext();
        }

    }
}
