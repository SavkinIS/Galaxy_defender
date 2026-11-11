using Core;
using Core.Boosts;
using Core.Currency;
using Core.Store;
using Galaxy.Bootstrap;
using Galaxy.Infrastructure.Boosters;
using Galaxy.Infrastructure.Platform;
using Galaxy.Infrastructure.Routing;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private LoadingCurtain _loadingCurtain;
    [SerializeField] private BoosterCatalogSO _boosterCatalog;
    [SerializeField] private StoreCatalogSO _storeCatalog;

    protected override void Configure(IContainerBuilder builder)
    {
        new PlatformInstaller().Install(builder);

        builder.RegisterComponentInNewPrefab(_loadingCurtain, Lifetime.Singleton)
            .DontDestroyOnLoad().Build();

        builder.Register<GamePauseService>(Lifetime.Singleton);
        builder.Register<SoftCurrencyService>(Lifetime.Singleton).As<ISoftCurrencyService>();
        builder.Register<GlobalRouter>(Lifetime.Singleton).As<IGlobalRouter>();
        builder.Register<FileSaveStorage>(Lifetime.Singleton).AsSelf().As<ISaveStorage>();
        builder.Register<SaveService>(Lifetime.Singleton).AsSelf().As<ISaveService>();
        builder.Register<BoosterInventoryService>(Lifetime.Singleton).AsSelf().As<IBoosterInventoryService>();

        RegisterBoosterCatalog(builder);
        RegisterStore(builder);

        builder.Register<BoosterPurchaseService>(Lifetime.Singleton).AsSelf().As<IBoosterPurchaseService>();

        builder.RegisterEntryPoint<GameReadyNotifier>();

        builder.RegisterComponentOnNewGameObject<SaveOnExitHandler>(Lifetime.Singleton, "SaveOnExitHandler")
            .DontDestroyOnLoad();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        builder.RegisterComponentOnNewGameObject<SoftCurrencyCheat>(Lifetime.Singleton, "SoftCurrencyCheat")
            .DontDestroyOnLoad();
#endif

        builder.RegisterBuildCallback(container =>
        {
            container.Resolve<LoadingCurtain>();
            container.Resolve<SaveOnExitHandler>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            container.Resolve<SoftCurrencyCheat>();
#endif
        });
    }

    private void RegisterBoosterCatalog(IContainerBuilder builder)
    {
        BoosterCatalogSO catalog = _boosterCatalog != null
            ? _boosterCatalog
            : Resources.Load<BoosterCatalogSO>("BoosterCatalog");

        if (catalog == null)
        {
            Log.Error("[RootLifetimeScope] BoosterCatalogSO is missing. Assign it in the inspector " +
                      "or place the asset at Assets/Resources/BoosterCatalog.asset");
            catalog = ScriptableObject.CreateInstance<BoosterCatalogSO>();
        }

        builder.RegisterInstance(catalog);
    }

    private void RegisterStore(IContainerBuilder builder)
    {
        StoreCatalogSO catalog = _storeCatalog != null
            ? _storeCatalog
            : Resources.Load<StoreCatalogSO>("StoreCatalog");

        if (catalog == null)
        {
            Log.Error("[RootLifetimeScope] StoreCatalogSO is missing. Assign it in the inspector " +
                      "or place the asset at Assets/Resources/StoreCatalog.asset");
            catalog = ScriptableObject.CreateInstance<StoreCatalogSO>();
        }

        builder.RegisterInstance(catalog);

#if Payments_yg
        builder.Register<YandexStorePlatform>(Lifetime.Singleton).AsSelf().As<IStorePlatform>();
#else
        builder.Register<EditorStorePlatform>(Lifetime.Singleton).AsSelf().As<IStorePlatform>();
#endif

        builder.Register<StorePurchaseService>(Lifetime.Singleton).AsSelf().As<IStorePurchaseService>();
    }
}
