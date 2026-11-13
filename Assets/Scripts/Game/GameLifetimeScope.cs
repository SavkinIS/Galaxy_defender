using Core;
using Core.Ad;
using Core.Currency;
using Core.Ship;
using Core.Waiting;
using Galaxy.Game.Ship;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Galaxy.Game
{

    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private EnemyMap _enemyMap;
        [SerializeField] private ShipsPuzzle _puzzle;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private GameSessionSettingsSO _gameSessionSettings;
        [SerializeField] private ShipParkingMover _shipParkingMover;
        [SerializeField] private BattleShipMover _battleShipMover;
        [SerializeField] private EnemyCellConnector _enemyCellConnector;
        [SerializeField] private WaitingZonesHandler _waitingZones;


        [Space] [Header("UI")] [SerializeField]
        private WinPopup _winPopup;

        [SerializeField] private SoftCurrencyView _softCurrencyView;
        [SerializeField] private AdUnlockZoneOffer _unlockZoneOffer;
        [SerializeField] private AdOverflowOffer _overflowOffer;
        [SerializeField] private GameOverPopup _gameOverPopup;
        [SerializeField] private BoosterHolder _boosterHolder;
        [SerializeField] private ExitButton _exitButton;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_enemyMap);
            builder.RegisterComponent(_puzzle);
            builder.RegisterComponent(_playerInput);
            builder.RegisterComponent(_shipParkingMover);
            builder.RegisterComponent(_battleShipMover);
            builder.RegisterComponent(_enemyCellConnector);
            builder.RegisterComponent(_waitingZones);

            builder.RegisterComponent(_softCurrencyView);
            builder.RegisterComponent(_winPopup);
            builder.RegisterComponent(_unlockZoneOffer);
            builder.RegisterComponent(_overflowOffer);
            builder.RegisterComponent(_gameOverPopup);
            builder.RegisterComponent(_boosterHolder);
            builder.RegisterComponent(_exitButton);

            builder.RegisterInstance(_gameSessionSettings);

            builder.Register<ShipsController>(Lifetime.Scoped).AsSelf().As<IShipsController>();
            builder.Register<GameSessionHandler>(Lifetime.Scoped).AsSelf().As<IGameSessionHandler>().As<IStartable>();
            builder.Register<WaitingZonesController>(Lifetime.Scoped).AsSelf().As<IWaitingZonesController>();

            builder.RegisterEntryPoint<GameEntryPoint>();

        }
    }
}