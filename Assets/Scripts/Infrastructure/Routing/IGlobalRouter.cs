using System;

namespace Galaxy.Infrastructure.Routing
{
    public interface IGlobalRouter
    {
        event Action OnSceneStartLoading;
        event Action<bool> OnSceneLoaded;
        event Action<float> OnSceneProgressChanged;
        
        void ChangeScene(SceneId sceneToLoad);
    }
}