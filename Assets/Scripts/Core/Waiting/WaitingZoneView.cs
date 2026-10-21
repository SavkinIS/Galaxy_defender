using System;
using UnityEngine;

namespace Core.Waiting
{
    public class WaitingZoneView : MonoBehaviour
    {
        [SerializeField] private GameObject _adBtn;
        [SerializeField] private Transform _shipPosition;
        [SerializeField] private GameObject _lockedOverlay;
        
        private Transform _transform;

        public Transform Transform => _transform;
        public Transform ShipPosition => _shipPosition;

        private void Awake()
        {
            _transform = transform;
        }

        public void PlaceShip(Transform ship)
        {
            if (ship == null || _shipPosition == null)
                return;

            ship.position = _shipPosition.position;
            ship.rotation = _shipPosition.rotation;
        }

        public void SetLocked(bool locked)
        {
            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(locked);
        }
    }
}