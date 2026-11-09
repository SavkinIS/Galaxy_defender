using System;
using System.Collections.Generic;
using Galaxy.Data.Infrastructure;
using Galaxy.Infrastructure.Boosters;
using UnityEngine;

namespace Core.Boosts
{
    /// <summary>
    /// Applies booster effects during a game session.
    /// Stock lives in <see cref="IBoosterInventoryService"/>, purchase in
    /// <see cref="IBoosterPurchaseService"/>; this class only owns target selection and
    /// interaction with the current board, so it belongs to the scene scope.
    /// </summary>
    public sealed class BoosterUsageController : IDisposable
    {
        private readonly Dictionary<BoosterType, IBooster> _boosters =
            new Dictionary<BoosterType, IBooster>();
        private readonly Dictionary<BoosterType, BoosterConfigSO> _configs =
            new Dictionary<BoosterType, BoosterConfigSO>();
        private readonly IBoosterInventoryService _inventory;
        private readonly IBoosterPurchaseService _purchase;
        private IBooster _active;

        public BoosterUsageController(BoosterCatalogSO catalog,
            IBoosterPurchaseService purchase, BoosterContext context, IBoosterInventoryService inventory)
        {
            _purchase = purchase;
            _inventory = inventory;

            IReadOnlyList<BoosterConfigSO> configs = catalog != null ? catalog.Boosters : null;

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

                    if (_inventory != null && !_inventory.TryGet(config.Type, out _))
                        _inventory.Set(config.Type, config.StartCount);
                }
            }

            if (_inventory != null)
                _inventory.Changed += OnInventoryChanged;
        }

        public event Action<BoosterType> StateChanged;
        public event Action<BoosterType> PurchaseRequested;

        public IReadOnlyCollection<BoosterType> Types => _boosters.Keys;

        public int GetCount(BoosterType type)
        {
            return _inventory != null ? _inventory.Get(type) : 0;
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

        public bool CanClick(BoosterType type)
        {
            if (!_boosters.TryGetValue(type, out var booster) || booster == null)
                return false;

            if (_active != null && _active.Type == type)
                return true;

            if (GetCount(type) > 0)
                return booster.CanApply();

            return _purchase != null && _purchase.CanPurchase(type);
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

        public void RefreshAll()
        {
            foreach (var type in _boosters.Keys)
                Notify(type);
        }

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
            return _purchase != null && _purchase.TryPurchase(type);
        }

        public void Dispose()
        {
            if (_inventory != null)
                _inventory.Changed -= OnInventoryChanged;
        }

        private void OnInventoryChanged(BoosterType type, int count)
        {
            Notify(type);
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
            if (_inventory != null)
                _inventory.TryConsume(type);

            Notify(type);
        }

        private void Notify(BoosterType type)
        {
            StateChanged?.Invoke(type);
        }
    }
}
