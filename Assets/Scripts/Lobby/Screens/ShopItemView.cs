using System;
using Core.Boosts;
using Galaxy.Data.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    public class ShopItemView : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private BoosterType _type;

        [Header("Visuals")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Interaction")]
        [SerializeField] private Button _buyButton;

        private Action<BoosterType> _onBuy;
        private bool _hasConfig;

        public BoosterType Type => _type;

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

        public void Bind(BoosterConfigSO config, int count, int maxCount, bool canPurchase,
            Action<BoosterType> onBuy)
        {
            _onBuy = onBuy;
            _hasConfig = config != null;

            if (_icon != null)
                _icon.sprite = config != null ? config.Icon : null;

            if (_nameText != null)
                _nameText.text = config != null ? config.name : _type.ToString();

            if (_countText != null)
                _countText.text = config != null ? $"{count}/{maxCount}" : string.Empty;

            if (_priceText != null)
                _priceText.text = config != null ? config.Price.ToString() : string.Empty;

            if (_buyButton != null)
                _buyButton.interactable = config != null && canPurchase;

            if (_canvasGroup != null)
                _canvasGroup.alpha = config != null ? 1f : 0.4f;
        }

        private void HandleBuy()
        {
            if (!_hasConfig)
                return;

            _onBuy?.Invoke(_type);
        }
    }
}
