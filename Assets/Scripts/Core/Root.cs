using Core.Ad;
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
        [SerializeField] private GameSessionSettingsSO _gameSessionSettings;
        [SerializeField] private AdUnlockZoneOffer _unlockZoneOffer;
        [SerializeField] private AdOverflowOffer _overflowOffer;
        [SerializeField] private GameOverPopup _gameOverPopup;
        [SerializeField] private UnityEngine.Events.UnityEvent _onWaitingZoneOverflow = new UnityEngine.Events.UnityEvent();

        private ShipsController _shipsController;
        private GameSessionHandler _gameSessionHandler;
        private WaitingZonesController _waitingZonesController;

        private void Awake()
        {
            // Ensure ADController exists in the scene
            if (ADController.Instance == null)
            {
                new GameObject("ADController").AddComponent<ADController>();
            }

            if (_unlockZoneOffer == null)
                _unlockZoneOffer = FindObjectOfType<AdUnlockZoneOffer>(true);
            if (_overflowOffer == null)
                _overflowOffer = FindObjectOfType<AdOverflowOffer>(true);
            if (_gameOverPopup == null)
                _gameOverPopup = FindObjectOfType<GameOverPopup>(true);

            _gameSessionHandler = new GameSessionHandler(_gameSessionSettings);
            _waitingZonesController = new WaitingZonesController(
                _waitingZones.Zones, _gameSessionHandler, _gameSessionSettings,
                _puzzle.RequestedWaitingSlots, _unlockZoneOffer, _overflowOffer);
            _puzzle.ConfigureGeneration(_waitingZonesController.Capacity,
                _battleShipMover.Points, _enemyCellConnector);
            _shipsController = new ShipsController(_puzzle, _shipParkingMover, _battleShipMover,
                _enemyMap, _enemyCellConnector, _waitingZonesController, _gameSessionHandler);
        }

        private void OnWaitingZoneOverflow()
        {
            _gameOverPopup?.Show();
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

        private void OnDestroy()
        {
            _gameSessionHandler?.Dispose();
        }

        private void UnSubscribe()
        {
            _playerInput.MouseClicked -= _shipsController.MouseClicked;
            _playerInput.MouseClicked -= _waitingZonesController.MouseClicked;
            _gameSessionHandler.WaitingZoneOverflow -= OnWaitingZoneOverflow;
        }

        private void Subscribe()
        {
            _playerInput.MouseClicked += _shipsController.MouseClicked;
            _playerInput.MouseClicked += _waitingZonesController.MouseClicked;
            _gameSessionHandler.WaitingZoneOverflow += OnWaitingZoneOverflow;
        }
    }
}