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
            int line = 0;

            if (_ship.IsX)
            {
                if (_ship.Placement.FrontX > 4)
                {
                    line = _ship.Placement.FrontX;
                }
                else
                {
                    line = _ship.Placement.FrontX * -1;
                }
            }
            else
            {
                if (_ship.Placement.FrontY > 4)
                {
                    line = _ship.Placement.FrontY;
                }
                else
                {
                    line = _ship.Placement.FrontY * -1;
                }
            }
            
            _shipParkingMover.EscapeFromParking(_ship.Transform, line, _ship.IsX, OnComplete);
        }

        private void OnComplete()
        {
            _ship.ParkingEscaped();
        }

        public void Exit()
        {
           
        }
    }
}