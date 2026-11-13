using Galaxy.Game.Ship;

namespace Core.Ship.States
{
    public class WaitingState : IShipState
    {
        private readonly Ship _ship;
        private readonly BattleShipMover _battleMover;
        private readonly IShipsController _shipsController;
        private readonly UnityEngine.WaitForSeconds _checkInterval =
            new UnityEngine.WaitForSeconds(0.1f);

        public WaitingState(Ship ship, BattleShipMover battleMover, IShipsController shipsController)
        {
            _ship = ship;
            _battleMover = battleMover;
            _shipsController = shipsController;
        }

        public void Enter()
        {
            _ship.StartWaitingRoutine(WaitForBattle());
        }

        private System.Collections.IEnumerator WaitForBattle()
        {
            while (true)
            {
                yield return _checkInterval;

                if (!_ship.IsNoEmpty)
                {
                    _ship.CompleteMission();
                    yield break;
                }

                if (!_shipsController.HasFreeBattleSlot)
                    continue;

                var points = _battleMover.Points;
                for (var index = 0; index < points.Count; index++)
                {
                    if (!_ship.CanShoot(points[index]))
                        continue;

                    if (!_shipsController.TryEnterBattle(_ship))
                        break;

                    _ship.ReturnToBattle();
                    yield break;
                }
            }
        }

        public void Exit()
        {
            _ship.StopWaitingRoutine();
        }
    }
}