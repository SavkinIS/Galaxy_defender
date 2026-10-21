using System;
using System.Collections.Generic;
using Core.Waiting;
using UnityEngine;

namespace Core.Ship
{
    public class ShipsController : IShipsController
    {
        private const int ShipsInBattle = 5;
        
        private readonly ShipsPuzzle _shipsPuzzle;
        private Dictionary<ShipView, Ship> _ships;
        private readonly ShipParkingMover _shipParkingMover;
        private readonly BattleShipMover _shipBattleMover;
        private readonly EnemyMap _enemyMap;
        private readonly EnemyCellConnector _enemyCellConnector;
        private readonly ShipShooterProvider _shootProvider;
        private readonly IWaitingZonesController _waitingZones;
        private readonly IGameSessionHandler _gameSession;

        private List<Ship> _activeShips =  new List<Ship>();
        private int _remainingParkingShips;

        public ShipsController(ShipsPuzzle shipsPuzzle, ShipParkingMover shipParkingMover,
            BattleShipMover shipBattleMover, EnemyMap enemyMap, EnemyCellConnector enemyCellConnector,
            IWaitingZonesController waitingZones, IGameSessionHandler gameSession)
        {
            _shipsPuzzle = shipsPuzzle;
            _shipParkingMover =  shipParkingMover;
            _shipBattleMover = shipBattleMover;
            _enemyMap = enemyMap;
            _enemyCellConnector = enemyCellConnector;
            _waitingZones = waitingZones;
            _gameSession = gameSession;

            _shootProvider = new ShipShooterProvider(_enemyMap, _enemyCellConnector);
        }
        
        public void Initialize()
        {
            _shipsPuzzle.Initialize();

            Dictionary<ShipData, ShipView> createdShips = _shipsPuzzle.CreatedShips;
            _ships = new Dictionary<ShipView, Ship>();

            if (createdShips == null)
                return;

            foreach (var createdShip in createdShips)
            {
                if (createdShip.Value != null && createdShip.Key != null)
                    _ships[createdShip.Value] = new Ship(
                        createdShip.Key,
                        createdShip.Value,
                        _shipParkingMover,
                        _shipsPuzzle.ReleaseShipCells,
                        _shipBattleMover, _shootProvider, this);
            }

            _remainingParkingShips = _ships.Count;
        }

        public void MouseClicked(Vector2 screenPosition)
        {
            var adController = ADController.Instance;
            if (adController != null && adController.IsShowing)
                return;

            if (!_gameSession.IsRunning || _activeShips.Count >= ShipsInBattle)
                return;
            
            if (RaycastLaunch.TryGetClickedObject<ShipView>(screenPosition, out var hitObject))
            {
                if (hitObject.TryGetComponent(out ShipView shipView))
                {
                    if (_ships.TryGetValue(shipView, out var ship))
                    {
                        if (ship != null && ship.IsOnParking && CanLeaveParking(ship)
                            && TryEnterBattle(ship))
                        {
                            ship.EscapeParking();
                        }
                    }
                }
            } 
        }

        public bool TryEnterBattle(Ship ship)
        {
            if (!_gameSession.IsRunning)
                return false;

            if (_activeShips.Contains(ship))
                return true;

            if (_activeShips.Count >= ShipsInBattle)
                return false;

            _activeShips.Add(ship);
            _waitingZones.ReleaseShip(ship.Transform);
            UpdateTimeAcceleration();
            return true;
        }

        public bool HasFreeBattleSlot => _gameSession.IsRunning && _activeShips.Count < ShipsInBattle;

        public void ReleaseBattleSlot(Ship ship)
        {
            if (_activeShips.Remove(ship))
                UpdateTimeAcceleration();
        }

        public void TryReserveWaitingZone(Ship ship, Action<Transform> onReserved, Action onFailed = null)
        {
            _waitingZones.TryReserve(ship.Transform, onReserved, onFailed);
        }

        public void ReleaseWaitingReservation(Ship ship)
        {
            _waitingZones.ReleaseShip(ship.Transform);
        }

        public void OnShipCompleted(Ship ship)
        {
            _waitingZones.ReleaseShip(ship.Transform);
            _activeShips.Remove(ship);
            UpdateTimeAcceleration();

            if (AreAllShipsCompleted())
                _gameSession.HandleBattleCompleted();
        }

        public void OnShipLeftParking()
        {
            if (_remainingParkingShips > 0)
                _remainingParkingShips--;

            UpdateTimeAcceleration();
        }

        public void OnShipWaiting(Ship ship)
        {
            _waitingZones.PlaceShip(ship.Transform);
            if (_activeShips.Remove(ship))
                UpdateTimeAcceleration();
        }

        private void UpdateTimeAcceleration()
        {
            _gameSession.UpdateTimeAcceleration(
                _activeShips.Count >= ShipsInBattle,
                _remainingParkingShips > 0);
        }

        private bool AreAllShipsCompleted()
        {
            if (_ships == null || _ships.Count == 0)
                return false;

            foreach (Ship ship in _ships.Values)
            {
                if (ship != null && ship.IsNoEmpty)
                    return false;
            }

            return true;
        }

        private bool CanLeaveParking(Ship ship)
        {
            return _shipsPuzzle.HasClearExit(ship.Placement);
        }
    }
}