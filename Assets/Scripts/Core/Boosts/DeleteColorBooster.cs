using UnityEngine;

namespace Core.Boosts
{
    /// <summary>
    /// Removes every block of the clicked color together with the ships of that color, then
    /// re-checks the battle end.
    /// </summary>
    public sealed class DeleteColorBooster : BoosterBase
    {
        public DeleteColorBooster(DeleteColorBoosterConfigSO config, BoosterContext context)
            : base(config, context)
        {
        }

        public override bool RequiresTarget => true;

        public override bool CanApply()
        {
            return Context.EnemyMap != null && Context.EnemyMap.GetActiveColors().Count > 0;
        }

        public override bool Apply()
        {
            return false;
        }

        public override BoosterTargetResult TryApplyAt(Vector2 screenPosition)
        {
            if (Context.EnemyMap == null)
                return BoosterTargetResult.Ignored;

            if (!RaycastLaunch.TryGetClickedObject<Cell>(screenPosition, out var cell))
                return BoosterTargetResult.Ignored;

            if (!Context.EnemyMap.TryGetColorIndex(cell, out var colorIndex))
                return BoosterTargetResult.Ignored;

            if (Context.EnemyMap.DestroyColor(colorIndex) == 0)
                return BoosterTargetResult.Ignored;

            Context.Ships?.RemoveParkingShipsByColor(colorIndex);
            Context.Ships?.CheckBattleCompletion();

            return BoosterTargetResult.Consumed;
        }
    }
}
