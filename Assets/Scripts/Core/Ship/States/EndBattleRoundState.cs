using Galaxy.Game.Ship;
using UnityEngine;

namespace Core.Ship.States
{
    public class EndBattleRoundState : IShipState
    {
        private readonly Ship _ship;
        private readonly BattleShipMover _battleMover;
        private readonly IShipsController _shipsController;
        private bool _hasReservation;
        private bool _hasArrived;
        private bool _isActive;

        public EndBattleRoundState(Ship ship, BattleShipMover battleMover,
            IShipsController shipsController)
        {
            _ship = ship;
            _battleMover = battleMover;
            _shipsController = shipsController;
        }

        public void Enter()
        {
            _hasArrived = false;
            _hasReservation = false;
            _isActive = true;

            if (!_ship.IsNoEmpty)
            {
                _ship.CompleteMission();
                return;
            }

            var points = _battleMover.Points;
            
            for (var index = 0; index < points.Count; index++)
            {
                if (!_ship.CanShoot(points[index]))
                    continue;

                if (!_shipsController.TryEnterBattle(_ship))
                    break;

                _ship.ParkingEscaped();
                return;
            }
            
            // The reservation may require a rewarded ad when every unlocked zone is busy,
            // so the placement continues in the callback.
            _shipsController.TryReserveWaitingZone(_ship, OnReserved, OnReserveFailed);
        }

        private void OnReserved(Transform target)
        {
            if (!_isActive)
            {
                _shipsController.ReleaseWaitingReservation(_ship);
                return;
            }

            _hasReservation = true;
            _ship.MoveToWaitingZone(target, OnArrived);
        }

        private void OnReserveFailed()
        {
            // The waiting zone overflow is reported by the zones controller.
        }

        private void OnArrived()
        {
            _hasArrived = true;
            _shipsController.OnShipWaiting(_ship);
            _ship.WaitingArrived();
        }

        public void Exit()
        {
            _isActive = false;
            _ship.StopWaitingMovement();
            if (!_hasArrived && _hasReservation)
                _shipsController.ReleaseWaitingReservation(_ship);

            _hasReservation = false;
        }
    }
}