using UnityEngine;

namespace Core.Waiting
{
    public class WaitingZone
    {
        private readonly WaitingZoneView _view;
        private Transform _ship;

        public WaitingZone(WaitingZoneView view)
        {
            _view = view;
        }

        public Transform ShipPosition => _view.ShipPosition;
        public bool IsFree => _ship == null && ShipPosition != null;

        public bool TryReserve(Transform ship)
        {
            if (ship == null || !IsFree)
                return false;

            _ship = ship;
            return true;
        }

        public void PlaceShip(Transform ship)
        {
            if (ship == null || _ship != ship)
                return;

            _view.PlaceShip(ship);
        }

        public void ReleaseShip(Transform ship)
        {
            if (_ship == ship)
                _ship = null;
        }
    }
}
