using Core.Ship;
using Core.Waiting;
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
        [SerializeField] private WaitingZonesHandler _waitingZones;
        [SerializeField] private UnityEngine.Events.UnityEvent _onWaitingZoneOverflow = new UnityEngine.Events.UnityEvent();

        private ShipsController _shipsController;
        private GameSessionHandler _gameSessionHandler;
        private WaitingZonesController _waitingZonesController;

        private void Awake()
        {
            _gameSessionHandler = new GameSessionHandler();
            _waitingZonesController = new WaitingZonesController(
                _waitingZones.Zones, _gameSessionHandler);
            _shipsController = new ShipsController(_puzzle, _shipParkingMover, _battleShipMover,
                _enemyMap, _enemyCellConnector, _waitingZonesController, _gameSessionHandler);
        }

        private void OnWaitingZoneOverflow()
        {
            _onWaitingZoneOverflow.Invoke();
        }

        private void Start()
        {
            _shipsController.Initialize();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            UnSubscribe();
        }

        private void UnSubscribe()
        {
            _playerInput.MouseClicked -= _shipsController.MouseClicked;
            _gameSessionHandler.WaitingZoneOverflow -= OnWaitingZoneOverflow;
        }

        private void Subscribe()
        {
            _playerInput.MouseClicked += _shipsController.MouseClicked;
            _gameSessionHandler.WaitingZoneOverflow += OnWaitingZoneOverflow;
        }
    }
}