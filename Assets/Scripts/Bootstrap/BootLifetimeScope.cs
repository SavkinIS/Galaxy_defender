using VContainer;
using VContainer.Unity;

namespace Galaxy.Bootstrap
{
    public class BootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            new PlatformInstaller().Install(builder);
            
            builder.RegisterEntryPoint<BootEntryPoint>();
        }
    }
}