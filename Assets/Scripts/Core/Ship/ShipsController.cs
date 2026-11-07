using System;
using System.Collections.Generic;
using Core.Waiting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Ship
{
    public class ShipsController : IShipsController
    {
        private const int BaseBattleSlots = 5;

        private ShipsPuzzle _shipsPuzzle;
        private Dictionary<ShipView, Ship> _ships;
        private ShipParkingMover _shipParkingMover;
        private BattleShipMover _shipBattleMover;
        private EnemyMap _enemyMap;
        private EnemyCellConnector _enemyCellConnector;
        private ShipShooterProvider _shootProvider;
        private IWaitingZonesController _waitingZones;
        private IGameSessionHandler _gameSession;

        private List<Ship> _activeShips =  new List<Ship>();
        private int _remainingParkingShips;
        private int _battleSlots = BaseBattleSlots;
        private bool _initialized;
        private bool _completionSuspended;

        public int BattleSlots => _battleSlots;

        public bool HasShipsOnParking
        {
            get
            {
                if (_ships == null)
                    return false;

                foreach (var ship in _ships.Values)
                {
                    if (ship != null && ship.IsOnParking)
                        return true;
                }

                return false;
            }
        }

        [Inject]
        public void Construct(ShipsPuzzle shipsPuzzle, ShipParkingMover shipParkingMover,
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
        
        public void MouseClicked(Vector2 screenPosition)
        {
            var adController = ADController.Instance;
            if (adController != null && adController.IsShowing)
                return;

            if (!_gameSession.IsRunning || _activeShips.Count >= _battleSlots)
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

        public bool TryEnterBattle(Ship ship, bool ignoreSlotLimit = false)
        {
            if (!_gameSession.IsRunning)
                return false;

            if (_activeShips.Contains(ship))
                return true;

            if (!ignoreSlotLimit && _activeShips.Count >= _battleSlots)
                return false;

            _activeShips.Add(ship);
            _waitingZones.ReleaseShip(ship.Transform);
            UpdateTimeAcceleration();
            return true;
        }

        public bool HasFreeBattleSlot => _gameSession.IsRunning && _activeShips.Count < _battleSlots;

        public void IncreaseBattleSlots(int delta)
        {
            if (delta <= 0)
                return;

            _battleSlots += delta;
            UpdateTimeAcceleration();
        }

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
            CheckBattleCompletion();
        }

        public void SuspendBattleCompletion(bool suspended)
        {
            _completionSuspended = suspended;
        }

        public void CheckBattleCompletion()
        {
            if (_completionSuspended)
                return;

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

        public bool TryGetShip(Transform shipTransform, out Ship ship)
        {
            ship = null;

            if (shipTransform == null || _ships == null)
                return false;

            foreach (var pair in _ships)
            {
                if (pair.Key == null || pair.Key.transform != shipTransform)
                    continue;

                ship = pair.Value;
                return ship != null;
            }

            return false;
        }

        public bool TryForceEscape(ShipView shipView)
        {
            if (shipView == null || _ships == null || !_gameSession.IsRunning)
                return false;

            if (!_ships.TryGetValue(shipView, out var ship) || ship == null || !ship.IsOnParking)
                return false;

            if (!TryEnterBattle(ship, true))
                return false;

            ship.EscapeParking();
            return true;
        }

        public int RemoveParkingShipsByColor(int colorIndex)
        {
            if (_ships == null)
                return 0;

            var removable = new List<ShipView>();

            foreach (var pair in _ships)
            {
                if (pair.Key == null || pair.Value == null || !pair.Value.IsOnParking)
                    continue;

                var data = pair.Key.Data;
                if (data == null || data.ColorIndex != colorIndex)
                    continue;

                removable.Add(pair.Key);
            }

            foreach (var view in removable)
            {
                var ship = _ships[view];
                _ships.Remove(view);
                ship.ForceRemoveFromParking();
            }

            return removable.Count;
        }

        private void UpdateTimeAcceleration()
        {
            _gameSession.UpdateTimeAcceleration(
                _activeShips.Count >= _battleSlots,
                _remainingParkingShips > 0);
        }

        private bool AreAllShipsCompleted()
        {
            if (!_initialized || _ships == null)
                return false;

            foreach (Ship ship in _ships.Values)
            {
                if (ship != null && ship.IsNoEmpty)
                    return false;
            }

            // The last ship may still be fighting: the battle ends only after it leaves.
            return _activeShips.Count == 0;
        }

        private bool CanLeaveParking(Ship ship)
        {
            return _shipsPuzzle.HasClearExit(ship.Placement);
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
            _initialized = true;
        }
    }
}