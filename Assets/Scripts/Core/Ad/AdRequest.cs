using System;

namespace Core.Ad
{
    public class AdRequest
    {
        public AdRequest(AdRequestType type, string message, Action onGranted, Action onDeclined = null,
            AdOfferPopup popup = null, bool requiresPopup = true)
        {
            Type = type;
            Message = message;
            OnGranted = onGranted;
            OnDeclined = onDeclined;
            Popup = popup;
            RequiresPopup = requiresPopup;
        }

     
        public AdRequestType Type { get; }

        public string Message { get; }

        public Action OnGranted { get; }

        public Action OnDeclined { get; }

        public AdOfferPopup Popup { get; }

        public bool RequiresPopup { get; }
    }
}
