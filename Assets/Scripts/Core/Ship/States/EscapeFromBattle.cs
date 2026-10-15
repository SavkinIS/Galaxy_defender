namespace Core.Ship.States
{
    public class EscapeFromBattle : IShipState
    {
        private readonly Ship _ship;

        public EscapeFromBattle(Ship ship)
        {
            _ship = ship;
        }
        
        public void Enter()
        {
            _ship.CompleteMission();
        }

        public void Exit()
        {
           
        }
    }
}