using System;
using YG;

namespace Galaxy.Infrastructure.Platform
{
    public class YandexPlatformService : IPlatformService
    {
        public bool IsReady => YG2.isSDKEnabled;
        public string Language { get;} = "ru-RU";
        public bool IsMobile => YG2.platform.Contains("mobile", StringComparison.InvariantCultureIgnoreCase);
        public event Action Ready;
        
        public void NotifyGameReady()
        {
            YG2.GameReadyAPI();
        }
        
        public YandexPlatformService()
        {
            YG2.onGetSDKData += OnSdkReady;
            if (YG2.isSDKEnabled)
                OnSdkReady();
        }
        private void OnSdkReady()
        {
            YG2.onGetSDKData -= OnSdkReady;
            Ready?.Invoke();
        }
    }
}