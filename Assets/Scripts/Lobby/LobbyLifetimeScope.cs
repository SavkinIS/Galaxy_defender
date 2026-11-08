using Core.Currency;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Galaxy.Lobby
{
    public class LobbyLifetimeScope : LifetimeScope
    { 
        [SerializeField] private GameStarter  gameStarter;
        [SerializeField] private SoftCurrencyView _softCurrencyView;
    
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(gameStarter);
            builder.RegisterComponent(_softCurrencyView);
        }
    }
}