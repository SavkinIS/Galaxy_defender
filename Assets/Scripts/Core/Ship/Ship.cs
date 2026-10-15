using System;
using Core.Ship.States;
using UnityEngine;

namespace Core.Ship
{
    public class Ship
    {
        private readonly ShipData _shipData;
        private readonly ShipView _view;
        private readonly ShipStateMachine _stateMachine;
        private readonly ShipParkingMover _shipParkingMover;
        private readonly Action<RimPlacement, int> _releaseParkingCells;
        private readonly ShipShooterProvider _shootProvider;
        private bool _isOnParking;
        private readonly Action<Ship> _onShipCompleted;

        public Ship(ShipData shipData, ShipView view, ShipParkingMover shipParkingMover,
            Action<RimPlacement, int> releaseParkingCells, BattleShipMover shipBattleMover,
            ShipShooterProvider shootProvider, Action<Ship> onShipCompleted)
        {
            _shipData = shipData;
            _view =  view;
            _shipParkingMover  = shipParkingMover;
            _releaseParkingCells = releaseParkingCells;
            _shootProvider =  shootProvider;
            _onShipCompleted = onShipCompleted;
            
            _stateMachine = new ShipStateMachine(_shipParkingMover, this, shipBattleMover);
            _isOnParking = true;
        }
        
        public Transform Transform => _view.transform;

        public bool IsOnParking => _isOnParking;
        
        public RimPlacement Placement => _view.Placement;
        public bool IsX => Placement.Dx != 0 && Math.Abs(Placement.Dx) > Math.Abs(Placement.Dy);
        
        public bool IsNoEmpty => _shipData.HasBullets;

        public void CompleteMission()
        {
            _view.CompleteMission();
            _stateMachine.Dispose();
            _onShipCompleted?.Invoke(this);
        }

        public void ReleaseParkingCells()
        {
            _releaseParkingCells(Placement, _shipData.Size);
        }

        public void EscapeParking()
        {
            _isOnParking = false;
            _view.EnableCounter();
            _view.Refresh();
            _stateMachine.ChangeState(typeof(EscapeParkingState));
        }

        public void ParkingEscaped()
        {
            _stateMachine.ChangeState(typeof(BattleState));
        }

        public bool TryShoot(PathPoint point)
        {
            if (!IsNoEmpty || !_shootProvider.TryShoot(point, _shipData.ColorIndex))
                return false;

            Shoot();
            return true;
        }

        private void Shoot()
        {
            _shipData.DecreaseBullets();
            _view.Refresh();

            if (IsNoEmpty == false)
            {
                _stateMachine.ChangeState((typeof(EscapeFromBattle)));
            }
        }

        public void EndRound()
        {
            _stateMachine.ChangeState(typeof(EndBattleRoundState));
        }
    }
}