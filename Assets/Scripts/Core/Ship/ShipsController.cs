using System.Collections.Generic;
using UnityEngine;

namespace Core.Ship
{
    public class ShipsController
    {
        private const int ShipsInBattle = 5;
        
        private readonly ShipsPuzzle _shipsPuzzle;
        private Dictionary<ShipView, Ship> _ships;
        private readonly ShipParkingMover _shipParkingMover;
        private readonly BattleShipMover _shipBattleMover;
        private readonly EnemyMap _enemyMap;
        private readonly EnemyCellConnector _enemyCellConnector;
        private readonly ShipShooterProvider _shootProvider;

        private List<Ship> _activeShips =  new List<Ship>();
        
        public ShipsController(ShipsPuzzle shipsPuzzle, ShipParkingMover shipParkingMover,  BattleShipMover shipBattleMover, EnemyMap enemyMap, EnemyCellConnector enemyCellConnector)
        {
            _shipsPuzzle = shipsPuzzle;
            _shipParkingMover =  shipParkingMover;
            _shipBattleMover = shipBattleMover;
            _enemyMap = enemyMap;
            _enemyCellConnector = enemyCellConnector;

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
                        _shipBattleMover, _shootProvider, OnShipCompleted);
            }
        }

        public void MouseClicked(Vector2 screenPosition)
        {
            if (_activeShips.Count == ShipsInBattle)
                return;
            
            if (RaycastLaunch.TryGetClickedObject<ShipView>(screenPosition, out var hitObject))
            {
                if (hitObject.TryGetComponent(out ShipView shipView))
                {
                    if (_ships.TryGetValue(shipView, out var ship))
                    {
                        if (ship != null && ship.IsOnParking && CanLeaveParking(ship))
                        {
                            ship.EscapeParking();
                            _activeShips.Add(ship);
                        }
                    }
                }
            } 
        }

        private void OnShipCompleted(Ship ship)
        {
            _activeShips.Remove(ship);
        }
        
        private bool CanLeaveParking(Ship ship)
        {
            return _shipsPuzzle.HasClearExit(ship.Placement);
        }
    }
}