using Galaxy.Lobby.Presenters;
using Galaxy.Lobby.Routing;

namespace Galaxy.Lobby.States
{
    public class SettingState :  ILobbyState
    {
        private readonly SettingPresenter _presenter;
        public LobbyStateId Id => LobbyStateId.Settings;

        public SettingState(SettingPresenter presenter)
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