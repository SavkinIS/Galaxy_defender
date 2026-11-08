using Galaxy.Infrastructure.Platform;
using VContainer;
using VContainer.Unity;

namespace Galaxy.Bootstrap
{
    public sealed class PlatformInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<IPlatformService, YandexPlatformService>(Lifetime.Singleton);
        }
    }
}