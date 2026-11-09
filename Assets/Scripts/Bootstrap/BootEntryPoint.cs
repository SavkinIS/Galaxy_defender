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
        private readonly ISaveService _saveServce;

        public BootEntryPoint(IGlobalRouter globalRouter, IPlatformService platformService, ISaveService saveService)
        {
            _globalRouter = globalRouter;
            _platformService = platformService;
            _saveServce = saveService;
        }
    
        public void Start()
        {
            _saveServce.Load();
            _platformService.Ready += ChangeScene;
            
            if (_platformService.IsReady)
                ChangeScene();
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
        }
    }
}
