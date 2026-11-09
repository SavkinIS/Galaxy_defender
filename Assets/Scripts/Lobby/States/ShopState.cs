using Galaxy.Lobby.Presenters;
using Galaxy.Lobby.Routing;

namespace Galaxy.Lobby.States
{
    public class ShopState :  ILobbyState
    {
        private readonly ShopPresenter _presenter;
        public LobbyStateId Id => LobbyStateId.Shop;

        public ShopState(ShopPresenter presenter)
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