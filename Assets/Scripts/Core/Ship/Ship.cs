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
        private bool _isOnParking;
        private readonly ShipParkingMover _shipParkingMover;

        public Ship(ShipData shipData, ShipView view, ShipParkingMover shipParkingMover)
        {
            _shipData = shipData;
            _view =  view;
            _shipParkingMover  = shipParkingMover;

            _stateMachine = new ShipStateMachine(_shipParkingMover, this);
            _isOnParking = true;
        }
        
        public Transform Transform => _view.transform;

        public bool IsOnParking => _isOnParking;
        
        public RimPlacement Placement => _view.Placement;
        public bool IsX => Placement.Dx != 0 && Math.Abs(Placement.Dx) > Math.Abs(Placement.Dy);

        public void EscapeParking()
        {
            _isOnParking = false;
            
            _stateMachine.ChangeState(typeof(EscapeParkingState));
        }

        public void ParkingEscaped()
        {
            _stateMachine.ChangeState(typeof(BattleState));
        }
    }
}