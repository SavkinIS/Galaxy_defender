using UnityEngine;

namespace Core.Boosts
{
    public enum BoosterTargetResult
    {
        /// <summary>The click did not complete the booster: nothing is spent.</summary>
        Ignored,

        /// <summary>The booster was applied, the use can be consumed.</summary>
        Consumed
    }

    public interface IBooster
    {
        BoosterType Type { get; }

        /// <summary>True when the booster waits for a click on a target in the world.</summary>
        bool RequiresTarget { get; }

        /// <summary>False when there is nothing to apply the booster to right now.</summary>
        bool CanApply();

        /// <summary>Applies the booster without a target. Returns false when nothing changed.</summary>
        bool Apply();

        /// <summary>Applies the booster to a clicked world position.</summary>
        BoosterTargetResult TryApplyAt(Vector2 screenPosition);

        /// <summary>Drops a pending target selection.</summary>
        void Cancel();
    }
}
