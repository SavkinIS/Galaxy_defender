using System;

namespace Core
{
    public enum GameSessionState
    {
        Running,
        Failed
    }

    public class GameSessionHandler : IGameSessionHandler
    {
        public GameSessionState State { get; private set; } = GameSessionState.Running;
        public bool IsRunning => State == GameSessionState.Running;

        public event Action WaitingZoneOverflow;

        public void HandleWaitingZoneOverflow()
        {
            if (!IsRunning)
                return;

            State = GameSessionState.Failed;
            WaitingZoneOverflow?.Invoke();
        }
    }
}
