using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Boosts
{
    public class BoosterBuyPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMPro.TextMeshProUGUI _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _cancelButton;

        private Action _onBuy;
        private Action _onCancel;

        private void Awake()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(OnBuyClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelClicked);

            Hide();
        }

        private void OnDestroy()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(OnBuyClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show(int price, bool canBuy, Action onBuy, Action onCancel = null)
        {
            _onBuy = onBuy;
            _onCancel = onCancel;

            if (_priceText != null)
                _priceText.text = price.ToString();

            if (_buyButton != null)
                _buyButton.interactable = canBuy;

            SetVisible(true);
        }

        public void Hide()
        {
            _onBuy = null;
            _onCancel = null;
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            
            gameObject.SetActive(visible);
            
            if (_root != null)
            {
                _root.SetActive(visible);
                
                return;
            }

        }

        private void OnBuyClicked()
        {
            var callback = _onBuy;
            Hide();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            var callback = _onCancel;
            Hide();
            callback?.Invoke();
        }
    }
}
