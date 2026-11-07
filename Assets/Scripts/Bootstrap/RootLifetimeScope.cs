using Core;
using Core.Currency;
using Galaxy.Infrastructure.Routing;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private LoadingCurtain _loadingCurtain;
    
    protected override void Configure(IContainerBuilder builder)
    {
       
        builder.RegisterComponentInNewPrefab(_loadingCurtain, Lifetime.Singleton)
            .DontDestroyOnLoad();
        
        builder.Register<GamePauseService>(Lifetime.Singleton);
        builder.Register<SoftCurrencyService>(Lifetime.Singleton).As<ISoftCurrencyService>();
        builder.Register<GlobalRouter>(Lifetime.Singleton).As<IGlobalRouter>();
    }
}
