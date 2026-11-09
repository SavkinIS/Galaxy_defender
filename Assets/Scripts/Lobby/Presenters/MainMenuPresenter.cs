using Core.Currency;
using Galaxy.Infrastructure.Routing;
using Galaxy.Lobby.Routing;
using Galaxy.Lobby.Screens;
using VContainer.Unity;

namespace Galaxy.Lobby.Presenters
{
    public class MainMenuPresenter : IInitializable
    {
        private readonly MainMenuScreen _mainMenuScreen;
        private readonly LobbyStateMachine _lobbyStateMachine;
        private readonly IGlobalRouter _globalRouter;
        private readonly ISoftCurrencyService _currencyService;

        public MainMenuPresenter(MainMenuScreen mainMenuScreen,
            LobbyStateMachine lobbyStateMachine, IGlobalRouter globalRouter,
            ISoftCurrencyService currencyService)
        {
            _mainMenuScreen = mainMenuScreen;
            _lobbyStateMachine = lobbyStateMachine;
            _globalRouter = globalRouter;
            _currencyService = currencyService;
        }

        public void Initialize()
        {
            _mainMenuScreen.Initialize(ChangeToGame, OpenShop, OpenSetting);
        }

        public void Activate()
        {
            _mainMenuScreen.Show();
        }

        public void Deactivate()
        {
            _mainMenuScreen.Hide();
        }
        
        private void ChangeToGame()
        {
            _globalRouter.ChangeScene(SceneId.Game);
        }

        private void OpenShop()
            => ChangeState(LobbyStateId.Shop);

        private void OpenSetting()
            => ChangeState(LobbyStateId.Settings);

        private void ChangeState(LobbyStateId stateId)
        {
            _lobbyStateMachine.ChangeState(stateId);
        }
    }
}