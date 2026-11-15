using System;
using System.Collections.Generic;
using Galaxy.Data.Infrastructure;
using Galaxy.Lobby.Screens.Shop;
using UnityEngine;

namespace Core.Store
{
    [Serializable]
    public struct BoosterReward
    {
        public BoosterType Type;
        public int Count;
    }
    
    [CreateAssetMenu(menuName = "Galaxy/Store Offer", fileName = "StoreOffer")]
    public sealed class StoreOfferSO : ScriptableObject
    {
        [Header("Product")]
        [SerializeField] private string _productId = "product_id";
        [SerializeField] private bool _repeatable;

        [Header("Presentation")]
        [SerializeField] private PanelType _panel = PanelType.Packs;
        [SerializeField] private StoreOfferTier _tier = StoreOfferTier.Basic;
        [SerializeField] private string _title = "Offer";
        [SerializeField, TextArea] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _badge;
        [SerializeField] private string _fallbackPrice = "";

        [Header("Reward")]
        [SerializeField] private int _softAmount;
        [SerializeField] private List<BoosterReward> _boosters = new List<BoosterReward>();
        [SerializeField] private bool _grantNoAds;

        public string ProductId => _productId;

        public bool Repeatable => _repeatable;

        public PanelType Panel => _panel;

        public StoreOfferTier Tier => _tier;

        public string Title => _title;

        public string Description => _description;

        public Sprite Icon => _icon;

        /// <summary>
        /// Shown until the platform returns a real localized price.
        /// </summary>
        public string FallbackPrice => _fallbackPrice;

        public int SoftAmount => _softAmount;

        public IReadOnlyList<BoosterReward> Boosters => _boosters;

        public bool GrantNoAds => _grantNoAds;

        public bool HasReward => _softAmount > 0 || _grantNoAds || HasBoosters;

        private bool HasBoosters
        {
            get
            {
                for (var index = 0; index < _boosters.Count; index++)
                {
                    if (_boosters[index].Count > 0)
                        return true;
                }

                return false;
            }
        }
    }
}
