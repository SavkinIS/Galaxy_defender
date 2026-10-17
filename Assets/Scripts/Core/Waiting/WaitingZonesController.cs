using System.Collections.Generic;
using UnityEngine;

namespace Core.Waiting
{
    public class WaitingZonesController : IWaitingZonesController
    {
        private readonly WaitingZone[] _zones;
        private readonly IGameSessionHandler _gameSession;
        private readonly Dictionary<Transform, WaitingZone> _reservations =
            new Dictionary<Transform, WaitingZone>();

        public int Capacity => _zones.Length;

        public WaitingZonesController(IReadOnlyList<WaitingZoneView> zoneViews,
            IGameSessionHandler gameSession, int requestedSlots = SlotRange.MaximumAllowedSlots)
        {
            _gameSession = gameSession;
            var zones = new List<WaitingZone>();
            var addedViews = new HashSet<WaitingZoneView>();
            int limit = Mathf.Clamp(requestedSlots, 0, SlotRange.MaximumAllowedSlots);
            if (zoneViews != null)
            {
                for (int index = 0; index < zoneViews.Count; index++)
                {
                    WaitingZoneView view = zoneViews[index];
                    if (view == null || !addedViews.Add(view))
                        continue;

                    bool available = view.ShipPosition != null && zones.Count < limit;
                    view.gameObject.SetActive(available);
                    if (available)
                        zones.Add(new WaitingZone(view));
                }
            }
            _zones = zones.ToArray();
        }

        public bool TryReserve(Transform ship, out Transform target)
        {
            target = null;
            if (ship == null)
                return false;

            if (_reservations.TryGetValue(ship, out var reservedZone))
            {
                target = reservedZone.ShipPosition;
                return true;
            }

            for (var index = 0; index < _zones.Length; index++)
            {
                var zone = _zones[index];
                if (zone == null || !zone.TryReserve(ship))
                    continue;

                _reservations.Add(ship, zone);
                target = zone.ShipPosition;
                return true;
            }

            _gameSession.HandleWaitingZoneOverflow();
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
