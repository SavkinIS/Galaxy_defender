namespace Core
{
    public interface IGameSessionHandler
    {
        bool IsRunning { get; }
        void UpdateTimeAcceleration(bool allBattleSlotsOccupied, bool shipsRemainOnParking);
        void HandleBattleCompleted();
        void HandleWaitingZoneOverflow();
    }
}
