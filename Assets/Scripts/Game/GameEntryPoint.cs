using System;
using Core;
using Core.Ad;
using Core.Boosts;
using Core.Ship;
using Core.Waiting;
using Galaxy.Game.Ship;
using Galaxy.Infrastructure.Boosters;
using UnityEngine;
using VContainer.Unity;

namespace Galaxy.Game
{

    public class GameEntryPoint : IStartable, IDisposable
    {
        private readonly GamePauseService _pauseService;
        private readonly IShipsController _shipsController;
        private readonly IWaitingZonesController _waitingZonesController;
        private readonly IGameSessionHandler _sessionHandler;
        private readonly PlayerInput _playerInput;
        private readonly GameOverPopup _gameOverPopup;

        private readonly WinPopup _winPopup;
        private readonly GameSessionSettingsSO _sessionSettings;
        private readonly ShipsPuzzle _puzzle;
        private readonly BattleShipMover _battleShipMover;
        private readonly EnemyCellConnector _enemyCellConnector;
        private readonly BoosterHolder _boosterHolder;
        private readonly EnemyMap _enemyMap;
        private readonly IBoosterInventoryService _boosterInventory;
        private readonly IBoosterPurchaseService _boosterPurchase;
        private readonly BoosterCatalogSO _boosterCatalog;

        private BoosterUsageController _boostersController;

        public GameEntryPoint(IShipsController shipsController,
            IWaitingZonesController waitingZones, IGameSessionHandler sessionHandler, PlayerInput playerInput,
            GameOverPopup gameOverPopup, WinPopup winPopup, GameSessionSettingsSO sessionSettings, ShipsPuzzle puzzle,
            BattleShipMover battleShipMover, EnemyCellConnector enemyCellConnector, BoosterHolder boosterHolder,
            EnemyMap enemyMap,
            GamePauseService pauseService, IBoosterInventoryService boosterInventory,
            IBoosterPurchaseService boosterPurchase,
            BoosterCatalogSO boosterCatalog)
        {
            _shipsController = shipsController;
            _waitingZonesController = waitingZones;
            _sessionHandler = sessionHandler;
            _playerInput = playerInput;
            _gameOverPopup = gameOverPopup;
            _winPopup = winPopup;
            _sessionSettings = sessionSettings;
            _puzzle = puzzle;
            _battleShipMover = battleShipMover;
            _enemyCellConnector = enemyCellConnector;
            _boosterHolder = boosterHolder;
            _enemyMap = enemyMap;
            _pauseService = pauseService;
            _boosterInventory = boosterInventory;
            _boosterPurchase = boosterPurchase;
            _boosterCatalog = boosterCatalog;

            Log.Info("GameEntryPoint initialized");
        }

        public void Start()
        {
            _puzzle.ConfigureGeneration(_waitingZonesController.Capacity,
                _battleShipMover.Points, _enemyCellConnector);

            _shipsController.Initialize();

            InitBoosters();

            Subscribe();
            Log.Info("GameEntryPoint Started");
        }

        private void InitBoosters()
        {
            if (_boosterHolder == null)
                return;

            _boosterHolder.Init(_shipsController, _waitingZonesController,
                _enemyMap, _pauseService, _boosterInventory, _boosterPurchase, _boosterCatalog);

            _boostersController = _boosterHolder.Controller;
            _boostersController?.RefreshAll();
        }

        private void UnSubscribe()
        {
            _playerInput.MouseClicked -= OnMouseClicked;
            _sessionHandler.WaitingZoneOverflow -= OnWaitingZoneOverflow;
            _sessionHandler.BattleCompleted -= OnBattleCompleted;
        }

        private void Subscribe()
        {
            _playerInput.MouseClicked += OnMouseClicked;
            _sessionHandler.WaitingZoneOverflow += OnWaitingZoneOverflow;
            _sessionHandler.BattleCompleted += OnBattleCompleted;
        }

        private void OnMouseClicked(Vector2 screenPosition)
        {
            if (_boostersController != null && _boostersController.TryHandleWorldClick(screenPosition))
                return;

            _shipsController?.MouseClicked(screenPosition);
            _waitingZonesController?.MouseClicked(screenPosition);
        }

        private void OnWaitingZoneOverflow()
        {
            _gameOverPopup?.Show();
        }

        private void OnBattleCompleted()
        {
            int reward = _sessionSettings != null
                ? _sessionSettings.WinReward
                : GameSessionSettingsSO.DefaultWinReward;

            _winPopup?.Show(reward);
        }

        public void Dispose()
        {
            UnSubscribe();
            Log.Info("GameEntryPoint Dispose");
        }
    }
}
