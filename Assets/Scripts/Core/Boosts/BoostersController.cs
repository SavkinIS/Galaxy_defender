using System;
using System.Collections.Generic;
using Core.Currency;
using UnityEngine;

namespace Core.Boosts
{
    public sealed class BoostersController
    {
        private const string CountKeyFormat = "Booster.{0}.Count";

        private readonly Dictionary<BoosterType, IBooster> _boosters =
            new Dictionary<BoosterType, IBooster>();
        private readonly Dictionary<BoosterType, BoosterConfigSO> _configs =
            new Dictionary<BoosterType, BoosterConfigSO>();
        private readonly Dictionary<BoosterType, int> _counts =
            new Dictionary<BoosterType, int>();
        private readonly ISoftCurrencyController _currency;
        private IBooster _active;

        public BoostersController(IReadOnlyList<BoosterConfigSO> configs,
            ISoftCurrencyController currency, BoosterContext context)
        {
            _currency = currency;

            if (configs != null)
            {
                for (var index = 0; index < configs.Count; index++)
                {
                    var config = configs[index];
                    if (config == null)
                        continue;

                    var booster = BoosterFactory.Create(config, context);
                    if (booster == null)
                        continue;

                    if (_boosters.ContainsKey(config.Type))
                    {
                        Debug.LogWarning(
                            $"[Boosters] Duplicate config for type {config.Type}: " +
                            $"'{config.name}' is ignored.", config);
                        continue;
                    }

                    _configs[config.Type] = config;
                    _boosters[config.Type] = booster;
                    _counts[config.Type] = LoadCount(config);
                }
            }

            if (_currency != null)
                _currency.BalanceChanged += OnBalanceChanged;
        }

        public event Action<BoosterType> StateChanged;
        public event Action<BoosterType> PurchaseRequested;

        public IReadOnlyCollection<BoosterType> Types => _boosters.Keys;

        public int GetCount(BoosterType type)
        {
            return _counts.TryGetValue(type, out var count) ? count : 0;
        }

        public int GetPrice(BoosterType type)
        {
            return _configs.TryGetValue(type, out var config) && config != null ? config.Price : 0;
        }

        public int GetMaxCount(BoosterType type)
        {
            return _configs.TryGetValue(type, out var config) && config != null ? config.MaxCount : 0;
        }

        public bool IsWaitingTarget(BoosterType type)
        {
            return _active != null && _active.Type == type;
        }

        /// <summary>
        /// A booster with stock is clickable when it can be applied; an empty one is clickable
        /// only when the purchase is affordable.
        /// </summary>
        public bool CanClick(BoosterType type)
        {
            if (!_boosters.TryGetValue(type, out var booster) || booster == null)
                return false;

            if (_active != null && _active.Type == type)
                return true;

            if (GetCount(type) > 0)
                return booster.CanApply();

            return _currency != null && _currency.Balance >= GetPrice(type);
        }

        public void Click(BoosterType type)
        {
            if (!_boosters.TryGetValue(type, out var booster) || booster == null)
                return;

            if (_active != null && _active.Type == type)
            {
                DeactivateActive();
                return;
            }

            if (!CanClick(type))
                return;

            DeactivateActive();

            if (GetCount(type) <= 0)
            {
                PurchaseRequested?.Invoke(type);
                return;
            }

            if (booster.RequiresTarget)
            {
                _active = booster;
                booster.Cancel();
                Notify(type);
                return;
            }

            if (!booster.Apply())
                return;

            Consume(type);
        }

        /// <summary>
        /// Re-reads the world state for every booster: stock and balance are cached in the views,
        /// so a board change (a ship parked, a zone filled) must trigger this.
        /// </summary>
        public void RefreshAll()
        {
            foreach (var type in _boosters.Keys)
                Notify(type);
        }

        /// <summary>
        /// Routes a world click to the active booster. Returns true when the click belongs to the
        /// booster, so ships and zones must not handle it.
        /// </summary>
        public bool TryHandleWorldClick(Vector2 screenPosition)
        {
            if (_active == null)
                return false;

            if (_active.TryApplyAt(screenPosition) != BoosterTargetResult.Consumed)
                return true;

            var type = _active.Type;
            _active = null;
            Consume(type);
            return true;
        }

        public void CancelActive()
        {
            DeactivateActive();
        }

        public bool TryPurchase(BoosterType type)
        {
            if (!_configs.TryGetValue(type, out var config) || config == null)
                return false;

            if (_currency == null || _currency.Balance < config.Price)
                return false;

            if (GetCount(type) >= config.MaxCount)
                return false;

            if (!_currency.TrySpend(config.Price))
                return false;

            SetCount(type, Mathf.Min(config.MaxCount, GetCount(type) + 1));
            return true;
        }

        private void DeactivateActive()
        {
            if (_active == null)
                return;

            var type = _active.Type;
            _active.Cancel();
            _active = null;
            Notify(type);
        }

        private void Consume(BoosterType type)
        {
            SetCount(type, Mathf.Max(0, GetCount(type) - 1));
        }

        private void SetCount(BoosterType type, int count)
        {
            _counts[type] = count;
            PlayerPrefs.SetInt(CountKey(type), count);
            PlayerPrefs.Save();
            Notify(type);
        }

        private void Notify(BoosterType type)
        {
            StateChanged?.Invoke(type);
        }

        private void OnBalanceChanged(int balance)
        {
            foreach (var type in _boosters.Keys)
                Notify(type);
        }

        private static string CountKey(BoosterType type)
        {
            return string.Format(CountKeyFormat, type);
        }

        private static int LoadCount(BoosterConfigSO config)
        {
            // First launch has no saved value: hand out the configured starter stock, so every
            // booster can be tried before the player pays for the next one.
            var stored = PlayerPrefs.GetInt(CountKey(config.Type), -1);

            if (stored < 0)
                return config.StartCount;

            return Mathf.Clamp(stored, 0, config.MaxCount);
        }
    }
}
