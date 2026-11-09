using Galaxy.Lobby.Routing;
using Galaxy.Lobby.Screens;
using VContainer.Unity;

namespace Galaxy.Lobby.Presenters
{
    public class ShopPresenter :  IInitializable
    {
        private readonly LobbyStateMachine _lobbyStateMachine;
        private readonly ShopScreen _screen;

        public ShopPresenter(ShopScreen screen,
            LobbyStateMachine lobbyStateMachine)
        {
            _screen = screen;
            _lobbyStateMachine = lobbyStateMachine;
        }

        public void Initialize()
        {
            _screen.Initialize(ToMainMenu);
        }

        public void Activate()
        {
            _screen.Show();
        }

        public void Deactivate()
        {
            _screen.Hide();
        }
        
        private void ToMainMenu()
            => ChangeState(LobbyStateId.MainMenu);

        private void ChangeState(LobbyStateId stateId)
        {
            _lobbyStateMachine.ChangeState(stateId);
        }
    }
}