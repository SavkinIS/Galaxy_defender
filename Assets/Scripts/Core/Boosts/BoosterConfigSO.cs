using UnityEngine;

namespace Core.Boosts
{
    public abstract class BoosterConfigSO : ScriptableObject
    {
        [SerializeField, Min(0)] private int _price = 100;
        [SerializeField, Min(0)] private int _startCount = 3;
        [SerializeField, Min(1)] private int _maxCount = 3;
        [SerializeField] private Sprite _icon;

        /// <summary>
        /// Taken from the config class, not from a serialized field: a field left at its default
        /// value silently collapsed every booster into the same entry.
        /// </summary>
        public abstract BoosterType Type { get; }

        public int Price => Mathf.Max(0, _price);
        public int StartCount => Mathf.Clamp(_startCount, 0, MaxCount);
        public int MaxCount => Mathf.Max(1, _maxCount);
        public Sprite Icon => _icon;
    }
}
