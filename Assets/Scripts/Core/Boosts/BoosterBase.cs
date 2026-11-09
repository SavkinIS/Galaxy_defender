using Galaxy.Data.Infrastructure;
using UnityEngine;

namespace Core.Boosts
{
    public abstract class BoosterBase : IBooster
    {
        protected readonly BoosterConfigSO Config;
        protected readonly BoosterContext Context;

        protected BoosterBase(BoosterConfigSO config, BoosterContext context)
        {
            Config = config;
            Context = context;
        }

        public BoosterType Type => Config.Type;

        public virtual bool RequiresTarget => false;

        public abstract bool CanApply();

        public abstract bool Apply();

        public virtual BoosterTargetResult TryApplyAt(Vector2 screenPosition)
        {
            return BoosterTargetResult.Ignored;
        }

        public virtual void Cancel()
        {
        }
    }
}
