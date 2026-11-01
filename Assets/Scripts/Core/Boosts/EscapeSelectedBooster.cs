using UnityEngine;

namespace Core.Boosts
{
    /// <summary>
    /// Releases the ship the player clicks on, ignoring both the clear-exit rule and the battle
    /// slot limit.
    /// </summary>
    public sealed class EscapeSelectedBooster : BoosterBase
    {
        public EscapeSelectedBooster(EscapeSelectedBoosterConfigSO config, BoosterContext context)
            : base(config, context)
        {
        }

        public override bool RequiresTarget => true;

        public override bool CanApply()
        {
            return Context.Ships != null && Context.Ships.HasShipsOnParking;
        }

        public override bool Apply()
        {
            return false;
        }

        public override BoosterTargetResult TryApplyAt(Vector2 screenPosition)
        {
            if (Context.Ships == null)
                return BoosterTargetResult.Ignored;

            if (!RaycastLaunch.TryGetClickedObject<ShipView>(screenPosition, out var shipView))
                return BoosterTargetResult.Ignored;

            return Context.Ships.TryForceEscape(shipView)
                ? BoosterTargetResult.Consumed
                : BoosterTargetResult.Ignored;
        }
    }
}
