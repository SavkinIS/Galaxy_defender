using Galaxy.Infrastructure.Routing;
using VContainer.Unity;

namespace Galaxy.Bootstrap 
{
    public class BootEntryPoint : IStartable
    {
        private readonly IGlobalRouter _globalRouter;

        public BootEntryPoint(IGlobalRouter globalRouter)
        {
            _globalRouter = globalRouter;
        }
    
        public void Start()
        {
            _globalRouter.ChangeScene(SceneId.Lobby);
        }
    }
}
