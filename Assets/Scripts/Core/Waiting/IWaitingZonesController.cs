using System;
using UnityEngine;

namespace Core.Waiting
{
    public interface IWaitingZonesController
    {
        /// <summary>
        /// Reserves a free unlocked zone for the ship. If every unlocked zone is busy and a locked
        /// zone is available, a rewarded ad is requested: on reward the zone is unlocked and the
        /// ship is reserved in it, otherwise the reservation fails.
        /// </summary>
        void TryReserve(Transform ship, Action<Transform> onReserved, Action onFailed = null);
        void PlaceShip(Transform ship);
        void ReleaseShip(Transform ship);
    }
}
