using System.Collections.Generic;
using Galaxy.Lobby.Routing;
using VContainer.Unity;

namespace Galaxy.Lobby
{
    public class LobbyEntryPoint : IStartable
    {
        private readonly LobbyStateMachine _stateMachine;

        public LobbyEntryPoint(LobbyStateMachine  stateMachine, IReadOnlyList<ILobbyState> states)
        {
            _stateMachine = stateMachine;
            
            foreach (var state in states)
                _stateMachine.RegisterState(state);
        }
        
        public void Start()
        {
            _stateMachine?.ChangeState(LobbyStateId.MainMenu);
        }
    }
}