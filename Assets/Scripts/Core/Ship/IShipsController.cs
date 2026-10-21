using System;
using UnityEngine;

namespace Core.Ship
{
    public interface IShipsController
    {
        bool HasFreeBattleSlot { get; }
        bool TryEnterBattle(Ship ship);
        void ReleaseBattleSlot(Ship ship);
        void TryReserveWaitingZone(Ship ship, Action<Transform> onReserved, Action onFailed = null);
        void ReleaseWaitingReservation(Ship ship);
        void OnShipWaiting(Ship ship);
        void OnShipCompleted(Ship ship);
        void OnShipLeftParking();
    }
}
