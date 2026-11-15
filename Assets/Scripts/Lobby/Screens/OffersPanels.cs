using System.Collections.Generic;
using UnityEngine;

namespace Galaxy.Lobby.Screens.Shop
{
    public class OffersPanels : MonoBehaviour
    {
        [SerializeField] private PanelType _panelType;
        [SerializeField] private RectTransform _contentHolder;
        [SerializeField] private List<ShopOfferItemView> _offers = new List<ShopOfferItemView>();
        [SerializeField] private List<ShopItemView> _items = new List<ShopItemView>();

        public PanelType PanelType => _panelType;

        public RectTransform ContentHolder => _contentHolder;

        public IReadOnlyList<ShopOfferItemView> Offers => _offers;

        public IReadOnlyList<ShopItemView> Items => _items;

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}