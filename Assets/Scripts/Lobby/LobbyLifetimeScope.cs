using Galaxy.Infrastructure.UI.Presenters;
using Galaxy.Lobby.Presenters;
using Galaxy.Lobby.Routing;
using Galaxy.Lobby.Screens;
using Galaxy.Lobby.States;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Galaxy.Lobby
{
    public class LobbyLifetimeScope : LifetimeScope
    { 
        [SerializeField] private MainMenuScreen _mainMenuScreen;
        [SerializeField] private ShopScreen _shopScreen;
        [SerializeField] private SettingsScreen _settingsScreen;
    
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_mainMenuScreen);
            builder.RegisterComponent(_shopScreen);
            builder.RegisterComponent(_settingsScreen);
            
            builder.RegisterComponent(_mainMenuScreen.SoftCurrencyView).Keyed("Main");
            builder.RegisterComponent(_shopScreen.SoftCurrencyView).Keyed("Shop");
            
            builder.Register<LobbyStateMachine>(Lifetime.Scoped);
            builder.Register<MainMenuPresenter>(Lifetime.Scoped).AsSelf().As<IInitializable>();
            builder.Register<ShopPresenter>(Lifetime.Scoped).AsSelf().As<IInitializable>();
            builder.Register<SettingPresenter>(Lifetime.Scoped).AsSelf().As<IInitializable>();
            builder.Register<MainMenuState>(Lifetime.Scoped).As<ILobbyState>();
            builder.Register<ShopState>(Lifetime.Scoped).As<ILobbyState>();
            builder.Register<SettingState>(Lifetime.Scoped).As<ILobbyState>();
            builder.Register<SoftCurrencyPresenter>(Lifetime.Scoped).AsSelf().As<IStartable>();
            
        
            builder.RegisterEntryPoint<LobbyEntryPoint>();
        }
    }
}