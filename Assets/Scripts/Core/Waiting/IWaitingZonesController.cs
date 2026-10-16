using UnityEngine;

namespace Core.Waiting
{
    public interface IWaitingZonesController
    {
        bool TryReserve(Transform ship, out Transform target);
        void PlaceShip(Transform ship);
        void ReleaseShip(Transform ship);
    }
}
