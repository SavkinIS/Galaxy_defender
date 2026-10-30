using System;

namespace Core
{
    public enum GameSessionState
    {
        Running,
        Failed,
        Won
    }

    public class GameSessionHandler : IGameSessionHandler, IDisposable
    {
        private readonly GameTimeHandler _gameTimeHandler;

        public GameSessionHandler(GameSessionSettingsSO settings)
        {
            float parkingEmptyTimeScale = settings != null
                ? settings.AcceleratedTimeScale
                : GameSessionSettingsSO.DefaultAcceleratedTimeScale;
            float fullBattleTimeScale = settings != null
                ? settings.FullBattleTimeScale
                : GameSessionSettingsSO.DefaultFullBattleTimeScale;
            _gameTimeHandler = new GameTimeHandler(parkingEmptyTimeScale, fullBattleTimeScale);
        }

        public GameSessionState State { get; private set; } = GameSessionState.Running;
        public bool IsRunning => State == GameSessionState.Running;

        public event Action WaitingZoneOverflow;
        public event Action BattleCompleted;

        public void UpdateTimeAcceleration(bool allBattleSlotsOccupied, bool shipsRemainOnParking)
        {
            if (IsRunning)
                _gameTimeHandler.UpdateAcceleration(allBattleSlotsOccupied, shipsRemainOnParking);
        }

        public void HandleBattleCompleted()
        {
            if (!IsRunning)
                return;

            State = GameSessionState.Won;
            _gameTimeHandler.Reset();
            BattleCompleted?.Invoke();
        }

        public void HandleWaitingZoneOverflow()
        {
            if (!IsRunning)
                return;

            State = GameSessionState.Failed;
            _gameTimeHandler.Reset();
            WaitingZoneOverflow?.Invoke();
        }

        public void Dispose()
        {
            _gameTimeHandler.Reset();
        }
    }
}
