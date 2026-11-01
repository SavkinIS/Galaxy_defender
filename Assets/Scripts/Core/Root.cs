using Core.Ad;
using Core.Boosts;
using Core.Currency;
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
        [SerializeField] private WinPopup _winPopup;
        [SerializeField] private SoftCurrencyView _softCurrencyView;
        [SerializeField] private BoosterHolder _boosterHolder;
        [SerializeField] private UnityEngine.Events.UnityEvent _onWaitingZoneOverflow = new UnityEngine.Events.UnityEvent();

        private SoftCurrencyController _softCurrencyController;
        private ShipsController _shipsController;
        private GameSessionHandler _gameSessionHandler;
        private WaitingZonesController _waitingZonesController;
        private GamePauseService _pauseService;
        private BoostersController _boostersController;

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
            if (_winPopup == null)
                _winPopup = FindObjectOfType<WinPopup>(true);
            if (_softCurrencyView == null)
                _softCurrencyView = FindObjectOfType<SoftCurrencyView>(true);
            if (_boosterHolder == null)
                _boosterHolder = FindObjectOfType<BoosterHolder>(true);

            _softCurrencyController = new SoftCurrencyController();
            _softCurrencyView?.Init(_softCurrencyController);
            _winPopup?.Init(_softCurrencyController);

            _pauseService = new GamePauseService();
            _gameSessionHandler = new GameSessionHandler(_gameSessionSettings, _pauseService);
            _waitingZonesController = new WaitingZonesController(
                _waitingZones.Zones, _gameSessionHandler, _gameSessionSettings,
                _puzzle.RequestedWaitingSlots, _unlockZoneOffer, _overflowOffer);
            _puzzle.ConfigureGeneration(_waitingZonesController.Capacity,
                _battleShipMover.Points, _enemyCellConnector);
            _shipsController = new ShipsController(_puzzle, _shipParkingMover, _battleShipMover,
                _enemyMap, _enemyCellConnector, _waitingZonesController, _gameSessionHandler);

            if (_boosterHolder != null)
            {
                _boosterHolder.Init(_softCurrencyController, _shipsController,
                    _waitingZonesController, _enemyMap, _pauseService);
                _boostersController = _boosterHolder.Controller;
            }
        }

        private void OnWaitingZoneOverflow()
        {
            _gameOverPopup?.Show();
            _onWaitingZoneOverflow.Invoke();
        }

        private void OnBattleCompleted()
        {
            int reward = _gameSessionSettings != null
                ? _gameSessionSettings.WinReward
                : GameSessionSettingsSO.DefaultWinReward;
            _winPopup?.Show(reward);
        }

        private void Start()
        {
            _shipsController.Initialize();
            _boostersController?.RefreshAll();
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
            _playerInput.MouseClicked -= OnMouseClicked;
            _gameSessionHandler.WaitingZoneOverflow -= OnWaitingZoneOverflow;
            _gameSessionHandler.BattleCompleted -= OnBattleCompleted;
        }

        private void Subscribe()
        {
            _playerInput.MouseClicked += OnMouseClicked;
            _gameSessionHandler.WaitingZoneOverflow += OnWaitingZoneOverflow;
            _gameSessionHandler.BattleCompleted += OnBattleCompleted;
        }

        /// <summary>
        /// A booster waiting for a target has priority: the same click must not release a ship or
        /// reserve a waiting zone.
        /// </summary>
        private void OnMouseClicked(Vector2 screenPosition)
        {
            if (_boostersController != null && _boostersController.TryHandleWorldClick(screenPosition))
                return;

            _shipsController?.MouseClicked(screenPosition);
            _waitingZonesController?.MouseClicked(screenPosition);
        }
    }
}