using System;
using Core.Ad;
using Galaxy.Infrastructure.Routing;
using VContainer;
using VContainer.Unity;

namespace Core
{
    public enum GameSessionState
    {
        Running,
        Failed,
        Won
    }

    public class GameSessionHandler : IGameSessionHandler, IDisposable, IStartable
    {
        private GameTimeHandler _gameTimeHandler;
        private GamePauseService _pauseService;
        private IGlobalRouter _globalRouter;
        private WinPopup _winPopUp;
        private GameOverPopup _gameOverPopUp;
        private ExitButton _exitButton;

        [Inject]
        public void Construct(GameSessionSettingsSO settings, GamePauseService pauseService,
            WinPopup winPopup, IGlobalRouter globalRouter, GameOverPopup gameOverPopup, ExitButton exitButton)
        {
            float parkingEmptyTimeScale = settings != null
                ? settings.AcceleratedTimeScale
                : GameSessionSettingsSO.DefaultAcceleratedTimeScale;
            float fullBattleTimeScale = settings != null
                ? settings.FullBattleTimeScale
                : GameSessionSettingsSO.DefaultFullBattleTimeScale;
            _pauseService = pauseService;
            _gameTimeHandler = new GameTimeHandler(parkingEmptyTimeScale, fullBattleTimeScale,
                _pauseService);

            _globalRouter = globalRouter;
            _winPopUp = winPopup;
            _gameOverPopUp = gameOverPopup;
            _exitButton = exitButton;
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
            _pauseService?.ForceResume();
        }

        public void Start()
        {
            _winPopUp.SetToLobbyCallback(ChangeScene);
            _gameOverPopUp.SetToLobbyCallback(ChangeScene);
            _exitButton.SetToLobbyCallback(ChangeScene);
            
            Log.Info($"{this.GetType().Name} Started");
        }

        private void ChangeScene()
        {
            _globalRouter.ChangeScene(SceneId.Lobby);
        }
    }
}
