using System;
using Core.Store;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    public class ShopOfferItemView : MonoBehaviour
    {
        [SerializeField] private string _productId;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _badgeText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private GameObject _ownedMark;
        [SerializeField] private CanvasGroup _canvasGroup;

        private StoreOfferSO _offer;
        private Action<StoreOfferSO> _onBuy;

        public string ProductId => _productId;

        public StoreOfferSO Offer => _offer;

        private void OnEnable()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(HandleBuy);
        }

        private void OnDisable()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(HandleBuy);
        }

        public void Bind(StoreOfferSO offer, string price, bool owned, bool canBuy, Action<StoreOfferSO> onBuy)
        {
            _offer = offer;
            _onBuy = onBuy;

            if (offer == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_icon != null)
                _icon.sprite = offer.Icon;

            if (_titleText != null)
                _titleText.text = offer.Title;

            if (_badgeText != null)
            {
                bool hasBadge = !string.IsNullOrEmpty(offer.Badge);
                _badgeText.gameObject.SetActive(hasBadge);

                if (hasBadge)
                    _badgeText.text = offer.Badge;
            }

            if (_priceText != null)
                _priceText.text = owned ? string.Empty : price;

            if (_ownedMark != null)
                _ownedMark.SetActive(owned);

            if (_buyButton != null)
                _buyButton.interactable = canBuy && !owned;

            if (_canvasGroup != null)
                _canvasGroup.alpha = canBuy || owned ? 1f : 0.6f;
        }

        private void HandleBuy()
        {
            if (_offer != null)
                _onBuy?.Invoke(_offer);
        }
    }
}
