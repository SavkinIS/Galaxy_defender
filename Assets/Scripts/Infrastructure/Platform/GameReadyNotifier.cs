using System;
using Galaxy.Infrastructure.Routing;
using VContainer.Unity;

namespace Galaxy.Infrastructure.Platform
{
    public sealed class GameReadyNotifier : IStartable, IDisposable
    {
        private readonly IGlobalRouter _router;
        private readonly IPlatformService _platform;

        public GameReadyNotifier(IGlobalRouter router, IPlatformService platform)
        {
            _router = router;
            _platform = platform;

            _router.OnSceneLoaded += OnSceneLoaded;
        }

        public void Start()
        {
           
        }

        private void OnSceneLoaded(bool success)
        {
            if (success == false)
                return;

            _router.OnSceneLoaded -= OnSceneLoaded;

            Log.Info("[GameReady] signal sent");
            _platform.NotifyGameReady();
        }

        public void Dispose()
        {
            _router.OnSceneLoaded -= OnSceneLoaded;
        }
    }
}
