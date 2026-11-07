using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;
using Core.Ad;
using VContainer;

namespace Core.Waiting
{
    public class WaitingZonesController : IWaitingZonesController
    {
        private WaitingZone[] _zones;
        private IGameSessionHandler _gameSession;
        private Dictionary<Transform, WaitingZone> _reservations =
            new Dictionary<Transform, WaitingZone>();
        private Dictionary<WaitingZoneView, WaitingZone> _viewToZone =
            new Dictionary<WaitingZoneView, WaitingZone>();
        private int _totalSlots;
        private AdUnlockZoneOffer _unlockZoneOffer;
        private AdOverflowOffer _overflowOffer;

        public int Capacity => _totalSlots;

        public bool AreAllStarterZonesOccupied
        {
            get
            {
                var hasStarter = false;

                for (var index = 0; index < _zones.Length; index++)
                {
                    var zone = _zones[index];
                    if (zone == null || !zone.IsStarter)
                        continue;

                    hasStarter = true;

                    if (!zone.IsOccupied)
                        return false;
                }

                return hasStarter;
            }
        }

        public IReadOnlyList<Transform> OccupiedShips
        {
            get
            {
                var ships = new List<Transform>();

                foreach (var reservation in _reservations)
                {
                    if (reservation.Key != null && reservation.Value != null
                        && reservation.Value.IsOccupied)
                        ships.Add(reservation.Key);
                }

                return ships;
            }
        }

        [Inject]
        public void Construct(IGameSessionHandler gameSession,
            GameSessionSettingsSO settings,
            ShipsPuzzle puzzle,
            AdUnlockZoneOffer unlockZoneOffer,
            AdOverflowOffer overflowOffer,
            WaitingZonesHandler waitingZonesHandler)
        {
            var zoneViews = waitingZonesHandler.Zones;
            _gameSession = gameSession;
            _unlockZoneOffer = unlockZoneOffer;
            _overflowOffer = overflowOffer;
            var zones = new List<WaitingZone>();

            int requestedSlots = puzzle.RequestedWaitingSlots > 0
                ? puzzle.RequestedWaitingSlots
                : SlotRange.MaximumAllowedSlots;

            int totalViews = zoneViews?.Count ?? 0;
            int slotsToConsider = Mathf.Min(requestedSlots, totalViews);
            int lockedCount = Mathf.Min(settings.InitialLockedZonesCount, slotsToConsider);
            int unlockedCount = slotsToConsider - lockedCount;

            _totalSlots = slotsToConsider;

            for (int i = 0; i < slotsToConsider; i++)
            {
                WaitingZoneView view = zoneViews[i];
                if (view == null)
                    continue;

                bool isLocked = i >= (slotsToConsider - lockedCount); 
                var zone = new WaitingZone(view, isLocked);
                zones.Add(zone);
                _viewToZone[view] = zone;

                view.gameObject.SetActive(true);
                view.SetLocked(isLocked);

            }

            _zones = zones.ToArray();
        }

        public void MouseClicked(Vector2 screenPosition)
        {
            var adController = ADController.Instance;
            if (adController != null && adController.IsShowing)
                return;

            if (!RaycastLaunch.TryGetClickedObject<WaitingZoneView>(screenPosition, out var view))
                return;

            if (!_viewToZone.TryGetValue(view, out var zone) || !zone.IsLocked)
                return;

            // The player clicked a locked zone: offer a rewarded ad to unlock it.
            if (_unlockZoneOffer != null)
                _unlockZoneOffer.Show(() => UnlockZone(zone));
        }

        private void UnlockZone(WaitingZone zone)
        {
            if (!zone.IsLocked)
                return;

            zone.SetLocked(false);
            zone.View.SetLocked(false);
        }

        public void TryReserve(Transform ship, Action<Transform> onReserved, Action onFailed = null)
        {
            if (ship == null)
            {
                onFailed?.Invoke();
                return;
            }

            if (_reservations.TryGetValue(ship, out var reservedZone))
            {
                onReserved?.Invoke(reservedZone.ShipPosition);
                return;
            }

            if (TryReserveInFreeZone(ship, out var target))
            {
                onReserved?.Invoke(target);
                return;
            }

            var lockedZone = _zones.FirstOrDefault(z => z != null && z.IsLocked);
            
            if (lockedZone == null)
            {
                onFailed?.Invoke();
                _gameSession.HandleWaitingZoneOverflow();
                return;
            }

            Action onAccepted = () =>
            {
                UnlockZone(lockedZone);
                if (TryReserveInFreeZone(ship, out var unlockedTarget))
                {
                    onReserved?.Invoke(unlockedTarget);
                    return;
                }

                onFailed?.Invoke();
                _gameSession.HandleWaitingZoneOverflow();
            };
            Action onDeclined = () =>
            {
                onFailed?.Invoke();
                _gameSession.HandleWaitingZoneOverflow();
            };

            if (_overflowOffer != null)
            {
                _overflowOffer.Show(onAccepted, onDeclined);
                return;
            }

            onFailed?.Invoke();
            _gameSession.HandleWaitingZoneOverflow();
        }

        private bool TryReserveInFreeZone(Transform ship, out Transform target)
        {
            target = null;

            if (_reservations.ContainsKey(ship))
                return false;

            for (var index = 0; index < _zones.Length; index++)
            {
                var zone = _zones[index];
                if (zone == null || !zone.TryReserve(ship))
                    continue;

                _reservations.Add(ship, zone);
                target = zone.ShipPosition;
                return true;
            }

            return false;
        }

        public void PlaceShip(Transform ship)
        {
            if (_reservations.TryGetValue(ship, out var zone))
                zone.PlaceShip(ship);
        }

        public void ReleaseShip(Transform ship)
        {
            if (!_reservations.TryGetValue(ship, out var zone))
                return;

            zone.ReleaseShip(ship);
            _reservations.Remove(ship);
        }
    }
}
