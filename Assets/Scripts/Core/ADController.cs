using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Ad;

namespace Core
{
    public class ADController
    {
        public static ADController Instance { get; private set; }

        public bool IsShowing => _isShowing;

        private readonly Queue<AdRequest> _requests = new Queue<AdRequest>();
        private readonly Dictionary<AdRequestType, AdOfferPopup> _popups = new Dictionary<AdRequestType, AdOfferPopup>();
        private bool _isShowing;

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
           // need reinplement
        }
    }
}
