namespace Core.Ship.States
{
    public class BattleState : IShipState
    {
        private readonly Ship _ship;
        private readonly BattleShipMover _shipMover;

        public BattleState(Ship ship, BattleShipMover  shipMover)
        {
            _ship = ship;
            _shipMover = shipMover;
        }
        
        public void Enter()
        {
            _shipMover.Move(
                _ship.Transform,
                OnComplete, OnPointReached);
        }

        private void OnComplete()
        {
            _ship.EndRound();
        }

        private void OnPointReached(PathPoint point)
        {
            if (_ship.TryShoot(point) && _ship.IsEmpty)
            {
            }
        }

        public void Exit()
        {
            
        }
    }
}