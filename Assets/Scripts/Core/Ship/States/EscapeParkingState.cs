using UnityEngine;

namespace Core.Ship.States
{
    public class EscapeParkingState : IShipState
    {
        private readonly ShipParkingMover _shipParkingMover;
        private readonly Ship _ship;

        public EscapeParkingState(ShipParkingMover shipParkingMover, Ship ship)
        {
            _shipParkingMover = shipParkingMover;
            _ship = ship;
        }

        public void Enter()
        {
            var placement = _ship.Placement;
            bool isMovingAlongX = placement.EndY == -1;
            int line = isMovingAlongX ? placement.FrontY : placement.FrontX;
            bool isPositiveDirection = isMovingAlongX ? placement.EndX == 0 : placement.EndY > 0;

            _shipParkingMover.EscapeFromParking(
                _ship.Transform,
                line,
                isMovingAlongX,
                isPositiveDirection,
                OnComplete);
        }

        private void OnComplete()
        {
            _ship.ReleaseParkingCells();
            _ship.ParkingEscaped();
        }

        public void Exit()
        {
           
        }
    }
}