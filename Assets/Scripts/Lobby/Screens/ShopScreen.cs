using System;
using System.Collections.Generic;
using System.Linq;
using Core.Currency;
using Galaxy.Lobby.Screens.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace Galaxy.Lobby.Screens
{
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _toMainMenuBtn;
        [SerializeField] private SoftCurrencyView _softCurrencyView;
        [SerializeField] private List<SelectedButtons> _panelButtons = new List<SelectedButtons>();
        [SerializeField] private List<OffersPanels> _panels = new List<OffersPanels>();
        [SerializeField] private PanelType _defaultPanel = PanelType.Packs;

        private Dictionary<PanelType, SelectedButtons> _panelsButtonsDic;
        private Dictionary<PanelType, OffersPanels> _panelsDic;

        public SoftCurrencyView SoftCurrencyView => _softCurrencyView;

        public IReadOnlyList<ShopItemView> Items => GetCards(p => p.Items);

        public IReadOnlyList<ShopOfferItemView> Offers => GetCards(p => p.Offers);

        public PanelType ActivePanel { get; private set; }

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

            _panelsButtonsDic = _panelButtons
                .Where(b => b != null)
                .GroupBy(b => b.PanelType)
                .ToDictionary(g => g.Key, g => g.First());

            _panelsDic = _panels
                .Where(p => p != null)
                .GroupBy(p => p.PanelType)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var button in _panelsButtonsDic.Values)
                button.Initialize(SelectPanel);

            SelectPanel(_panelsButtonsDic.ContainsKey(_defaultPanel)
                ? _defaultPanel
                : _panelsButtonsDic.Keys.FirstOrDefault());
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

        public void SelectPanel(PanelType panelType)
        {
            ActivePanel = panelType;

            foreach (var pair in _panelsButtonsDic)
                pair.Value.SetActive(pair.Key == panelType);

            foreach (var pair in _panelsDic)
                pair.Value.SetVisible(pair.Key == panelType);
        }

        private void ToMainMenu() => BackRequested?.Invoke();

        private IReadOnlyList<TCard> GetCards<TCard>(Func<OffersPanels, IReadOnlyList<TCard>> selector)
        {
            var cards = new List<TCard>();

            if (_panels == null)
                return cards;

            for (var index = 0; index < _panels.Count; index++)
            {
                var panel = _panels[index];
                if (panel == null)
                    continue;

                var panelCards = selector(panel);
                if (panelCards == null)
                    continue;

                for (var cardIndex = 0; cardIndex < panelCards.Count; cardIndex++)
                {
                    if (panelCards[cardIndex] != null)
                        cards.Add(panelCards[cardIndex]);
                }
            }

            return cards;
        }
    }
}
