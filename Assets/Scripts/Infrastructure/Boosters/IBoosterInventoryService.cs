using System;
using Galaxy.Data.Infrastructure;

namespace Galaxy.Infrastructure.Boosters
{
    public interface IBoosterInventoryService
    {
        /// <summary>Raised with the new count after every change.</summary>
        event Action<BoosterType, int> Changed;

        bool TryGet(BoosterType type, out int count);
        int Get(BoosterType type);
        void Set(BoosterType type, int count);
        void Add(BoosterType type, int delta);
        bool TryConsume(BoosterType type);
    }
}
