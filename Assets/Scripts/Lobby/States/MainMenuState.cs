using Galaxy.Lobby.Presenters;
using Galaxy.Lobby.Routing;

namespace Galaxy.Lobby.States
{
    public class MainMenuState :  ILobbyState
    {
        private readonly MainMenuPresenter _presenter;
        public LobbyStateId Id => LobbyStateId.MainMenu;

        public MainMenuState(MainMenuPresenter presenter)
        {
            _presenter = presenter;
        }
        
        public void Enter()
        {
            _presenter.Activate();
        }

        public void Exit()
        {
            _presenter.Deactivate();
        }
    }
}