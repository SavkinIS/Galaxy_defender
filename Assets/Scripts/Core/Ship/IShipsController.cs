using UnityEngine;

namespace Core.Ship
{
    public interface IShipsController
    {
        bool HasFreeBattleSlot { get; }
        bool TryEnterBattle(Ship ship);
        void ReleaseBattleSlot(Ship ship);
        bool TryReserveWaitingZone(Ship ship, out Transform target);
        void ReleaseWaitingReservation(Ship ship);
        void OnShipWaiting(Ship ship);
        void OnShipCompleted(Ship ship);
        void OnShipLeftParking();
    }
}
