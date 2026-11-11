using System;
using System.Collections.Generic;
using Core.Currency;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _toMainMenuBtn;
        [SerializeField] private SoftCurrencyView _softCurrencyView;
        [SerializeField] private List<ShopItemView> _items = new List<ShopItemView>();
        [SerializeField] private List<ShopOfferItemView> _offers = new List<ShopOfferItemView>();

        public SoftCurrencyView SoftCurrencyView => _softCurrencyView;

        public IReadOnlyList<ShopItemView> Items => _items;

        public IReadOnlyList<ShopOfferItemView> Offers => _offers;

        public event Action BackRequested;

        private void OnEnable()
        {
            if (_toMainMenuBtn != null)
                _toMainMenuBtn.onClick.AddListener(ToMainMenu);
        }

        private void OnDisable()
        {
            if (_toMainMenuBtn != null)
                _toMainMenuBtn.onClick.RemoveListener(ToMainMenu);
        }

        public void Initialize()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
        }

        private void ToMainMenu() => BackRequested?.Invoke();
    }
}
