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

        public WaitingZonesController(IReadOnlyList<WaitingZoneView> zoneViews,
            IGameSessionHandler gameSession)
        {
            _gameSession = gameSession;
            _zones = new WaitingZone[zoneViews?.Count ?? 0];

            for (var index = 0; index < _zones.Length; index++)
            {
                if (zoneViews[index] != null)
                    _zones[index] = new WaitingZone(zoneViews[index]);
            }
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
