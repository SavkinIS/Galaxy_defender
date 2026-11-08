using System;

namespace Galaxy.Infrastructure.Platform
{
    public interface IPlatformService
    {
        bool IsReady { get; }
        string Language { get; }
        bool IsMobile { get; }
        event Action Ready;
        void NotifyGameReady();
    }
}
