using System;
using System.Collections.Generic;

namespace Core.Ship.States
{
    public class ShipStateMachine : IDisposable
    {
        private readonly Dictionary<Type, IShipState> _states;
        
        private IShipState _currentState;

        public ShipStateMachine(ShipParkingMover shipParkingMover, Ship ship, BattleShipMover shipBattleMover)
        {
            _states = new Dictionary<Type, IShipState>()
            {
                {typeof(ParkingState), new ParkingState()},
                {typeof(EscapeParkingState), new EscapeParkingState(shipParkingMover, ship)},
                //{typeof(MoveToBattleFromParkingState), new MoveToBattleFromParkingState()},
                {typeof(BattleState), new BattleState(ship, shipBattleMover)},
                {typeof(EndBattleRoundState), new EndBattleRoundState()},
                {typeof(WaitingState), new WaitingState()},
                {typeof(MoveToBattleFromWaitingZone), new MoveToBattleFromWaitingZone()},
                {typeof(EscapeFromBattle), new EscapeFromBattle()},
            };
            
            _currentState = _states[typeof(ParkingState)];
            _currentState?.Enter();
        }

        public void ChangeState(Type stateType)
        {
            if (_states.TryGetValue(stateType, out IShipState state))
            {
                if (_currentState == state || state == null)
                    return;
                
                _currentState?.Exit();
                _currentState = state;
                _currentState?.Enter();
            }
        }
        
        public void Dispose()
        {
            _currentState?.Exit();
            _states.Clear();
        }
    }
}