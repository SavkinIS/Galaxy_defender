using Galaxy.Data.Infrastructure;
using Galaxy.Lobby.Screens.Shop;
using UnityEngine;

namespace Core.Boosts
{
    public abstract class BoosterConfigSO : ScriptableObject
    {
        [SerializeField] private PanelType _panel = PanelType.Boosters;
        [SerializeField, Min(0)] private int _price = 100;
        [SerializeField, Min(0)] private int _startCount = 3;
        [SerializeField, Min(1)] private int _maxCount = 3;
        [SerializeField] private Sprite _icon;

        public abstract BoosterType Type { get; }

        public PanelType Panel => _panel;

        public int Price => Mathf.Max(0, _price);
        public int StartCount => Mathf.Clamp(_startCount, 0, MaxCount);
        public int MaxCount => Mathf.Max(1, _maxCount);
        public Sprite Icon => _icon;
    }
}
