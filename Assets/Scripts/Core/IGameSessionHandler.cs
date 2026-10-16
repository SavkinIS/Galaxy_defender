namespace Core
{
    public interface IGameSessionHandler
    {
        bool IsRunning { get; }
        void HandleWaitingZoneOverflow();
    }
}
