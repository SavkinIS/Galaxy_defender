using System.Collections.Generic;
using UnityEngine;

namespace Galaxy.Lobby.Screens.Shop
{
    public class OffersPanels : MonoBehaviour
    {
        [SerializeField] private PanelType _panelType;
        [SerializeField] private List<ShopOfferItemView> _offers = new List<ShopOfferItemView>();

        public PanelType PanelType => _panelType;

        public IReadOnlyList<ShopOfferItemView> Offers => _offers;

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}