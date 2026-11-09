using System;
using Galaxy.Data.Infrastructure;

namespace Galaxy.Infrastructure.Boosters
{
    public class BoosterInventoryService : IBoosterInventoryService
    {
        private readonly ISaveService _saveService;

        public event Action<BoosterType, int> Changed;

        public BoosterInventoryService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public bool TryGet(BoosterType type, out int count)
        {
            var boosters = _saveService.GetSaveData().Boosters;

            if (boosters.TryGetValue(type, out int stored))
            {
                count = stored;
                return true;
            }

            count = 0;
            return false;
        }

        public int Get(BoosterType type)
        {
            return TryGet(type, out int count) ? count : 0;
        }

        public void Set(BoosterType type, int count)
        {
            int clamped = Math.Max(0, count);
            var boosters = _saveService.GetSaveData().Boosters;

            if (boosters.TryGetValue(type, out int current) && current == clamped)
                return;

            boosters[type] = clamped;
            Commit(type, clamped);
        }

        public void Add(BoosterType type, int delta)
        {
            if (delta == 0)
                return;

            var boosters = _saveService.GetSaveData().Boosters;
            int current = boosters.TryGetValue(type, out int stored) ? stored : 0;
            int next = Math.Max(0, current + delta);

            boosters[type] = next;
            Commit(type, next);
        }

        public bool TryConsume(BoosterType type)
        {
            if (!TryGet(type, out int count) || count <= 0)
                return false;

            Set(type, count - 1);
            return true;
        }

        private void Commit(BoosterType type, int count)
        {
            _saveService.Save();
            Changed?.Invoke(type, count);
        }
    }
}
