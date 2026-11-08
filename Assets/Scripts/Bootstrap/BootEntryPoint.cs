using System;
using Galaxy.Infrastructure.Platform;
using Galaxy.Infrastructure.Routing;
using VContainer.Unity;

namespace Galaxy.Bootstrap 
{
    public class BootEntryPoint : IStartable, IDisposable
    {
        private readonly IGlobalRouter _globalRouter;
        private readonly IPlatformService _platformService;

        public BootEntryPoint(IGlobalRouter globalRouter, IPlatformService platformService)
        {
            _globalRouter = globalRouter;
            _platformService = platformService;
        }
    
        public void Start()
        {
            _globalRouter.OnSceneLoaded += OnSceneLoaded;
            _platformService.Ready += ChangeScene;
            
            if (_platformService.IsReady)
                ChangeScene();
        }

        private void OnSceneLoaded(bool success)
        {
            if (success == false)
                return;
            
            _globalRouter.OnSceneLoaded -= OnSceneLoaded;
            _platformService.NotifyGameReady();
        }

        private void ChangeScene()
        {
            _platformService.Ready -= ChangeScene;
            
            Log.Info($"Platform is mobile {_platformService.IsMobile } " +
                     $"Language {_platformService.Language}");
            
            _globalRouter.ChangeScene(SceneId.Lobby);
            
        }

        public void Dispose()
        {
            _platformService.Ready -= ChangeScene;
            _globalRouter.OnSceneLoaded -= OnSceneLoaded;
        }
    }
}
