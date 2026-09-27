using System.Collections.Generic;
using UnityEngine;

namespace Core.Ship
{
    public class ShipsController
    {
        private readonly ShipsPuzzle _shipsPuzzle;
        private Dictionary<ShipView, Ship> _ships;

        public ShipsController(ShipsPuzzle shipsPuzzle)
        {
            _shipsPuzzle = shipsPuzzle;
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
                    _ships[createdShip.Value] = new Ship(createdShip.Key,  createdShip.Value);
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
                        if (ship != null)
                        {
                           
                        }
                    }
                }
            } 
        }
    }
}