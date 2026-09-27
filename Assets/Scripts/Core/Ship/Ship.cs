namespace Core.Ship
{
    public class Ship
    {
        private readonly ShipData _shipData;
        private readonly ShipView _view;

        public Ship(ShipData shipData, ShipView view)
        {
            _shipData = shipData;
            _view =  view;
        }
    }
}