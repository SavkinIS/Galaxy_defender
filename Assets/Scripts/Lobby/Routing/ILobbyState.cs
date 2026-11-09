namespace Galaxy.Lobby.Routing
{
    public interface ILobbyState
    {
        LobbyStateId Id { get; }
        void Enter();
        void Exit();
    }
}