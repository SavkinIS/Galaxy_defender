using System;
using Core.Ship;
using UnityEngine;

namespace Core
{
    public class Root : MonoBehaviour
    {
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private EnemyMap _enemyMap;
        [SerializeField] private ShipsPuzzle _puzzle;
        [SerializeField] private ShipParkingMover _shipParkingMover;
        [SerializeField] private BattleShipMover _battleShipMover;
        [SerializeField] private EnemyCellConnector _enemyCellConnector;
        
        private ShipsController _shipsController;

        private void Awake()
        {
            _shipsController = new ShipsController(_puzzle, _shipParkingMover, _battleShipMover, _enemyMap, _enemyCellConnector);
        }

        private void Start()
        {
            _shipsController.Initialize();
        }

        private void OnEnable()
        {
            Subscibe();
        }

        private void OnDisable()
        {
            UnSubscibe();
        }

        private void UnSubscibe()
        {
            _playerInput.MouseClicked -= _shipsController.MouseClicked;
        }

        private void Subscibe()
        {
            _playerInput.MouseClicked += _shipsController.MouseClicked;
        }
    }
}