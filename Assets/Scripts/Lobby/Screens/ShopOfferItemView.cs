using System;
using System.Collections.Generic;
using Core.Boosts;
using Core.Store;
using Galaxy.Lobby.Screens.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    public class ShopOfferItemView : MonoBehaviour
    {
        [SerializeField] private string _productId;
        [SerializeField] private PanelType _panel = PanelType.Packs;
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _basicSprite;
        [SerializeField] private Sprite _goldSprite;
        [SerializeField] private Sprite _eliteSprite;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private BoosterRewardView _boosterRewardViewPrefab;
        [SerializeField] private Transform _boosterRewardsHolder;
        [SerializeField] private Transform _grantNoAdsPanel;
        [SerializeField] private SoftRewardView _softRewardView;

        private readonly List<BoosterRewardView> _boosterRewards = new List<BoosterRewardView>();
        private StoreOfferSO _offer;
        private Action<StoreOfferSO> _onBuy;

        public string ProductId => _productId;

        public PanelType Panel => _panel;

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

        public void Bind(StoreOfferSO offer, string price, bool owned, bool canBuy,
            BoosterCatalogSO boosterCatalog, Action<StoreOfferSO> onBuy)
        {
            _offer = offer;
            _onBuy = onBuy;

            if (offer == null || owned)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_background != null)
            {
                var sprite = GetTierSprite(offer.Tier);
                if (sprite != null)
                    _background.sprite = sprite;
            }

            if (_icon != null)
                _icon.sprite = offer.Icon;

            if (_titleText != null)
                _titleText.text = offer.Title;

            if (_priceText != null)
                _priceText.text = owned ? string.Empty : price;

            if (_buyButton != null)
                _buyButton.interactable = canBuy && !owned;
            
            _softRewardView.Bind(offer.SoftAmount);
            _grantNoAdsPanel.gameObject.SetActive(offer.GrantNoAds);

            RefreshBoosterRewards(offer, boosterCatalog);
        }

        private void RefreshBoosterRewards(StoreOfferSO offer, BoosterCatalogSO boosterCatalog)
        {
            var rewards = offer.Boosters;
            var rewardIndex = 0;

            for (var index = 0; index < rewards.Count; index++)
            {
                var reward = rewards[index];
                if (reward.Count <= 0)
                    continue;

                var view = GetOrCreateReward(rewardIndex);
                if (view == null)
                    continue;

                Sprite icon = null;
                if (boosterCatalog != null && boosterCatalog.TryGet(reward.Type, out var config))
                    icon = config.Icon;

                view.Bind(icon, reward.Count);
                view.gameObject.SetActive(true);
                
                rewardIndex++;
            }
            
            

            for (var index = rewardIndex; index < _boosterRewards.Count; index++)
                _boosterRewards[index].gameObject.SetActive(false);
        }

        private BoosterRewardView GetOrCreateReward(int index)
        {
            if (_boosterRewardViewPrefab == null)
                return null;

            while (_boosterRewards.Count <= index)
            {
                var parent = _boosterRewardsHolder != null ? _boosterRewardsHolder : transform;
                var view = Instantiate(_boosterRewardViewPrefab, parent);
                _boosterRewards.Add(view);
            }

            return _boosterRewards[index];
        }

        private Sprite GetTierSprite(StoreOfferTier tier)
        {
            switch (tier)
            {
                case StoreOfferTier.Gold:
                    return _goldSprite;
                case StoreOfferTier.Elite:
                    return _eliteSprite;
                default:
                    return _basicSprite;
            }
        }

        private void HandleBuy()
        {
            if (_offer != null)
                _onBuy?.Invoke(_offer);
        }
    }
}

