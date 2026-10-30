using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Ad
{
    public abstract class AdOfferPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _declineButton;

        private Action _onAccepted;
        private Action _onDeclined;

        internal bool IsConfigured => _acceptButton != null && _declineButton != null;
        
        public abstract AdRequestType Type { get; }

        public void Show(Action onAccepted, Action onDeclined = null)
        {
            ADController adController = ADController.Instance;
            if (adController == null)
            {
                onDeclined?.Invoke();
                return;
            }

            adController.ShowRewardedAd(Type, onAccepted, onDeclined);
        }

        internal void Present(string message, Action onAccepted, Action onDeclined)
        {
            _onAccepted = onAccepted;
            _onDeclined = onDeclined;

            if (_messageText != null)
                _messageText.text = message;

            gameObject.SetActive(true);
            GetPopupRoot().SetActive(true);
        }

        internal void Dismiss()
        {
            _onAccepted = null;
            _onDeclined = null;
            GetPopupRoot().SetActive(false);
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            if (_acceptButton != null)
                _acceptButton.onClick.AddListener(Accept);

            if (_declineButton != null)
                _declineButton.onClick.AddListener(Decline);
        }

        private void OnDestroy()
        {
            if (_acceptButton != null)
                _acceptButton.onClick.RemoveListener(Accept);

            if (_declineButton != null)
                _declineButton.onClick.RemoveListener(Decline);
        }

        private void Accept()
        {
            _onAccepted?.Invoke();
        }

        private void Decline()
        {
            _onDeclined?.Invoke();
        }

        private GameObject GetPopupRoot()
        {
            return _popupRoot != null ? _popupRoot : gameObject;
        }
    }
}
