using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Galaxy.Infrastructure.Routing
{
    public class GlobalRouter : IGlobalRouter
    {
        private bool _loadingInProcess;
        private bool _isSuccess;

        public event Action OnSceneStartLoading;
        public event Action<bool> OnSceneLoaded;
        public event Action<float> OnSceneProgressChanged;


        public async void ChangeScene(SceneId sceneToLoad)
        {
            if (_loadingInProcess)
                return;
            
            _loadingInProcess = true;
            _isSuccess = false;
            
            try
            {
                await Task.Delay(1000);
                
                OnSceneStartLoading?.Invoke();
                AsyncOperation operation = SceneManager.LoadSceneAsync(BuildSettings.GetSceneName(sceneToLoad));

                if (operation == null)
                {
                    throw new NullReferenceException($"Cannot load scene '{sceneToLoad}'");
                }
                
                while (!operation.isDone)
                {
                    OnSceneProgressChanged?.Invoke(operation.progress);
                    await Task.Yield();
                }

                _isSuccess = true;
                
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            finally
            {
                _loadingInProcess = false;
                OnSceneLoaded?.Invoke(_isSuccess);
                _isSuccess = false;
            }
        }
    }
}