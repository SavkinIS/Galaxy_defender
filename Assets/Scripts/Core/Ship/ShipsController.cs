using System.Collections.Generic;
using UnityEngine;

namespace Core.Ship
{
    public class ShipShooterProvider
    {
        private readonly EnemyMap _enemyMap;
        private readonly EnemyCellConnector _enemyCellConnector;

        public ShipShooterProvider(EnemyMap enemyMap, EnemyCellConnector enemyCellConnector)
        {
            _enemyMap = enemyMap;
            _enemyCellConnector = enemyCellConnector;
        }

        public bool HasTargetForShoot(PathPoint position)
        {
            var targetCellIndex =  _enemyCellConnector.GetCellIndex(position);
            
            if (targetCellIndex.x < 0 || targetCellIndex.y < 0)
                return false;
            
            
           return _enemyMap.CanDestroyCell(targetCellIndex);
            
        }

        public void Shoot(PathPoint point)
        {
            var targetCellIndex =  _enemyCellConnector.GetCellIndex(point);

            if (targetCellIndex.x < 0 || targetCellIndex.y < 0)
                return;
            
            _enemyMap.DestroyCell(targetCellIndex);
        }
    }
    
    public class ShipsController
    {
        private readonly ShipsPuzzle _shipsPuzzle;
        private Dictionary<ShipView, Ship> _ships;
        private readonly ShipParkingMover _shipParkingMover;
        private readonly BattleShipMover _shipBattleMover;
        private readonly EnemyMap _enemyMap;
        private readonly EnemyCellConnector _enemyCellConnector;
        private readonly ShipShooterProvider _shootProvider;

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
                        _shipBattleMover, _shootProvider);
            }
        }

        public void MouseClicked(Vector2 screenPosition)
        {
            if (RaycastLaunch.TryGetClickedObject<ShipView>(screenPosition, out var hitObject))
            {
                if (hitObject.TryGetComponent(out ShipView shipView))
                {
                    if (_ships.TryGetValue(shipView, out var ship))
                    {
                        if (ship != null && ship.IsOnParking && CanLeaveParking(ship))
                        {
                            ship.EscapeParking();
                        }
                    }
                }
            } 
        }

        private bool CanLeaveParking(Ship ship)
        {
            return _shipsPuzzle.HasClearExit(ship.Placement);
        }
    }
}