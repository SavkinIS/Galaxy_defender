using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Waiting
{
    public interface IWaitingZonesController
    {
        /// <summary>
        /// True when every starter zone holds a ship. Zones unlocked with a rewarded ad are not
        /// taken into account.
        /// </summary>
        bool AreAllStarterZonesOccupied { get; }

        /// <summary>
        /// Ships reserved in any zone, including the zones unlocked with a rewarded ad.
        /// </summary>
        IReadOnlyList<Transform> OccupiedShips { get; }

        int Capacity { get; }

        /// <summary>
        /// Reserves a free unlocked zone for the ship. If every unlocked zone is busy and a locked
        /// zone is available, a rewarded ad is requested: on reward the zone is unlocked and the
        /// ship is reserved in it, otherwise the reservation fails.
        /// </summary>
        void TryReserve(Transform ship, Action<Transform> onReserved, Action onFailed = null);
        void PlaceShip(Transform ship);
        void ReleaseShip(Transform ship);
        void MouseClicked(Vector2 screenPosition);
    }
}
