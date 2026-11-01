using System;
using UnityEngine;

namespace Core.Ship
{
    public interface IShipsController
    {
        int BattleSlots { get; }
        bool HasShipsOnParking { get; }
        bool HasFreeBattleSlot { get; }
        bool TryEnterBattle(Ship ship, bool ignoreSlotLimit = false);
        void ReleaseBattleSlot(Ship ship);
        void IncreaseBattleSlots(int delta);
        void TryReserveWaitingZone(Ship ship, Action<Transform> onReserved, Action onFailed = null);
        void ReleaseWaitingReservation(Ship ship);
        void OnShipWaiting(Ship ship);
        void OnShipCompleted(Ship ship);
        void OnShipLeftParking();
        void SuspendBattleCompletion(bool suspended);
        void CheckBattleCompletion();
        bool TryGetShip(Transform shipTransform, out Ship ship);
        bool TryForceEscape(ShipView shipView);
        int RemoveParkingShipsByColor(int colorIndex);
    }
}
