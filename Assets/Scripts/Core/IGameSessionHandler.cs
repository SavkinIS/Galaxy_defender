using System;

namespace Core
{
    public interface IGameSessionHandler
    {
        event Action BattleCompleted;

        bool IsRunning { get; }
        void UpdateTimeAcceleration(bool allBattleSlotsOccupied, bool shipsRemainOnParking);
        void HandleBattleCompleted();
        void HandleWaitingZoneOverflow();
    }
}
