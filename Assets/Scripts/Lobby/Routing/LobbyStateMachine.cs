using System.Collections.Generic;
using System.Linq;

namespace Galaxy.Lobby.Routing
{
    public class LobbyStateMachine
    {
        private readonly Dictionary<LobbyStateId, ILobbyState> _states =  new Dictionary<LobbyStateId, ILobbyState>();
        private ILobbyState _curreneState;

        public void RegisterState(ILobbyState  state)
        {
            _states[state.Id] = state;
        }

        public void ChangeState(LobbyStateId id)
        {
            if (_states.TryGetValue(id,out ILobbyState nextState) == false 
                || _curreneState?.Id == id)
                return;
            
            _curreneState?.Exit();
            _curreneState = nextState;
            _curreneState?.Enter();
        }
    }
}