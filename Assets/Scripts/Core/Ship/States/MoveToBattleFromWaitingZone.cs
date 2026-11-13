using Galaxy.Game.Ship;

namespace Core.Ship.States
{
    public class MoveToBattleFromWaitingZone : IShipState
    {
        private readonly Ship _ship;
        private readonly BattleShipMover _battleMover;
        private readonly IShipsController _shipsController;
        private bool _hasArrived;

        public MoveToBattleFromWaitingZone(Ship ship, BattleShipMover battleMover,
            IShipsController shipsController)
        {
            _ship = ship;
            _battleMover = battleMover;
            _shipsController = shipsController;
        }

        public void Enter()
        {
            _hasArrived = false;
            _ship.MoveToBattle(_battleMover.Points[0].Transform, OnArrived);
        }

        private void OnArrived()
        {
            _hasArrived = true;
            _ship.ParkingEscaped();
        }

        public void Exit()
        {
            _ship.StopBattleMovement();
            if (!_hasArrived)
                _shipsController.ReleaseBattleSlot(_ship);
        }
    }
}