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
        private readonly IShipsController _shipsController;
        private bool _parkingCellsReleased;

        public Ship(ShipData shipData, ShipView view, ShipParkingMover shipParkingMover,
            Action<RimPlacement, int> releaseParkingCells, BattleShipMover shipBattleMover,
            ShipShooterProvider shootProvider, IShipsController shipsController)
        {
            _shipData = shipData;
            _view =  view;
            _shipParkingMover  = shipParkingMover;
            _releaseParkingCells = releaseParkingCells;
            _shootProvider =  shootProvider;
            _shipsController = shipsController;

            _stateMachine = new ShipStateMachine(_shipParkingMover, this, shipBattleMover,
                shipsController);
            _isOnParking = true;
        }
        
        public Transform Transform => _view.transform;

        public bool IsOnParking => _isOnParking;
        
        public RimPlacement Placement => _view.Placement;
        public bool IsX => Placement.Dx != 0 && Math.Abs(Placement.Dx) > Math.Abs(Placement.Dy);
        
        public bool IsNoEmpty => _shipData.HasBullets;

        /// <summary>
        /// Set when a volley is over or the ship left the battle, so an orchestrator can wait for
        /// the ship instead of polling its ammo.
        /// </summary>
        public bool IsVolleyFinished { get; private set; } = true;

        public void CompleteMission()
        {
            IsVolleyFinished = true;
            _view.CompleteMission();
            _stateMachine.Dispose();
            _shipsController.OnShipCompleted(this);
        }

        /// <summary>
        /// Removes the ship from the parking lot without completing its mission: the cells are
        /// released, the view is hidden, but no battle slot or completion event is involved.
        /// </summary>
        public void ForceRemoveFromParking()
        {
            if (!_isOnParking)
                return;

            _isOnParking = false;
            IsVolleyFinished = true;
            ReleaseParkingCells();
            _view.Hide();
        }

        public void ReleaseParkingCells()
        {
            if (_parkingCellsReleased)
                return;

            _parkingCellsReleased = true;
            _releaseParkingCells(Placement, _shipData.Size);
            _shipsController.OnShipLeftParking();
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

        public bool CanShoot(PathPoint point)
        {
            return IsNoEmpty && _shootProvider.CanShoot(point, _shipData.ColorIndex);
        }

        public void MoveToWaitingZone(Transform target, Action onComplete)
        {
            _view.MoveToWaitingZone(target, onComplete);
        }

        public void StopWaitingMovement()
        {
            _view.StopWaitingMovement();
        }

        public void WaitingArrived()
        {
            _stateMachine.ChangeState(typeof(WaitingState));
        }

        public void StartWaitingRoutine(System.Collections.IEnumerator routine)
        {
            _view.StartWaitingRoutine(routine);
        }

        public void StopWaitingRoutine()
        {
            _view.StopWaitingRoutine();
        }

        public void ReturnToBattle()
        {
            _stateMachine.ChangeState(typeof(MoveToBattleFromWaitingZone));
        }

        public void MoveToBattle(Transform target, Action onComplete)
        {
            _view.MoveToBattle(target, onComplete);
        }

        public void StopBattleMovement()
        {
            _view.StopBattleMovement();
        }

        public bool TryShoot(PathPoint point)
        {
            if (!IsNoEmpty || !_shootProvider.TryShoot(point, _shipData.ColorIndex))
                return false;

            Shoot();
            return true;
        }

        public void StartVolley()
        {
            IsVolleyFinished = false;

            // A ship may still be flying into a zone: without this it would arrive after the
            // volley started and overwrite the volley state with WaitingState.
            StopWaitingMovement();
            _stateMachine.ChangeState(typeof(VolleyState));
        }

        /// <summary>
        /// Hits any block of the ship color, even one buried inside a chain.
        /// </summary>
        public bool TryVolleyShoot()
        {
            if (!IsNoEmpty || !_shootProvider.TryVolleyShoot(_shipData.ColorIndex))
                return false;

            Shoot();
            return true;
        }

        public void LeaveAfterVolley()
        {
            IsVolleyFinished = true;
            _stateMachine.ChangeState(typeof(EscapeFromBattle));
        }

        public void StartVolleyRoutine(System.Collections.IEnumerator routine)
        {
            _view.StartVolleyRoutine(routine);
        }

        public void StopVolleyRoutine()
        {
            _view.StopVolleyRoutine();
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