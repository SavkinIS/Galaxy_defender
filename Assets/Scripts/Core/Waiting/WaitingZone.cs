using UnityEngine;

namespace Core.Waiting
{
    public class WaitingZone
    {
        public WaitingZoneView View { get; }
        private readonly bool _isStarter;
        private Transform _ship;
        private bool _isLocked;

        public WaitingZone(WaitingZoneView view, bool isLocked = false)
        {
            View = view;
            _isLocked = isLocked;
            _isStarter = !isLocked;
        }

        public Transform ShipPosition => View.ShipPosition;
        public bool IsFree => _ship == null && ShipPosition != null && !_isLocked;
        public bool IsLocked => _isLocked;
        public bool IsStarter => _isStarter;
        public bool IsOccupied => _ship != null;

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

            View.PlaceShip(ship);
        }

        public void ReleaseShip(Transform ship)
        {
            if (_ship == ship)
                _ship = null;
        }

        public void SetLocked(bool locked)
        {
            _isLocked = locked;
        }
    }
}
